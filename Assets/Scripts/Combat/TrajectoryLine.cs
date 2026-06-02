using System.Collections.Generic;
using UnityEngine;

public class TrajectoryLine
{
    public LineRenderer Line;
    readonly float m_Gravity = 9.81f;
    [SerializeField] float m_TimeStep = 0.05f;
    [SerializeField] float m_MaxTime = 5f;
    public LayerMask CollisionMask;

    public void UpdateTrajectory(Vector3 startPos, Vector3 startVelocity)
    {
        List<Vector3> points = new List<Vector3>();
        Vector3 currentPos = startPos;
        Vector3 currentVel = startVelocity;

        for (float t = 0f; t < m_MaxTime; t += m_TimeStep) {
            points.Add(currentPos);
            Debug.DrawLine(currentPos, currentPos + currentVel * m_TimeStep, Color.yellowGreen, 0.1f);
            if (
                Physics.Raycast(
                    currentPos, Vector3.Normalize(currentVel),
                    out RaycastHit info, currentVel.magnitude * m_TimeStep,
                    CollisionMask, QueryTriggerInteraction.Ignore
                )
            ) {
                points.Add(info.point);
                break;
            }
            currentVel += Vector3.down * m_Gravity * m_TimeStep;
            currentPos += currentVel * m_TimeStep;

        }

        Line.positionCount = points.Count;
        Line.SetPositions(points.ToArray());
    }

    public void Hide() => Line.positionCount = 0;
}

