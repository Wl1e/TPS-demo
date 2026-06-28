using UnityEngine;

namespace TPSDemo
{
    public interface IActiveItem
    {
        public float UseTime { get; }
        public void Use(GameObject target);
    }

    public class MedicalKit : MonoBehaviour, IActiveItem
    {
        [Tooltip("可恢复生命值")]
        [SerializeField] private float m_ReceiveHealth = 0f;
        [Tooltip("使用时间")]
        [SerializeField] private float m_UseTime = 1f;

        public float UseTime => m_UseTime;

        public void Use(GameObject target)
        {
            if (!target.TryGetComponent<PlayerController>(out var player)) {
                return;
            }

            var health = player.Health;
            health.Heal(m_ReceiveHealth);
        }
    }
}
