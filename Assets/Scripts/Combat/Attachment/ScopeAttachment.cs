using UnityEngine;

namespace TPSDemo
{
    /// <summary>
    /// 瞄准镜附件 — 挂载到武器的 ScopeSocket 上
    /// 子物体必须包含 CameraSocket (Transform)，标记瞄准时相机移到的位置
    /// </summary>
    public class ScopeAttachment: AttachmentBase
    {
        public enum ScopeRatio : int { One = 1, Two = 2 }
        [Header("瞄准类型")]
        public Define.ViewPerspective Type = Define.ViewPerspective.FPP;

        [SerializeField] ScopeRatio m_Ratio = ScopeRatio.One;
        public ScopeRatio Ratio => m_Ratio;

        [Tooltip("FOV 过渡时间（秒）")]
        [SerializeField] float m_TransitionDuration = 0.2f;
        public float TransitionDuration => m_TransitionDuration;

        [Header("相机挂载点")]
        [Tooltip("瞄准时相机移到的位置 (子物体)")]
        [SerializeField] Transform m_CameraSocket;
        public Transform CameraSocket => m_CameraSocket;

        [Header("准星")]
        public CrosshairData ScopedCrosshair;

        public override void OnEquip() { }
        public override void OnUnequip() { }
        public override void OnAim() { }
        public override void OnFire() { }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (m_CameraSocket == null) {
                Transform t = transform.Find("CameraSocket");
                if (t != null)
                    m_CameraSocket = t;
            }
        }

        void OnDrawGizmosSelected()
        {
            if (m_CameraSocket == null)
                return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(m_CameraSocket.position, 0.02f);
            Gizmos.DrawRay(m_CameraSocket.position, m_CameraSocket.forward * 0.2f);
        }
#endif
    }
}
