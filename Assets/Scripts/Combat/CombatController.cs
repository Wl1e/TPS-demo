// CombatController应位于Player的Children下
using System;
using System.Collections.Generic;
using UnityEngine;

using SlotEntry = Tools.Entry<TPSDemo.Combat.Slot, TPSDemo.CombatSlot>;

namespace TPSDemo.Combat
{
    public enum Slot
    {
        Unarmed = 0,
        Firearm = 1,
        Throwable = 2,
    }

    public enum EquipState
    {
        None,
        Equipping,
        Holding,
        Switching,
        Unequipping,
        Reloading
    }
} // namespace Combat

namespace TPSDemo
{
    using Combat;
    public class CombatController : MonoBehaviour
    {
        Slot m_ActiveSlot = Slot.Unarmed;
        //EquipState m_EquipState = EquipState.None;
        [SerializeField] List<SlotEntry> m_Slots;
        Dictionary<Slot, ICombatSlot> m_Lookup = new Dictionary<Slot, ICombatSlot>();

        PlayerController m_PlayerController;
        PlayerRuntimeData m_PlayerRuntimeData;

        public Slot CurrentActiveSlot => m_ActiveSlot;

        public float EquipTime = 0.3f;
        public float SwitchTime = 0.3f;
        public float EndTime = 0f;

        // Action
        public event Action<Slot> OnSlotChange;

        // Event
        public GameEvent TryReloadEvent;
        public GameEvent Firearm1Event;
        public GameEvent Firearm2Event;
        //public IntEvent ChangeFirearmEvent;
        public BoolEvent TryFireEvent;
        public GameEvent EquipGrenadeEvent;

        private void Awake()
        {
            foreach (var slot in m_Slots) {
                m_Lookup.Add(slot.Key, slot.Value);
                slot.Value.OnAttack += OnAttack;
                slot.Value.SetExitFunc(SlotExited);
            }
            //m_Lookup[Slot.None] = null;
        }

        private void OnEnable()
        {
            TryFireEvent.RegisterListener(HandleTryAttack);
            TryReloadEvent.RegisterListener(HandleTryReload);
            Firearm1Event.RegisterListener(HandleTryFirearm1);
            Firearm2Event.RegisterListener(HandleTryFirearm2);
            //ChangeFirearmEvent.RegisterListener(HandleChangeFirearm);
            EquipGrenadeEvent.RegisterListener(HandleTryEquipGrenade);
            EventManager.AddListener<Event.AimEvent>(OnAim);
        }

        private void OnDisable()
        {
            TryFireEvent.UnregisterListener(HandleTryAttack);
            TryReloadEvent.UnregisterListener(HandleTryReload);
            Firearm1Event.UnregisterListener(HandleTryFirearm1);
            Firearm2Event.UnregisterListener(HandleTryFirearm2);
            //ChangeFirearmEvent.UnregisterListener(HandleChangeFirearm);
            EquipGrenadeEvent.UnregisterListener(HandleTryEquipGrenade);
            EventManager.RemoveListener<Event.AimEvent>(OnAim);
        }

        private void Start()
        {
            m_PlayerController = GetComponentInParent<PlayerController>();
            m_PlayerRuntimeData = m_PlayerController.RuntimeData;
            ActiveSlot(Slot.Firearm);
        }
        ICombatSlot GetActiveSlot() => m_Lookup[m_ActiveSlot];
        bool ValidSlotIndex(int index)
        {
            return index >= 0 && index < m_Slots.Count;
        }
        void ActiveSlot(Slot slot)
        {
            if (!m_Lookup[slot].ValidActive()) {
                return;
            }
            m_ActiveSlot = slot;
            foreach (var entry in m_Lookup) {
                entry.Value.SetActive(m_ActiveSlot == entry.Key);
            }
            EndTime = Time.time + EquipTime;
            m_PlayerRuntimeData.ActiveSlot = m_ActiveSlot;
            OnSlotChange?.Invoke(m_ActiveSlot);
            print("Active: " + m_ActiveSlot);
            m_PlayerRuntimeData.AniParameter.CombatSlot = (int)m_ActiveSlot;
        }

        void SlotExited(Slot slot)
        {
            if (m_ActiveSlot == slot) {
                ActiveSlot(Slot.Unarmed);
            }
        }

        void HandleTryReload()
        {
            var slot = GetActiveSlot();
            if (slot != null) {
                if (slot is IFirearmSlot firearmSlot) {
                    //m_EquipState = EquipState.Reloading;
                    firearmSlot.TryReload();
                }
            }
        }

        void TryEquipFirearm(int index)
        {
            if (!m_PlayerController.Loadout.HasWeapon(index)) {
                ActiveSlot(Slot.Unarmed);
                return;
            }
            if (m_ActiveSlot != Slot.Firearm) {
                ActiveSlot(Slot.Firearm);
            }
            if (GetActiveSlot() is IFirearmSlot firearmSlot) {
                firearmSlot.TrySwitchFirearm(index);
                //m_EquipState = EquipState.Switching;
                EndTime = Time.time + SwitchTime;
            }
        }

        void HandleTryFirearm1() => TryEquipFirearm(1);

        void HandleTryFirearm2() => TryEquipFirearm(2);

        void HandleTryAttack(bool value)
        {
            GetActiveSlot()?.Attack(value);
        }

        void HandleTryEquipGrenade()
        {
            if (m_ActiveSlot != Slot.Throwable) {
                ActiveSlot(Slot.Throwable);
            } else {
                ActiveSlot(Slot.Unarmed);
            }
        }

        public bool ValidAim()
        {
            var slot = GetActiveSlot();
            if (slot != null) {
                return slot.ValidAim();
            }
            return false;
        }

        void OnAim(Event.AimEvent evt)
        {
            GetActiveSlot()?.OnAim(evt.IsAiming);
            m_PlayerRuntimeData.AniParameter.Attack = false;
        }

        void OnAttack(int weaponType, bool attack)
        {
            
        }
    }
}
