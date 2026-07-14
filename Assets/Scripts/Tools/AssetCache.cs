using NUnit.Framework;
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
        private abstract class AssetEntryBase
        {
            public abstract void Release();
        }
        private class AssetEntry<T>: AssetEntryBase where T : Object
        {
            public override void Release() => Handler.Release();

            public string Key = "";
            public T Asset = null;
            
            public AsyncOperationHandle<T> Handler;
            public AssetEntry(string key, T prefab, AsyncOperationHandle<T> handler)
            {
                Key = key;
                Asset = prefab;
                Handler = handler;
            }
        }

        private class AssetListEntry<T> : AssetEntryBase
        {
            public override void Release() => Handler.Release();

            public string Key = "";
            public IList<T> Asset = null;

            public AsyncOperationHandle<IList<T>> Handler;
            public AssetListEntry(string key, IList<T> prefab, AsyncOperationHandle<IList<T>> handler)
            {
                Key = key;
                Asset = prefab;
                Handler = handler;
            }
        }

        static private readonly Dictionary<string, AssetEntryBase> s_Cache = new();
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
            if (s_Cache.TryGetValue(r.AssetGUID, out var cached) && cached is AssetEntry<GameObject> goEntry) {
                var instance = Object.Instantiate(goEntry.Asset, position, rotation, parent);
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
            s_Cache[r.AssetGUID] = new AssetEntry<GameObject>(r.AssetGUID, handler.Result, handler);
            onLoaded?.Invoke(Object.Instantiate(handler.Result, position, rotation, parent));
        }

        static public IEnumerator GetOrLoad<T>(string path, System.Action<T> onLoaded = null) where T : Object
        {
            if (s_Cache.TryGetValue(path, out var cached) && cached is AssetEntry<T> tEntry) {
                onLoaded?.Invoke(tEntry.Asset);
                yield break;
            }

            var handler = Addressables.LoadAssetAsync<T>(path);
            yield return handler;

            if (handler.Status != AsyncOperationStatus.Succeeded) {
                Debug.LogError($"InstantiateAsync {path} fail, message {handler.OperationException.Message}");
                onLoaded?.Invoke(null);
                yield break;
            }
            s_Cache[path] = new AssetEntry<T>(path, handler.Result, handler);
            onLoaded?.Invoke(handler.Result);
        }

        static public IEnumerator GetOrLoadByLabel<T>(string path, System.Action<IList<T>> onLoaded = null) where T : Object
        {
            if (s_Cache.TryGetValue(path, out var cached) && cached is AssetListEntry<T> tEntry) {
                onLoaded?.Invoke(tEntry.Asset);
                yield break;
            }

            var handler = Addressables.LoadAssetsAsync<T>(path);
            yield return handler;

            if (handler.Status != AsyncOperationStatus.Succeeded) {
                Debug.LogError($"InstantiateAsync {path} fail, message {handler.OperationException.Message}");
                onLoaded?.Invoke(null);
                yield break;
            }
            s_Cache[path] = new AssetListEntry<T>(path, handler.Result, handler);
            onLoaded?.Invoke(handler.Result);
        }

        static public IEnumerator Load(AssetReference r)
        {
            var handler = r.LoadAssetAsync<GameObject>();
            yield return handler;

            if (handler.Status != AsyncOperationStatus.Succeeded) {
                Debug.LogError($"InstantiateAsync {r} fail, message {handler.OperationException.Message}");
                yield break;
            }
            s_Cache[r.AssetGUID] = new AssetEntry<GameObject>(r.AssetGUID, handler.Result, handler);
        }

        static public IEnumerator DownloadDependencies(string key, bool showProgress = false, System.Action onCompleted = null)
        {
            var locOp = Addressables.LoadResourceLocationsAsync(key);
            yield return locOp;

            var locations = locOp.Result;
            Debug.Log($"共 {locations.Count} 个资产:");
            foreach (var loc in locations) {
                Debug.Log($"  - {loc.PrimaryKey}");  // asset 的 address/name
            }

            var handler = Addressables.DownloadDependenciesAsync(key);
            if (showProgress) {
                while (!handler.IsDone) {
                    Debug.Log($"加载{key}中，进度{handler.PercentComplete}");
                    EventManager.Broadcast(new Event.MessageLogEvent { Message = $"加载{key}中，进度{handler.PercentComplete * 100}%" });
                    yield return null;
                }
            } else {
                yield return handler;
            }
            Debug.Log($"加载{key}完成");
            EventManager.Broadcast(new Event.MessageLogEvent { Message = $"加载{key}完成" });

            onCompleted?.Invoke();
            handler.Release();
        }

        //static public IEnumerator LoadByLabel(string label)
        //{
        //    Debug.Log("加载Label: " + label);
        //    AsyncOperationHandle<IList<IResourceLocation>> location = Addressables.LoadResourceLocationsAsync(label);
        //    yield return location;

        //    if (location.Status != AsyncOperationStatus.Succeeded) {
        //        Debug.Log($"加载{label}组数据时出错，{location.OperationException.Message}");
        //        location.Release();
        //        yield break;
        //    }

        //    foreach(var loc in location.Result) {

        //        var handler = Addressables.LoadAssetAsync(loc);
        //        yield return handler;

        //        if(handler.Status != AsyncOperationStatus.Succeeded) {
        //            Debug.Log($"加载{label}组{loc.PrimaryKey}数据时出错，{handler.OperationException.Message}");
        //        }
        //        Debug.Log("加载 " + handler.Result.name);

        //        s_Cache[loc.PrimaryKey] = new AssetEntry<T>(loc.PrimaryKey, handler.Result, handler);
        //    }

        //    location.Release();
        //}

        static public void ReleaseByLabel(string label)
        {
            foreach (var kv in s_Cache) {
                if(kv.Key.StartsWith(label)) {
                    kv.Value.Release();
                }
            }
        }

        public static void Remove(string key)
        {
            if (s_Cache.TryGetValue(key, out var entry)) {
                entry.Release();
                s_Cache.Remove(key);
                Debug.Log($"AssetCache 已释放更新资源: {key}");
            }
        }

        public static int RemoveAll(List<string> keys)
        {
            int count = 0;
            foreach (var key in keys) {
                if (s_Cache.Remove(key, out var entry)) {
                    entry.Release();
                    count++;
                }
            }
            return count;
        }

        public static void ClearCache()
        {
            foreach (var kv in s_Cache) {
                kv.Value.Release();
            }
            s_Cache.Clear();
        }
    }
}
