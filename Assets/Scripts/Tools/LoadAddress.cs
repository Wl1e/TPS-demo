using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TPSDemo
{
    static public class LoadAddress
	{
        static public IEnumerator CreateGameObject(AssetReference reference)
        {
            var handler = reference.InstantiateAsync();
            yield return handler;

            if(handler.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded) {
                Debug.LogError($"Create GO {reference.SubObjectName} Fail, Message {handler.OperationException.Message}");
                yield break;
            }

        }
        static public IEnumerator CreateGameObject(
            AssetReference reference,
            Vector3 pos,
            Quaternion rotation,
            Transform parent = null
        )
        {
            var handler = reference.InstantiateAsync(pos, rotation, parent);
            yield return handler;

            if (handler.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded) {
                Debug.LogError($"Create GO {reference.SubObjectName} Fail, Message {handler.OperationException.Message}");
                yield break;
            }

            //obj = handler.Result;
        }
	}
}
