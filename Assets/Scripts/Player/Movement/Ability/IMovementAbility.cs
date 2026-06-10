using UnityEngine;

namespace TPSDemo
{

    public class MovementModifier
    {
        public Vector3 Velocity = Vector3.zero;
        public bool OverrideRotation = false;
        public Vector3 Euler = Vector3.zero;
    }
    public interface IMovementAbility
    {
        public PlayerMovementState State { get; }
        public void Initialize(PlayerMovement movement);

        public MovementModifier Process();
    }
}
