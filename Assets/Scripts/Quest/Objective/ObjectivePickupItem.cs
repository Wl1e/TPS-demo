using System.Collections.Generic;
using UnityEngine;


namespace TPSDemo
{
    using PickupItemEvent = Event.PickupItemEvent;
    public class ObjectivePickupItem : Objective
    {
        public int TargetId;
        public int Total;
        public int Cur;
        
        public override void Initialize(ObjectiveConfig config)
        {
            if (config is not ObjectivePickupItemConfig trueConfig) {
                return;
            }
            if(!trueConfig.Item.TryGetComponent<ItemPickup>(out var item)) {
                Debug.LogError($"Objective {trueConfig.Id} Config Item is not ItemPickup");
                return;
            }
            TargetId = item.Id;
            Total = trueConfig.Count;
            Cur = 0;
            
            //player.InteractionController.OnPickup += OnPickupItem;
            EventManager.AddListener<PickupItemEvent>(OnPickupItem);
        }

        public override void Destroy()
        {
            EventManager.RemoveListener<PickupItemEvent>(OnPickupItem);
        }

        public void OnPickupItem(PickupItemEvent evt)
        {
            Debug.Log($"Pickup Item{evt.ItemId}, m_Target is {TargetId}");
            if (TargetId != evt.ItemId || m_ActorId != evt.ActorId) {
                return;
            }
            Cur += evt.Amount;
            OnUpdate?.Invoke(this);
            Check();
        }

        public override void Check()
        {
            if (Cur >= Total) {
                Complete();
            }
        }

        public override void GetProcess(out ObjectiveProgress process)
        {
            process.ObjectiveId = Id;
            process.Cur = Cur;
            process.Max = Total;
        }
    }
}
