
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{
    // 改类的所有方法必须由Server调用
    public class WorldItemManager : Singleton<WorldItemManager>
    {
        public Transform ItemRoot;

        // 没找到使用场景暂时就这样
        private readonly HashSet<ItemPickup> m_WorldItems = new();

        protected override void Awake()
        {
            base.Awake();
            ItemRoot = new GameObject("ItemRoot").transform;
        }

        public IEnumerator SpawnItem(ItemData data, Vector3 pos, System.Action<GameObject> cb = null, bool destroyWithScene = true)
        {
            GameObject pickupObj = null;
            yield return AssetCache.GetOrLoad(
                data.PickupPrefab,
                obj => pickupObj = obj
            );

            if(!pickupObj.TryGetComponent<NetworkObject>(out var no)) {
                yield break;
            }

            no.SetSceneObjectStatus(true);
            no.DestroyWithScene = destroyWithScene;
            if (!no.IsSpawned) {
                no.Spawn();
                yield return new WaitUntil(() => no.IsSpawned);
            }

            no.TrySetParent(ItemRoot);
            pickupObj.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, 0, 0));
            var pickup = pickupObj.GetComponent<ItemPickup>();
            m_WorldItems.Add(pickup);

            cb?.Invoke(pickupObj);
        }

        public void EraseItem(ItemPickup item)
        {
            m_WorldItems.Remove(item);
        }

        static public IEnumerator CreateItemGO(ItemData data, System.Action<GameObject> Completed)
        {
            yield return AssetCache.GetOrLoad(data.Prefab, Completed);
        }

        static public IEnumerator CreateItemGO(
            ItemData data,
            System.Action<GameObject> Completed,
            Vector3 pos,
            Quaternion rotation,
            Transform parent = null)
        {
            yield return CreateItemGO(data, obj => {
                obj.transform.SetParent(parent);
                obj.transform.SetPositionAndRotation(pos, rotation);
                Completed(obj);
            });
        }

        static public IEnumerator CreateItemGO<T>(ItemData data, System.Action<T> Completed) where T: class
        {
            yield return CreateItemGO(data, obj => {
                Completed(obj.GetComponent<T>());
            });
        }

        static public IEnumerator CreateItemGO<T>(
            ItemData data,
            System.Action<T> Completed,
            Vector3 pos,
            Quaternion rotation,
            Transform parent = null
        ) where T : class
        {
            yield return CreateItemGO(data, obj => {
                obj.transform.SetParent(parent);
                obj.transform.SetPositionAndRotation(pos, rotation);
                Completed(obj.GetComponent<T>());
            });
        }
    }
}
