using NUnit.Framework.Internal;
using UnityEngine;

namespace TPSDemo
{
    public class EffectBuilder: MonoBehaviour
    {
        GameObject m_EffectPrefab = null;
        string m_EffectName = "Effect";
        Transform m_Parent = null;
        Vector3 m_Position = Vector3.zero;
        float m_Scale = 1f;
        Quaternion m_Rotation = Quaternion.identity;
        float m_Duration = float.NegativeInfinity;
        //Material m_Material = null;
        //Color m_Color;
        private bool m_IsRunning = false;

        private ParticleSystem m_ParticleSystem = null;

        private GameObject m_Effect = null;

        public event System.Action<EffectBuilder> OnCompleted;

        private void Update()
        {
            if (m_IsRunning && m_Effect) {
                if (m_Duration > 0f) {
                    m_Duration -= Time.deltaTime;
                } else {
                    m_IsRunning = false;
                    Destroy(m_Effect);
                    m_Effect = null;
                    OnCompleted?.Invoke(this);
                }
            }
        }

        private void Initialze()
        {
            m_EffectName = "Effect";
            m_Parent = null;
            m_Position = Vector3.zero;
            m_Scale = 1f;
            m_Duration = float.NegativeInfinity;
            m_Rotation = Quaternion.identity;
            m_EffectPrefab = null;
            m_ParticleSystem = null;
        }

        public EffectBuilder SetEffect(GameObject effect)
        {
            Initialze();
            m_EffectPrefab = effect;
            return this;
        }

        public EffectBuilder WithParent(Transform parent)
        {
            m_Parent = parent;
            return this;
        }

        public EffectBuilder WithColor(Color color)
        {
            //m_Color = color;
            return this;
        }

        public EffectBuilder WithPosition(Vector3 position)
        {
            m_Position = position;
            return this;
        }

        public EffectBuilder WithName(string name)
        {
            m_EffectName = name;
            return this;
        }

        public EffectBuilder WithMaterial(Material material)
        {
            //m_Material = material;
            return this;
        }

        public EffectBuilder WithScale(float scale)
        {
            m_Scale = scale;
            return this;
        }
        public EffectBuilder WithRotation(Quaternion rotation)
        {
            m_Rotation = rotation;
            return this;
        }
        public EffectBuilder LookAt(Vector3 dir)
        {
            m_Rotation = Quaternion.LookRotation(dir);
            return this;
        }

        public EffectBuilder WithDuration(float duration)
        {
            m_Duration = duration;
            return this;
        }

        public GameObject Create()
        {
            if(!m_EffectPrefab) {
                OnCompleted?.Invoke(this);
                return null;
            }
            m_Effect = Instantiate(m_EffectPrefab, transform);
            m_Effect.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            m_ParticleSystem = m_Effect.GetComponent<ParticleSystem>();

            gameObject.name = m_EffectName;
            gameObject.transform.localScale = Vector3.one * m_Scale;
            gameObject.transform.rotation = m_Rotation;

            if(m_Parent != null) {
                gameObject.transform.SetParent(m_Parent);
                gameObject.transform.localPosition = m_Position;
            } else {
                gameObject.transform.position = m_Position;
            }

            if(m_Duration == float.NegativeInfinity) {
                m_Duration = m_ParticleSystem.main.duration;
            }

            m_IsRunning = true;

            return gameObject;
        }
    }
}
