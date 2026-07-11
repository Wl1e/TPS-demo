using System.Collections;
using System.Collections.Generic;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.UIElements;

namespace TPSDemo
{
	public static class AssetCache
	{
        private class AssetEntry
        {
            public string Key = "";
            public GameObject Prefab = null;
            public AsyncOperationHandle<GameObject> Handler;
            public CountDownLatch RefCount = new();
            public AssetEntry(string key, GameObject prefab, AsyncOperationHandle<GameObject> handler)
            {
                Key = key;
                Prefab = prefab;
                Handler = handler;
            }
        }

        static private readonly Dictionary<string, AssetEntry> s_Cache = new();
        // 无法监听对应对象的销毁，除非让所有AA管理的类继承一个基类
        //static private readonly Dictionary<string, CountDownLatch> m_Locks;


        static public IEnumerator GetOrLoad(AssetReference r, System.Action<GameObject> onLoaded = null) => GetOrLoad(r, Vector3.zero, Quaternion.identity, null, onLoaded);

        static public IEnumerator GetOrLoad(
            AssetReference r,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null,
            System.Action<GameObject> onLoaded = null
        )
        {
            if (s_Cache.TryGetValue(r.AssetGUID, out var cached)) {
                var instance = Object.Instantiate(cached.Prefab, position, rotation, parent);
                onLoaded?.Invoke(instance);
                yield break;
            }

            var handler = r.LoadAssetAsync<GameObject>();
            yield return handler;

            if (handler.Status != AsyncOperationStatus.Succeeded) {
                Debug.LogError($"InstantiateAsync {r} fail, message {handler.OperationException.Message}");
                onLoaded?.Invoke(null);
                yield break;
            }
            s_Cache[r.AssetGUID] = new AssetEntry(r.AssetGUID, handler.Result, handler);
            onLoaded?.Invoke(Object.Instantiate(handler.Result, position, rotation, parent));
        }

        static public IEnumerator Load(AssetReference r)
        {
            var handler = r.LoadAssetAsync<GameObject>();
            yield return handler;

            if (handler.Status != AsyncOperationStatus.Succeeded) {
                Debug.LogError($"InstantiateAsync {r} fail, message {handler.OperationException.Message}");
                yield break;
            }
            s_Cache[r.AssetGUID] = new AssetEntry(r.AssetGUID, handler.Result, handler);
        }

        static public IEnumerator LoadByLabel(string label)
        {
            Debug.Log("加载Label: " + label);
            AsyncOperationHandle<IList<IResourceLocation>> location = Addressables.LoadResourceLocationsAsync(label);
            yield return location;

            if (location.Status != AsyncOperationStatus.Succeeded) {
                Debug.Log($"加载{label}组数据时出错，{location.OperationException.Message}");
                location.Release();
                yield break;
            }

            foreach(var loc in location.Result) {
                var handler = Addressables.LoadAssetAsync<GameObject>(loc);
                yield return handler;

                if(handler.Status != AsyncOperationStatus.Succeeded) {
                    Debug.Log($"加载{label}组{loc.PrimaryKey}数据时出错，{handler.OperationException.Message}");
                }
                Debug.Log("加载 " + handler.Result.name);

                s_Cache[loc.PrimaryKey] = new AssetEntry(loc.PrimaryKey, handler.Result, handler);
            }

            location.Release();
        }

        static public void ReleaseByLabel(string label)
        {
            foreach (var kv in s_Cache) {
                // kv.Key = AssetGUID
                // kv.Value = AsyncOperationHandle<GameObject>
                Addressables.Release(kv.Value);
            }
            s_Cache.Clear();
        }
    }
}
