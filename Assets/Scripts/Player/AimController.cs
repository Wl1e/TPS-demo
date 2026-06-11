using UnityEngine;


namespace TPSDemo
{
using Event;
    using Unity.Netcode;

    [RequireComponent(typeof(WeaponManager))]
    public class AimController : NetworkBehaviour
    {
        // Camera
        Camera m_Camera;

        // AimPoint
        [Tooltip("看向目标的Transform")]
        public Transform VisualAimPointTransform;
        [Tooltip("目标可以碰撞的层级")]
        public LayerMask AimRayCastLayerMask = ~0;
        [Tooltip("目标最远距离")]
        public float AimRayDistance = 200f;
        Vector3 m_VisualAimPosition;

        // Events
        public BoolEvent OnTryAimEvent;

        // Weapon
        CombatController m_CombatController;
        WeaponManager m_WeaponManager;
        /// <summary>
        /// 后坐力前往速度
        /// </summary>
        public float RecoilKickSpeed = 50f;
        /// <summary>
        /// 后坐力恢复速度
        /// </summary>
        public float RecoilReturnSpeed => m_WeaponManager.CurrentFirearm?.RecoilReturnSpeed ?? 0;
        Vector3 m_CurrentRecoilOffset;
        Vector3 m_TargetRecoilOffset;

        public IWeapon CurrentWeapon = null;

        /// <summary>
        /// 瞄准状态
        /// </summary>
        public bool IsAiming { get; private set; } = false;

        PlayerController m_PlayerController;
        PlayerRuntimeData m_RuntimeData;
        CameraController m_CameraController;

        void Start()
        {
            m_PlayerController = GetComponentInParent<PlayerController>();
            m_CameraController = m_PlayerController.CameraController;
            m_RuntimeData = m_PlayerController.RuntimeData;
            m_CombatController = GetComponent<CombatController>();
            m_WeaponManager = GetComponent<WeaponManager>();

            m_Camera = Camera.main;
        }

        private void OnEnable()
        {
            OnTryAimEvent.RegisterListener(HandleTryAim);
            EventManager.AddListener<WeaponChangedEvent>(HandleChangeWeapon);
            EventManager.AddListener<WeaponFiredEvent>(OnFired);
        }

        private void OnDisable()
        {
            OnTryAimEvent.UnregisterListener(HandleTryAim);
            EventManager.RemoveListener<WeaponChangedEvent>(HandleChangeWeapon);
            EventManager.RemoveListener<WeaponFiredEvent>(OnFired);
        }

        void Update()
        {
            if (IsOwner && IsAiming) {
                UpdateAimPositon();
            } else {

            }
        }

        void UpdateAimPositon()
        {
            if (!m_Camera) {
                // 玩家还没生成（相机未就绪）或相机被销毁时，确保网络同步有值可用，而不是零向量
                return;
            }

            Ray ray = m_Camera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
            Vector3 targetPosition;
            float distance = 0f;
            if (Physics.Raycast(ray, out RaycastHit hit, AimRayDistance, AimRayCastLayerMask)) {
                targetPosition = hit.point;
                distance = hit.distance;
            } else {
                targetPosition = ray.origin + ray.direction * AimRayDistance;
                distance = AimRayDistance;
            }
            // TODO 如果距离过近，会有IK动画问题，引入visualAimBlendDistance
            m_VisualAimPosition = targetPosition;
            VisualAimPointTransform.position = m_VisualAimPosition;
        }

        bool ValidAim()
        {
            return m_CombatController.ValidAim();
        }

        void HandleTryAim(bool tryAim)
        {
            if (tryAim && !ValidAim()) {
                return;
            } else if (!tryAim && !IsAiming) {
                return;
            }
            IsAiming = tryAim;
            if (!IsAiming) {
                m_CurrentRecoilOffset = Vector3.zero;
                m_TargetRecoilOffset = Vector3.zero;
            }
            m_RuntimeData.IsAiming = IsAiming;
            m_RuntimeData.AniParameter.IsAim = IsAiming;
            if(IsAiming) {
                VisualAimPointTransform.gameObject.SetActive(true);
            } else {
                VisualAimPointTransform.gameObject.SetActive(false);
            }
            m_PlayerController.AnimatorController.SetAimWeight(IsAiming);
            EventManager.Broadcast(new AimEvent { IsAiming = IsAiming });
        }

        void HandleChangeWeapon(WeaponChangedEvent evt)
        {
            CurrentWeapon = m_WeaponManager.CurrentFirearm;
        }

        void ApplyRecoil()
        {
            if (CurrentWeapon is null) {
                return;
            }
            float Frequency = CurrentWeapon.RecoilFrequency;
            float Force = CurrentWeapon.RecoilForce;
            // TODO 后续可以将后坐力交给Weapon自己提供
            Vector2 recoil = new Vector2(
                Mathf.Sin(Time.time * Frequency) * Force,
                -(Mathf.Sin(Time.time * Frequency * 2f) * 0.5f + 0.5f) * Force
            );
            m_CameraController.AddRecoil(recoil);
        }

        void OnFired(WeaponFiredEvent evt)
        {
            ApplyRecoil();
        }
    }
}
