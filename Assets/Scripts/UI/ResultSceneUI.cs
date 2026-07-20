using UnityEngine;
using UnityEngine.UI;

namespace TPSDemo.UI
{
    public class ResultSceneUI : MonoBehaviour
    {
        public Button returnBtn;

        private void Awake()
        {
            returnBtn.onClick.AddListener(OnReturnClicked);
        }

        private void OnReturnClicked()
        {
            var mgr = GameNetworkManager.Instance;
            if (mgr != null && mgr.IsListening) {
                mgr.Disconnect();
            }
            UnityEngine.SceneManagement.SceneManager.LoadScene("Boot");
        }
    }
}
