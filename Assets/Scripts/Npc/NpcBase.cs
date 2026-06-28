using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace TPSDemo
{

    public class NpcBase: NetworkBehaviour, IInteractive
    {
        // Base
        /// <summary>
        /// npcm名字
        /// </summary>
        string m_NpcName;
        public string Name => m_NpcName;
        Animator m_Animator;

        public Transform ModelTransform;

        /// <summary>
        /// 当前交互的玩家(Local)
        /// </summary>
        private PlayerController m_Player;

        public Transform SeePos;
        [Tooltip("最大转头角度")]
        public float SeeAngleRange = 70f;
        [Tooltip("转身门槛")]
        public float TurnThreshold = 70f;
        [Tooltip("背身门槛")]
        public float TurnBackThreshold = 150f;
        float RigWeight = 0f;

        // Animator
        [Tooltip("是否面向玩家")]
        public bool IsFacePlayer;
        public float ViewRadius = 2f;
        [SerializeField] Rig m_Rig;
        [SerializeField] MultiAimConstraint m_MultiAimConstraint;
        [Tooltip("如果要转身，则等待的时间")]
        public float WaitTime;
        Coroutine m_WaitAnimatorCoroutine = null;

        // Dialog
        [Tooltip("对话数据")]
        [SerializeField] DialogueData m_DialogueData;
        public DialogueData DialogueData => m_DialogueData;
        /// <summary>
        /// 当前是否正在和player交流
        /// </summary>
        private readonly NetworkVariable<bool> m_Chatting = new(false);
        /// <summary>
        /// 当前交互玩家(Server)
        /// </summary>
        private readonly NetworkVariable<int> m_ChattingPlayer = new(-1);
        public bool Chatting => m_Chatting.Value;
        [Tooltip("是否可交谈")]
        [SerializeField] bool m_CanChat;

        // Interactive
        [SerializeField] float m_InteractRadius = 1f;
        public float InteractRadius => m_InteractRadius;

        // 与animator保持一致
        enum TurnDir : int
        {
            LeftTurn = 1,
            RightTurn = 2,
            BackTurn = 3,
        }

        private void Awake()
        {
            m_Animator = GetComponentInChildren<Animator>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if(IsClient) {
                m_ChattingPlayer.OnValueChanged += OnPlayerChatting;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsClient) {
                m_ChattingPlayer.OnValueChanged -= OnPlayerChatting;
            }
            base.OnNetworkDespawn();
        }

        void Update()
        {
            if (m_Player) {
                var angle = GetAngle(m_Player.Actor.AimPoint.position);
                if (angle >= -SeeAngleRange && angle <= SeeAngleRange) {
                    SeePos.position = m_Player.Actor.AimPoint.position;
                    SeeTarget();
                } else {
                    ResetTarget();
                }
                if (!InInteractRange(m_Player.transform.position)) {
                    m_Player = null;
                    m_Animator.Play("Idle");
                }
            } else {
                ResetTarget();
            }
        }
        public void StopChat() => StopChatServerRpc();
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void StopChatServerRpc()
        {
            m_Chatting.Value = false;
            m_ChattingPlayer.Value = -1;
        }

        void SeeTarget()
        {
            if (RigWeight != 1f) {
                RigWeight = Mathf.Clamp01(RigWeight + 0.05f);
                m_Rig.weight = RigWeight;
            }
        }

        void ResetTarget()
        {
            if (RigWeight != 0f) {
                RigWeight = Mathf.Clamp01(RigWeight - 0.05f);
                m_Rig.weight = RigWeight;
            }
        }

        bool InInteractRange(Vector3 position)
        {
            return (position - transform.position).sqrMagnitude <= m_InteractRadius * m_InteractRadius;
        }

        public void Interact(GameObject player)
        {
            if (!InInteractRange(player.transform.position)) {
                return;
            }
            if (Chatting || !m_CanChat) {
                return;
            }
            m_Player = player.GetComponent<PlayerController>();
            bool needWait = FaceTarget(player.transform.position);
            if (needWait) {
                if (m_WaitAnimatorCoroutine != null) {
                    StopCoroutine(m_WaitAnimatorCoroutine);
                }
                m_WaitAnimatorCoroutine = StartCoroutine(Turn());
            } else {
                print($"Client {OwnerClientId} Start Dialog");
                Dialog();
            }
        }

        float GetAngle(Vector3 position)
        {
            Vector3 dir = ModelTransform.InverseTransformDirection((position - transform.position).normalized);
            float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            return Mathf.DeltaAngle(0f, angle);
        }

        bool FaceTarget(Vector3 position)
        {
            if (!IsFacePlayer) {
                return false;
            }
            var angle = GetAngle(position);
            TurnDir turnDir;
            if (angle > -TurnThreshold && angle < TurnThreshold) {
                return false;
            } else if (angle > TurnThreshold && angle < TurnBackThreshold) {
                turnDir = TurnDir.RightTurn;
            } else if (angle < -TurnThreshold && angle > -TurnBackThreshold) {
                turnDir = TurnDir.LeftTurn;
            } else {
                turnDir = TurnDir.BackTurn;
            }
            m_Animator.SetFloat("TurnDir", (int)turnDir);
            m_Animator.SetTrigger("Turn");
            RigWeight = 0f;
            return true;
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void ChatNpcServerRpc(int playerId)
        {
            m_Chatting.Value = true;
            m_ChattingPlayer.Value = playerId;
        }

        void Dialog()
        {
            ChatNpcServerRpc(m_Player.Id);
        }

        IEnumerator Turn()
        {
            yield return new WaitForSeconds(WaitTime);
            if (m_Player) {
                Interact(m_Player.gameObject);
            }
        }

        private void OnPlayerChatting(int previousValue, int newValue)
        {
            if(IsClient && m_Player != null && m_Player.Id == newValue) {
                print($"Client {newValue} Start Dialog");
                DialogueSystem.Instance.Enter(m_Player, this);
            }
        }
    }
}
