using UnityEngine;

public interface IGrenade
{
    Rigidbody Rigidbody {  get; }
    CapsuleCollider CapsuleCollider { get; }
    public float Speed { get; }
    public void Throw(int actorId, Vector3 dir);
    public void OnHold();
    public void OnStore();
}
