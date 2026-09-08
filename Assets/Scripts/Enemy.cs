using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    // =====================================================
    // MOVIMIENTO / PATRULLA
    // =====================================================

    [Header("Patrulla")]
    public float speed = 2f;
    public float patrolDistance = 4f;

    private Vector3 startPosition;
    private int direction = 1;

    // =====================================================
    // VIDA
    // =====================================================

    [Header("Vida")]
    public float maxHealth = 3f;
    private float currentHealth;
    public float tiempoMuerte = 1f;
    private bool muerto = false;

    // =====================================================
    // DISPARO
    // =====================================================

    [Header("Disparo")]
    public GameObject bulletPrefab;
    public float fireRate = 2f;
    public float detectionRange = 8f;
    public Transform firePoint;

    private float lastFireTime;
    private Transform player;

    // =====================================================
    // ATAQUE
    // =====================================================

    [Header("Ataque")]
    public bool canShoot = true;
    public float damage = 1f;
    public float damageCooldown = 1f;

    private float lastDamageTime;

    // =====================================================
    // ANIMACIÓN
    // =====================================================

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // =====================================================
    // FÍSICA
    // =====================================================

    private Rigidbody2D rb;

    // =====================================================
    // PLAYER
    // =====================================================

    private AudioSource playerAudio;

    void Start()
    {
        // Componentes
        rb = GetComponent<Rigidbody2D>();

        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Posición inicial para la patrulla
        startPosition = transform.position;

        // Vida inicial
        currentHealth = maxHealth;

        // Buscar Player
        GameObject playerObject = GameObject.FindWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
            Debug.Log(player.tag);
        }
    }


    void Update()
    {
        // =================================================
        // DISPARO
        // =================================================

        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(
                transform.position,
                player.position
            );

            if (
                distanceToPlayer <= detectionRange &&
                Time.time - lastFireTime >= fireRate
            )
            {
                Shoot();

                lastFireTime = Time.time;
            }
        }

        if (player == null)
        {
            GameObject jugador = GameObject.FindWithTag("Player");

            if (jugador != null)
            {
                player = jugador.transform;
                playerAudio = player.GetComponent<AudioSource>();
            }

            return;
        }
    }


    protected virtual void FixedUpdate()
    {
        // =================================================
        // PATRULLA
        // =================================================

        if (rb == null)
            return;

        if (muerto)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        rb.linearVelocity = new Vector2(
            direction * speed,
            rb.linearVelocity.y
        );

        float distanceTraveled =
            transform.position.x - startPosition.x;

        if (distanceTraveled > patrolDistance)
        {
            direction = -1;
        }
        else if (distanceTraveled < -patrolDistance)
        {
            direction = 1;
        }

        // Girar enemigo
        transform.rotation = Quaternion.Euler(
            0f,
            direction < 0 ? 180f : 0f,
            0f
        );

        // =================================================
        // ANIMACIÓN
        // =================================================

        if (animator != null)
        {
            animator.SetBool("isWalking", Mathf.Abs(rb.linearVelocity.x) > 0.1f);
        }
    }


    // =====================================================
    // DISPARO
    // =====================================================
   
    protected virtual void Shoot()
    {

        if (bulletPrefab == null || firePoint == null || !canShoot)
            return;

        Vector2 shootDirection =
            (player.position - firePoint.position).normalized;

        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            Quaternion.identity
        );

        Bullet bulletScript = bullet.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            bulletScript.SetOwner(gameObject);
            bulletScript.SetDirection(shootDirection);
        }
    }


    // =====================================================
    // RECIBIR DAÑO
    // =====================================================

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        Debug.Log(
            gameObject.name +
            " recibió " +
            amount +
            " de daño. Vida: " +
            currentHealth
        );

        if (currentHealth <= 0f)
        {
            muerto = true;
            Die();
        }
    }


    void Die()
    {
        animator.SetBool("isDiying", true);

        StartCoroutine(DestruirDespuesDeMorir());
    }

    private IEnumerator DestruirDespuesDeMorir()
    {
        yield return new WaitForSeconds(tiempoMuerte);

        Destroy(gameObject);
    }


    // =====================================================
    // ATAQUE AL PLAYER
    // =====================================================

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            TryDamagePlayer(collision);
        }
    }


    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            TryDamagePlayer(collision);
        }
    }


    private void TryDamagePlayer(Collider2D collision)
    {
        if (Time.time - lastDamageTime < damageCooldown)
            return;

        PlayerController playerHealth = collision.GetComponent<PlayerController>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);

            lastDamageTime = Time.time;
        }
    }


    // =====================================================
    // GIZMOS
    // =====================================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            detectionRange
        );
    }
}