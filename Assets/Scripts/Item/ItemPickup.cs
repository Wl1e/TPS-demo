using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{

    public class ItemPickup: ItemBase, IPickupable
    {
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            //Debug.Log("ItemPickup Spawn");
        }

        public string Hint => $"拾取{Name}";
        public float HoldDuration => 0f;
        public virtual void OnInteractPress(GameObject interactor)
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
                print("CanAddWeapon " + player.Loadout.CanAddWeapon());
                if (!player.Loadout.CanAddWeapon()) {
                    return;
                }
                player.Loadout.EquipWeapon(this);
            } else {
                player.Inventory.AddItemClientRpc(Id, Amount);
                // 更新Objective
                EventManager.Broadcast(new Event.PickupItemEvent {
                    ActorId = playerId,
                    ItemId = Id,
                    Amount = Amount,
                });
            }
        }

        //private void OnDestroy() => WorldItemManager.Instance.EraseItem(this);
    }
}
