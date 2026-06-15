using System.Collections.Generic;
using UnityEngine;

namespace TPSDemo
{
    [CreateAssetMenu(fileName = "ItemDataList", menuName = "ItemData/ItemDataList")]
    public class ItemDataList: GameResource
    {
        [SerializeField] List<ItemData> m_DataList;

        public List<ItemData> GetItemDatas(ItemType type)
        {
            return m_DataList.FindAll(data => data.Type == type);
        }

        public ItemData GetItemData(int id)
        {
            int idx = m_DataList.FindIndex(data => data.Id == id);
            return idx != -1 ? m_DataList[idx] : null;
        }
    }
}
