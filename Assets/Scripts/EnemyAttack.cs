using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public float damage = 1f;
    public float damageCooldown = 1f;  // seconds between hits
    private float lastDamageTime;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            TryDamagePlayer(collision);
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            TryDamagePlayer(collision);
    }

    void TryDamagePlayer(Collider2D collision)
    {
        if (Time.time - lastDamageTime < damageCooldown) return;

        collision.GetComponent<PlayerHealth>()?.TakeDamage(damage);
        lastDamageTime = Time.time;
    }
}