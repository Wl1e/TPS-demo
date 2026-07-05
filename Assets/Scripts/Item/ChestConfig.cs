using System;
using System.Collections;
using UnityEngine;

namespace TPSDemo
{
    [CreateAssetMenu(menuName = "Config/Chest/ChestConfig", fileName = "ChestConfig")]
    public class ChestConfig : ScriptableObject
    {
        public int ChestId;
        public string ChestName = "宝箱";

        [Header("Loot")]
        public ChestLootEntry[] LootTable;

        public float ItemSpawnRadius = 0.5f;  // 物品散开半径
    }

    [Serializable]
    public struct ChestLootEntry
    {
        public ItemData Item;
        public int MinAmount;
        public int MaxAmount;
    }
}
