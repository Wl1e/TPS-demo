using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "FindRandomLOcationAroundActor", story: "Find a random location [location] around [actor] within range [range]", category: "Action", id: "efed00ff209cbd9d77a6cb55261c9cdf")]
public partial class FindRandomLocationAroundActorAction: Action
{
    [SerializeReference] public BlackboardVariable<Vector3> Location;
    [SerializeReference] public BlackboardVariable<GameObject> Actor;
    [SerializeReference] public BlackboardVariable<float> Range;

    public int MaxAttempts = 10;

    

    protected override Status OnStart()
    {
        if(!Actor.Value) {
            return Status.Failure;
        }
        if (!Actor.Value.TryGetComponent(out NavMeshAgent agent)) {
            return Status.Failure;
        }
        for (int i = 0; i < MaxAttempts; i++) {
            Vector2 randomDirection = UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range(Range.Value / 2, Range.Value);
            Vector3 V3Dir = new(randomDirection.x, 0, randomDirection.y);
            V3Dir += Actor.Value.transform.position;

            var path = new NavMeshPath();
            if (!agent.CalculatePath(V3Dir, path)) {
                continue;
            }
            Location.Value = V3Dir;
            return Status.Success;
        }
        return Status.Failure;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

