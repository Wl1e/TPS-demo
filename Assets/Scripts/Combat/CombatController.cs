// CombatController应位于Player的Children下
using Combat;
using System;
using System.Collections.Generic;
using UnityEngine;

using SlotEntry = Tools.Entry<Combat.Slot, CombatSlot>;

namespace Combat {
    public enum Slot
    {
        Unarmed,
        Firearm,
        Throwable
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


public class CombatController: MonoBehaviour
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
    public IntEvent NumberInputEvent;
    //public IntEvent ChangeFirearmEvent;
    public BoolEvent TryFireEvent;
    public GameEvent EquipGrenadeEvent;

    private void Awake()
    {
        foreach(var slot in m_Slots) {
            m_Lookup.Add(slot.Key, slot.Value);
            slot.Value.OnAttack += OnAttack;
        }
        //m_Lookup[Slot.None] = null;
    }

    private void OnEnable()
    {
        TryFireEvent.RegisterListener(HandleTryAttack);
        TryReloadEvent.RegisterListener(HandleTryReload);
        NumberInputEvent.RegisterListener(HandleNumberInput);
        //ChangeFirearmEvent.RegisterListener(HandleChangeFirearm);
        EquipGrenadeEvent.RegisterListener(HandleTryEquipGrenade);
        EventManager.AddListener<Event.AimEvent>(OnAim);
    }

    private void OnDisable()
    {
        TryFireEvent.UnregisterListener(HandleTryAttack);
        TryReloadEvent.UnregisterListener(HandleTryReload);
        NumberInputEvent.UnregisterListener(HandleNumberInput);
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

    void HandleNumberInput(int index)
    {
        if (m_ActiveSlot != Slot.Firearm) {
            ActiveSlot(Slot.Firearm);
        }
        if (GetActiveSlot() is IFirearmSlot firearmSlot) {
            firearmSlot.TrySwitchFirearm(index);
            //m_EquipState = EquipState.Switching;
            EndTime = Time.time + SwitchTime;
        }
    }

    void HandleChangeFirearm(int value)
    {
        if (m_ActiveSlot != Slot.Firearm) {
            ActiveSlot(Slot.Firearm);
        }
        if (GetActiveSlot() is IFirearmSlot firearmSlot) {
            firearmSlot.TryChangeFirearmIndex(value);
            //m_EquipState = EquipState.Switching;
            EndTime = Time.time + SwitchTime;
        }
    }

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
    }

    void OnAttack(int weaponType, bool isEnd)
    {
        m_PlayerRuntimeData.AniParameter.Attack = !isEnd;
        m_PlayerRuntimeData.AniParameter.WeaponType = weaponType;
    }
}
