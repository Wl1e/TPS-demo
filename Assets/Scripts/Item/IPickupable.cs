using UnityEngine;

namespace TPSDemo
{
    public interface IPickupable : IInteractive
    {
        public void WhenSee();
    }
}
