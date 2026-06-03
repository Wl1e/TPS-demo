using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDataList", menuName = "ItemData/ItemDataList")]
public class ItemDataList: ScriptableObject
{
    [SerializeField] List<TPSDemo.ItemData> m_DataList;

    static ItemDataList m_Instance;
    public static List<TPSDemo.ItemData> GetItemDatas(TPSDemo.ItemType type)
    {
        if(m_Instance == null) {
            m_Instance = Resources.Load<ItemDataList>("ItemDataList");
        }
        return m_Instance.m_DataList.FindAll(data => data.Type == type);
    }

    public static TPSDemo.ItemData GetItemData(int id)
    {
        if(m_Instance == null) {
            m_Instance = Resources.Load<ItemDataList>("ItemDataList");
        }
        int idx = m_Instance.m_DataList.FindIndex(data => data.Id == id);
        return idx != -1 ? m_Instance.m_DataList[idx] : null;
    }
}

