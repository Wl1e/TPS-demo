using UnityEngine;

namespace TPSDemo
{
    public class GameFlowManager : MonoBehaviour
    {
        PlayerManager m_PlayerManager;
        [SerializeField] UI.UIController m_UI;
        void Start()
        {
            //Cursor.lockState = CursorLockMode.Locked;
            m_PlayerManager = GetComponentInChildren<PlayerManager>();
        }

        private void OnDestroy()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}
