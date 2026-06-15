using System;
using UnityEngine;

namespace TPSDemo
{

    public class ItemPickup : ItemBase, IPickupable
    {
        public void Interact(GameObject obj)
        {
            if (obj.TryGetComponent<PlayerController>(out var player)) {
                if (Type == ItemType.Weapon) {
                    player.Loadout.EquipWeapon(Data.Prefab);
                } else {
                    player.Inventory.AddItem(this);
                }
                player.InteractionController.OnPickupItem(this);
                Destroy(gameObject);
            }
        }
        public void WhenSee()
        {

        }

        private void OnDestroy() => WorldItemManager.Instance.EraseItem(this);
    }
}
