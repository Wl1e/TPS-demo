using System;
using Unity.Behavior;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "FindClosestTargetWithTagInDistance", story: "Find [Target] Closest To [Agent] With [Tag] In [Distance]", category: "Action/Find", id: "c5d833f79fa890abd86728017f791df7")]
public partial class FindClosestTargetWithTagInDistanceAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<string> Tag;
    [SerializeReference] public BlackboardVariable<float> Distance;
    protected override Status OnStart()
    {
        if (Agent.Value == null) {
            LogFailure("No agent provided.");
            return Status.Failure;
        }

        Target.Value = null;
        Vector3 agentPosition = Agent.Value.transform.position;

        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(Tag.Value);
        float closestDistanceSq = Mathf.Infinity;
        GameObject closestGameObject = null;
        foreach (GameObject gameObject in gameObjects) {
            float distanceSq = Vector3.SqrMagnitude(agentPosition - gameObject.transform.position);
            if (closestGameObject == null || distanceSq < closestDistanceSq) {
                closestDistanceSq = distanceSq;
                closestGameObject = gameObject;
            }
        }
        if (closestDistanceSq <= Distance * Distance) {
            Target.Value = closestGameObject;
        }
        return Target.Value == null ? Status.Failure : Status.Success;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

