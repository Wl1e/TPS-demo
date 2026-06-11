using System.Collections.Generic;
using UnityEngine;


namespace TPSDemo
{
    using PickupItemEvent = Event.PickupItemEvent;

    public class ObjectivePickupItem : Objective
    {
        public GameObject Target;
        public int Total;
        public int Cur;
        
        public override void Initialize(ObjectiveConfig config)
        {
            if (config is not ObjectivePickupItemConfig trueConfig) {
                return;
            }
            Target = trueConfig.Item;
            Total = trueConfig.Count;
            Cur = 0;
            EventManager.AddListener<PickupItemEvent>(OnPickupItem);
        }

        public override void Destroy()
        {
            EventManager.RemoveListener<PickupItemEvent>(OnPickupItem);
        }

        public void OnPickupItem(PickupItemEvent evt)
        {
            GameObject item = evt.Item;
            if (item != Target) {
                return;
            }
            Cur++;
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
