using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TPSDemo.UI
{

    public class InteractionHintUI : MonoBehaviour
    {
        public TextMeshProUGUI Hint;
        public Image Progress;

        private void Start()
        {
            EventManager.AddListener<Event.UpdateInteractionHintEvent>(OnHintUpdate);
            gameObject.SetActive(false);
        }

        private void OnHintUpdate(Event.UpdateInteractionHintEvent evt)
        {
            if (string.IsNullOrEmpty(evt.Hint)) {
                if (gameObject.activeSelf) {
                    gameObject.SetActive(false);
                }
            } else {
                if (!gameObject.activeSelf) {
                    gameObject.SetActive(true);
                }
            }
            Hint.text = evt.Hint;
            Progress.fillAmount = evt.Progress;
        }
    }
}
