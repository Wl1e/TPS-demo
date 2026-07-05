using System.Collections.Generic;
using UnityEngine;

namespace TPSDemo
{

    [System.Serializable]
    public struct GoodConfig
    {
        public ItemData Good;
        public int Amount;
        public int Price;
        public float Discount;
    }

    [CreateAssetMenu(fileName = "ShopConfig", menuName = "Config/Shop/ShopConfig")]
    public class ShopConfig : ScriptableObject
    {
        public int ShopId;
        public string ShopName;
        public int MoneyId;
        public bool Restock = false;
        public float RestockTime = 0f;
        public bool RandomGoods = false;
        public List<GoodConfig> Goods;
    }
}
