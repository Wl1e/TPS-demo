using System.Collections.Generic;
using UnityEngine;

using DataEntry = Tools.Entry<PlayerMovementState, BakedAnimData>;

[CreateAssetMenu(fileName = "BakeDataList", menuName = "Bake/BakeDataList")]
public class BakeDataManager : ScriptableObject
{
    [SerializeField] List<DataEntry> m_Data;
    [SerializeField] List<DataEntry> m_AimData;
    public BakedAnimData GetAnimData(PlayerMovementState state, bool isAiming)
    {
        return (isAiming ? m_AimData : m_Data).Find(data => data.Key == state).Value;
    }
}
