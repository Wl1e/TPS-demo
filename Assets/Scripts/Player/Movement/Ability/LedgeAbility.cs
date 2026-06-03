using UnityEngine;
using System.Collections;

namespace TPSDemo
{
	public class LedgeAbility: MonoBehaviour, IMovementAbility
	{
        PlayerMovement m_Movement;
        public ClimbContoller ClimbContoller;
        public PlayerMovementState State => PlayerMovementState.Ledge;
        public void Initialize(PlayerMovement movement)
        {
            m_Movement = movement;
        }

        public MovementModifier Process()
        {
            var inputX = m_Movement.RawInput.x;
            var normal = ClimbContoller.ClimbNormal;
            var XAxis = Vector3.Cross(Vector3.ProjectOnPlane(Vector3.up, normal).normalized, normal).normalized;
            Debug.DrawLine(transform.position, transform.position + XAxis, Color.darkRed, 1f);
            return new MovementModifier {
                Velocity = XAxis * -inputX,
                OverrideRotation = true,
                Euler = Quaternion.LookRotation(-normal).eulerAngles
            };
        }
    }
}
