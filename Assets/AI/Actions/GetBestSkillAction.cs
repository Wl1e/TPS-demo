using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Get best skill", story: "[Self] to [Target] Get Best Skill [SkillIdx]", category: "Action", id: "49e15514d2708d16b8154c0378bf8b02")]
public partial class GetBestSkillAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<int> SkillId;
    protected override Status OnStart()
    {
        if(Self == null || Target == null) {
            return Status.Failure;
        }
        int result = -1;
        if(Self.Value.TryGetComponent<TPSDemo.SkillController>(out var sc)) {
            result = sc.GetBestSkill(Target.Value.transform);
        }
        SkillId.Value = result;
        return Status.Success;
    }
}

