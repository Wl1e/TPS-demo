using UnityEngine;
using UI;

public class GameFlowManager : MonoBehaviour
{
    public PlayerController Player;
    PlayerManager m_PlayerManager;
    [SerializeField] UIController m_UI;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        m_PlayerManager = GetComponentInChildren<PlayerManager>();
        PlayerDataProxy.Instance.RegisterPlayer(Player);
        Player.Initialize();
        m_UI.Initialize();
    }

    private void OnDestroy()
    {
        PlayerDataProxy.Instance.UnregisterPlayer();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
