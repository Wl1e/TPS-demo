using System.Collections.Generic;
using UnityEngine;

namespace TPSDemo
{
    [CreateAssetMenu(fileName = "ItemDataList", menuName = "ItemData/ItemDataList")]
    public class ItemDataList: GameResource
    {
        [SerializeField] List<ItemData> m_DataList;
        private Dictionary<int, ItemData> m_Lookup = new();

        public ItemData GetItemData(int id)
        {
            return m_Lookup.GetValueOrDefault(id, null);
        }

        private void OnValidate()
        {
            foreach (var data in m_DataList) {
                if (m_Lookup.ContainsKey(data.Id)) {
                    Debug.LogError($"Item {m_Lookup[data.Id].Name} and item {data.Name} has same Id");
                    continue;
                }
                m_Lookup[data.Id] = data;
            }
        }
    }
}
