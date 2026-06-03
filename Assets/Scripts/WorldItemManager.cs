
using System.Collections.Generic;
using UnityEngine;

namespace TPSDemo
{
    public class WorldItemManager : MonoBehaviour
    {
        public Transform ItemRoot;

        HashSet<ItemPickup> m_WorldItems = new HashSet<ItemPickup>();

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
    }
}
