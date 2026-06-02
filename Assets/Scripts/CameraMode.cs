using Unity.Cinemachine;
using UnityEngine;

[RequireComponent (typeof(CinemachineCamera))]
public class CameraMode : MonoBehaviour
{

    public string ModeName;
    public PlayerMovement.CouplingMode CouplingMode;
    public int Priority => m_CinemachineCamera.Priority;
    CinemachineCamera m_CinemachineCamera;
    static public int ActivePriority = 11;
    static public int InactivePriority = 10;

    public float DefaultFOV = 60f;
    public float DefaultBlendTime = 0.2f;

    public void SetFOV(float fov) => m_CinemachineCamera.Lens.FieldOfView = fov;
    public float BlendTime => DefaultBlendTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        m_CinemachineCamera = GetComponent<CinemachineCamera>();
    }

    public void SetActive(bool active)
    {
        m_CinemachineCamera.Priority = active ? ActivePriority : InactivePriority;
        m_CinemachineCamera.Lens.FieldOfView = DefaultFOV;
    }

    public void SetTarget(Transform target)
    {
        m_CinemachineCamera.Follow = target;
        m_CinemachineCamera.LookAt = target;
    }
}
