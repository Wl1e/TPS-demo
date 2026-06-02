using System;
using System.Linq;
using Unity.Behavior;
using Unity.VisualScripting;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Agent see Target", story: "[Agent] see [Target]", category: "Conditions", id: "313573549b70636fab7b64c52e6904e5")]
public partial class AgentSeeTargetCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    // 排除掉Npc、Pickup、Bullet层
    LayerMask SeeLayer = ~((1 << 21) | (1 << 14) | (1 << 22));

    public override bool IsTrue()
    {
        var agentEyePos = Agent.Value.GetComponent<Actor>()?.AimPoint.position ?? Agent.Value.transform.position;
        var targetAimPos = Target.Value.GetComponent<Actor>()?.AimPoint.position ?? Target.Value.transform.position;
        RaycastHit[] info = Physics.RaycastAll(agentEyePos, Vector3.Normalize(targetAimPos - agentEyePos),
                     Vector3.Distance(agentEyePos, targetAimPos) + 0.2f, SeeLayer, QueryTriggerInteraction.Ignore);
        info.OrderBy(hit => hit.distance);

        bool found = (info.Length == 0);
        if (info.Length > 0) {
            var agentCollider = Agent.Value.GetComponentsInChildren<Collider>();
            foreach (RaycastHit hit in info) {
                Debug.DrawLine(agentEyePos, hit.point, Color.green);       // 射线到命中点
                Debug.DrawLine(hit.point, hit.point + hit.normal * 0.5f, Color.blue);  // 法线
                Debug.Log($"Collider: {hit.collider.gameObject}");
                if (hit.collider.gameObject.GetEntityId() == Target.Value.GetEntityId()) {
                    found = true;
                    break;
                }
                if (!agentCollider.Contains(hit.collider)) {
                    break;
                }
            }
        }
        Debug.Log("See Target Condition: " + found);
        return found;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
