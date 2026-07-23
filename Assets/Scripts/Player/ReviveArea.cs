using UnityEngine;

namespace TPSDemo
{

    public class ReviveArea : MonoBehaviour, IInteractive
    {
        [SerializeField] private PlayerController m_Owner;
        [Tooltip("需要持续时间")]
        [SerializeField] float m_ReviveDuration = 3f;
        public float HoldDuration => m_ReviveDuration;

        public string Hint => $"救起 {m_Owner.name}";

        public void OnInteractPress(GameObject interactor)
        { }

        public void OnInteractHold(GameObject interactor)
        { }

        public void OnInteractRelease(GameObject interactor, bool completed)
        {
            if(m_Owner.TryGetComponent<PlayerLifeController>(out var playerLifeController)) {
                playerLifeController.OnReviveInteract(m_Owner.gameObject);
            }
        }
    }
}
