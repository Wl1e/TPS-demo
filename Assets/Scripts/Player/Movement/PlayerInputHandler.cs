using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;


public class PlayerInputHandler : NetworkBehaviour
{
    CountDownLatch m_InputBlock = new CountDownLatch();
    bool m_OpenInventory = false;
    [SerializeField] private Vector2Event m_OnMoveInput;
    [SerializeField] private GameEvent m_OnJumpInput;
    [SerializeField] private GameEvent m_OnSprintInput;
    [SerializeField] private GameEvent m_OnCrouchInput;
    [SerializeField] private Vector2Event m_OnLookInput;
    [SerializeField] private IntEvent m_OnNumberInput;
    [SerializeField] private IntEvent m_OnScrollInput;
    [SerializeField] private BoolEvent m_OnFireInput;
    [SerializeField] private BoolEvent m_OnAimInput;
    [SerializeField] private GameEvent m_OnReloadInput;
    [SerializeField] private GameEvent m_OnInteractionInput;
    [SerializeField] private GameEvent m_OnInventoryInput;
    [SerializeField] private GameEvent m_OnGrenadeInput;
    [SerializeField] private GameEvent m_OnWeapon1Input;
    [SerializeField] private GameEvent m_OnWeapon2Input;
    [SerializeField] private BoolEvent m_OnActiveCursorInput;

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

    void RegisterInputAction()
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
        map.FindAction("Fire").performed += OnFire;
        map.FindAction("Fire").canceled += OnFire;
        map.FindAction("Reload").performed += OnReload;
        map.FindAction("Interaction").performed += OnInteraction;
        map.FindAction("Inventory").performed += OnInventory;
        map.FindAction("Grenade").performed += OnGrenade;
        map.FindAction("Weapon1").performed += OnWeapon1;
        map.FindAction("Weapon2").performed += OnWeapon2;
        map.FindAction("ActiveCursor").performed += OnActiveCursor;
    }

    void UnregisterInputAction()
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
        map.FindAction("Fire").performed -= OnFire;
        map.FindAction("Fire").canceled -= OnFire;
        map.FindAction("Reload").performed -= OnReload;
        map.FindAction("Interaction").performed -= OnInteraction;
        map.FindAction("Inventory").performed -= OnInventory;
        map.FindAction("Grenade").performed -= OnGrenade;
        map.FindAction("Weapon1").performed -= OnWeapon1;
        map.FindAction("Weapon2").performed -= OnWeapon2;
        map.FindAction("ActiveCursor").performed -= OnActiveCursor;
    }

    bool ValidPlayerInput()
    {
        return Cursor.lockState == CursorLockMode.Locked && !m_InputBlock.IsLockd() && !m_OpenInventory;
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
    void OnMove(InputAction.CallbackContext ctx)
    {
        if(!ValidPlayerInput()) {
            return;
        }
        Vector2 moveValue = ctx.ReadValue<Vector2>();
        m_OnMoveInput.Raise(moveValue);
    }
    void OnJump(InputAction.CallbackContext ctx)
    {
        if (!ValidPlayerInput()) {
            return;
        }
        m_OnJumpInput.Raise();
    }
    void OnSprint(InputAction.CallbackContext ctx)
    {
        if (!ValidPlayerInput()) {
            return;
        }
        m_OnSprintInput.Raise();
    }
    void OnCrouch(InputAction.CallbackContext ctx)
    {
        if (!ValidPlayerInput()) {
            return;
        }
        m_OnCrouchInput.Raise();
    }
    void OnLook(InputAction.CallbackContext ctx)
    {
        if (!ValidPlayerInput()) {
            return;
        }
        Vector2 lookValue = ctx.ReadValue<Vector2>();
        m_OnLookInput.Raise(lookValue);
    }

    void OnScroll(InputAction.CallbackContext ctx)
    {
        if (!ValidPlayerInput()) {
            return;
        }
        m_OnScrollInput.Raise(Mathf.FloorToInt(ctx.ReadValue<Vector2>().y));
    }
    void OnFire(InputAction.CallbackContext ctx)
    {
        if (!ValidPlayerInput()) {
            return;
        }
        m_OnFireInput.Raise(ctx.ReadValueAsButton());
    }
    void OnAimPressed(InputAction.CallbackContext ctx)
    {
        if (!ValidPlayerInput()) {
            return;
        }
        m_OnAimInput.Raise(true);
    }
    void OnAimReleased(InputAction.CallbackContext ctx)
    {
        if (!ValidPlayerInput()) {
            return;
        }
        m_OnAimInput.Raise(false);
    }
    void OnReload(InputAction.CallbackContext ctx)
    {
        if (!ValidPlayerInput()) {
            return;
        }
        m_OnReloadInput.Raise();
    }
    void OnInteraction(InputAction.CallbackContext ctx)
    {
        if (!ValidPlayerInput()) {
            return;
        }
        m_OnInteractionInput.Raise();
    }
    void OnInventory(InputAction.CallbackContext ctx)
    {
        m_OpenInventory = !m_OpenInventory;
        m_OnInventoryInput.Raise();
    }
    void OnGrenade(InputAction.CallbackContext ctx)
    {
        if (!ValidPlayerInput()) {
            return;
        }
        m_OnGrenadeInput.Raise();
    }

    void OnWeapon1(InputAction.CallbackContext ctx)
    {
        if (!ValidPlayerInput()) {
            return;
        }
        m_OnWeapon1Input.Raise();
    }
    void OnWeapon2(InputAction.CallbackContext ctx)
    {
        if (!ValidPlayerInput()) {
            return;
        }
        m_OnWeapon2Input.Raise();
    }

    void OnActiveCursor(InputAction.CallbackContext ctx)
    {
        if (!ValidPlayerInput()) {
            return;
        }
        m_OnActiveCursorInput.Raise(ctx.ReadValueAsButton());
    }
    #endregion
}
