using UnityEngine;

namespace TPSDemo
{
    public class FactoryBase<KeyType, ValueType, ResourceType>
        where ResourceType : GameResource
        where ValueType: class
    {
        public ResourceType Resource;
        public FactoryBase(string resourceName = null)
        {
            if(resourceName != null) {
                Resource = ResourceManager.Instance.GetResource<ResourceType>(resourceName);
            }
        }

        public delegate ValueType CreateDelegate();
        private readonly System.Collections.Generic.Dictionary<KeyType, CreateDelegate> m_Targets = new();
        public virtual void Register<T>(KeyType i) where T: ValueType, new() => m_Targets[i] = () => new T();
        public virtual void Unregister(KeyType i) => m_Targets.Remove(i);

        public virtual ValueType Create(KeyType i)
        {
            if (m_Targets.TryGetValue(i, out var dele)) {
                return dele();
            }
            Debug.Log($"{nameof(ValueType)} {i} not register");
            return null;
        }
    }

    static public class SkillFactory
    {
        static readonly FactoryBase<int, SkillBase, SkillDatabase> m_Factory = new();
        static public void Register<T>(int id) where T : SkillBase, new() => m_Factory.Register<T>(id);
        static public SkillBase Create(int id) => m_Factory.Create(id);
    }
}
