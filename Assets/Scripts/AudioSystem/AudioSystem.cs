using UnityEngine;
using System.Collections;
using UnityEngine.Pool;

namespace TPSDemo
{
	public class AudioSystem: MonoBehaviour
	{
        #region AudioInfo
        public struct AudioInfo
        {
            public AudioDef AudioDef;
            public Transform AttachTransform;
            public Vector3 Position;
        }
        #endregion

        ObjectPool<AudioEmitter> m_AudioEmitterPool;
        public int DefaultEmitterSize = 10;
        public int MaxEmitterSize = 100;

        private void Awake()
        {
            m_AudioEmitterPool = new ObjectPool<AudioEmitter>(
                createFunc: () => new AudioEmitter(),
                defaultCapacity: DefaultEmitterSize,
                maxSize: MaxEmitterSize
            );
        }

        public void Player(AudioInfo info)
        {
            if(!info.AudioDef) {
                return;
            }
            Play(info.AudioDef, info.Position, info.AttachTransform);
        }

        void Play(AudioDef audioDef, Vector3 Position, Transform attach = null)
        { }
    }
}
