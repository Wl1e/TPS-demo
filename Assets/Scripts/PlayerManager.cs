
using UnityEngine;

public class PlayerManager: MonoBehaviour
{
    public PlayerController m_PlayerPrefab;
    public PlayerController SpawnPlayer()
    {
        return Instantiate(m_PlayerPrefab);
    }
}
