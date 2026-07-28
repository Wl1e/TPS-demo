using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace TPSDemo.UI
{
    public class ResultSceneUI : MonoBehaviour
    {
        public Button returnBtn;

        private void Start()
        {
            returnBtn.onClick.AddListener(OnReturnClicked);
            Cursor.lockState = CursorLockMode.None;
            if(UIController.Instance) {
                UIController.Instance.CloseCurPanel();
            }
        }

        private void OnReturnClicked()
        {
            GameModeManager.ExitGame();
        }
    }
}
