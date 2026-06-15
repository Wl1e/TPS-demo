using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TPSDemo
{
    /// <summary> 资源管理器：统一管理和查询 GameResource 资产 </summary>
    public class ResourceManager : Singleton<ResourceManager>
    {
        [Tooltip("资源列表")]
        public GameResourceList ResourceList;
        private readonly Dictionary<string, GameResource> m_Lookups = new();

        /// <summary> 根据 ID 获取第一个匹配的资源，没有则返回 null </summary>
        public T GetResource<T>(string id) where T : GameResource
        {
            if (m_Lookups.ContainsKey(id)) {
                if (m_Lookups[id] == null) {
                    m_Lookups.Remove(id);
                } else {
                    return m_Lookups[id] as T;
                }
            }
            foreach (var res in ResourceList.Resources) {
                if (res != null && res is T && res.Id == id) {
                    m_Lookups[id] = res;
                    return res as T;
                }
            }
            return null;
        }
    }
}
