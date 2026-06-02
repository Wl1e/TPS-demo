using System;
using Unity.VisualScripting;
using UnityEngine;

public enum PlayerMovementState
{
    Idle,
    Walk,
    WalkBackward,
    Jump,
    Sprint,
    Crouch,
    Climb,
    Ledge,
}

public class PlayerController : MonoBehaviour
{
    PlayerMovement m_Movement;
    CharacterController m_CharacterController;
    PlayerStateMachine m_FSM;
    CameraController m_CameraController;
    AimController m_AimController;
    Health m_Health;
    WeaponManager m_WeaponManager;
    Inventory m_Inventory;
    PlayerEconomy m_Economy;
    Actor m_Actor;
    PlayerInputHandler m_InputHandle;
    Loadout m_Loadout;
    ClimbContoller m_ClimbContoller;

    CountDownLatch m_CursorBlock = new CountDownLatch();

    public PlayerMovement Movement => m_Movement;
    public Health Health => m_Health;
    public WeaponManager WeaponManager => m_WeaponManager;
    public Loadout Loadout => m_Loadout;
    public Inventory Inventory => m_Inventory;
    public PlayerEconomy Economy => m_Economy;
    public Actor Actor => m_Actor;
    public ClimbContoller ClimbController => m_ClimbContoller;
    public CharacterController CharacterController => m_CharacterController;
    public PlayerStateMachine StateMachine => m_FSM;

    public Transform CameraRoot;
    public PlayerRuntimeData RuntimeData = new PlayerRuntimeData();

    [SerializeField] private GameEvent OnJumpInput;
    [SerializeField] private GameEvent OnSprintInput;
    [SerializeField] private GameEvent OnCrouchInput;
    [SerializeField] private Vector2Event OnLookInput;
    public Action<string, string> OnStateChanged;

    public int ID => m_Actor.Id;
    private void Awake()
    {
        m_CharacterController = GetComponent<CharacterController>();
        m_Movement = GetComponent<PlayerMovement>();
        m_FSM = GetComponent<PlayerStateMachine>();
        m_CameraController = GetComponent<CameraController>();
        m_Health = GetComponent<Health>();
        m_Inventory = GetComponent<Inventory>();
        m_Economy = GetComponent<PlayerEconomy>();
        m_Actor = GetComponent<Actor>();
        m_InputHandle = GetComponent<PlayerInputHandler>();
        m_ClimbContoller = GetComponentInChildren<ClimbContoller>();

        // Combat
        m_AimController = GetComponentInChildren<AimController>();
        m_WeaponManager = GetComponentInChildren<WeaponManager>();
        m_Loadout = GetComponentInChildren<Loadout>();

        gameObject.tag = "Player";

        RuntimeData.IsAiming = m_AimController.IsAiming;
        RuntimeData.CameraRoot = CameraRoot;
        RuntimeData.State = PlayerMovementState.Idle;
    }

    void Start()
    {
        m_FSM.OnStateChanged += (string pre, string cur) => OnStateChanged?.Invoke(pre, cur);
        m_FSM.InitializeFSM();
    }

    public void Initialize()
    {
        m_WeaponManager.Initialize();
        print($"Firearm: {m_WeaponManager.CurrentFirearm}");
    }

    private void OnEnable()
    {
        OnJumpInput.RegisterListener(OnJump);
        OnSprintInput.RegisterListener(OnSprint);
        OnCrouchInput.RegisterListener(OnCrouch);
        //OnLookInput.RegisterListener(OnLook);
    }

    private void OnDisable()
    {
        
        OnJumpInput.UnregisterListener(OnJump);
        OnSprintInput.UnregisterListener(OnSprint);
        OnCrouchInput.UnregisterListener(OnCrouch);
        //OnLookInput.UnregisterListener(OnLook);
    }

    void OnJump()
    {
        m_FSM.WantJump = true;
    }

    void OnSprint()
    {
        m_FSM.WantSprint = !m_FSM.WantSprint;
    }

    void OnCrouch()
    {
        m_FSM.WantCrouch = !m_FSM.WantCrouch;
    }

    public void SetInputActive(bool active, bool activeCursor)
    {
        if(!activeCursor) {
            m_CursorBlock.Increase();
        } else {
            m_CursorBlock.Decrease();
        }
        if (Cursor.lockState == CursorLockMode.Locked && m_CursorBlock.IsLockd()) {
            Cursor.lockState = CursorLockMode.None;
        } else if(Cursor.lockState == CursorLockMode.None && !m_CursorBlock.IsLockd()) {
            Cursor.lockState = CursorLockMode.Locked;
        }
        if (active) {
            m_Movement.MovementLock.Decrease();
        } else {
            m_Movement.MovementLock.Increase();
        }
        if(m_CursorBlock.IsLockd()) {
            m_FSM.ChangeState("Idle");
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        m_ClimbContoller.OnPlayerCollision(hit);
    }
}
