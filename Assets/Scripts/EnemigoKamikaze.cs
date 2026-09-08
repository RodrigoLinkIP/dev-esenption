using UnityEngine;

public class EnemigoKamikaze : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidad = 5f;

    [Header("Vida")]
    [SerializeField] private float vida = 1f;

    [Header("Daño")]
    [SerializeField] private float daño = 1f;

    private Transform jugador;
    private Rigidbody2D rb;

    private bool impacto = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Buscar al jugador automáticamente
        GameObject jugadorObject =
            GameObject.FindWithTag("Player");

        if (jugadorObject != null)
        {
            jugador = jugadorObject.transform;
        }
    }

    private void FixedUpdate()
    {
        if (impacto || jugador == null || rb == null)
            return;

        Vector2 direccion =
            ((Vector2)jugador.position - rb.position).normalized;

        rb.linearVelocity =
            direccion * velocidad;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (impacto)
            return;

        if (collision.gameObject.CompareTag("Player"))
        {
            impacto = true;

            PlayerController jugadorController =
                collision.gameObject.GetComponentInParent<PlayerController>();

            if (jugadorController != null)
            {
                jugadorController.TakeDamage(daño);
            }

            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (impacto)
            return;

        if (collision.CompareTag("Player"))
        {
            impacto = true;

            PlayerController jugadorController =
                collision.GetComponentInParent<PlayerController>();

            if (jugadorController != null)
            {
                jugadorController.TakeDamage(daño);
            }

            Destroy(gameObject);
        }
    }

    public void TakeDamage(float cantidad)
    {
        vida -= cantidad;

        if (vida <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
