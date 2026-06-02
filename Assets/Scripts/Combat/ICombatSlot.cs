using System;
using UnityEngine;

public interface ICombatSlot
{
    public bool IsActive { get; }
    event Action<int, bool> OnAttack;
    public bool ValidActive();
    public void SetActive(bool isActive);
    public void Attack(bool isEnd);
    public void OnAim(bool isAiming);
    public bool ValidAim();
    public void SetExitFunc(Func<bool> func);
}

public abstract class CombatSlot : MonoBehaviour, ICombatSlot
{
    public abstract bool IsActive { get; }
    public event Action<int, bool> OnAttack;
    public Func<bool> m_ExitFunc;
    protected void RaiseAttack(int weaponType, bool isEnd)
    {
        OnAttack?.Invoke(weaponType, isEnd);
    }
    public abstract bool ValidActive();
    public abstract void SetActive(bool isActive);
    public abstract void Attack(bool isEnd);
    public abstract void OnAim(bool isAiming);
    public abstract bool ValidAim();
    public void SetExitFunc(Func<bool> func)
    {
        m_ExitFunc = func;
    }
    protected void Exit()
    {
        m_ExitFunc.Invoke();
    }
}

// 如果要将firearm的逻辑从combatslot基类中剥离，
// 可以给combatslot再新加一个基类combatslotbase，combatcontroller中用combatslotbase引用各slot
// 新增一个firearmcombatslot给weaponmanager用

public abstract class FirearmCombatSlot : CombatSlot, IFirearmSlot
{
    public abstract float ReloadTime { get; }

    public abstract void TrySwitchFirearm(int index);

    public abstract void TryChangeFirearmIndex(int value);

    public abstract void TryReload();
}
