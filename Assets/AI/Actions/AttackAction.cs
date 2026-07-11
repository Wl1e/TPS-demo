using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Attack", story: "[Attacker] attack [Target]", category: "Action", id: "e214e03901521f4e46df50d4243342e0")]
public partial class AttackAction : Action
{
    [SerializeReference] public BlackboardVariable<TPSDemo.AttackerBase> Attacker;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    protected override Status OnStart()
    {
        if(Attacker?.Value == null || Target?.Value == null) {
            return Status.Failure;
        }
        if (Attacker.Value.CanAttack()) {
            Attacker.Value.Attack(Target.Value.transform);
        }
        return Status.Success;
    }

    //protected override Status OnUpdate()
    //{
        
    //}

    protected override void OnEnd()
    {
    }
}

