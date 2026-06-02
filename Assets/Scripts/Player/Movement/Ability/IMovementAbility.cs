using UnityEngine;

public class MovementModifier
{
    public Vector3 Velocity;
    public bool OverrideRotation = false;
    public Vector3 Euler;
}
public interface IMovementAbility
{
    public PlayerMovementState State { get; }
    public void Initialize(PlayerMovement movement);

    public MovementModifier Process();
}
