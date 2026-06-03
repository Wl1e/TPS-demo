using System;

namespace TPSDemo
{

    public interface IFireMechanism
    {
        public float FireInternal { get; }
        public bool IsFiring { get; }

        public event Action OnShouldFire;

        public void StartFire();
        public void UpdateFire(float deltaTime);
        public void StopFire();
    }
}
