using TPSDemo.Event;
using UnityEngine;
using UnityEngine.UI;

namespace TPSDemo.UI
{
    public class UIController : Singleton<UIController>
    {
        [SerializeField] HUD m_HUD;
        [SerializeField] InventoryUI m_InventoryUI;
        [SerializeField] DialogUI m_DialogUI;
        [SerializeField] LoadoutUI m_LoadoutUI;
        [SerializeField] QuestPanelUI m_QuestUI;
        [SerializeField] SettingUI m_SettingUI;
        [SerializeField] ShopUIManager m_ShopUI;

        [SerializeField] GameEvent m_ActiveCursorEvent;

        [SerializeField] Image Frame;

        IPanel m_CurrentPanel = null;

        [SerializeField] GameEvent m_OpenSettingEvent;

        protected override void Awake()
        {
            base.Awake();
            if(Instance != this) {
                Destroy(transform.parent.gameObject);
            }
        }

        private void Start()
        {
            if (PlayerDataProxy.Instance.HasPlayer()) {
                Initialize();
            } else {
                EventManager.AddListener<PlayerFinishedInitialzeEvent>(OnPlayerFinishedInitialze);
            }
            m_ActiveCursorEvent.RegisterListener(() => {
                if (m_CurrentPanel == null) {
                    if (Cursor.lockState == CursorLockMode.Locked) {
                        Cursor.lockState = CursorLockMode.None;
                    } else if (Cursor.lockState == CursorLockMode.None) {
                        Cursor.lockState = CursorLockMode.Locked;
                    }
                }
            } );
        }

        public void OnPlayerFinishedInitialze(PlayerFinishedInitialzeEvent evt)
        {
            EventManager.RemoveListener<PlayerFinishedInitialzeEvent>(OnPlayerFinishedInitialze);
            Initialize();
        }

        public void Initialize()
        {
            Frame.gameObject.SetActive(false);
            m_HUD.Initialze();
            m_InventoryUI.Initialize();
            m_LoadoutUI.Initialize();
            m_QuestUI.Initialize();
            m_SettingUI.Initialize();

            // inventory and loadout
            EventManager.AddListener<InventoryStateChangeEvent>(
                 evt => {
                     if (m_CurrentPanel == null) {
                        Open(m_InventoryUI);
                    } else {
                        Close(m_InventoryUI);
                     }
                }
            );

            // dialog
            EventManager.AddListener<StartDialogEvent>(evt => Open(m_DialogUI));
            EventManager.AddListener<EndDialogEvent>(evt => Close(m_DialogUI));

            // quest
            EventManager.AddListener<QuestStateChangeEvent>(
                evt => {
                    if (m_CurrentPanel == null) {
                        Open(m_QuestUI);
                    } else {
                        Close(m_QuestUI);
                    }
                }
            );

            m_OpenSettingEvent.RegisterListener(OnEscPressed);

            EventManager.AddListener<OpenShopUIEvent>(evt => Open(m_ShopUI));
            EventManager.AddListener<CloseShopUIEvent>(evt => Close(m_ShopUI));
        }

        private void OnEscPressed()
        {
            if(m_CurrentPanel != null) {
                Close(m_CurrentPanel);
            } else {
                Open(m_SettingUI);
            }
        }

        private void Open(IPanel panel)
        {
            if (m_CurrentPanel == null) {
                m_CurrentPanel = panel;
                m_CurrentPanel.Open();
                Debug.Log("Open " + panel);
                if (Cursor.lockState == CursorLockMode.Locked) {
                    Cursor.lockState = CursorLockMode.None;
                }
            }
        }

        public void Close(IPanel panel)
        {
            if(m_CurrentPanel == panel) {
                m_CurrentPanel.Close();
                m_CurrentPanel = null;
                Debug.Log("Close " + panel);
                if (Cursor.lockState == CursorLockMode.None) {
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }

        public void CloseCurPanel()
        {
            if (m_CurrentPanel != null) {
                Close(m_CurrentPanel);
            }
        }
    }
}
