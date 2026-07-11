using Unity.Netcode;
using UnityEngine;
using static UnityEngine.AdaptivePerformance.Provider.AdaptivePerformanceSubsystemDescriptor;

namespace TPSDemo
{
	public class DamageValueDisplayService: NetworkSingleton<DamageValueDisplayService>
	{
        //public struct DamageValueInfo
        //{
        //    public float Value;
        //    public Vector3 Position;
        //    public bool IsCritical;
        //}
        public void RequestDamageValue(float value, Vector3 position, bool isCritical)
        {
            DVClientRpc(value, position, isCritical);
        }

        [ClientRpc]
        private void DVClientRpc(float value, Vector3 position, bool isCritical)
        {
            Director.Instance.RequestDamageValue(
                position,
                value,
                isCritical
            );
        }
	}
}
