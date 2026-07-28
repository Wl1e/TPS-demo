using System;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{
    [Obsolete("废弃")]
    public class WeaponPickup : ItemPickup
    {
        [Header("武器相关")]
        [Tooltip("武器所有子弹数")]
        public int m_CurrentAmmo = -1;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            m_CurrentAmmo = (Data as WeaponItemData).DefaultClipSize;
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        protected void InteractServerRpc(int playerId)
        {
            var player = ActorManager.Instance.GetActor(playerId).GetComponent<PlayerController>();
            //player.Loadout.EquipWeapon(this, m_CurrentAmmo);
        }
    }
}
