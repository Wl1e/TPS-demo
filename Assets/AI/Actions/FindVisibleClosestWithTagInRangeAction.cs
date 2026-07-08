using System;
using System.Linq;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Find Visible Closest With Tag in Range", story: "Find [m_Target] closest and visible to [Self] with [Tag] In [Range]", category: "Action", id: "eaac8f6d597438909a24060d1cd199da")]
public partial class FindVisibleClosestWithTagInRangeAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<string> Tag;
    [SerializeReference] public BlackboardVariable<float> Range;

    Vector3 m_AgentEyePos;
    Vector3 m_AgentPos;
    Collider[] agentCollider;

    protected override Status OnStart()
    {
        var agent = Self.Value;
        if (agent == null) {
            LogFailure("No agent provided.");
            return Status.Failure;
        }

        agentCollider = agent.GetComponentsInChildren<Collider>();
        if (agent.TryGetComponent<TPSDemo.Actor>(out var actor)) {
            m_AgentEyePos = actor.AimPoint.position;
        } else {
            m_AgentEyePos = agent.transform.position;
        }
        m_AgentPos = agent.transform.position;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(Tag.Value);
        float closestDistanceSq = Range.Value * Range.Value;
        GameObject closestGameObject = null;
        foreach (GameObject gameObject in gameObjects) {
            float distanceSq = Vector3.SqrMagnitude(m_AgentPos - gameObject.transform.position);
            if (distanceSq < closestDistanceSq) {
                if (!gameObject.TryGetComponent<TPSDemo.Actor>(out var actor1)) {
                    continue;
                }
                Vector3 targetAimPos = actor1.AimPoint.position;

                Debug.DrawLine(m_AgentEyePos, targetAimPos, Color.yellow);
                RaycastHit[] info = Physics.RaycastAll(m_AgentEyePos, Vector3.Normalize(targetAimPos - m_AgentEyePos),
                     Mathf.Sqrt(distanceSq) + 1f, -1, QueryTriggerInteraction.Ignore);

                bool found = false;
                if (info.Length > 0) {
                    bool isAgent = true;
                    
                    foreach (RaycastHit hit in info) {
                        Debug.DrawLine(m_AgentEyePos, hit.point, Color.green);
                        Debug.DrawLine(hit.point, hit.point + hit.normal * 0.5f, Color.blue);
                        if (hit.collider.gameObject.GetEntityId() == gameObject.GetEntityId()) {
                            found = true;
                            break;
                        }
                        if (!agentCollider.Contains(hit.collider)) {
                            isAgent = false;
                            break;
                        }
                    }
                    if (!isAgent) {
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
        // 为了和寻路兼容，如果没找到改成返回running，意思是没找到就一直找
        return closestGameObject != null ? Status.Success : Status.Running;
    }

    protected override void OnEnd()
    {
    }
}

