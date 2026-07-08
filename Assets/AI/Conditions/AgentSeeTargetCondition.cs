using System;
using System.Linq;
using Unity.Behavior;
using Unity.VisualScripting;
using UnityEngine;

namespace TPSDemo.AI
{
    static public class AITool
    {
        static public bool AgentSeeTarget(GameObject agent, GameObject target, LayerMask seeLayer)
        {

            Vector3 agentEyePos;
            if (agent.TryGetComponent<Actor>(out var actor)) {
                agentEyePos = actor.AimPoint.position;
            } else {
                agentEyePos = actor.transform.position;
            }

            Vector3 targetAimPos;
            if (target.TryGetComponent<Actor>(out var actor1)) {
                targetAimPos = actor1.AimPoint.position;
            } else {
                targetAimPos = actor1.transform.position;
            }
            RaycastHit[] info = Physics.RaycastAll(agentEyePos, Vector3.Normalize(targetAimPos - agentEyePos),
                         Vector3.Distance(agentEyePos, targetAimPos) + 0.2f, seeLayer, QueryTriggerInteraction.Ignore);
            info.OrderBy(hit => hit.distance);

            bool found = (info.Length == 0);
            if (info.Length > 0) {
                var agentCollider = agent.GetComponentsInChildren<Collider>();
                foreach (RaycastHit hit in info) {
                    Debug.DrawLine(agentEyePos, hit.point, Color.green);
                    if (hit.collider.gameObject.GetEntityId() == target.GetEntityId()) {
                        found = true;
                        break;
                    }
                    if (!agentCollider.Contains(hit.collider)) {
                        Debug.Log("Collider: " + hit.collider.gameObject);
                        break;
                    }
                }
            }
            foreach (var hit in info) {
                Debug.Log(hit.collider.gameObject);
            }
            return found;
        }
    }
}

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Agent see m_Target", story: "[Agent] see [m_Target]", category: "Conditions", id: "313573549b70636fab7b64c52e6904e5")]
public partial class AgentSeeTargetCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    LayerMask m_SeeLayer = -1;

    public override bool IsTrue()
    {
        return TPSDemo.AI.AITool.AgentSeeTarget(Agent.Value, Target.Value, m_SeeLayer);
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
