using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace TPSDemo
{
    public class AddressableHotUpdateChecker : MonoBehaviour
    {
        [Tooltip("自动检查间隔（秒），0 表示仅手动触发")]
        public float CheckInterval = 10f;

        [Tooltip("检查到更新后是否自动清空 AssetCache")]
        public bool AutoClearCache = true;

        void Start()
        {
            if (CheckInterval > 0f) {
                StartCoroutine(AutoCheckLoop());
            }
        }

        IEnumerator AutoCheckLoop()
        {
            while (CheckInterval != 0f) {
                Debug.Log("检查远程更新");
                yield return new WaitForSeconds(CheckInterval);
                yield return CheckAsync();
            }
        }

        // 手动触发
        public void CheckNow() => StartCoroutine(CheckAsync());

        IEnumerator CheckAsync()
        {
            var checkHandle = Addressables.CheckForCatalogUpdates(false);
            yield return checkHandle;

            if(checkHandle.Status != AsyncOperationStatus.Succeeded) {
                yield break;
            }

            var catalogs = checkHandle.Result;
            if (catalogs == null || catalogs.Count == 0) {
                yield break;
            }

            Debug.Log($"发现{catalogs.Count}个Catalog需要更新");

            AssetCache.ClearCache();
            var handle = Addressables.ClearDependencyCacheAsync(catalogs, false);
            yield return handle;
            handle.Release();

            var updateHandle = Addressables.UpdateCatalogs(catalogs, false);
            while(!updateHandle.IsDone) {
                Debug.Log($"热更新中：{updateHandle.PercentComplete}");
                yield return null;
            }

            if (updateHandle.Status != AsyncOperationStatus.Succeeded) {
                Debug.Log("UpdateCatalogs fault");
                yield break;
            }

            System.Collections.Generic.List<string> keys = new();
            foreach (var catalogLocator in updateHandle.Result) {
                foreach(var key in catalogLocator.Keys) {
                    Debug.Log("热更新 key: " + key.ToString());
                    keys.Add(key.ToString());
                }
            }

            updateHandle.Release();
            checkHandle.Release();

            EventManager.Broadcast(new Event.AssetUpdateEvent{ Keys = keys });

            Debug.Log($"Addressables 热更新完成，已清理 {keys.Count} 个旧缓存");
        }
    }
}
