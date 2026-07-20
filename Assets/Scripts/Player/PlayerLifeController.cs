using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{
    public class PlayerLifeController : NetworkBehaviour
    {
        [SerializeField] GameObject m_ReviveArea;
        [SerializeField] float m_ReviveDuration = 3f;

        NetworkVariable<bool> m_IsDead = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        PlayerController m_Player;
        PlayerInputHandler m_InputHandler;
        Renderer[] m_Renderers;
        bool m_IsDeathEntered = false;

        public bool IsDead => m_IsDead.Value;

        private void Awake()
        {
            m_Player = GetComponent<PlayerController>();
            m_InputHandler = GetComponent<PlayerInputHandler>();
            m_Renderers = GetComponentsInChildren<Renderer>(true);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            m_IsDead.OnValueChanged += OnDeathStateChanged;
            //if (m_IsDead.Value) {
            //    OnDeathStateChanged(false, true);
            //}
        }

        void OnDeathStateChanged(bool oldValue, bool newValue)
        {
            if (newValue) {
                EnterDeath();
            } else {
                EnterRevive();
            }
        }

        private void EnterDeath()
        {
            if (m_IsDeathEntered) {
                return;
            }
            m_IsDeathEntered = true;

            UpdateComponentState(false);
            //HideRenderersForOwner();
            m_ReviveArea.gameObject.SetActive(true);
        }

        private void EnterRevive()
        {
            if (!m_IsDeathEntered)
                return;
            m_IsDeathEntered = false;

            UpdateComponentState(true);
            //ShowRenderers();

            m_ReviveArea.gameObject.SetActive(false);
        }

        private void UpdateComponentState(bool enable)
        {
            if (!IsOwner) {
                return;
            }

            SetComponentEnabled(m_Player.Movement, enable);
            SetComponentEnabled(m_Player.StateMachine, enable);
            SetComponentEnabled(m_Player.CameraController, enable);
            if (m_Player.CharacterController != null) {
                m_Player.CharacterController.enabled = enable;
            }

            SetComponentEnabled(m_Player.InteractionController, enable);

            var aim = m_Player.GetComponentInChildren<AimController>();
            SetComponentEnabled(aim, enable);
            var weapon = m_Player.GetComponentInChildren<WeaponManager>();
            SetComponentEnabled(weapon, enable);
            var combat = m_Player.GetComponentInChildren<CombatController>();
            SetComponentEnabled(combat, enable);
            SetComponentEnabled(m_InputHandler, enable);

            if (TryGetComponent<AudioListener>(out var listener)) {
                listener.enabled = enable;
            }
        }

        private void SetComponentEnabled(Behaviour component, bool enabled)
        {
            if (component != null) {
                component.enabled = enabled;
            }
        }


        public void HandleDeathLocally()
        {
            if(!IsOwner) {
                return;
            }
            if(IsDead) {
                return;
            }
            EnterDeath();
            m_IsDead.Value = true;
        }

        public void OnReviveInteract(GameObject reviver)
        {
            if (!IsDead) {
                return;
            }

            var netObj = reviver.GetComponent<NetworkObject>();
            if (netObj != null) {
                StartReviveServerRpc(netObj);
            }
        }

        [ServerRpc]
        private void StartReviveServerRpc(NetworkObjectReference reviverRef)
        {
            if (!m_IsDead.Value) {
                return;
            }

            m_IsDead.Value = false;
            m_Player.Health.Revive();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            m_IsDead.OnValueChanged -= OnDeathStateChanged;
        }

        // 显示或隐藏实体
        //private void HideRenderersForOwner()
        //{
        //    if (!IsOwner) {
        //        return;
        //    }

        //    foreach (var r in m_Renderers) {
        //        r.enabled = false;
        //    }
        //}

        //private void ShowRenderers()
        //{
        //    foreach (var r in m_Renderers) {
        //        r.enabled = true;
        //    }
        //}
    }
}
