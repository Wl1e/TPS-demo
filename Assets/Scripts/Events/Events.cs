


using System.Collections.Generic;
using UnityEngine;

// 事件越来越多，后续考虑换成SO Event
// 把GO统一换成ID
namespace TPSDemo.Event
{
    public abstract class InternalEvent
    { }

    #region Actor
    /// <summary>
    /// 死亡事件(玩家、敌人)
    /// </summary>
    public class ActorDiedEvent: InternalEvent
    {
        public int ActorId;
        public int AttackerId;
    }
    public class HealthChangedEvent : InternalEvent
    {
        public float value;
    }
    #endregion

    #region Player

    public class PlayerStateChangeEvent : InternalEvent
    {
        public string PreState;
        public string CurState;
    }
    public class AimEvent : InternalEvent
    {
        public bool IsAiming;
    }

    public class PlayerFinishedInitialzeEvent: InternalEvent
    {
    }
    public class PickupItemEvent: InternalEvent
    {
        public int ActorId;
        public int ItemId;
        public int Amount;
    }

    public class PlayerEconomyChangedEvent: InternalEvent
    {
        public int MoneyId;
        public int Amount;
    }

    #endregion

    #region Loadout

    public class WeaponChangedEvent: InternalEvent
    {
        public int OldIdx;
        public int NewIdx;
    }
    public class WeaponStartReloadEvent: InternalEvent
    {
        public int WeaponIdx;
    }
    public class WeaponEndReloadEvent: InternalEvent
    {
        public int WeaponIdx;
    }
    public class UpdateLoadoutUIEvent: InternalEvent
    {
        public UI.WeaponUIData Weapon1;
        public UI.WeaponUIData Weapon2;
    }

    // ui -> logic
    public class SwapWeaponEvent : InternalEvent
    {
        public int Idx1;
        public int Idx2;
    }

    public class TryReloadEvent : InternalEvent
    {
        public int WeaponIdx;
    }

    #endregion

    #region Combat

    public class WeaponFiredEvent : InternalEvent
    { }

    public class BulletHitTargetEvent: InternalEvent
    {
        // 后续改用ActorID，通过ActorManager获取实体
        public GameObject Attacker;
        public GameObject Victim;
    }

    #endregion

    #region Inventory

    public class InventoryStateChangeEvent : InternalEvent
    {
        public bool IsOpened;
    }

    public class InventoryUpdateEvent: InternalEvent
    {
    }
    public class InventoryTrySwapItem: InternalEvent
    {
        public int SlotIdx1;
        public int SlotIdx2;
    }
    #endregion

    #region DialogueSystem
    // logic -> ui
    public class StartDialogEvent : InternalEvent
    {
        public int PlayerId;
        public NpcBase Npc;
    }
    public class UpdateDialogEvent : InternalEvent
    {
        public DialogueNode DialogueNode;
    }
    public class EndDialogEvent : InternalEvent
    {
        public DialogState State;
    }
    // ui -> logic
    public class SelectOptionEvent: InternalEvent
    {
        public int Option;
    }
    // 对话打开商店、发送任务等自定义事件
    public class DialogCustomEvent: InternalEvent
    {
        public OptionAction Action;
        public string Data;
        public int PlayerId;
    }

    #endregion

    #region ShopSystem
    // ui -> logic
    public class OpenShopEvent: InternalEvent
    {
        public int playerId;
        public int ShopId;
    }
    public class CloseShopEvent : InternalEvent
    {
    }
    public class TryBuyEvent : InternalEvent
    {
        public int Slot;
    }

    // logic -> ui
    public class ShopOpenEvent : InternalEvent
    {
        public int ShopId;
        public string ShopName;
        public List<ShopEntry> Goods;
    }

    public class ShopCloseEvent : InternalEvent
    {
        public int ShopId;
    }

    public class ShopBuyEvent : InternalEvent
    {
        public int ShopId;
        public int Slot;
        public int Price;
        public bool IsSuccess;
        public string FailInfo;
    }
    #endregion

    #region Quest
    // logic -> ui
    public class QuestStateChangeEvent: InternalEvent
    {
        public bool IsOpened;
    }

    public class QuestUpdateEvent : InternalEvent
    {
    }

    // ui -> logic
    public class TryCancelQuestEvent: InternalEvent
    {
        public int QuestId;
    }
    public class TryQuestRewardEvent: InternalEvent
    {
        public int QuestId;
    }
    #endregion

    #region MessageLog
    /// <summary>
    /// 屏幕上方消息提示（购买成功/任务更新等）
    /// </summary>
    public class MessageLogEvent: InternalEvent
    {
        public string Message;
        public float Duration = -1f;
    }
    #endregion
}
