using System.Collections.Generic;
using UnityEngine;

namespace TPSDemo
{

    [CreateAssetMenu(fileName = "ShopList", menuName = "Config/Shop/ShopList")]
    public class ShopList: GameResource
    {
        public List<ShopConfig> Configs;
        public ShopConfig GetConfig(int shopId)
        {
            int idx = Configs.FindIndex(config => config.ShopId == shopId);
            return idx != -1 ? Configs[idx] : null;
        }
    }

}
