using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;


public class PlayerInputHandler : NetworkBehaviour
{
    CountDownLatch m_InputBlock = new CountDownLatch();
    bool m_OpenInventory = false;
    [SerializeField] private Vector2Event m_MoveInput;
    [SerializeField] private GameEvent m_JumpInput;
    [SerializeField] private GameEvent m_SprintInput;
    [SerializeField] private GameEvent m_CrouchInput;
    [SerializeField] private Vector2Event m_LookInput;
    [SerializeField] private IntEvent m_NumberInput;
    [SerializeField] private IntEvent m_ScrollInput;
    [SerializeField] private BoolEvent m_FireInput;
    [SerializeField] private BoolEvent m_AimInput;
    [SerializeField] private GameEvent m_ReloadInput;
    [SerializeField] private GameEvent m_InteractionInput;
    [SerializeField] private GameEvent m_InventoryInput;
    [SerializeField] private GameEvent m_GrenadeInput;
    [SerializeField] private GameEvent m_Weapon1Input;
    [SerializeField] private GameEvent m_Weapon2Input;
    [SerializeField] private BoolEvent m_ActiveCursorInput;
    [SerializeField] private GameEvent m_QuestPanelInput;

    public override void OnNetworkSpawn()
    {
        if (IsOwner) {
            RegisterInputAction();
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    public override void OnNetworkDespawn()
    {
        if (IsOwner) {
            UnregisterInputAction();
        }
    }

    private void RegisterInputAction()
    {
        InputActionMap map = InputSystem.actions.FindActionMap("Player");
        map.FindAction("Move").performed += OnMove;
        map.FindAction("Move").canceled += OnMove;
        map.FindAction("Jump").performed += OnJump;
        map.FindAction("Sprint").performed += OnSprint;
        map.FindAction("Crouch").performed += OnCrouch;
        map.FindAction("Look").performed += OnLook;
        map.FindAction("Scroll").performed += OnScroll;
        map.FindAction("Aim").performed += OnAimPressed;
        map.FindAction("Aim").canceled += OnAimReleased;
        map.FindAction("Attack").performed += OnFire;
        map.FindAction("Attack").canceled += OnFire;
        map.FindAction("Reload").performed += OnReload;
        map.FindAction("Interaction").performed += OnInteraction;
        map.FindAction("Inventory").performed += OnInventory;
        map.FindAction("Grenade").performed += OnGrenade;
        map.FindAction("Weapon1").performed += OnWeapon1;
        map.FindAction("Weapon2").performed += OnWeapon2;
        map.FindAction("ActiveCursor").performed += OnActiveCursor;
        map.FindAction("Quest").performed += OnQuest;
    }

    private void UnregisterInputAction()
    {
        InputActionMap map = InputSystem.actions.FindActionMap("Player");
        map.FindAction("Move").performed -= OnMove;
        map.FindAction("Move").canceled -= OnMove;
        map.FindAction("Jump").performed -= OnJump;
        map.FindAction("Sprint").performed -= OnSprint;
        map.FindAction("Crouch").performed -= OnCrouch;
        map.FindAction("Look").performed -= OnLook;
        map.FindAction("Aim").performed -= OnAimPressed;
        map.FindAction("Aim").canceled -= OnAimReleased;
        map.FindAction("Attack").performed -= OnFire;
        map.FindAction("Attack").canceled -= OnFire;
        map.FindAction("Reload").performed -= OnReload;
        map.FindAction("Interaction").performed -= OnInteraction;
        map.FindAction("Inventory").performed -= OnInventory;
        map.FindAction("Grenade").performed -= OnGrenade;
        map.FindAction("Weapon1").performed -= OnWeapon1;
        map.FindAction("Weapon2").performed -= OnWeapon2;
        map.FindAction("ActiveCursor").performed -= OnActiveCursor;
        map.FindAction("Quest").performed -= OnQuest;
    }

    private bool ValidPlayerInput()
    {
        return !m_InputBlock.IsLockd() && !m_OpenInventory;
    }

    public void SetActive(bool active)
    {
        if(active) {
            m_InputBlock.Decrease();
        } else {
            m_InputBlock.Increase();
        }
    }

    #region
    private void OnMove(InputAction.CallbackContext ctx)
    {
        if(!ValidPlayerInput()) {
            return;
        }
        Vector2 moveValue = ctx.ReadValue<Vector2>();
        m_MoveInput.Raise(moveValue);
    }
    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (!ValidPlayerInput()) {
            return;
        }
        m_JumpInput.Raise();
    }
    private void OnSprint(InputAction.CallbackContext ctx)
    {
        if (!ValidPlayerInput()) {
            return;
        }
        m_SprintInput.Raise();
    }
    private void OnCrouch(InputAction.CallbackContext ctx)
    {
        if (!ValidPlayerInput()) {
            return;
        }
        m_CrouchInput.Raise();
    }
    private void OnLook(InputAction.CallbackContext ctx)
    {
        if (!ValidPlayerInput()) {
            return;
        }
        Vector2 lookValue = ctx.ReadValue<Vector2>();
        m_LookInput.Raise(lookValue);
    }

    private void OnScroll(InputAction.CallbackContext ctx)
    {
        if (!ValidPlayerInput()) {
            return;
        }
        m_ScrollInput.Raise(Mathf.FloorToInt(ctx.ReadValue<Vector2>().y));
    }
    private void OnFire(InputAction.CallbackContext ctx)
    {
        if (!ValidPlayerInput()) {
            return;
        }
        m_FireInput.Raise(ctx.ReadValueAsButton());
    }
    private void OnAimPressed(InputAction.CallbackContext ctx)
    {
        if (!ValidPlayerInput()) {
            return;
        }
        m_AimInput.Raise(true);
    }
    private void OnAimReleased(InputAction.CallbackContext ctx)
    {
        if (!ValidPlayerInput()) {
            return;
        }
        m_AimInput.Raise(false);
    }
    private void OnReload(InputAction.CallbackContext ctx)
    {
        if (!ValidPlayerInput()) {
            return;
        }
        m_ReloadInput.Raise();
    }
    private void OnInteraction(InputAction.CallbackContext ctx)
    {
        if (!ValidPlayerInput()) {
            return;
        }
        m_InteractionInput.Raise();
    }
    private void OnInventory(InputAction.CallbackContext ctx)
    {
        m_OpenInventory = !m_OpenInventory;
        m_InventoryInput.Raise();
    }
    private void OnGrenade(InputAction.CallbackContext ctx)
    {
        if (!ValidPlayerInput()) {
            return;
        }
        m_GrenadeInput.Raise();
    }

    private void OnWeapon1(InputAction.CallbackContext ctx)
    {
        if (!ValidPlayerInput()) {
            return;
        }
        m_Weapon1Input.Raise();
    }
    private void OnWeapon2(InputAction.CallbackContext ctx)
    {
        if (!ValidPlayerInput()) {
            return;
        }
        m_Weapon2Input.Raise();
    }

    private void OnActiveCursor(InputAction.CallbackContext ctx)
    {
        if (!ValidPlayerInput()) {
            return;
        }
        m_ActiveCursorInput.Raise(ctx.ReadValueAsButton());
    }

    private void OnQuest(InputAction.CallbackContext obj)
    {
        m_QuestPanelInput.Raise();
    }
    #endregion
}
