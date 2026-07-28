using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;

namespace TPSDemo
{
	public class AudioSystem
	{
        #region AudioInfo
        public struct AudioInfo
        {
            public AudioClip AudioClip;
            public AudioGroup Group;
            public Transform AttachTransform;
            public Vector3 Position;
            public float Duration;
        }

        public enum AudioGroup
        {
            Master = 0,
            BGM = 1,
            SFX = 2,
        };
        #endregion

        ObjectPool<AudioPlayer> m_AudioPlayerPool;
        GameObject m_AudioHolder;
        public int DefaultEmitterSize = 10;
        public int MaxEmitterSize = 100;

        private AudioMixer m_Mixer;

        GameObject CreateAudioGO()
        {
            GameObject audioSource = new("AGO", typeof(AudioSource));
            var player = audioSource.AddComponent<AudioPlayer>();
            player.OnRelease += (AudioPlayer player) => m_AudioPlayerPool?.Release(player);
            player.transform.SetParent(m_AudioHolder.transform);
            return audioSource;
        }

        public void Initialize(AudioMixer mixer)
        {
            m_Mixer = mixer;
            m_AudioHolder = GameObject.Find("AudioHolder");
            if(!m_AudioHolder) {
                m_AudioHolder = new GameObject("AudioHolder");
                GameFlowManager.Instance.SetDDOL(m_AudioHolder);
            }
            m_AudioPlayerPool = new ObjectPool<AudioPlayer>(
                createFunc: () => {
                    var audioSource = CreateAudioGO();
                    return audioSource.GetComponent<AudioPlayer>();
                },
                actionOnDestroy: player => {
                    Object.Destroy(player.gameObject);
                },
                actionOnGet: player => {
                    player.gameObject.SetActive(true);
                },
                actionOnRelease: player => {
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
            Play(
                info.AudioClip, Director.Instance.GetGroup(info.Group),
                info.Position, info.Duration, info.AttachTransform
            );
        }

        void Play(AudioClip audioClip, AudioMixerGroup group, Vector3 Position, float duration, Transform attach = null)
        {
            var audioPlayer = m_AudioPlayerPool.Get();
            if (attach != null) {
                audioPlayer.transform.SetParent(attach);
                audioPlayer.transform.localPosition = Position;
            } else {
                audioPlayer.transform.position = Position;
            }
            //audioPlayer.
            audioPlayer.Play(audioClip, group, duration);
        }

        // Play只针对触发型音效，持续型音效会反复调用造成性能浪费和预料之外的效果
        public AudioPlayer Borrow() => m_AudioPlayerPool.Get();
    }
}
