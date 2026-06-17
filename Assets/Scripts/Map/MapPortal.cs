using UnityEngine;
using System.Collections;

namespace TPSDemo
{
	public class MapPortal: MonoBehaviour
	{
        [SerializeField] private Collider Portal;
        [SerializeField] private int TargetMapId = -1;

        private void OnTriggerEnter(Collider other)
        {
            if(TargetMapId != -1 && other.gameObject.CompareTag("Player")) {
                MapManager.Instance.EnterMap(TargetMapId);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            
        }
    }
}
