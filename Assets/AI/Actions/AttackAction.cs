using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Attack", story: "[Agent] attack [Target]", category: "Action/Combat", id: "e214e03901521f4e46df50d4243342e0")]
public partial class AttackAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    TPSDemo.IAttacker m_Attacker = null;
    protected override Status OnStart()
    {
        if(Agent?.Value == null || Target?.Value == null) {
            return Status.Failure;
        }
        m_Attacker = Agent.Value.GetComponentInChildren<TPSDemo.IAttacker>();
        if(m_Attacker == null) {
            return Status.Failure;
        }
        if (m_Attacker.CanAttack()) {
            var aimPoint = Target.Value.GetComponent<TPSDemo.Actor>()?.AimPoint.position ?? Target.Value.transform.position;
            m_Attacker.Attack(aimPoint);
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

