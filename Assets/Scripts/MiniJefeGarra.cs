using UnityEngine;
using System.Collections;

public class MiniJefeGarra : Enemy
{
    public enum EstadoMinijefe { Volando, Cayendo, Subiendo }
    public EstadoMinijefe estadoActual = EstadoMinijefe.Volando;

    [Header("Físicas del Minijefe")]
    public float velocidadCaida = 15f;
    public float velocidadSubida = 5f;


    [Header("Visión Láser")]
    public float distanciaDeteccion = 10f;
    public float offsetY = 1.5f;

    private float alturaOriginal;
    private Animator minijefeAnimator;
    private Rigidbody2D minijefeRb;

    // Usamos Awake para no interferir con el Start() de la clase Enemy original
    void Awake()
    {
        alturaOriginal = transform.position.y;
        minijefeAnimator = GetComponentInChildren<Animator>();
        minijefeRb = GetComponent<Rigidbody2D>();
    }

    // Sobrescribimos el FixedUpdate para CANCELAR el patrullaje del Enemy original
    protected override void FixedUpdate()
    {
        // Al no poner "base.FixedUpdate()", el Dron Garra ignora el patrullaje normal.
        switch (estadoActual)
        {
            case EstadoMinijefe.Volando:
                // ¡MAGIA!: Ejecutamos el FixedUpdate de Enemy.cs para que patrulle gratis
                base.FixedUpdate();

                // Lógica del láser hacia abajo
                Vector2 origenRayo = new Vector2(transform.position.x, transform.position.y - offsetY);
                RaycastHit2D hit = Physics2D.Raycast(origenRayo, Vector2.down, distanciaDeteccion);
                Debug.DrawRay(origenRayo, Vector2.down * distanciaDeteccion, Color.red);

                if (hit.collider != null && hit.collider.CompareTag("Player"))
                {
                    CambiarEstado(EstadoMinijefe.Cayendo);
                }
                break;

            case EstadoMinijefe.Cayendo:
                // Forzamos el movimiento hacia abajo y anulamos el movimiento horizontal
                minijefeRb.linearVelocity = new Vector2(0f, -velocidadCaida);
                break;

            case EstadoMinijefe.Subiendo:
                // Lo hacemos subir lentamente
                minijefeRb.linearVelocity = new Vector2(0f, velocidadSubida);

                // Si alcanzó o superó su altura inicial, se estabiliza y vuelve a patrullar
                if (transform.position.y >= alturaOriginal)
                {
                    transform.position = new Vector2(transform.position.x, alturaOriginal);
                    CambiarEstado(EstadoMinijefe.Volando);
                }
                break;
        }
    }
    protected override void Shoot()
    {
        // Lo dejamos vacío intencionalmente. ¡El Dron Garra no dispara!
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Lógica de frenado al chocar con el suelo
        if (estadoActual == EstadoMinijefe.Cayendo && collision.gameObject.CompareTag("Suelo"))
        {
            estadoActual = EstadoMinijefe.Subiendo;
            // Aquí iría el animator.SetTrigger para subir
        }
    }
    // Método maestro para gestionar transiciones físicas y de animación
    public void CambiarEstado(EstadoMinijefe nuevoEstado)
    {
        estadoActual = nuevoEstado;

        if (nuevoEstado == EstadoMinijefe.Cayendo)
        {
            // Frenamos en seco la inercia horizontal de la patrulla
            minijefeRb.linearVelocity = Vector2.zero;

            if (minijefeAnimator != null)
                minijefeAnimator.SetTrigger("Atacar");
        }
        else if (nuevoEstado == EstadoMinijefe.Volando)
        {
            if (minijefeAnimator != null)
                minijefeAnimator.SetTrigger("Volver");
        }
    }
}