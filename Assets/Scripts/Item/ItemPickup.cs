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

        public string Hint => $"拾取{Name}";
        public float HoldDuration => 0f;
        public void OnInteractPress(GameObject interactor)
        {
            //Debug.Log($"IsSpawned: {NetworkObject.IsSpawned}, IsClient: {IsClient}, IsServer: {IsServer}");
            if (interactor.TryGetComponent<PlayerController>(out var player)) {
                player.InteractionController.OnPickupItem(this);
                InteractServerRpc(player.Id);
            }
        }
        public void OnInteractHold(GameObject interactor)
        { }
        public void OnInteractRelease(GameObject interactor, bool completed)
        {
            if (completed) {
                DespawnServerRpc();
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void DespawnServerRpc() => NetworkObject.Despawn();

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
        }

        //private void OnDestroy() => WorldItemManager.Instance.EraseItem(this);
    }
}
