using UnityEngine;
using System.Collections;

public class ClimbAbility : MonoBehaviour, IMovementAbility
{
    public PlayerMovementState State => PlayerMovementState.Climb;

    public float ClimbSpeed = 1f;

    PlayerMovement m_Movement;
    public ClimbContoller ClimbController;

    public float Gravity = 0f;

    void IMovementAbility.Initialize(PlayerMovement movement)
    {
        m_Movement = movement;
    }

    public MovementModifier Process()
    {
        var normal = ClimbController.ClimbNormal.normalized;
        var lastNormal = ClimbController.LastNormal.normalized;
        var rawInput = m_Movement.RawInput;
        var YAxis = Vector3.ProjectOnPlane(Vector3.up, normal).normalized;
        var XAxis = Vector3.Cross(YAxis, normal).normalized;
        //print($"X: {XAxis}, Y: {YAxis}, n: {normal}");
        Vector3 move = (YAxis * rawInput.y + XAxis * -rawInput.x) * ClimbSpeed;
        return new MovementModifier { Velocity = move, OverrideRotation = true, Euler = Quaternion.LookRotation(-normal).eulerAngles };
    }
}
