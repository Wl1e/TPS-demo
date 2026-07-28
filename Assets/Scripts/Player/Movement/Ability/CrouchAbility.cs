using UnityEngine;

namespace TPSDemo
{

    public class CrouchAbility : MonoBehaviour, IMovementAbility
    {
        public float CrouchSpeed = 1f;
        public float Acceleration = 8f;
        public PlayerMovement m_Movement;

        public PlayerMovementState State => PlayerMovementState.Crouch;

        public void Initialize(PlayerMovement movement)
        {
            m_Movement = movement;
        }

        public MovementModifier Process()
        {
            var velocity = m_Movement.Velocity;
            MovementModifier result = new MovementModifier();
            float velocityY = velocity.y;
            var VelocityXZ = Vector3.ProjectOnPlane(velocity, Vector3.up);
            if (m_Movement.Input != Vector3.zero) {
                VelocityXZ += m_Movement.InputGlobal * Acceleration * Time.deltaTime;
                VelocityXZ = Vector3.ClampMagnitude(VelocityXZ, CrouchSpeed);
            } else {
                VelocityXZ = Vector3.MoveTowards(VelocityXZ, Vector3.zero, Acceleration * Time.deltaTime);
            }
            if (!m_Movement.IsGrounded) {
                velocityY -= m_Movement.Gravity * Time.deltaTime;
            }
            result.Velocity = VelocityXZ + Vector3.up * velocityY;
            return result;
        }
    }
}
