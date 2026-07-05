using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsNotNull", story: "Check [m_Target] Is Not Null", category: "Variable Conditions", id: "aff58aaa5e735a906ef70ba8a51c9da4")]
public partial class IsNotNullCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    public override bool IsTrue()
    {
        return Target.Value != null;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
