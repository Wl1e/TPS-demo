using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace TPSDemo
{
    /// <summary>
    /// 强要求必须有 AlarmController 组件
    /// </summary>
	public class AlarmSkill: SkillBase
	{
        private const int SkillId = (int)SkillName.AlarmSkill;
        public override int Id => SkillId;

        //private AlarmController m_AlarmController;

        // 警报生效范围
        public float AlarmRange => (Config as AlarmSkillConfig).AlarmRange;
        // 警报持续时间
        public float AlarmTime => (Config as AlarmSkillConfig).AlarmTime;

        // 正在警报
        private bool m_IsAlarm = false;

        [RuntimeInitializeOnLoadMethod]
        private static void RegisterSelf() => SkillFactory.Register<AlarmSkill>(SkillId);

        public override void Initialize(EnemyController enemy, SkillConfig config)
        {
            base.Initialize(enemy, config);
            m_EnemyController.AEPlayer.LoopAudio("Alarm", (Config as AlarmSkillConfig).AlarmClip);
        }

        public override bool ValidPerform(Transform target)
        {
            if (target == null) {
                return false;
            }
            var sqrDistance = Vector3.SqrMagnitude(target.position - m_EnemyController.transform.position);
            return !InCD() && InRange(sqrDistance, Config.AttackRange);
        }

        protected override void Perform(float deltaTime)
        {
            if (m_IsAlarm) {
                if(m_Duration >= AlarmTime) {
                    ChangeState(SkillState.Recovery);
                }
                return;
            }
            m_IsAlarm = true;

            Alarm();
        }

        private void Alarm()
        {
            var alarmPos = m_EnemyController.transform.position;
            m_EnemyController.AEPlayer.Play(
                "Alarm",
                AudioSystem.AudioGroup.SFX,
                AlarmTime,
                alarmPos, Quaternion.identity
            );

            var colliders = Physics.OverlapSphere(
                alarmPos, AlarmRange,
                LayerMask.NameToLayer("Enemy"),
                QueryTriggerInteraction.Ignore
            );

            foreach (var collider in colliders) {
                var enemy = collider.GetComponentInParent<EnemyController>();
                enemy.SetTarget(m_Target.gameObject);
            }
        }
    }
}
