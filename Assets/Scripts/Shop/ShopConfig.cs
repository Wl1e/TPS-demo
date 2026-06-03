using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopConfig", menuName = "Shop/ShopConfig")]
public class ShopConfig : ScriptableObject
{
    public int ShopId;
    public string ShopName;
    public bool Restock = false;
    public float RestockTime = 0f;
    public bool RandomGoods = false;
    public List<TPSDemo.ShopEntry> Goods = new List<TPSDemo.ShopEntry>();
}
