using UnityEngine;

public interface IInteractive
{
    public float InteractRadius { get; }
    public void Interact(GameObject player);
}
