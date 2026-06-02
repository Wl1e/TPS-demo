using System;
using System.Linq;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using static UnityEngine.UI.Image;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Find Visible Closest With Tag", story: "Find [Target] closest and visible to [Agent] with tag: [Tag]", category: "Action", id: "eaac8f6d597438909a24060d1cd199da")]
public partial class FindVisibleClosestWithTagAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<string> Tag;

    Collider[] agentCollider;

    protected override Status OnStart()
    {
        if (Agent.Value == null) {
            LogFailure("No agent provided.");
            return Status.Failure;
        }

        //if (agentCollider.Length == 0) {
        //    agentCollider = Agent.Value.GetComponentsInChildren<Collider>();
        //}

        Vector3 agentPosition = Agent.Value.transform.position;
        var agentEyePos = Agent.Value.GetComponent<Actor>()?.AimPoint.position ?? Agent.Value.transform.position;

        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(Tag.Value);
        float closestDistanceSq = Mathf.Infinity;
        GameObject closestGameObject = null;
        foreach (GameObject gameObject in gameObjects) {
            float distanceSq = Vector3.SqrMagnitude(agentPosition - gameObject.transform.position);
            if (distanceSq < closestDistanceSq) {

                var targetAimPos = gameObject.GetComponent<Actor>()?.AimPoint.position ?? gameObject.transform.position;
                Debug.DrawLine(agentEyePos, targetAimPos, Color.yellow);
                RaycastHit[] info = Physics.RaycastAll(agentEyePos, Vector3.Normalize(targetAimPos - agentEyePos),
                     Mathf.Sqrt(distanceSq) + 1f, -1, QueryTriggerInteraction.Ignore);

                bool found = false;
                if (info.Length > 0) {
                    bool isAgent = true;
                    agentCollider = Agent.Value.GetComponentsInChildren<Collider>();
                    foreach (RaycastHit hit in info) {
                        Debug.DrawLine(agentEyePos, hit.point, Color.green);       // 射线到命中点
                        Debug.DrawLine(hit.point, hit.point + hit.normal * 0.5f, Color.blue);  // 法线
                        if (hit.collider.gameObject.GetEntityId() == gameObject.GetEntityId()) {
                            found = true;
                            break;
                        }
                        if (!agentCollider.Contains(hit.collider)) {
                            isAgent = false;
                            break;
                        }
                    }
                    if(!isAgent) {
                        continue;
                    }
                }
                if (found) {
                    closestDistanceSq = distanceSq;
                    closestGameObject = gameObject;
                }
            }
        }

        Target.Value = closestGameObject;
        return Status.Success;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

