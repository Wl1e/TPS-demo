using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Perform skill", story: "[Self] perform skill [SkillIdx] to [Target]", category: "Action", id: "08c058e375e0775fe40b67ddfdccbd29")]
public partial class PerformSkillAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<int> SkillIdx;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    private TPSDemo.SkillController m_SkillController;

    protected override Status OnStart()
    {
        if (Self == null || Target == null || SkillIdx == null) {
            Debug.Log(1);
            return Status.Failure;
        }
        var enemy = Self.Value.GetComponent<TPSDemo.EnemyController>();
        if (!enemy) {
            return Status.Failure;
        }
        if (!enemy.TryGetComponent(out m_SkillController)) {
            return Status.Failure;
        }
        if (!m_SkillController.IsFinished) {
            return Status.Failure;
        }
        if (!m_SkillController.ValidPerformSkill(Target.Value.transform, SkillIdx.Value)) {
            return Status.Failure;
        }
        m_SkillController.PerformSkill(Target.Value.transform, SkillIdx.Value);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (m_SkillController.IsFinished) {
            return Status.Success;
        }
        return Status.Running;
    }

    protected override void OnEnd()
    {
    }
}

