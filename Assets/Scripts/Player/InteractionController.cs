using UnityEngine;

public class InteractionController : MonoBehaviour
{
    [SerializeField] float m_InteractRange = 3f;
    [SerializeField] LayerMask interactLayerMask;
    [SerializeField] GameEvent onInteractInput;

    IInteractive m_CurrentTarget;  // 当前瞄准的可互动对象

    private void Awake()
    {
        onInteractInput.RegisterListener(OnInteract);
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        // Debug.DrawRay(ray.origin, ray.direction, Color.green, 0.1f, true);

        if (Physics.Raycast(ray, out RaycastHit hit, m_InteractRange, interactLayerMask)) {
            if(hit.collider.TryGetComponent<IInteractive>(out var interactive)) {
                // 显示交互 UI
                m_CurrentTarget = interactive;
                //print("see target: " + m_CurrentTarget.ToString());
            } else {
                m_CurrentTarget = null;
            }
        } else {
            m_CurrentTarget = null;
        }
    }

    void OnInteract()
    {
        m_CurrentTarget?.Interact(gameObject);
    }
}
