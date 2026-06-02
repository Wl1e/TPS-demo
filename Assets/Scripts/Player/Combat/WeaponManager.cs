using Event;
using System;
using System.Collections;
using UnityEngine;

public class WeaponManager: FirearmCombatSlot
{
    // IFirearmController
    bool m_IsActive = false;
    public override bool IsActive => m_IsActive;
    public override float ReloadTime => m_ReloadTime;

    public int CurrentFirearmIndex => m_CurrentFirearmIndex;
    public IWeapon CurrentFirearm => m_CurrentFirearm;

    public Action<int, bool> m_OnAttack;
    

    // Weapon
    PlayerRuntimeData m_RuntimeData;
    Inventory m_Inventory;
    Loadout m_Loadout;
    int m_CurrentFirearmIndex = -1;
    IWeapon m_CurrentFirearm = null;
    [SerializeField] float m_ReloadTime = 1f;

    Coroutine m_ReloadCoroutine = null;

    public Transform AimTarget;
    public Transform RightHand;
    public Transform Back;

    public GameEvent Weapon1Event;
    public GameEvent Weapon2Event;

    void Start()
    {
        m_Loadout = GetComponentInParent<Loadout>();
        
        m_Loadout.OnAddWeapon += OnWeaponAdded;
        m_Loadout.OnRemoveWeapon += OnWeaponRemoved;

        var playerController = GetComponentInParent<PlayerController>();
        m_Inventory = playerController.Inventory;
        m_RuntimeData = playerController.RuntimeData;
    }

    private void OnDestroy()
    {
        m_Loadout.OnAddWeapon -= OnWeaponAdded;
        m_Loadout.OnRemoveWeapon -= OnWeaponRemoved;
    }

    private void OnEnable()
    {

        Weapon1Event.RegisterListener(SwitchWeapon1);
        Weapon2Event.RegisterListener(SwitchWeapon2);
        EventManager.AddListener<TryReloadEvent>(TryReload2);
    }

    private void OnDisable()
    {
        Weapon1Event.UnregisterListener(SwitchWeapon1);
        Weapon2Event.UnregisterListener(SwitchWeapon2);
        EventManager.RemoveListener<TryReloadEvent>(TryReload2);
    }

    public void Initialize()
    {
        var firearms = m_Loadout.GetAllWeapon();
        int index = -1;
        for (int i = 0; i < firearms.Count; ++i) {
            if (firearms[i] != null) {
                OnWeaponAdded(firearms[i], i);
                if (index == -1) {
                    index = i;
                }
            }
        }
        if (index != -1) {
            TrySwitchFirearm(index);
        }
    }

    // Loadout
    void OnWeaponAdded(IWeapon weapon, int idx)
    {
        weapon.OnFire += () => OnWeaponFire(idx);
    }

    void OnWeaponFire(int idx)
    {
        if (idx != m_CurrentFirearmIndex) {
            return;
        }
        EventManager.Broadcast(new WeaponFiredEvent());
    }

    void OnWeaponRemoved(IWeapon weapon)
    { }


    // IFirearmController
    public override bool ValidActive() => m_Loadout != null && m_Loadout.WeaponCount > 0;

    public override void SetActive(bool isActive)
    {
        m_IsActive = isActive;
        if (m_IsActive) {

        } else {
            if (m_ReloadCoroutine != null) {
                StopCoroutine(m_ReloadCoroutine);
                m_ReloadCoroutine = null;
            }
        }
    }

    public override bool ValidAim()
    {
        return m_CurrentFirearm != null;
    }

    public override void OnAim(bool isAiming)
    {
        if (isAiming) {
            m_CurrentFirearm.SetParent(RightHand);
        } else {
            m_CurrentFirearm.SetParent(Back);
        }
    }

    public override void Attack(bool down)
    {
        if (!m_RuntimeData.IsAiming) {
            return;
        }
        if (down) {
            m_CurrentFirearm?.StartFire(AimTarget);
            RaiseAttack(0, false);
        } else {
            m_CurrentFirearm?.EndFire();
            RaiseAttack(0, true);
        }
    }

    void SwitchWeapon1() => TrySwitchFirearm(1);
    void SwitchWeapon2() => TrySwitchFirearm(2);

    public override void TrySwitchFirearm(int idx)
    {
        int oldIdx = m_CurrentFirearmIndex;
        m_CurrentFirearmIndex = idx;

        if (m_CurrentFirearm != null) {
            m_CurrentFirearm.EndFire();
        }

        m_CurrentFirearm = m_Loadout.GetWeapon(idx);
        if(m_CurrentFirearm == null) {
            //Exit();
            return;
        }

        if (m_ReloadCoroutine != null) {
            StopCoroutine(m_ReloadCoroutine);
            m_ReloadCoroutine = null;
        }
        // Wait?
        EventManager.Broadcast(new WeaponChangedEvent { OldIdx = oldIdx, NewIdx = idx });
    }

    public override void TryChangeFirearmIndex(int value)
    {
        int firearmCount = m_Loadout.WeaponCount;
        if (firearmCount > 0) {
            int newIdx = (
                m_CurrentFirearmIndex - value + firearmCount
            ) % firearmCount;
            if (newIdx != m_CurrentFirearmIndex) {
                TrySwitchFirearm(newIdx);
            }
        }
    }


    // Reload
    public override void TryReload()
    {
        if (m_CurrentFirearmIndex == -1) {
            return;
        }
        if (!m_CurrentFirearm.ValidReload() || m_Inventory.GetAmount(m_CurrentFirearm.AmmoId) <= 0) {
            return;
        }
        int amount = m_CurrentFirearm.CurrentAmmo;
        int ammoId = m_CurrentFirearm.AmmoId;
        m_CurrentFirearm.StartReload();
        PutAmmoIntoInventory(ammoId, amount);
        EventManager.Broadcast(new WeaponStartReloadEvent {
            WeaponIdx = m_CurrentFirearmIndex
        });
        m_ReloadCoroutine = StartCoroutine(ReloadCoroutineFunc(m_CurrentFirearm.ReloadTime));
    }

    void TryReload2(TryReloadEvent evt)
    {
        // FIXME: 界面拖动是有可能让当前未持有的武器换弹的，怎么办，要切枪吗
        TryReload();
    }

    void PutAmmoIntoInventory(int ammoId, int amount)
    {
        if (amount > 0) {
            m_Inventory.AddItem(ammoId, amount);
        }
    }

    int GetLoadAmmo(IWeapon weapon)
    {
        int ammoId = weapon.AmmoId;
        int ammoAmount = m_Inventory.GetAmount(ammoId);
        ammoAmount = Mathf.Min(ammoAmount, weapon.ClipAmmo);
        ammoAmount = m_Inventory.ReduceItemAmount(ammoId, ammoAmount);
        return ammoAmount;
    }
    IEnumerator ReloadCoroutineFunc(float time)
    {
        yield return new WaitForSeconds(time);
        m_CurrentFirearm.EndReload(GetLoadAmmo(m_CurrentFirearm));
        EventManager.Broadcast(new WeaponEndReloadEvent {
            WeaponIdx = m_CurrentFirearmIndex
        });
    }
}
