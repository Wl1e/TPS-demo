using System;
using UnityEngine;

namespace TPSDemo
{
    public enum MapType
    {
        Boot,
        Hub,       // 起始城镇/大厅
        Combat,    // 战斗区域
    }

    public enum MapState
    {
        Idle,
        Active,
        Completed
    }

    /// <summary>
    /// 地图配置 ScriptableObject — 定义地图元数据、场景引用和约束条件
    /// </summary>
    [CreateAssetMenu(menuName = "Config/Map/MapConfig", fileName = "MapConfig")]
    public class MapConfig : ScriptableObject
    {
        [Header("基本信息")]
        [Tooltip("地图 ID")]
        public int MapId;
        [Tooltip("地图名称")]
        public string MapName;
        [Tooltip("地图描述")]
        [TextArea(2, 4)]
        public string Description;
        [Tooltip("地图类型")]
        public MapType Type;

        [Header("场景")]
        [Tooltip("该地图对应的 Unity 场景名（Assets/Scenes/ 下）")]
        public string SceneName;

        [Header("目标")]
        [Tooltip("完成地图需要的目标配置")]
        public ObjectiveConfig[] ObjConfigs;
    }
}
