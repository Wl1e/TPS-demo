using UnityEngine;

public interface IMovement
{
    public Vector3 Velocity { get; }
    public bool IsGround { get; }
    void UpdateMovement();
}

public abstract class Movement: MonoBehaviour, IMovement
{
    Vector3 m_Velocity;
    bool m_IsGround;
    public float Gravity;
    public Vector3 Velocity => m_Velocity;
    public bool IsGround => m_IsGround;
    public abstract void UpdateMovement();
    public abstract void GroundCheck();
    public abstract void Rotate(float degree);
    public abstract void RotateTo(Quaternion q);
}

public class EnemyMovement : MonoBehaviour
{
    public float Speed;
    Vector3 m_Velocity;
    public float Gravity;
    public float GroundCheckDistance = 0.05f;
    bool m_IsGrounded;
    public bool IsGrounded => m_IsGrounded;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateMovement()
    {
        transform.position += m_Velocity * Time.deltaTime;
    }

    void Move(Vector3 dir)
    {
        m_Velocity = dir * Speed;
    }

    void GroundCheck()
    {
        //m_IsGrounded = false;
        //float radius = m_CharacterController.radius;
        //Vector3 start = transform.position + m_CharacterController.center - m_CharacterController.height / 2 * Vector3.up + radius * Vector3.up;
        //Vector3 end = transform.position + m_CharacterController.center + m_CharacterController.height / 2 * Vector3.up - radius * Vector3.up;
        //if (Physics.CapsuleCast(start, end, m_CharacterController.radius,
        //    Vector3.down, out RaycastHit hitInfo, GroundCheckDistance)
        //) {
        //    if (hitInfo.collider.gameObject != gameObject) {
        //        m_IsGrounded = true;
        //    }
        //}
    }
}
