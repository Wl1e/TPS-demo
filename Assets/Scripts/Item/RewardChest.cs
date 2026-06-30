using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace TPSDemo
{
    // 设置z轴正方向为箱子朝向
    public class RewardChest : NetworkBehaviour, IInteractive
    {
        private static readonly int OpenHash = Animator.StringToHash("Open");

        public enum ChestState
        {
            None,
            Closed,
            //Opening,
            Opened
        };

        [Tooltip("宝箱配置")]
        [SerializeField] private ChestConfig m_Config;
        [Tooltip("道具生成时间")]
        [SerializeField] private float m_LootSpawnTime = 0.5f;
        [Tooltip("道具生成点")]
        [SerializeField] private Transform m_LootSpawnPos;
        [Tooltip("抛出力度")]
        [SerializeField] private float m_ThrowForce = 1f;
        public float WaitThrowTime = 1f;
        private Animator m_Animator;
        private readonly NetworkVariable<ChestState> m_StateNV = new(ChestState.Closed);

        private void Awake()
        {
            m_Animator = GetComponent<Animator>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            m_StateNV.OnValueChanged += OnChestStateChanged;
        }

        private void OnChestStateChanged(ChestState previousValue, ChestState newValue)
        {
        }

        // === IInteractive ===
        void IInteractive.Interact(GameObject playerObj)
        {
            if (!playerObj.TryGetComponent<PlayerController>(out var player)) {
                return;
            }
            player.InteractionController.OnInteracted();

            if (m_StateNV.Value != ChestState.Closed) {
                return;
            }
            TryOpenServerRpc();
        }

        // === Server: 验证 + 状态变更 ===
        [ServerRpc]
        void TryOpenServerRpc()
        {
            if (m_StateNV.Value != ChestState.Closed) {
                return;
            }
            m_StateNV.Value = ChestState.Opened;
            m_Animator.Play(OpenHash);
            StartCoroutine(SpawnLoot());
        }

        private IEnumerator SpawnLoot()
        {
            yield return new WaitForSeconds(m_LootSpawnTime);
            foreach (var entry in m_Config.LootTable) {
                int amount = Random.Range(entry.MinAmount, entry.MaxAmount + 1);
                if (amount <= 0) {
                    continue;
                }

                Vector3 dir = Random.insideUnitSphere * m_Config.ItemSpawnRadius;
                dir.y = Mathf.Abs(dir.y);
                dir.z = Mathf.Abs(dir.z);

                //Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Chest"), LayerMask.NameToLayer("Pickup"), true);
                StartCoroutine(WorldItemManager.Instance.SpawnItem(
                    entry.Item, m_LootSpawnPos.position, amount, obj => {
                        if(obj.TryGetComponent(out Rigidbody rb)) {
                            rb.AddForce(dir * m_ThrowForce, ForceMode.Impulse);
                        }
                    }
                ));
                //yield return new WaitForSeconds(WaitThrowTime);
                //Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Chest"), LayerMask.NameToLayer("Pickup"), false);
            }
        }
    }
}
