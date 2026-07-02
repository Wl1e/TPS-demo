using System;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{

    public class ItemPickup : ItemBase, IPickupable
    {
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            //print("ItemPickup Spawn");
        }
        public void Interact(GameObject obj)
        {
            Debug.Log($"IsSpawned: {NetworkObject.IsSpawned}, IsClient: {IsClient}, IsServer: {IsServer}");
            if (obj.TryGetComponent<PlayerController>(out var player)) {
                InteractServerRpc(player.Id);
                player.InteractionController.OnPickupItem(this);
            }
        }
        public void WhenSee()
        {

        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void InteractServerRpc(int playerId)
        {
            var player = ActorManager.Instance.GetActor(playerId).GetComponent<PlayerController>();
            if (Type == ItemType.Weapon) {
                if (!player.Loadout.CanAddWeapon()) {
                    return;
                }
                player.Loadout.EquipWeapon(Data);
            } else {
                player.Inventory.AddItemClientRpc(playerId, Id, Amount);
            }
            NetworkObject.Despawn();
        }

        //private void OnDestroy() => WorldItemManager.Instance.EraseItem(this);
    }
}
