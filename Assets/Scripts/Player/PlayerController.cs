using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{
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

    public class PlayerController : NetworkBehaviour
    {
        #region component

        private PlayerMovement m_Movement;
        private CharacterController m_CharacterController;
        private PlayerStateMachine m_FSM;
        private CameraController m_CameraController;
        private AimController m_AimController;
        private Health m_Health;
        private WeaponManager m_WeaponManager;
        private Inventory m_Inventory;
        private readonly PlayerEconomy m_Economy = new();
        private Actor m_Actor;
        private PlayerInputHandler m_InputHandler;
        private Loadout m_Loadout;
        private ClimbContoller m_ClimbContoller;
        private CombatController m_CombatController;
        private AnimatorController m_AnimatorController;
        private QuestController m_QuestController;
        private InteractionController m_InteractionController;
        private PlayerLifeController m_LifeController;
        private AudioAndEffectPlayGlobal m_AudioEffectPlayer;


        #endregion component

        #region Property

        /// <summary>
        /// 移动控制
        /// </summary>
        public PlayerMovement Movement => m_Movement;

        /// <summary>
        /// 生命
        /// </summary>
        public Health Health => m_Health;

        /// <summary>
        /// 相机控制
        /// </summary>
        public CameraController CameraController => m_CameraController;

        /// <summary>
        /// 枪械控制
        /// </summary>
        public WeaponManager WeaponManager => m_WeaponManager;

        /// <summary>
        /// 装备
        /// </summary>
        public Loadout Loadout => m_Loadout;

        /// <summary>
        /// 战斗控制
        /// </summary>
        public CombatController CombatController => m_CombatController;

        /// <summary>
        /// 玩家仓库
        /// </summary>
        public Inventory Inventory => m_Inventory;

        /// <summary>
        /// 经济系统
        /// </summary>
        public PlayerEconomy Economy => m_Economy;

        /// <summary>
        /// Actor基类
        /// </summary>
        public Actor Actor => m_Actor;

        /// <summary>
        /// 攀爬控制
        /// </summary>
        public ClimbContoller ClimbController => m_ClimbContoller;

        /// <summary>
        /// 动画控制
        /// </summary>
        public AnimatorController AnimatorController => m_AnimatorController;

        public CharacterController CharacterController => m_CharacterController;

        /// <summary>
        /// 状态机
        /// </summary>
        public PlayerStateMachine StateMachine => m_FSM;

        /// <summary>
        /// 任务控制
        /// </summary>
        public QuestController QuestController => m_QuestController;

        /// <summary>
        /// 相机根节点
        /// </summary>
        public Transform CameraRoot;

        /// <summary>
        /// 玩家运行时数据
        /// </summary>
        public PlayerRuntimeData RuntimeData = new();

        /// <summary>
        /// 交互控制
        /// </summary>
        public InteractionController InteractionController => m_InteractionController;

        /// <summary>
        /// 音效动画播放rpc
        /// </summary>
        public PlayerLifeController LifeController => m_LifeController;
        public AudioAndEffectPlayGlobal AudioEffectPlayer => m_AudioEffectPlayer;

        #endregion Property

        public System.Collections.Generic.List<Vector2Int> Money;

        [SerializeField] private GameEvent OnJumpInput;
        [SerializeField] private GameEvent OnSprintInput;
        [SerializeField] private GameEvent OnCrouchInput;
        [SerializeField] private Vector2Event OnLookInput;
        [SerializeField] private BoolEvent OnActiveCursorInput;

        public AudioClip m_MovementAudio;

        public int Id => m_Actor.Id;

        private void Awake()
        {
            m_CharacterController = GetComponent<CharacterController>();
            m_Movement = GetComponent<PlayerMovement>();
            m_FSM = GetComponent<PlayerStateMachine>();
            m_CameraController = GetComponent<CameraController>();
            m_Health = GetComponent<Health>();
            m_Inventory = GetComponent<Inventory>();
            m_Actor = GetComponent<Actor>();
            m_InputHandler = GetComponent<PlayerInputHandler>();
            m_ClimbContoller = GetComponentInChildren<ClimbContoller>();
            m_AnimatorController = GetComponent<AnimatorController>();
            m_QuestController = GetComponent<QuestController>();
            m_InteractionController = GetComponent<InteractionController>();
            m_LifeController = GetComponent<PlayerLifeController>();
            m_AudioEffectPlayer = GetComponent<AudioAndEffectPlayGlobal>();

            // Combat
            m_AimController = GetComponentInChildren<AimController>();
            m_WeaponManager = GetComponentInChildren<WeaponManager>();
            m_Loadout = GetComponentInChildren<Loadout>();
            m_CombatController = GetComponentInChildren<CombatController>();

            gameObject.tag = "Player";

            RuntimeData.IsAiming = m_AimController.IsAiming;
            RuntimeData.CameraRoot = CameraRoot;
            RuntimeData.State = PlayerMovementState.Idle;

            foreach (var e in Money) {
                m_Economy.AddMoney(e.x, e.y);
            }
        }

        private void DisableClientComponents()
        {
            if (m_Movement != null) {
                m_Movement.enabled = false;
            }
            if (m_FSM != null) {
                m_FSM.enabled = false;
            }
            if (m_AimController != null) {
                m_AimController.enabled = false;
            }
            if (m_CameraController != null) {
                m_CameraController.enabled = false;
            }
            if (m_InteractionController != null) {
                m_InteractionController.enabled = false;
            }
            if (m_InputHandler) {
                m_InputHandler.enabled = false;
            }
            if (m_CharacterController) {
                m_CharacterController.enabled = false;
            }
            if(TryGetComponent<AudioListener>(out var listener)) {
                listener.enabled = false;
            }

        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsOwner) {
                m_CharacterController.enabled = false;
                PlayerDataProxy.Instance.RegisterPlayer(this);
                m_FSM.InitializeFSM();
                RegisterEvents();
                m_Health.OnTakeDamaged += OnPlayerTakeDamage;
                m_Health.OnDied += OnDied;
            } else {
                DisableClientComponents();
            }
            if(IsServer) {
                GameModeManager.Instance.RegisterPlayer(this);
                StartCoroutine(PlayerInitializeCoroutine());
            }
        }
        public override void OnNetworkDespawn()
        {
            if (IsOwner) {
                UnregisterEvents();
                PlayerDataProxy.Instance.UnregisterPlayer();
            }
            base.OnNetworkDespawn();
        }

        #region Initialize

        // 初始化需要在Server端运行的东西
        private IEnumerator PlayerInitializeCoroutine()
        {
            yield return m_Loadout.EquipWeaponCo(m_Loadout.DefaultWeapon);
            BootTel.TeleportToHub();
            InitializeFinishClientRpc();
        }

        [ClientRpc]
        private void InitializeFinishClientRpc()
        {
            // 通知UI和DebugLayer
            if (IsOwner) {
                m_CharacterController.enabled = true;
                EventManager.Broadcast(new Event.PlayerFinishedInitialzeEvent());
                print($"Player {Id} Spawn");
            }
        }

        private void RegisterEvents()
        {
            OnJumpInput.RegisterListener(OnJump);
            OnSprintInput.RegisterListener(OnSprint);
            OnCrouchInput.RegisterListener(OnCrouch);
            OnActiveCursorInput.RegisterListener(OnActiveCursor);
            //OnLookInput.RegisterListener(OnLook);
        }

        private void UnregisterEvents()
        {
            OnJumpInput.UnregisterListener(OnJump);
            OnSprintInput.UnregisterListener(OnSprint);
            OnCrouchInput.UnregisterListener(OnCrouch);
            OnActiveCursorInput.UnregisterListener(OnActiveCursor);
            //OnLookInput.UnregisterListener(OnLook);
        }

        #endregion Initialize

        #region Callback

        private void OnPlayerTakeDamage(DamageInfo info)
        {
            RuntimeData.AniParameter.TakeDamage = true;
        }

        private void OnJump()
        {
            m_FSM.WantJump = true;
        }

        private void OnSprint()
        {
            m_FSM.WantSprint = !m_FSM.WantSprint;
        }

        private void OnCrouch()
        {
            m_FSM.WantCrouch = !m_FSM.WantCrouch;
        }

        private void OnActiveCursor(bool active)
        {
            Cursor.lockState = CursorLockMode.None;
        }

        private void OnDied(int attackerId)
        {
            if (m_LifeController != null) {
                m_LifeController.HandleDeathLocally();
            }

            DiedServerRpc(attackerId);
        }

        #endregion Callback

        [ServerRpc]
        private void DiedServerRpc(int attackerId)
        {
            EventManager.Broadcast(new Event.ActorDiedEvent { ActorId = Id, AttackerId = attackerId });
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            m_ClimbContoller.OnPlayerCollision(hit);
        }
    }
}
