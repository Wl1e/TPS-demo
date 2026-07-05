using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsNull", story: "Check [m_Target] Is Null", category: "Variable Conditions", id: "843279849cc5ce1dbecc380453a0c048")]
public partial class IsNullCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    public override bool IsTrue()
    {
        return Target.Value == null;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
