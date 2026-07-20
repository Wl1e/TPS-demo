using UnityEngine;

namespace TPSDemo
{
    public class ReviveInteraction : MonoBehaviour, IInteractive
    {
        PlayerLifeController m_LifeController;

        void Awake()
        {
            m_LifeController = GetComponentInParent<PlayerLifeController>();
        }

        public float HoldDuration => 8f;
        public void OnInteractPress(GameObject interactor)
        {
            m_LifeController?.OnReviveInteract(interactor);
        }
        public void OnInteractHold(GameObject interactor)
        { }
        public void OnInteractRelease(GameObject interactor, bool completed)
        { }
    }
}
