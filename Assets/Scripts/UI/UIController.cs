using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

namespace TPSDemo.UI
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] HUD m_HUD;
        [SerializeField] InventoryUI m_InventoryUI;
        [SerializeField] DialogUI m_DialogUI;
        [SerializeField] LoadoutUI m_LoadoutUI;

        [SerializeField] Image Frame;

        private void Start()
        {
            if (PlayerDataProxy.Instance.HasPlayer()) {
                Initialize();
            } else {
                EventManager.AddListener<Event.PlayerFinishedInitialzeEvent>(OnPlayerFinishedInitialze);
            }
        }

        public void OnPlayerFinishedInitialze(Event.PlayerFinishedInitialzeEvent evt)
        {
            EventManager.RemoveListener<Event.PlayerFinishedInitialzeEvent>(OnPlayerFinishedInitialze);
            Initialize();
        }

        public void Initialize()
        {
            Frame.gameObject.SetActive(false);
            m_HUD.Initialze();
            m_InventoryUI.Initialize();
            m_LoadoutUI.Initialize();
        }
    }
}
