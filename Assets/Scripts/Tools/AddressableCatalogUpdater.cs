using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace TPSDemo
{
    public class AddressableCatalogUpdater: MonoBehaviour
    {
        public float Progress => m_Progress;
        public bool IsUpdating => m_IsUpdating;
        public string StatusText => m_StatusText;

        float m_Progress;
        bool m_IsUpdating;
        string m_StatusText;

        public string CachePath => Application.temporaryCachePath;

        //private void Start()
        //{
        //    StartCoroutine(CheckAndUpdate());
        //}

        public IEnumerator CheckAndUpdate(System.Action<bool, string> onCompleted = null)
        {
            m_IsUpdating = true;
            m_Progress = 0f;

            var checkOp = Addressables.CheckForCatalogUpdates(false);
            yield return checkOp;

            if (checkOp.Status != AsyncOperationStatus.Succeeded)
            {
                m_StatusText = "检查更新失败";
                m_IsUpdating = false;
                onCompleted?.Invoke(false, checkOp.OperationException?.Message);
                yield break;
            }

            var catalogs = checkOp.Result;
            if (catalogs == null || catalogs.Count == 0)
            {
                m_StatusText = "已是最新";
                m_Progress = 1f;
                m_IsUpdating = false;
                onCompleted?.Invoke(true, null);
                yield break;
            }

            m_StatusText = $"发现 {catalogs.Count} 个更新";

            var updateOp = Addressables.UpdateCatalogs(catalogs, false);
            while (!updateOp.IsDone)
            {
                m_Progress = updateOp.PercentComplete;
                yield return null;
            }

            if (updateOp.Status != AsyncOperationStatus.Succeeded)
            {
                m_StatusText = "更新失败";
                m_IsUpdating = false;
                onCompleted?.Invoke(false, updateOp.OperationException?.Message);
                yield break;
            }

            AssetCache.ClearCache();
            m_StatusText = "更新完成";
            m_Progress = 1f;
            m_IsUpdating = false;

            Debug.Log("Addressables Catalog 更新完成，已清空 AssetCache");
            onCompleted?.Invoke(true, null);
        }
    }
}
