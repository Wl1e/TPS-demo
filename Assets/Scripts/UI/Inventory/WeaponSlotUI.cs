using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class WeaponSlotUI : MonoBehaviour, IDropTarget
    {
        public GameObject ItemPrefab;

        public WeaponItemUI Weapon;
        public LoadoutUI Loadout;
        public int SlotIdx;

        public DragType Type => DragType.Weapon;
        public void OnDrop(IDragable drag)
        {
            var itemType = ItemUIUtils.GetItemType(drag.ItemId);
            if (itemType == ItemType.Weapon) {
                Loadout.SwapWeapon(SlotIdx, drag.SlotIdx);
            } else if(itemType == ItemType.Ammo) {
                EventManager.Broadcast(new Event.TryReloadEvent { WeaponIdx = SlotIdx });
            }
        }
        public void SetWeapon(WeaponUIData data)
        {
            int weaponId = data.WeaopnId;
            if (weaponId == 0) {
                DestroyItem();
                return;
            }
            if (Weapon == null) {
                var itemObj = Instantiate(ItemPrefab, transform);
                Weapon = itemObj.GetComponent<WeaponItemUI>();
                Weapon.Slot = this;
            }
            Weapon.UpdateInfo(weaponId, data.AttachmentIdList);
        }

        public void DestroyItem()
        {
            if (Weapon) {
                Destroy(Weapon.gameObject);
            }
        }

        public void OnDragEnter(IDragable obj)
        {
        }
        public void OnDragExit(IDragable obj)
        {
        }

    }
}
