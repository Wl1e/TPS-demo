using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "GetDistanceSqBetweenTransformAndTarget", story: "Get DistanceSq Between [Transform] And [m_Target] To [Variable]", category: "Action", id: "1d6ff66485e7b2665173a1cae13e7eb7")]
public partial class GetDistanceSqBetweenTransformAndTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<Transform> Transform;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> Variable;
    protected override Status OnStart()
    {
        Variable.Value = Vector3.SqrMagnitude(Target.Value.transform.position - Transform.Value.position);
        return Status.Success;
    }
}

