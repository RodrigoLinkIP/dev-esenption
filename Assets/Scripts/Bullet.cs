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
            EnemigoKamikaze enemigoKamikaze = collision.GetComponent<EnemigoKamikaze>();

            if (enemigoKamikaze != null)
            {
                enemigoKamikaze.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }

            Dron2 dron2 = collision.GetComponent<Dron2>();

            if (dron2 != null)
            {
                dron2.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }

            Enemy enemy = collision.GetComponentInParent<Enemy>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }

            MiniJefeGarra miniJefeGarra = collision.GetComponent<MiniJefeGarra>();

            if (miniJefeGarra != null)
            {
                miniJefeGarra.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }

            FinalBoss boss = collision.GetComponentInParent<FinalBoss>();

            if (boss != null)
            {
                boss.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
        }

        if (collision.CompareTag("Player"))
        {
            collision.GetComponentInParent<PlayerController>()?.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}