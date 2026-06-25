using UnityEngine;

namespace TPSDemo
{

    public class RootMotionMovement : MonoBehaviour
    {
        public enum CouplingMode
        {
            Decoupled,
            Coupled
        }

        PlayerRuntimeData m_PlayerRuntimeData;
        CharacterController m_CharacterController;

        // 定义最大速度约束动画
        public float WalkMaxSpeed = 1.5f;
        public float RunMaxSpeed = 3f;
        public float Acceleration = 10f;
        public float JumpForce = 5f;
        public float Gravity = 9.81f;

        Vector3 m_RawInput = Vector3.zero;

        float m_RotationVelocity;

        bool m_Running = false;
        Vector3 m_InputVelocity;
        bool m_IsGrounded;
        bool m_JumpThisFrame;

        public Vector2Event MoveInput;

        private void OnEnable()
        {
            MoveInput.RegisterListener(OnMoveInput);
        }

        private void OnDisable()
        {
            MoveInput.UnregisterListener(OnMoveInput);
        }

        private void Start()
        {
            m_PlayerRuntimeData = GetComponent<PlayerController>().RuntimeData;
            m_CharacterController = GetComponent<CharacterController>();
        }

        private void FixedUpdate()
        {
        }

        void OnMoveInput(Vector2 input)
        {
            float maxSpeed = m_Running ? RunMaxSpeed : WalkMaxSpeed;
            m_RawInput = Vector3.Normalize(Quaternion.Euler(0, m_PlayerRuntimeData.CameraRoot.eulerAngles.y, 0) * new Vector3(input.x, 0, input.y));
            m_InputVelocity += m_RawInput * Acceleration * Time.deltaTime;
            m_InputVelocity = Vector3.ClampMagnitude(m_InputVelocity, maxSpeed);
            //m_PlayerRuntimeData.AniParameter.Velocity = m_InputVelocity;
        }

        void GroundCheck()
        {
            m_IsGrounded = false;
            float checkDistance = m_CharacterController.skinWidth + 0.03f;
            float radius = m_CharacterController.radius;
            Vector3 start = transform.position + m_CharacterController.center - m_CharacterController.height / 2 * Vector3.up + radius * Vector3.up;
            Vector3 end = transform.position + m_CharacterController.center + m_CharacterController.height / 2 * Vector3.up - radius * Vector3.up;
            if (Physics.CapsuleCast(start, end, m_CharacterController.radius,
                Vector3.down, out RaycastHit hitInfo, checkDistance)
            ) {
                if (hitInfo.collider.gameObject != gameObject) {
                    m_IsGrounded = true;
                }
            }
        }

        public void SetVelocity(Vector3 velocity)
        {
            m_CharacterController.SimpleMove(velocity);
        }
        public void SetRotation(Quaternion deltaRotation)
        {
            transform.Rotate(deltaRotation.eulerAngles);
        }
    }
}
