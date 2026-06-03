using System;
using UnityEngine;

namespace TPSDemo
{

    public class ItemPickup : ItemBase, IPickupable
    {
        public float InteractRadius => 3f;
        public void Interact(GameObject obj)
        {
            if (obj.TryGetComponent<PlayerController>(out var player)) {
                if (Type == ItemType.Weapon) {
                    player.Loadout.EquipWeapon(Data.Prefab);
                } else {
                    player.Inventory.AddItem(this);
                }
                Destroy(gameObject);
            }
        }
        public void WhenSee()
        {

        }

        private void OnDestroy() => FindAnyObjectByType<WorldItemManager>()?.EraseItem(this);
    }
}
