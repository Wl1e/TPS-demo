using Unity.Netcode;

namespace TPSDemo
{
    /// <summary>
    /// 同步WeaponManger状态
    /// </summary>
    [System.Serializable]
    public struct WeaponState: INetworkSerializable
    {
        /// <summary>
        /// 当前武器下标
        /// </summary>
        public int WeaponIdx;
        /// <summary>
        /// 是否在瞄准
        /// </summary>
        public bool IsAiming;
        /// <summary>
        /// 瞄准权重
        /// </summary>
        public float AimWeight;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref WeaponIdx);
            serializer.SerializeValue(ref IsAiming);
            serializer.SerializeValue(ref AimWeight);
        }
    }
}
