using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBarUI : MonoBehaviour
{
    public Slider healthSlider;
    public Transform target; // Inimigo
    public Vector3 offset = new Vector3(0, 2, 0); // Posição acima da cabeça

    void Update()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
            transform.forward = Camera.main.transform.forward;
        }
    }

    public void SetMaxHealth(int maxHealth)
    {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;
    }

    public void SetHealth(int currentHealth)
    {
        healthSlider.value = currentHealth;
    }
}
