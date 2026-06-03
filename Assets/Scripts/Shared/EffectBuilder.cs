using UnityEngine;
using System.Collections;

namespace TPSDemo
{
    public class EffectBuilder
    {
        GameObject m_EffectPrefab;
        string m_EffectName = "Effect";
        Transform m_Parent = null;
        Vector3 m_Position = Vector3.zero;
        float m_Scale = 1f;
        Quaternion m_Rotation = Quaternion.identity;
        float m_Duration = 1f;
        //Material m_Material = null;
        //Color m_Color;

        public EffectBuilder(GameObject effectPrefab)
        {
            m_EffectPrefab = effectPrefab;
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
            var effect = Object.Instantiate(m_EffectPrefab);
            effect.name = m_EffectName;
            effect.transform.localScale = Vector3.one * m_Scale;
            effect.transform.rotation = m_Rotation;

            if(m_Parent != null) {
                effect.transform.SetParent(m_Parent);
                effect.transform.localPosition = m_Position;
            } else {
                effect.transform.position = m_Position;
            }

            if (m_Duration > 0) {
                Object.Destroy(effect, m_Duration);
            }

            return effect;
        }
    }
}
