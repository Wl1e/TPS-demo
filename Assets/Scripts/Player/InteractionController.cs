using System;
using UnityEngine;

namespace TPSDemo
{
    public class InteractionController : MonoBehaviour
    {
        [Tooltip("交互距离")]
        [SerializeField] float m_InteractRange = 3f;
        [Tooltip("交互层级")]
        [SerializeField] LayerMask m_InteractLayerMask;
        [Tooltip("交互输入")]
        [SerializeField] GameEvent m_OnInteractInput;

        [SerializeField] AudioClip m_PickupAudio;

        PlayerController m_PlayerController;

        // 当前瞄准的可互动对象
        IInteractive m_CurrentTarget;

        public event Action<int, int> OnPickup;
        public event Action OnInteraction;

        private void Awake()
        {
            m_PlayerController = GetComponent<PlayerController>();
        }

        private void OnEnable()
        {
            m_OnInteractInput.RegisterListener(OnInteract);
        }
        private void OnDisable()
        {
            m_OnInteractInput.UnregisterListener(OnInteract);
        }

        void Update()
        {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            // Debug.DrawRay(ray.origin, ray.direction, Color.green, 0.1f, true);

            if (Physics.Raycast(ray, out RaycastHit hit, m_InteractRange, m_InteractLayerMask)) {
                if (hit.collider.TryGetComponent<IInteractive>(out var interactive)) {
                    // 显示交互 UI
                    //print("interactive: " + interactive);
                    m_CurrentTarget = interactive;
                } else {
                    m_CurrentTarget = null;
                }
            } else {
                m_CurrentTarget = null;
            }
        }

        public void OnPickupItem(ItemPickup item)
        {
            OnPickup?.Invoke(item.Id, item.Amount);
            Director.Instance.RequestAudio(m_PickupAudio).WithPosition(transform.position).Play();
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

        void OnInteract()
        {
            m_CurrentTarget?.Interact(gameObject);
        }
    }
}
