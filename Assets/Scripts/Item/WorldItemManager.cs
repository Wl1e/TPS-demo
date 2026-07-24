
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{
    // 改类的所有方法必须由Server调用
    public class WorldItemManager : Singleton<WorldItemManager>
    {
        private const ulong m_InvalidOwnerId = ulong.MaxValue;
        public Transform ItemRoot;

        // 没找到使用场景暂时就这样
        private readonly HashSet<ItemPickup> m_WorldItems = new();

        private static bool IsServer => NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;

        protected override void Awake()
        {
            base.Awake();
            ItemRoot = new GameObject("ItemRoot").transform;
        }

        public IEnumerator SpawnItem(ItemData data, Vector3 pos, int amount, System.Action<GameObject> cb = null, bool destroyWithScene = true)
        {
            if (!IsServer) {
                Debug.LogError($"WorldItemManager.SpawnItem called from non-Server!\n{System.Environment.StackTrace}");
                yield break;
            }
            var pickupObj = Instantiate(data.PickupPrefab, pos, Quaternion.identity, ItemRoot);
            var pickup = pickupObj.GetComponent<ItemPickup>();
            pickup.Amount = amount;

            var no = pickupObj.GetComponent<NetworkObject>();

            no.DestroyWithScene = destroyWithScene;
            if (!no.IsSpawned) {
                no.Spawn();
                yield return new WaitUntil(() => no.IsSpawned);
            }

            //no.TrySetParent(ItemRoot);
            //pickupObj.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, 0, 0));
            m_WorldItems.Add(pickup);

            cb?.Invoke(pickupObj);
        }

        // pickup全是NO，不需要AA管理，直接Instantiate
        //public IEnumerator SpawnItem(ItemData data, Vector3 pos, System.Action<GameObject> cb = null, bool destroyWithScene = true)
        //{
        //    GameObject pickupObj = null;
        //    yield return AssetCache.GetOrLoad(
        //        data.PickupPrefab,
        //        obj => pickupObj = obj
        //    );

        //    if (!pickupObj.TryGetComponent<NetworkObject>(out var no)) {
        //        yield break;
        //    }

        //    no.SetSceneObjectStatus(true);
        //    no.DestroyWithScene = destroyWithScene;
        //    if (!no.IsSpawned) {
        //        no.Spawn();
        //        yield return new WaitUntil(() => no.IsSpawned);
        //    }

        //    no.TrySetParent(ItemRoot);
        //    pickupObj.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, 0, 0));
        //    var pickup = pickupObj.GetComponent<ItemPickup>();
        //    m_WorldItems.Add(pickup);

        //    cb?.Invoke(pickupObj);
        //}

        public void EraseItem(ItemPickup item)
        {
            m_WorldItems.Remove(item);
        }

        // 有很多ItemData内的Prefab也是网络同步的，所以
        public IEnumerator CreateItemGO(
            ItemData data,
            Vector3 position,
            Quaternion rotation,
            Transform parent,
            System.Action<GameObject> completed,
            ulong ownerId = m_InvalidOwnerId
        )
        {
            if (!IsServer) {
                Debug.LogError($"WorldItemManager.CreateItemGO called from non-Server!\n{System.Environment.StackTrace}");
                yield break;
            }
            if (data.IsNetCodePrefab) {
                var go = Instantiate(data.NOPrefab, position, rotation, parent);
                if (!go.TryGetComponent<NetworkObject>(out var no)) {
                    Debug.LogError($"Item {data.Name} is NetCodeItem, but NOPrefab dont have NetworkObject");
                    yield break;
                }

                if (!no.IsSpawned) {
                    if (ownerId == m_InvalidOwnerId) {
                        no.Spawn();
                    } else {
                        no.SpawnWithOwnership(ownerId);
                    }
                    while(!no.IsSpawned) {
                        yield return null;
                    }
                }
                completed?.Invoke(go);

            } else {
                yield return AssetCache.GetOrLoad(data.Prefab, position, rotation, parent, completed);
            }
        }

        public IEnumerator CreateItemGO(ItemData data, System.Action<GameObject> completed = null, ulong ownerId = m_InvalidOwnerId)
            => CreateItemGO(data, Vector3.zero, Quaternion.identity, null, completed, ownerId);



        public IEnumerator CreateItemGO<T>(ItemData data, System.Action<T> completed = null, ulong ownerId = m_InvalidOwnerId) where T : class
            => CreateItemGO<T>(data, Vector3.zero, Quaternion.identity, null, completed, ownerId);

        public IEnumerator CreateItemGO<T>(
            ItemData data,
            Vector3 pos,
            Quaternion rotation,
            Transform parent = null,
            System.Action<T> completed = null,
            ulong ownerId = m_InvalidOwnerId
        ) where T : class
        {
            yield return CreateItemGO(data,
                pos,
                rotation,
                parent,
                obj => {
                    if (obj.TryGetComponent<T>(out var component)) {
                        completed?.Invoke(component);
                    } },
                ownerId
            );
        }

    }
}
