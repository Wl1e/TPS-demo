using UnityEngine;

namespace TPSDemo
{
    [CreateAssetMenu(menuName = "Resource/ResourceList", fileName = "ResourceList")]
	public class GameResourceList: ScriptableObject
	{
        public System.Collections.Generic.List<GameResource> Resources;
    }
}
