using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TPSDemo.UI
{
using Event;

    public class DialogUI : MonoBehaviour, IPanel
    {
        public Image BackGround;
        public TextMeshProUGUI Name;
        public TextMeshProUGUI ContentText;
        public ScrollRect OptionsPanel;
        public GameObject OptionButtonPrefab;

        int m_CurrentPlayerId;
        string m_NpcName;

        void Start()
        {
            //SetActive
            EventManager.AddListener<StartDialogEvent>(DialogStart);
            EventManager.AddListener<EndDialogEvent>(DialogEnd);
            EventManager.AddListener<UpdateDialogEvent>(DialogUpdate);
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener<StartDialogEvent>(DialogStart);
            EventManager.RemoveListener<EndDialogEvent>(DialogEnd);
            EventManager.RemoveListener<UpdateDialogEvent>(DialogUpdate);
        }

        void DialogStart(StartDialogEvent evt)
        {
            m_CurrentPlayerId = evt.PlayerId;
            m_NpcName = evt.Npc.Name;
        }

        void DialogEnd(EndDialogEvent evt)
        {
            ClearUI();
        }

        void DialogUpdate(UpdateDialogEvent evt)
        {
            ClearUI();
            var node = evt.DialogueNode;
            Name.text = m_NpcName;
            ContentText.text = node.Content;
            for (int i = 0; i < node.Options.Count; i++) {
                var option = node.Options[i];
                AddOption(option.Content, i);
            }
        }

        public void AddOption(string content, int id)
        {
            var optionButton = Instantiate(OptionButtonPrefab, OptionsPanel.content);
            optionButton.name = id.ToString();
            optionButton.GetComponentInChildren<TMP_Text>().text = content;
            var buttonCmp = optionButton.GetComponent<Button>();
            buttonCmp.onClick.AddListener(() => OnOptionClicked(optionButton));
        }

        void OnOptionClicked(GameObject button)
        {
            int id = int.Parse(button.name);
            EventManager.Broadcast(new SelectOptionEvent { Option = id });
        }

        void ClearUI()
        {
            Name.text = "";
            ContentText.text = "";
            for (int i = OptionsPanel.content.childCount - 1; i >= 0; i--) {
                Destroy(OptionsPanel.content.GetChild(i).gameObject);
            }
        }

        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }

}
