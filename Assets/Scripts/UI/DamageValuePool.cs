using UnityEngine;
using UnityEngine.Pool;

namespace TPSDemo.UI
{
    public class DamageValuePool
    {
        ObjectPool<DamageValueUI> m_ObjectPool;
        [SerializeField] private int DefaultSize = 8;
        [SerializeField] private int MaxSize = 128;
        public Transform m_DVRoot;
        public DamageValueUI m_DVPrefab;

        public void Initialize(DamageValueUI prefab)
        {
            m_DVPrefab = prefab;
            m_ObjectPool = new ObjectPool<DamageValueUI>(
                createFunc: CreateDVGO,
                actionOnDestroy: Object.Destroy,
                actionOnGet: dv => dv.gameObject.SetActive(true),
                actionOnRelease: dv => dv.gameObject.SetActive(false),
                defaultCapacity: DefaultSize,
                maxSize: MaxSize
            );
            var e = GameObject.Find("DamageValueRoot");
            if(e) {
                m_DVRoot = e.transform;
            } else {
                m_DVRoot = new GameObject("DamageValueRoot").transform;
                GameFlowManager.Instance.SetDDOL(m_DVRoot.gameObject);
            }
        }

        private DamageValueUI CreateDVGO()
        {
            DamageValueUI dv = Object.Instantiate(m_DVPrefab, Vector3.zero, Quaternion.identity, m_DVRoot);
            dv.OnRelease += dv => m_ObjectPool.Release(dv);
            return dv;
        }

        public void ShowDV(Vector3 pos, float damage, bool isCritical)
        {
            DamageValueUI dv = m_ObjectPool.Get();
            dv.transform.position = pos;
            dv.Show(damage.ToString(), isCritical);
        }
    }
}
