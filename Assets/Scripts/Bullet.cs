using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 2f;
    public float damage = 1f;

    private Vector2 direction;

    private GameObject owner;

    public void SetOwner(GameObject o) { owner = o; }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == owner) return; // ignore who shot it

        if (collision.CompareTag("Bullet")) return; // ignore other bullets

        Debug.Log($"Bullet hit: {collision.gameObject.name}, tag: {collision.gameObject.tag}");

        if (collision.CompareTag("Enemy"))
        {
            collision.GetComponent<EnemyHealth>()?.TakeDamage(damage);
            Destroy(gameObject);
        }

        if (collision.CompareTag("Player"))
        {
            collision.GetComponentInParent<PlayerHealth>()?.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}