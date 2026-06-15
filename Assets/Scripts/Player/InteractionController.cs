using System;
using UnityEngine;

namespace TPSDemo
{
    public class InteractionController : MonoBehaviour
    {
        [Tooltip("交互距离")]
        [SerializeField] float m_InteractRange = 3f;
        [Tooltip("交互层级")]
        [SerializeField] LayerMask interactLayerMask;
        [Tooltip("交互输入")]
        [SerializeField] GameEvent onInteractInput;

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
            onInteractInput.RegisterListener(OnInteract);
        }
        private void OnDisable()
        {
            onInteractInput.UnregisterListener(OnInteract);
        }

        void Update()
        {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            // Debug.DrawRay(ray.origin, ray.direction, Color.green, 0.1f, true);

            if (Physics.Raycast(ray, out RaycastHit hit, m_InteractRange, interactLayerMask)) {
                if (hit.collider.TryGetComponent<IInteractive>(out var interactive)) {
                    // 显示交互 UI
                    m_CurrentTarget = interactive;
                    //print("see target: " + m_CurrentTarget.ToString());
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
