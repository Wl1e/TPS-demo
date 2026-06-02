using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopList", menuName = "Shop/ShopList")]
public class ShopList: ScriptableObject
{
    public List<ShopConfig> Configs;
}

