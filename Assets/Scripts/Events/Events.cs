


using System.Collections.Generic;
using UnityEngine;

// 事件越来越多，后续考虑换成SO Event
namespace TPSDemo.Event
{
    public abstract class InternalEvent
    { }

    //public class ObjectiveGameEvent: InternalEvent
    //{
    //    public Objective objective;
    //}
    public class EntityDiedEvent: InternalEvent
    {
        public GameObject Entity;
        public GameObject Attacker;
    }
    #region Player

    public class PlayerStateChangeEvent : InternalEvent
    {
        public string PreState;
        public string CurState;
    }
    public class PlayerViewPerspectiveChangeEvent : InternalEvent
    {
        public Define.ViewPerspective ViewPerspective;
    }
    public class CrosshairChangedEvent : InternalEvent
    {
        public CrosshairData Data;
    }
    public class AimEvent : InternalEvent
    {
        public bool IsAiming;
    }

    public class HealthChangedEvent : InternalEvent
    {
        public float value;
    }

    public class PlayerCollisionEvent : InternalEvent
    {
        public Collision Collision;
    }

    public class PlayerFinishedInitialzeEvent: InternalEvent
    {
    }

    #endregion

    public class PickupItemEvent: InternalEvent
    {  public GameObject Item; }

    public class TaskCheckEvent: InternalEvent
    { }

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
        public UI.WeaponUIData weapon1;
        public UI.WeaponUIData weapon2;
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

}
