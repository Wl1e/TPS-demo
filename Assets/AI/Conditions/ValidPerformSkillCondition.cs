using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Valid perform skill", story: "[Self] valid perform skill to [Target] skillIdx: [SkillIdx]", category: "Conditions", id: "4581336c07f42a3987b7cbf45ff21540")]
public partial class ValidPerformSkillCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<int> SkillIdx;

    public override bool IsTrue()
    {
        if (Self == null || Target == null || SkillIdx == null) {
            return false;
        }

        if(!TPSDemo.AI.AITool.AgentSeeTarget(Self.Value, Target.Value, -1)) {
            return false;
        }

        SkillIdx.Value = -1;

        if (Self.Value.TryGetComponent<TPSDemo.SkillController>(out var sc)) {
            if(!sc.IsFinished) {
                return false;
            }
            SkillIdx.Value = sc.GetBestSkill(Target.Value.transform);
        }

        return SkillIdx.Value != -1;
    }

    //public override void OnStart()
    //{
    //}

    //public override void OnEnd()
    //{
    //}
}
