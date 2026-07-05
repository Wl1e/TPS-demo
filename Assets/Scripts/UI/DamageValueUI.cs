using TMPro;
using TPSDemo;
using UnityEngine;

public class DamageValueUI : MonoBehaviour
{
    /// <summary>
    /// 向前伸出的偏移量，用于避免伤害数字和受击物体重叠
    /// </summary>
    static public float DamageValueOffset = 0.5f;
    [Tooltip("伤害数字的显示时间")]
    public float DisplayTime = 1.0f;
    [Tooltip("伤害数字颜色")]
    public Color DamageColor = Color.red;

    [Tooltip("伤害数字的缩放")]
    public Vector3 Scale = Vector3.one;

    [Tooltip("是否移动")]
    public bool Move = true;
    [Tooltip("随机移动")]
    public bool RandomMove = false;
    [Tooltip("移动速度")]
    public float MoveSpeed = 1.0f;
    [Tooltip("伤害数字移动方向")]
    public Vector2 MoveDirection = Vector3.up;

    public TextMeshPro Value;

    public event System.Action<DamageValueUI> OnRelease;

    /// <summary>
    /// 剩余时间
    /// </summary>
    private float m_Duration = 0f;
    private Vector3 m_MoveDir;

    private void Update()
    {
        if(m_Duration > 0f) {
            m_Duration -= Time.deltaTime;

            Color newColor = Value.color;
            newColor.a = m_Duration / DisplayTime;
            Value.color = newColor;

            // 看向摄像机
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);

            if (Move) {
                transform.position += m_MoveDir * MoveSpeed * Time.deltaTime;
            }

            if(m_Duration <= 0f) {
                OnRelease?.Invoke(this);
            }
        }
    }

    public void Show(string value, bool isCritical)
    {
        Value.text = value;
        Value.color = DamageColor;
        transform.localScale = Scale;
        m_Duration = DisplayTime;
        if(!Move) {
            return;
        }
        if (RandomMove) {
            MoveDirection = new Vector3(Random.Range(-1f, 1f), Random.Range(0.5f, 1.5f), 0).normalized;
        }
    }
}
