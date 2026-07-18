using System;
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
        [SerializeField] ShopUI m_ShopUI;

        [SerializeField] Image Frame;

        IPanel m_CurrentPanel = null;

        [SerializeField] GameEvent m_OpenSettingEvent;

        private void Start()
        {
            if (PlayerDataProxy.Instance.HasPlayer()) {
                Initialize();
            } else {
                EventManager.AddListener<PlayerFinishedInitialzeEvent>(OnPlayerFinishedInitialze);
            }
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

            EventManager.AddListener<ShopOpenEvent>(evt => Open(m_ShopUI));
            EventManager.AddListener<ShopCloseEvent>(evt => Close(m_ShopUI));
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
                print("Open " + panel);
                if (Cursor.lockState == CursorLockMode.Locked) {
                    Cursor.lockState = CursorLockMode.None;
                }
            }
        }

        private void Close(IPanel panel)
        {
            if(m_CurrentPanel == panel) {
                m_CurrentPanel.Close();
                m_CurrentPanel = null;
                print("Close " + panel);
                if (Cursor.lockState == CursorLockMode.None) {
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }

        public void CloseAllUI()
        {
            m_HUD.gameObject.SetActive(false);
            m_InventoryUI.gameObject.SetActive(false);
            m_DialogUI.gameObject.SetActive(false);
            m_LoadoutUI.gameObject.SetActive(false);
            m_QuestUI.gameObject.SetActive(false);
            m_SettingUI.gameObject.SetActive(false);
        }
    }
}
