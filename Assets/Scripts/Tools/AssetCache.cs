using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace TPSDemo
{
	public static class AssetCache
	{
        static private readonly Dictionary<string, GameObject> s_Cache = new();
        static private readonly Dictionary<string, AsyncOperationHandle<GameObject>> s_Handles = new();

        static public System.Collections.IEnumerator GetOrLoad(AssetReference r, System.Action<GameObject> onLoaded)
        {
            if (s_Cache.TryGetValue(r.AssetGUID, out var cached)) {
                var instance = Object.Instantiate(cached);
                onLoaded(instance);           // 命中 → 同步回调
                yield break;
            }

            var handle = r.LoadAssetAsync<GameObject>();
            s_Handles[r.AssetGUID] = handle;
            yield return handle;

            if (handle.Status != AsyncOperationStatus.Succeeded) {
                Debug.LogError($"InstantiateAsync {r} fail, message {handle.OperationException.Message}");
                onLoaded(null);
                yield break;
            }
            s_Cache[r.AssetGUID] = handle.Result;
            onLoaded(Object.Instantiate(handle.Result));
        }

        static public void ReleaseByLabel(string label)
        {
            foreach (var kv in s_Handles) {
                // kv.Key = AssetGUID
                // kv.Value = AsyncOperationHandle<GameObject>
                Addressables.Release(kv.Value);
            }
            s_Cache.Clear();
            s_Handles.Clear();
        }
    }
}
