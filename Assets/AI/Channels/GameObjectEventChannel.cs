using Unity.Behavior;
using UnityEngine;

[CreateAssetMenu(fileName = "GameObjectEventChannel", menuName = "Behavior/Event Channel/GameObject")]
[EventChannelDescription(message: "[Target]")]
public class GameObjectEventChannel : EventChannel<GameObject>
{
}
