using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{
    public class PlayerLifeController : NetworkBehaviour
    {
        [SerializeField] GameObject m_ReviveArea;

        private readonly NetworkVariable<bool> m_IsDead = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        PlayerController m_Player;

        public bool IsDead => m_IsDead.Value;

        private void Awake()
        {
            m_Player = GetComponent<PlayerController>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            m_IsDead.OnValueChanged += OnDeathStateChanged;
            m_ReviveArea.SetActive(false);
            //if (m_IsDead.Value) {
            //    OnDeathStateChanged(false, true);
            //}
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            m_IsDead.OnValueChanged -= OnDeathStateChanged;
        }

        void OnDeathStateChanged(bool oldValue, bool newValue)
        {
            if (IsOwner) {
                Debug.Log("died: " + IsDead);
            }
            m_Player.RuntimeData.IsDied = IsDead;
            if (newValue) {
                EnterDeath();
            } else {
                EnterRevive();
            }
        }

        private void EnterDeath()
        {
            //UpdateComponentState(false);
            //HideRenderersForOwner();
            m_ReviveArea.SetActive(true);
        }

        private void EnterRevive()
        {
            //UpdateComponentState(true);
            //ShowRenderers();
            m_ReviveArea.SetActive(false);
        }


        public void HandleDeathLocally()
        {
            if(!IsOwner) {
                return;
            }
            if(IsDead) {
                return;
            }

            //EnterDeath();
            m_IsDead.Value = true;
        }

        public void OnReviveInteract(GameObject reviver)
        {
            if (!IsDead) {
                return;
            }

            var netObj = reviver.GetComponent<NetworkObject>();
            if (netObj != null) {
                ReviveRpc(netObj);
            }
        }

        //[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        //private void ReviveServerRpc(NetworkObjectReference reviverRef) => ReviveClientRpc(reviverRef);

        [Rpc(SendTo.Owner)]
        private void ReviveRpc(NetworkObjectReference reviverRef)
        {
            if (!IsDead) {
                return;
            }

            m_IsDead.Value = false;

            EventManager.Broadcast(new Event.ActorReviveEvent { ActorId = m_Player.Id });
            m_Player.Health.ReviveServerRpc();
        }

        //private void UpdateComponentState(bool enable)
        //{
        //    SetComponentEnabled(m_Player.Movement, enable);
        //    SetComponentEnabled(m_Player.StateMachine, enable);
        //    SetComponentEnabled(m_Player.CameraController, enable);
        //    if (m_Player.CharacterController != null) {
        //        m_Player.CharacterController.enabled = enable;
        //    }

        //    SetComponentEnabled(m_Player.InteractionController, enable);

        //    var aim = m_Player.GetComponentInChildren<AimController>();
        //    SetComponentEnabled(aim, enable);
        //    var weapon = m_Player.GetComponentInChildren<WeaponManager>();
        //    SetComponentEnabled(weapon, enable);
        //    var combat = m_Player.GetComponentInChildren<CombatController>();
        //    SetComponentEnabled(combat, enable);
        //    SetComponentEnabled(m_Player.PlayerInputHandler, enable);

        //    //if (m_StateMachine.Controller.TryGetComponent<AudioListener>(out var listener)) {
        //    //    listener.enabled = enable;
        //    //}

        //    m_Player.RuntimeData.DisableCombat = !enable;
        //    m_Player.RuntimeData.CanUseActiveItem = enable;
        //}

        //private void SetComponentEnabled(Behaviour component, bool enabled)
        //{
        //    if (component != null) {
        //        component.enabled = enabled;
        //    }
        //}
    }
}
