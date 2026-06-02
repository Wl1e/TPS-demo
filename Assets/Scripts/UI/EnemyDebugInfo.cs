using TMPro;
using UI;
using UnityEngine;

public class EnemyDebugInfo : MonoBehaviour
{
    public TextMeshProUGUI HealthLabel;
    public EnemyController Enemy;
    Health health;
    void Start()
    {
        health = Enemy.Health;
        health.OnTakeDamaged += (GameObject obj, float value) => UpdateHealth(health.CurrentHealth);
        health.OnHealed += (float value) => UpdateHealth(health.CurrentHealth);
        UpdateHealth(health.CurrentHealth);
    }

    void UpdateHealth(float value)
    {
        HealthLabel.text = $"Health: {value}";
    }
}
