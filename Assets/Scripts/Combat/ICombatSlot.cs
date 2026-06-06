using System;
using UnityEngine;
using Unity.Netcode;

namespace TPSDemo
{
using Combat;

    public interface ICombatSlot
    {
        public Slot Slot { get; }
        public bool IsActive { get; }
        event Action<int, bool> OnAttack;
        public bool ValidActive();
        public void SetActive(bool isActive);
        public void Attack(bool isEnd);
        public void OnAim(bool isAiming);
        public bool ValidAim();
        public void SetExitFunc(Action<Slot> func);
    }

    public abstract class CombatSlot : NetworkBehaviour, ICombatSlot
    {
        public abstract bool IsActive { get; }
        public event Action<int, bool> OnAttack;
        public Action<Slot> m_ExitFunc;
        [SerializeField] Slot m_Slot;
        public Slot Slot => m_Slot;
        protected void RaiseAttack(int weaponType, bool isEnd)
        {
            OnAttack?.Invoke(weaponType, isEnd);
        }
        public abstract bool ValidActive();
        public abstract void SetActive(bool isActive);
        public abstract void Attack(bool isEnd);
        public abstract void OnAim(bool isAiming);
        public abstract bool ValidAim();
        public void SetExitFunc(Action<Slot> func)
        {
            m_ExitFunc = func;
        }
        protected void Exit()
        {
            m_ExitFunc?.Invoke(m_Slot);
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
}
