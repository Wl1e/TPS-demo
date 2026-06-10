// CombatController应位于Player的Children下
using System;
using System.Collections.Generic;
using Unity.Netcode;
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
    public class CombatController : NetworkBehaviour
    {
        NetworkVariable<Slot> m_ActiveSlot = new NetworkVariable<Slot>(Slot.Unarmed);
        //EquipState m_EquipState = EquipState.None;
        [SerializeField] List<SlotEntry> m_Slots;
        Dictionary<Slot, ICombatSlot> m_Lookup = new Dictionary<Slot, ICombatSlot>();

        PlayerController m_PlayerController;
        PlayerRuntimeData m_PlayerRuntimeData;

        public Slot CurrentActiveSlot => m_ActiveSlot.Value;

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
            m_PlayerController = GetComponentInParent<PlayerController>();
            m_PlayerRuntimeData = m_PlayerController.RuntimeData;
            foreach (var slot in m_Slots) {
                m_Lookup.Add(slot.Key, slot.Value);
                slot.Value.SetExitFunc(SlotExited);
            }
            //m_Lookup[Slot.None] = null;
        }
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            m_ActiveSlot.OnValueChanged += OnSlotChanged;

            if(IsOwner) {
                RegisterEvents();
                ActiveSlot(Slot.Firearm);
            }
        }
        public override void OnNetworkDespawn()
        {
            if (IsOwner) {
                UnregisterEvents();
            }
            m_ActiveSlot.OnValueChanged -= OnSlotChanged;
            base.OnNetworkDespawn();
        }

        private void RegisterEvents()
        {
            TryFireEvent.RegisterListener(HandleTryAttack);
            TryReloadEvent.RegisterListener(HandleTryReload);
            Firearm1Event.RegisterListener(HandleTryFirearm1);
            Firearm2Event.RegisterListener(HandleTryFirearm2);
            //ChangeFirearmEvent.RegisterListener(HandleChangeFirearm);
            EquipGrenadeEvent.RegisterListener(HandleTryEquipGrenade);
            EventManager.AddListener<Event.AimEvent>(OnAim);
        }

        private void UnregisterEvents()
        {
            TryFireEvent.UnregisterListener(HandleTryAttack);
            TryReloadEvent.UnregisterListener(HandleTryReload);
            Firearm1Event.UnregisterListener(HandleTryFirearm1);
            Firearm2Event.UnregisterListener(HandleTryFirearm2);
            //ChangeFirearmEvent.UnregisterListener(HandleChangeFirearm);
            EquipGrenadeEvent.UnregisterListener(HandleTryEquipGrenade);
            EventManager.RemoveListener<Event.AimEvent>(OnAim);
        }

        ICombatSlot GetActiveSlot() => m_Lookup[CurrentActiveSlot];
        bool ValidSlotIndex(int index)
        {
            return index >= 0 && index < m_Slots.Count;
        }
        [ServerRpc]
        void ActiveSlotServerRpc(Slot slot)
        {
            print("TryActive: " + slot);
            if (!m_Lookup[slot].ValidActive()) {
                return;
            }
            print("Active: " + slot);
            m_ActiveSlot.Value = slot;
            //foreach (var entry in m_Lookup) {
            //    entry.Value.SetActive(CurrentActiveSlot == entry.Key);
            //}
        }

        void ActiveSlot(Slot slot)
        {
            ActiveSlotServerRpc(slot);
        }

        void SlotExited(Slot slot)
        {
            if (IsOwner && CurrentActiveSlot == slot) {
                ActiveSlotServerRpc(Slot.Unarmed);
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
            print("TryEquipFirearm");
            if (!m_PlayerController.Loadout.HasWeapon(index)) {
                ActiveSlot(Slot.Unarmed);
                return;
            }
            if (CurrentActiveSlot != Slot.Firearm) {
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
            if (CurrentActiveSlot != Slot.Throwable) {
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
        void OnSlotChanged(Slot pre, Slot cur)
        {
            print("OnSlotChanged");
            if(IsOwner) {
                foreach (var entry in m_Lookup) {
                    entry.Value.SetActive(CurrentActiveSlot == entry.Key);
                }
                EndTime = Time.time + EquipTime;
                m_PlayerRuntimeData.ActiveSlot = CurrentActiveSlot;
                m_PlayerRuntimeData.AniParameter.CombatSlot = (int)CurrentActiveSlot;
                OnSlotChange?.Invoke(CurrentActiveSlot);
                print("Active: " + m_ActiveSlot);
            }
        }
    }
}
