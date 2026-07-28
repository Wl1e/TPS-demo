
using System;
using UnityEngine;

namespace TPSDemo
{
    public class PlayerRuntimeData
    {
        /// <summary>
        /// 瞄准状态
        /// </summary>
        public bool IsAiming = false;
        /// <summary>
        /// 状态机（移动状态）
        /// </summary>
        public PlayerMovementState State = PlayerMovementState.Idle;
        /// <summary>
        /// 相机根节点
        /// </summary>
        public Transform CameraRoot = null;

        /// <summary>
        /// 当前CombatController槽位
        /// </summary>
        public Combat.Slot ActiveSlot = Combat.Slot.None;
        /// <summary>
        /// 禁止切换战斗模块（强制Unarmed）
        /// </summary>
        public bool DisableCombat = false;

        /// <summary>
        /// 动画参数
        /// </summary>
        public AnimatorParameter AniParameter = new();

        /// <summary>
        /// 瞄准方式（暂未使用）
        /// </summary>
        public Define.ViewPerspective AimType;

        /// <summary>
        /// 正在主动道具
        /// </summary>
        public bool UsingActiveItem = false;

        /// <summary>
        /// 能否使用主动道具
        /// </summary>
        public bool CanUseActiveItem = true;

        /// <summary>
        /// 玩家死亡状态
        /// </summary>
        public bool IsDied = false;
    }
}
