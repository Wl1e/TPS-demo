using System;
using System.Collections.Generic;
using UnityEngine;

namespace TPSDemo
{

    [CreateAssetMenu(menuName = "Config/ObjectiveConfig/ObjectivePickupItemConfig", fileName = "ObjectivePickupItemConfig")]
    public class ObjectivePickupItemConfig : ObjectiveConfig
    {
        // FIXME: 后续改为ID
        public GameObject Item;
        public int Count;

        public override string GetObjectiveText() => $"获取 {Count}个 {Item.name}";
    }
}
