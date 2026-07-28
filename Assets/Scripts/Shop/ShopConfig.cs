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
        public int RestockCnt;
        public float RestockTime;
    }

    [CreateAssetMenu(fileName = "ShopConfig", menuName = "Config/Shop/ShopConfig")]
    public class ShopConfig : ScriptableObject
    {
        public int ShopId;
        public string ShopName;
        public int MoneyId;
        public bool RandomGoods = false;
        public GameObject ShopUI;

        public List<GoodConfig> Goods;
    }
}
