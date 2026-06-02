using System;
using System.Collections;
using UnityEngine;

public class ThrowController: CombatSlot
{
    bool m_IsActive = false;
    public override bool IsActive => m_IsActive;

    // TrajectoryLine
    public LineRenderer TrajectoryLine;
    TrajectoryLine m_TrajectoryLine = new TrajectoryLine();
    bool m_ShowTrajectoryLine = false;
    public float HorizonalAngle = 30f;

    [SerializeField] PlayerController m_Owner;
    Inventory m_Inventory;
    PlayerRuntimeData m_PlayerRuntimeData;
    GameObject m_CurrentGrenadeObj;
    public IGrenade CurrentGrenade { get; private set; }

    [SerializeField] Transform GrenadeRoot;
    [SerializeField] Vector3 m_GrenadeRootOffset = new Vector3(0, 0.12f, 0.07f);
    [SerializeField] Vector3 m_GrenadeRootRotate = new Vector3(180f, 90f, 90f);
    [SerializeField] float m_ThrowPosOffset = 0.6f;

    private void Start()
    {
        m_Owner = GetComponentInParent<PlayerController>();
        m_Inventory = m_Owner.Inventory;
        m_PlayerRuntimeData = m_Owner.RuntimeData;
        m_TrajectoryLine.Line = TrajectoryLine;
        m_TrajectoryLine.CollisionMask = -1;
    }

    private void Update()
    {
        if(m_ShowTrajectoryLine) {
            UpdateTrajectory(m_CurrentGrenadeObj.transform.position, m_PlayerRuntimeData.CameraRoot.rotation);
        }
    }

    public override bool ValidActive()
    {
        return m_Inventory != null && m_Inventory.GetItems(ItemType.Grenade).Count > 0;
    }

    public override void SetActive(bool isActive)
    {
        if (isActive && !ValidActive()) {
            return;
        }
        m_IsActive = isActive;
        if (m_IsActive) {
            EquipGrenade();
        } else {
            UnequipGrenade();
        }
    }

    public void EquipGrenade()
    {
        var grenades = m_Inventory.GetItems(ItemType.Grenade);
        if (grenades.Count < 0) {
            return;
        }
        var grenade = grenades[0];
        m_CurrentGrenadeObj = Instantiate(ItemDataList.GetItemData(grenade.itemId).Prefab, GrenadeRoot, false);
        m_CurrentGrenadeObj.transform.localPosition = m_GrenadeRootOffset;
        m_CurrentGrenadeObj.transform.rotation = Quaternion.Euler(m_GrenadeRootRotate);

        CurrentGrenade = m_CurrentGrenadeObj.GetComponent<IGrenade>();

        // Ignore Collsion
        CurrentGrenade.Rigidbody.isKinematic = true;

        CurrentGrenade.OnHold();
        m_TrajectoryLine.CollisionMask = m_CurrentGrenadeObj.layer;
    }

    Vector3 GetTrajectoryStartDir(Quaternion dir)
    {
        return Vector3.Normalize((dir * Vector3.forward) + new Vector3(0f, HorizonalAngle / 90f, 0));
    }

    public void SetTrajectoryVisible(bool visible)
    {
        m_ShowTrajectoryLine = visible;
        if (!m_ShowTrajectoryLine) {
            m_TrajectoryLine.Hide();
        }
    }
    public void UpdateTrajectory(Vector3 position, Quaternion rotation)
    {
        if (m_CurrentGrenadeObj) {
            m_TrajectoryLine.UpdateTrajectory(
                position, GetTrajectoryStartDir(rotation) * CurrentGrenade.Speed
            );
        }
    }

    public void UnequipGrenade()
    {
        if (m_CurrentGrenadeObj) {
            CurrentGrenade.OnStore();
            Destroy(m_CurrentGrenadeObj);
            CurrentGrenade = null;
            m_CurrentGrenadeObj = null;
        }
    }

    public override void Attack(bool isEnd)
    {
        if(!m_CurrentGrenadeObj || CurrentGrenade == null) {
            return;
        }
        if (isEnd) {
            m_CurrentGrenadeObj.transform.SetParent(null);

            // 把手雷往前推一定距离，防止和玩家碰撞
            m_CurrentGrenadeObj.transform.position = m_PlayerRuntimeData.CameraRoot.position + transform.forward * m_ThrowPosOffset;

            CurrentGrenade.Rigidbody.isKinematic = false;

            CurrentGrenade.Throw(
                m_Owner.Actor.Id,
                GetTrajectoryStartDir(m_PlayerRuntimeData.CameraRoot.rotation)
            );

            m_CurrentGrenadeObj = null;
            CurrentGrenade = null;
            m_TrajectoryLine.Hide();

            if(ValidActive()) {
                EquipGrenade();
            } else {
                SetTrajectoryVisible(false);
                m_TrajectoryLine.Hide();
                Exit();
            }
        }
    }

    public override bool ValidAim()
    {
        return CurrentGrenade != null;
    }

    public override void OnAim(bool isAiming)
    {
        SetTrajectoryVisible(isAiming);
    }
}

