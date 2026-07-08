using System;
using Unity.Behavior;
using UnityEngine;



[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Not see", story: "[Self] not see [Target]", category: "Conditions", id: "666580a94b540772be31dcb562ace1d5")]
public partial class NotSeeCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    private LayerMask m_SeeLayer = -1;

    public override bool IsTrue()
    {
        return !TPSDemo.AI.AITool.AgentSeeTarget(Self.Value, Target.Value, m_SeeLayer);
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
