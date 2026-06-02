using UnityEngine;
using System.Collections;

namespace TPSDemo
{
    public class EffectBuilder
    {
        GameObject m_EffectPrefab;
        string m_EffectName = "";
        Transform m_Parent = null;
        Vector3 m_Position = Vector3.zero;
        Material m_Material = null;
        float m_Scale = 1f;
        Quaternion m_Rotation = Quaternion.identity;
        Color m_Color;

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
            m_Color = color;
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
            m_Material = material;
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

        public void Create()
        {
            var effect = Object.Instantiate(m_EffectPrefab);
            if (m_EffectName != "") {
                effect.name = m_EffectName;
            }
        }
    }
}
