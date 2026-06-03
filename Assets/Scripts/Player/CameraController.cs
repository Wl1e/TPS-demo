
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace TPSDemo
{
    using Event;

    public class CameraController : MonoBehaviour
    {
        [Tooltip(tooltip: "灵敏度")]
        public float Sensitivity;
        [Tooltip(tooltip: "相机根节点")]
        [SerializeField] Transform m_CameraRoot;

        // Cinemachine
        [Tooltip(tooltip: "Cinemachine列表")]
        [SerializeField] List<CameraMode> m_Modes = new List<CameraMode>();
        List<CameraMode> m_InitializedCameraModes = new List<CameraMode>();
        public string DefaultMode;
        public CameraMode CurrentMode { get; private set; }
        [Tooltip(tooltip: "俯仰最大角度")]
        public float VerticalLookLimit = 70f;
        public bool EnableLook;
        float m_HorizontalAngle;
        float m_VerticalAngle;

        public PlayerMovement.CouplingMode PlayerCouplingMode { get; private set; }
        public float HorizontalAngle => m_HorizontalAngle;
        PlayerController m_PlayerController;
        PlayerRuntimeData m_PlayerRuntimeData;

        Camera m_Camera;
        CinemachineBrain m_CameraBrain;

        // Event
        public Vector2Event LookEvent;

        // FPP TPP
        [Tooltip(tooltip: "瞄准时视角")]
        public Define.ViewPerspective AimType;

        void Awake()
        {
            foreach (var mode in m_Modes) {
                var instance = Instantiate(mode);
                m_InitializedCameraModes.Add(instance);
                instance.SetTarget(m_CameraRoot);
            }
        }
        private void Start()
        {
            m_Camera = Camera.main;
            m_Camera.TryGetComponent(out m_CameraBrain);
            if (!m_CameraBrain) {
                Debug.LogError("MainCamera dont have CinemachineBrain component");
            }
            m_PlayerController = GetComponent<PlayerController>();
            m_PlayerRuntimeData = m_PlayerController.RuntimeData;
            SwitchCameraMode(DefaultMode);
        }

        private void OnEnable()
        {
            EventManager.AddListener<AimEvent>(OnAim);
            LookEvent.RegisterListener(OnLookInput);
        }
        void OnDisable()
        {
            EventManager.RemoveListener<AimEvent>(OnAim);
            LookEvent.UnregisterListener(OnLookInput);
        }

        // Update is called once per frame
        void LateUpdate()
        {
            if (EnableLook) {
                Quaternion horizontalRotation = Quaternion.Euler(0, m_HorizontalAngle, 0);
                Quaternion verticalRotation = Quaternion.Euler(m_VerticalAngle, 0, 0);
                m_CameraRoot.rotation = horizontalRotation * verticalRotation;
            }
        }
        public void SwitchCameraMode(string modeName)
        {
            var mode = m_InitializedCameraModes.Find(m => m.ModeName == modeName);
            if (mode == null) {
                print($"{modeName} not found");
                return;
            }
            CurrentMode = mode;
            PlayerCouplingMode = CurrentMode.CouplingMode;
            foreach (CameraMode m in m_InitializedCameraModes) {
                m.SetActive(m == CurrentMode);
            }
            m_PlayerController.Movement.RotationType = PlayerCouplingMode;
        }

        void OnLookInput(Vector2 lookInput)
        {
            if (!EnableLook) {
                return;
            }
            m_HorizontalAngle += lookInput.x * Sensitivity;
            m_VerticalAngle = Mathf.Clamp(m_VerticalAngle + lookInput.y * Sensitivity, -VerticalLookLimit, VerticalLookLimit);
        }

        CameraMode GetCamera(string cameraName)
        {
            int idx = m_InitializedCameraModes.FindIndex(cam => cam.ModeName == cameraName);
            return idx == -1 ? null : m_InitializedCameraModes[idx];
        }

        float GetAimFOV()
        {
            var weaponManager = GetComponentInChildren<WeaponManager>();
            var scope = weaponManager.CurrentFirearm.GetScopeRatio();
            switch (scope) {
                case 1f:
                    return 60f;
                case 2f:
                    return 40f;
                default:
                    return 1f;
            }
        }

        void OnAim(AimEvent evt)
        {
            var type = AimType;
            var modeName = "FreeLook";
            if (evt.IsAiming) {
                if (type == Define.ViewPerspective.FPP) {
                    modeName = "FPP";
                } else if (type == Define.ViewPerspective.TPP) {
                    modeName = "Aim";
                }
            }

            var cam = GetCamera(modeName);
            if (cam == null) {
                print($"{modeName} not found");
                return;
            }
            m_CameraBrain.DefaultBlend.Time = cam.BlendTime;
            SwitchCameraMode(modeName);
            if(m_PlayerRuntimeData.ActiveSlot == Combat.Slot.Firearm) {
                cam.SetFOV(GetAimFOV());
            }

            print("Change Mode: " + modeName);
        }
    }
}
