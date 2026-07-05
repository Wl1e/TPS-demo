using UnityEngine;
using UnityEngine.Pool;

namespace TPSDemo
{
	public class EffectPool
	{
        [SerializeField] private int DefaultSize = 8;
        [SerializeField] private int MaxSize = 128;
        private Transform m_EffectPoolRoot;
        private ObjectPool<EffectBuilder> m_Pool;

		public void Initialize()
		{
            m_EffectPoolRoot = new GameObject("EffectPool").transform;
            GameFlowManager.Instance.SetDDOL(m_EffectPoolRoot.gameObject);

            m_Pool = new ObjectPool<EffectBuilder>(
                createFunc: () => CreateEffectGO(),
                actionOnDestroy: effect => Object.Destroy(effect.gameObject),
                actionOnGet: effect => effect.gameObject.SetActive(true),
                actionOnRelease: effect => {
                    effect.transform.SetParent(m_EffectPoolRoot);
                    effect.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                    effect.gameObject.SetActive(false);
                },
                defaultCapacity: DefaultSize,
                maxSize: MaxSize
            );
        }

		private EffectBuilder CreateEffectGO()
        {
            GameObject effectBuilder = new("Effect", typeof(EffectBuilder));
            effectBuilder.transform.SetParent(m_EffectPoolRoot);
            var effect = effectBuilder.GetComponent<EffectBuilder>();
            effect.OnCompleted += effect => {
                m_Pool.Release(effect);
            };
            return effect;
        }

        public EffectBuilder GetEffectBuilder() => m_Pool.Get();
	}
}
