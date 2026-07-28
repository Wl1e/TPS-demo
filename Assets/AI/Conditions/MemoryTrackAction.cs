using System;
using TPSDemo;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Memory track", story: "[Self] memory track [Target] time: [Duration]", category: "Action", id: "d820441494ef666fdb8415d6af78d168")]
public partial class MemoryTrackAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> Duration;

    private LayerMask m_SeeLayer = -1;

    private float m_LastTime = -1f;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        var target = Target.Value;
        if (target == null || Self.Value == null) {
            return Status.Failure;
        }

        bool isDied = false;
        if (target.TryGetComponent<Health>(out var health)) {
            isDied = health.IsDied;
        }

        bool canSee = TPSDemo.AI.AITool.AgentSeeTarget(Self.Value, Target.Value, m_SeeLayer);
        if(canSee && !isDied) {
            m_LastTime = Time.time;
            return Status.Running;
        }

        if (m_LastTime + Duration.Value > Time.time) {
            return Status.Running;
        }

        Target.Value = null;

        return Status.Failure;
    }

    protected override void OnEnd()
    {
    }
}

