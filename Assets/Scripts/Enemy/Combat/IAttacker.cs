using UnityEngine;

public interface IAttacker
{
    public bool CanFire();
    public bool InFireRange(Transform target);
    public void Fire(Vector3 pos);
}

public abstract class AttackerBase: MonoBehaviour, IAttacker
{
    public EnemyController Owner;
    public abstract bool CanFire();
    public abstract bool InFireRange(Transform target);
    public abstract void Fire(Vector3 pos);
}

