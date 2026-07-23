using UnityEngine;
using Unity.Netcode;

namespace TPSDemo
{
    [RequireComponent(typeof(PlayableController))]
	public class NetworkPlayable: NetworkBehaviour
	{
        private PlayableController m_PlayableController;
		// Use this for initialization
		private void Awake()
		{
            m_PlayableController = GetComponent<PlayableController>();
            if (IsOwner) {
                m_PlayableController.OnAnimationPlay += OnPlayablePlay;
            }
        }

        [ClientRpc]
        private void PlayClientRpc(int preIdx, int curIdx, float duration)
        {
            if(IsOwner) {
                return;
            }
            m_PlayableController.Play(preIdx, curIdx, duration);
        }

        private void OnPlayablePlay(int preIdx, int curIdx, float duration)
        {
            PlayClientRpc(preIdx, curIdx, duration);
        }
    }
}
