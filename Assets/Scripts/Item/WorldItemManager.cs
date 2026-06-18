
using System.Collections.Generic;
using UnityEngine;

namespace TPSDemo
{
    public class WorldItemManager : Singleton<WorldItemManager>
    {
        public Transform ItemRoot;

        private readonly HashSet<ItemPickup> m_WorldItems = new();

        public void SpawnItem(ItemData data, Vector3 pos)
        {
            var item = Instantiate(data.Prefab, pos, Quaternion.Euler(0, 0, 0), ItemRoot);
            var pickup = item.GetComponent<ItemPickup>();
            m_WorldItems.Add(pickup);
        }

        public void EraseItem(ItemPickup item)
        {
            m_WorldItems.Remove(item);
        }

        static public GameObject CreateItemGO(ItemData data) => Instantiate(data.Prefab);
        static public T CreateItemGO<T>(ItemData data) => CreateItemGO(data).GetComponent<T>();
    }
}
