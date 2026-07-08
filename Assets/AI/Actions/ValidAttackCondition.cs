using System;
using TPSDemo;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "ValidAttack", story: "[Attacker] Valid Attack [Target]", category: "Conditions", id: "d0031375aaea9339b1a0e621f63639b0")]
public partial class ValidAttackCondition : Condition
{
    [SerializeReference] public BlackboardVariable<AttackerBase> Attacker;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    public override bool IsTrue()
    {
        var attacker = Attacker.Value;
        var target = Target.Value;
        //Debug.Log($"Attacker: {attacker}, Target: {target}");
        // attacker.CanAttack()放到Attack方法中自己判断
        return attacker
            && target
            && attacker.InAttackRange(target.transform)
            && TPSDemo.AI.AITool.AgentSeeTarget(attacker.Owner.gameObject, target, -1);
    }

    public override void OnStart()
    {
        if(Attacker ==  null || Target == null) {
            Debug.Log("Attacker or target is null");
        }
    }

    public override void OnEnd()
    {
    }
}
