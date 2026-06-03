using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UIElements;

namespace TPSDemo
{

    public class NpcBase : MonoBehaviour, IInteractive
    {
        // Base
        string m_NpcName;
        public string Name => m_NpcName;
        Animator m_Animator;

        public Transform ModelTransform;

        PlayerController m_Player;

        public Transform SeePos;
        public float SeeAngleRange = 70f;
        public float TurnThreshold = 70f;
        public float TurnBackThreshold = 150f;
        float RigWeight = 0f;

        // Animator
        public bool IsFacePlayer;
        public float ViewRadius = 2f;
        [SerializeField] Rig m_Rig;
        [SerializeField] MultiAimConstraint m_MultiAimConstraint;
        public float WaitTime;
        Coroutine m_WaitAnimatorCoroutine = null;

        // Dialog
        [SerializeField] DialogueData m_DialogueData;
        public DialogueData DialogueData => m_DialogueData;
        bool m_Chatting = false;
        public void StopChat() => m_Chatting = false;
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

        void Start()
        {
            m_Animator = GetComponentInChildren<Animator>();
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
            m_Player = player.GetComponent<PlayerController>();
            bool needWait = FaceTarget(player.transform.position);
            if (needWait) {
                if (m_WaitAnimatorCoroutine != null) {
                    StopCoroutine(m_WaitAnimatorCoroutine);
                }
                m_WaitAnimatorCoroutine = StartCoroutine(Turn());
            } else {
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
            print("angle: " + angle);
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

        void Dialog()
        {
            if (m_Chatting || !m_CanChat) {
                return;
            }
            m_Chatting = true;
            DialogueSystem.Instance.Enter(m_Player, this);
        }

        IEnumerator Turn()
        {
            yield return new WaitForSeconds(WaitTime);
            print("Play: " + m_Player);
            if (m_Player) {
                Interact(m_Player.gameObject);
            }
        }
    }
}
