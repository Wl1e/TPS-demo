using System;
using UnityEngine;

namespace TPSDemo
{
    public class InteractionController : MonoBehaviour
    {
        [Tooltip("交互距离")]
        [SerializeField] private float m_InteractRange = 3f;
        [Tooltip("交互层级")]
        [SerializeField] private LayerMask m_InteractLayerMask;
        [Tooltip("交互输入")]
        [SerializeField] private BoolEvent m_OnInteractInput;

        [SerializeField] private AudioClip m_PickupAudio;

        private PlayerController m_PlayerController;

        /// <summary>
        /// 当前是否正在交互
        /// </summary>
        private bool m_IsInteracting = false;
        /// <summary>
        /// 当前正在交互的对象
        /// </summary>
        private IInteractive m_InteractingObj = null;
        /// <summary>
        /// 交互时间
        /// </summary>
        private float m_InteractTime = 0f;

        // 当前瞄准的可互动对象
        IInteractive m_CurrentTarget;

        public event Action<int, int> OnPickup;
        public event Action OnInteraction;

        private void Awake()
        {
            m_PlayerController = GetComponent<PlayerController>();
        }

        private void Start()
        {
            m_PlayerController.AudioEffectPlayer.AddAudio("Interact", m_PickupAudio);
        }

        private void OnEnable()
        {
            m_OnInteractInput.RegisterListener(OnInteract);
        }
        private void OnDisable()
        {
            m_OnInteractInput.UnregisterListener(OnInteract);
        }

        private void Update()
        {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            // Debug.DrawRay(ray.origin, ray.direction, Color.green, 0.1f, true);

            if (Physics.Raycast(ray, out RaycastHit hit, m_InteractRange, m_InteractLayerMask)) {
                if (hit.collider.TryGetComponent<IInteractive>(out var interactive)) {
                    UpdateCurrentTarget(interactive);
                } else {
                    UpdateCurrentTarget(null);
                }
            } else {
                UpdateCurrentTarget(null);
            }

            if(m_IsInteracting) {
                m_InteractTime += Time.deltaTime;
                if (m_InteractTime >= m_InteractingObj.HoldDuration) {
                    FinishInteraction();
                } else {
                    m_InteractingObj.OnInteractHold(gameObject);
                    EventManager.Broadcast(
                        new Event.UpdateInteractionHintEvent {
                            Hint = m_CurrentTarget.Hint,
                            Progress = Mathf.Clamp01(m_InteractTime / m_InteractingObj.HoldDuration)
                        }
                    );
                }
            }
        }

        private void UpdateCurrentTarget(IInteractive interactive)
        {
            if (m_CurrentTarget != interactive) {
                m_CurrentTarget = interactive;
                // 显示交互 UI
                EventManager.Broadcast(
                    new Event.UpdateInteractionHintEvent {
                        Hint = m_CurrentTarget?.Hint
                    }
                );
            }
        }

        public void OnPickupItem(ItemPickup item)
        {
            OnPickup?.Invoke(item.Id, item.Amount);
            m_PlayerController.AudioEffectPlayer.Play("Interact",
                AudioSystem.AudioGroup.SFX, float.PositiveInfinity,
                transform.position, Quaternion.identity);
            EventManager.Broadcast(new Event.PickupItemEvent {
                ActorId = m_PlayerController.Id,
                ItemId = item.Id,
                Amount = item.Amount,
            });
        }
        public void OnInteracted()
        {
            OnInteraction?.Invoke();
        }

        private void OnInteract(bool pressed)
        {
            if (pressed && m_CurrentTarget != null) {
                if (!m_IsInteracting) {
                    m_InteractTime = 0f;
                    m_InteractingObj = m_CurrentTarget;
                    m_InteractingObj.OnInteractPress(gameObject);
                    m_IsInteracting = true;
                }
            }
            if(!pressed && m_InteractingObj != null) {
                FinishInteraction();
            }
        }

        private void FinishInteraction()
        {
            if (!m_IsInteracting) {
                return;
            }
            m_IsInteracting = false;
            m_InteractingObj?.OnInteractRelease(gameObject, m_InteractTime >= m_InteractingObj.HoldDuration);
            m_InteractTime = 0f;
            m_InteractingObj = null;
        }
    }
}
