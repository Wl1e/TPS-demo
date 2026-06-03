using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

namespace TPSDemo
{
	public class AudioSystem
	{
        #region AudioInfo
        public struct AudioInfo
        {
            public AudioClip AudioClip;
            public Transform AttachTransform;
            public Vector3 Position;
        }
        #endregion


        ObjectPool<AudioPlayer> m_AudioPlayerPool;
        GameObject m_AudioHolder;
        public int DefaultEmitterSize = 10;
        public int MaxEmitterSize = 100;

        GameObject CreateAudioGO()
        {
            GameObject audioSource = new GameObject("AGO", typeof(AudioSource));
            var player = audioSource.AddComponent<AudioPlayer>();
            player.OnRelease += m_AudioPlayerPool.Release;
            return audioSource;
        }

        public void Initialize()
        {
            m_AudioHolder = new GameObject("AudioHolder");
            Object.DontDestroyOnLoad(m_AudioHolder);
            m_AudioPlayerPool = new ObjectPool<AudioPlayer>(
                createFunc: () => {
                    var audioSource = CreateAudioGO();
                    return audioSource.GetComponent<AudioPlayer>();
                },
                actionOnDestroy: (AudioPlayer player) => {
                    Object.Destroy(player.gameObject);
                },
                actionOnGet: (AudioPlayer player) => {
                    player.gameObject.SetActive(true);
                },
                actionOnRelease: (AudioPlayer player) => {
                    player.transform.SetParent(m_AudioHolder.transform);
                    player.transform.localPosition = Vector3.zero;
                    player.gameObject.SetActive(false);
                },
                defaultCapacity: DefaultEmitterSize,
                maxSize: MaxEmitterSize
            );
        }

        public void Play(AudioInfo info)
        {
            //Debug.Log($"Play Audio: {info.AudioClip.name} at {info.Position}");
            if (!info.AudioClip) {
                return;
            }
            Play(info.AudioClip, info.Position, info.AttachTransform);
        }

        void Play(AudioClip audioClip, Vector3 Position, Transform attach = null)
        {
            var audioPlayer = m_AudioPlayerPool.Get();
            if (attach != null) {
                audioPlayer.transform.SetParent(attach);
                audioPlayer.transform.localPosition = Position;
            } else {
                audioPlayer.transform.position = Position;
            }
            audioPlayer.Play(audioClip);
        }
    }
}
