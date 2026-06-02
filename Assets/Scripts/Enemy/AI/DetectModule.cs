using System;
using System.Linq;
using UnityEngine;

public class DetectModule : MonoBehaviour
{
    public GameObject Target;
    public float DetectionRange = 20f;
    public float MaxMemoryTime = 4f;

    public Action OnDetected;
    public Action OnLost;

    float m_TimeLastSeeTarget;
    bool m_HasTarget = false;

    private void Start()
    {
    }

    public void Detect(Actor actor, Collider[] ignoreCollider)
    {
        if(Target != null && !m_HasTarget && (m_TimeLastSeeTarget + MaxMemoryTime) < Time.time) {
            Target = null;
        }
        bool found = false;
        float minDistance = float.PositiveInfinity;
        Actor target = null;
        foreach (var other in ActorManager.Instance.Actors.Values) {
            if(found) {
                break;
            }
            if (other == actor) {
                continue;
            }
            if (other == null) {
                continue;
            }
            if (!actor.IsHostile(other)) {
                print(actor.Affiliation.ToString() + other.Affiliation);
                continue;
            }
            var distance = Vector3.Distance(actor.AimPoint.position, other.AimPoint.position);
            if (distance >= minDistance) {
                continue;
            }
            Vector3 dir = Vector3.Normalize(other.AimPoint.position - actor.AimPoint.position);
            var hits = Physics.RaycastAll(actor.AimPoint.position, dir, DetectionRange, -1, QueryTriggerInteraction.Ignore);
            foreach (var hit in hits) {
                if(!ignoreCollider.Contains(hit.collider)) {
                    minDistance = distance;
                    target = other;
                    found = true;
                    break;
                }
            }
        }

        if(m_HasTarget && target == null) {
            OnLost?.Invoke();
        } else if(!m_HasTarget && target != null) {
            Target = target.gameObject;
            m_TimeLastSeeTarget = Time.time;
            OnDetected?.Invoke();
        }
        m_HasTarget = target != null;
        return;
    }
}
