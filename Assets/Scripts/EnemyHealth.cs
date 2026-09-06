using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 3f;
    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)  // Bullet.cs already calls this
    {
        Debug.Log("TakeDamage called!");
        currentHealth -= amount;
        if (currentHealth <= 0f) Die();
    }

    void Die()
    {
        Destroy(gameObject);
    }
}