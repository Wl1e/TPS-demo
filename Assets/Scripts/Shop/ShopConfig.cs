using System.Collections.Generic;
using UnityEngine;

namespace TPSDemo
{

    [CreateAssetMenu(fileName = "ShopConfig", menuName = "Shop/ShopConfig")]
    public class ShopConfig : ScriptableObject
    {
        public int ShopId;
        public string ShopName;
        public int MoneyId;
        public bool Restock = false;
        public float RestockTime = 0f;
        public bool RandomGoods = false;
        public List<ShopEntry> Goods = new List<ShopEntry>();
    }
}
