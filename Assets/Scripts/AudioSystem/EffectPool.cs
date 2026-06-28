using UnityEngine;
using UnityEngine.Pool;

namespace TPSDemo
{
	public class EffectPool
	{
        [SerializeField] private int DefaultSize = 8;
        [SerializeField] private int MaxSize = 128;
        private Transform EffectPoolRoot;
        private ObjectPool<EffectBuilder> m_Pool;

		public void Initialize()
		{
            EffectPoolRoot = new GameObject("EffectPool").transform;
            GameFlowManager.Instance.SetDDOL(EffectPoolRoot.gameObject);

            m_Pool = new ObjectPool<EffectBuilder>(
                createFunc: () => CreateEffectGO(),
                actionOnDestroy: effect => Object.Destroy(effect.gameObject),
                actionOnGet: effect => effect.gameObject.SetActive(true),
                actionOnRelease: effect => {
                    effect.transform.SetParent(EffectPoolRoot);
                    effect.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                    effect.gameObject.SetActive(false);
                    Debug.Log("effect.gameObject: " + effect.gameObject.activeSelf);
                },
                defaultCapacity: DefaultSize,
                maxSize: MaxSize
            );
        }

		private EffectBuilder CreateEffectGO()
        {
            GameObject effectBuilder = new("Effect", typeof(EffectBuilder));
            effectBuilder.transform.SetParent(EffectPoolRoot);
            var effect = effectBuilder.GetComponent<EffectBuilder>();
            effect.OnCompleted += effect => {
                m_Pool.Release(effect);
                Debug.Log("Release Effect");
            };
            return effect;
        }

        public EffectBuilder GetEffectBuilder() => m_Pool.Get();
	}
}
