using UnityEngine;
using System.Collections;

public class MiniJefeGarra : MonoBehaviour
{
    public enum EstadoMinijefe
    {
        Volando,
        Cayendo,
        Subiendo,
        Muriendo
    }

    // =========================================================
    // VIDA
    // =========================================================

    [Header("Vida")]
    [SerializeField] private float maxHealth = 20f;

    private float currentHealth;
    private bool faseDos = false;
    private bool muerto = false;


    // =========================================================
    // VUELO
    // =========================================================

    [Header("Vuelo")]
    [SerializeField] private float velocidadVuelo = 3f;
    [SerializeField] private float velocidadVueloFase2 = 4f;

    private int direccion = 1;


    // =========================================================
    // PAREDES
    // =========================================================

    [Header("Detección de paredes")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallCheckDistance = 0.3f;
    [SerializeField] private LayerMask wallLayer;


    // =========================================================
    // ATAQUE VERTICAL
    // =========================================================

    [Header("Ataque vertical")]
    [SerializeField] private float velocidadCaida = 15f;
    [SerializeField] private float velocidadSubida = 5f;

    // Posición original donde empezó el mini jefe
    private Vector2 posicionOriginal;


    // =========================================================
    // DETECCIÓN DEL JUGADOR
    // =========================================================

    [Header("Detección del jugador")]
    [SerializeField] private float distanciaDeteccion = 10f;
    [SerializeField] private float offsetY = 1.5f;
    [SerializeField] private LayerMask playerLayer;


    // =========================================================
    // DETECCIÓN DEL SUELO
    // =========================================================

    [Header("Detección del suelo")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private LayerMask groundLayer;


    // =========================================================
    // DAÑO
    // =========================================================

    [Header("Daño")]
    [SerializeField] private float dañoAplastamiento = 2f;


    // =========================================================
    // SPAWN DE KAMIKAZES
    // =========================================================

    [Header("Spawn de Kamikazes")]
    [SerializeField] private GameObject enemigoKamikazePrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float tiempoEntreSpawns = 5f;
    [SerializeField] private int maxKamikazes = 3;

    private float tiempoDesdeSpawn = 0f;
    private int kamikazesVivos = 0;


    // =========================================================
    // FASE 2 - SPAWN
    // =========================================================

    [Header("Fase 2 - Spawn")]
    [SerializeField] private float tiempoEntreSpawnsFase2 = 3f;
    [SerializeField] private int maxKamikazesFase2 = 5;


    // =========================================================
    // JUGADOR
    // =========================================================

    [Header("Jugador")]
    [SerializeField] private Transform player;


    // =========================================================
    // REFERENCIAS
    // =========================================================

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private EstadoMinijefe estadoActual =
        EstadoMinijefe.Volando;

    public GameObject paredes;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        animator = GetComponentInChildren<Animator>();

        spriteRenderer =
            GetComponentInChildren<SpriteRenderer>();

        currentHealth = maxHealth;

        // Guardamos la posición inicial COMPLETA
        posicionOriginal = rb.position;


        // Buscar jugador automáticamente
        if (player == null)
        {
            GameObject jugadorObject =
                GameObject.FindWithTag("Player");

            if (jugadorObject != null)
            {
                player = jugadorObject.transform;
            }
        }


        CambiarEstado(
            EstadoMinijefe.Volando
        );
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (muerto)
            return;


        // Buscar jugador si todavía no existe
        if (player == null)
        {
            GameObject jugadorObject =
                GameObject.FindWithTag("Player");

            if (jugadorObject != null)
            {
                player = jugadorObject.transform;
            }
        }


        // No generar kamikazes mientras está cayendo
        if (estadoActual != EstadoMinijefe.Cayendo)
        {
            ControlarSpawn();
        }
    }


    // =========================================================
    // FIXED UPDATE
    // =========================================================

    private void FixedUpdate()
    {
        if (muerto || rb == null)
            return;


        switch (estadoActual)
        {
            case EstadoMinijefe.Volando:

                Volar();

                DetectarJugadorConLaser();

                break;


            case EstadoMinijefe.Cayendo:

                Caer();

                DetectarSuelo();

                break;


            case EstadoMinijefe.Subiendo:

                Subir();

                break;


            case EstadoMinijefe.Muriendo:

                rb.linearVelocity = Vector2.zero;

                break;
        }
    }


    // =========================================================
    // VOLAR
    // =========================================================

    private void Volar()
    {
        // ---------------------------------------------
        // Detectar pared
        // ---------------------------------------------

        if (wallCheck != null)
        {
            RaycastHit2D hit =
                Physics2D.Raycast(
                    wallCheck.position,
                    Vector2.right * direccion,
                    wallCheckDistance,
                    wallLayer
                );


            Debug.DrawRay(
                wallCheck.position,
                Vector2.right *
                direccion *
                wallCheckDistance,
                Color.blue
            );


            if (hit.collider != null)
            {
                direccion *= -1;

                ActualizarRotacion();
            }
        }


        // ---------------------------------------------
        // Velocidad
        // ---------------------------------------------

        float velocidadActual =
            faseDos
            ? velocidadVueloFase2
            : velocidadVuelo;


        // ---------------------------------------------
        // Movimiento horizontal
        // ---------------------------------------------

        rb.linearVelocity =
            new Vector2(
                direccion * velocidadActual,
                0f
            );
    }


    // =========================================================
    // DETECTAR JUGADOR
    // =========================================================

    private void DetectarJugadorConLaser()
    {
        if (player == null)
            return;


        Vector2 origenRayo =
            new Vector2(
                rb.position.x,
                rb.position.y - offsetY
            );


        RaycastHit2D hit =
            Physics2D.Raycast(
                origenRayo,
                Vector2.down,
                distanciaDeteccion,
                playerLayer
            );


        Debug.DrawRay(
            origenRayo,
            Vector2.down * distanciaDeteccion,
            Color.red
        );


        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Player"))
            {
                CambiarEstado(
                    EstadoMinijefe.Cayendo
                );
            }
        }
    }


    // =========================================================
    // CAER
    // =========================================================

    private void Caer()
    {
        // Solo movimiento vertical
        rb.linearVelocity =
            new Vector2(
                0f,
                -velocidadCaida
            );
    }


    // =========================================================
    // DETECTAR SUELO
    // =========================================================

    private void DetectarSuelo()
    {
        if (groundCheck == null)
            return;


        RaycastHit2D hit =
            Physics2D.Raycast(
                groundCheck.position,
                Vector2.down,
                groundCheckDistance,
                groundLayer
            );


        Debug.DrawRay(
            groundCheck.position,
            Vector2.down * groundCheckDistance,
            Color.green
        );


        if (hit.collider != null)
        {
            CambiarEstado(
                EstadoMinijefe.Subiendo
            );
        }
    }


    // =========================================================
    // SUBIR
    // =========================================================

    private void Subir()
    {
        // ---------------------------------------------
        // Posición actual
        // ---------------------------------------------

        Vector2 posicionActual =
            rb.position;


        // ---------------------------------------------
        // Nueva posición
        // ---------------------------------------------

        float nuevaY =
            posicionActual.y +
            velocidadSubida *
            Time.fixedDeltaTime;


        // Nunca superar la altura original
        if (nuevaY >= posicionOriginal.y)
        {
            nuevaY =
                posicionOriginal.y;
        }


        Vector2 nuevaPosicion =
            new Vector2(
                posicionActual.x,
                nuevaY
            );


        // ---------------------------------------------
        // Mover Rigidbody
        // ---------------------------------------------

        rb.MovePosition(
            nuevaPosicion
        );


        // ---------------------------------------------
        // Llegó arriba
        // ---------------------------------------------

        if (nuevaY >= posicionOriginal.y)
        {
            rb.MovePosition(
                new Vector2(
                    rb.position.x,
                    posicionOriginal.y
                )
            );


            rb.linearVelocity =
                Vector2.zero;


            CambiarEstado(
                EstadoMinijefe.Volando
            );
        }
    }


    // =========================================================
    // CAMBIAR ESTADO
    // =========================================================

    private void CambiarEstado(
        EstadoMinijefe nuevoEstado
    )
    {
        // No hacer nada si ya estamos en ese estado
        if (estadoActual == nuevoEstado)
            return;


        // Si está muerto, no aceptar cambios
        if (muerto &&
            nuevoEstado != EstadoMinijefe.Muriendo)
            return;


        estadoActual = nuevoEstado;


        // ---------------------------------------------
        // Limpiar velocidad al cambiar de estado
        // ---------------------------------------------

        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;
        }


        if (animator == null)
            return;


        switch (nuevoEstado)
        {
            // -----------------------------------------
            // VOLANDO
            // -----------------------------------------

            case EstadoMinijefe.Volando:

                animator.SetBool(
                    "isWalking",
                    true
                );

                animator.SetBool(
                    "isAttacking",
                    false
                );

                animator.SetBool(
                    "isDying",
                    false
                );

                animator.SetTrigger(
                    "Volver"
                );

                break;


            // -----------------------------------------
            // CAYENDO
            // -----------------------------------------

            case EstadoMinijefe.Cayendo:

                animator.SetBool(
                    "isWalking",
                    false
                );

                animator.SetBool(
                    "isAttacking",
                    true
                );

                animator.SetBool(
                    "isDying",
                    false
                );

                break;


            // -----------------------------------------
            // SUBIENDO
            // -----------------------------------------

            case EstadoMinijefe.Subiendo:

                animator.SetBool(
                    "isWalking",
                    false
                );

                animator.SetBool(
                    "isAttacking",
                    false
                );

                animator.SetBool(
                    "isDying",
                    false
                );

                animator.SetTrigger(
                    "Volver"
                );

                break;


            // -----------------------------------------
            // MURIENDO
            // -----------------------------------------

            case EstadoMinijefe.Muriendo:

                animator.SetBool(
                    "isWalking",
                    false
                );

                animator.SetBool(
                    "isAttacking",
                    false
                );

                animator.SetBool(
                    "isDying",
                    true
                );

                break;
        }
    }


    // =========================================================
    // ROTACIÓN
    // =========================================================

    private void ActualizarRotacion()
    {
        transform.rotation =
            Quaternion.Euler(
                0f,
                direccion < 0 ? 180f : 0f,
                0f
            );
    }


    // =========================================================
    // SPAWN
    // =========================================================

    private void ControlarSpawn()
    {
        if (
            enemigoKamikazePrefab == null ||
            spawnPoints == null ||
            spawnPoints.Length == 0
        )
        {
            return;
        }


        tiempoDesdeSpawn +=
            Time.deltaTime;


        float tiempoActual =
            faseDos
            ? tiempoEntreSpawnsFase2
            : tiempoEntreSpawns;


        int maxActual =
            faseDos
            ? maxKamikazesFase2
            : maxKamikazes;


        if (
            tiempoDesdeSpawn >= tiempoActual &&
            kamikazesVivos < maxActual
        )
        {
            SpawnKamikaze();

            tiempoDesdeSpawn = 0f;
        }
    }


    // =========================================================
    // SPAWN KAMIKAZE
    // =========================================================

    private void SpawnKamikaze()
    {
        if (
            enemigoKamikazePrefab == null ||
            spawnPoints == null ||
            spawnPoints.Length == 0
        )
        {
            return;
        }


        Transform puntoSpawn =
            spawnPoints[
                Random.Range(
                    0,
                    spawnPoints.Length
                )
            ];


        GameObject enemigo =
            Instantiate(
                enemigoKamikazePrefab,
                puntoSpawn.position,
                Quaternion.identity
            );


        kamikazesVivos++;


        KamikazeTracker tracker =
            enemigo.GetComponent<KamikazeTracker>();


        if (tracker == null)
        {
            tracker =
                enemigo.AddComponent<KamikazeTracker>();
        }


        tracker.Inicializar(this);
    }


    // =========================================================
    // KAMIKAZE MURIÓ
    // =========================================================

    public void KamikazeMurio()
    {
        kamikazesVivos =
            Mathf.Max(
                0,
                kamikazesVivos - 1
            );
    }


    // =========================================================
    // RECIBIR DAÑO
    // =========================================================

    public void TakeDamage(float cantidad)
    {
        if (muerto)
            return;


        currentHealth -= cantidad;


        currentHealth =
            Mathf.Max(
                0f,
                currentHealth
            );


        Debug.Log(
            "MiniJefeGarra recibió " +
            cantidad +
            " de daño. Vida: " +
            currentHealth
        );


        if (
            !faseDos &&
            currentHealth <= maxHealth * 0.5f
        )
        {
            ActivarFaseDos();
        }


        if (currentHealth <= 0f)
        {
            Morir();
        }
    }


    // =========================================================
    // FASE 2
    // =========================================================

    private void ActivarFaseDos()
    {
        if (faseDos)
            return;


        faseDos = true;


        Debug.Log(
            "🔥 MINIJEFE GARRA ENTRA EN FASE 2"
        );


        tiempoDesdeSpawn = 0f;
    }


    // =========================================================
    // MORIR
    // =========================================================

    private void Morir()
    {
        if (muerto)
            return;


        CambiarEstado(
            EstadoMinijefe.Muriendo
        );


        muerto = true;


        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;
        }


        StartCoroutine(
            DestruirDespuesDeMorir()
        );
    }


    // =========================================================
    // DESTRUIR DESPUÉS DE MORIR
    // =========================================================

    private IEnumerator DestruirDespuesDeMorir()
    {
        if (paredes != null)
        {
            paredes.SetActive(false);
        }


        yield return new WaitForSeconds(2f);


        Destroy(gameObject);
    }


    // =========================================================
    // COLISIÓN
    // =========================================================

    private void OnCollisionEnter2D(
        Collision2D collision
    )
    {
        if (muerto)
            return;


        // ---------------------------------------------
        // Daño al jugador mientras cae
        // ---------------------------------------------

        if (
            estadoActual ==
            EstadoMinijefe.Cayendo
        )
        {
            if (
                collision.gameObject.CompareTag(
                    "Player"
                )
            )
            {
                PlayerController jugador =
                    collision.gameObject
                        .GetComponentInParent<PlayerController>();


                if (jugador != null)
                {
                    jugador.TakeDamage(
                        dañoAplastamiento
                    );
                }
            }
        }

        // IMPORTANTE:
        // Ya NO cambiamos aquí a Subiendo.
        //
        // DetectarSuelo() es ahora el único sistema
        // encargado de detectar el suelo y cambiar
        // al estado Subiendo.
    }


    // =========================================================
    // GIZMOS
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        // ---------------------------------------------
        // DETECCIÓN DEL JUGADOR
        // ---------------------------------------------

        Gizmos.color = Color.red;


        Vector3 origen =
            transform.position;


        origen.y -= offsetY;


        Gizmos.DrawLine(
            origen,
            origen +
            Vector3.down *
            distanciaDeteccion
        );


        // ---------------------------------------------
        // WALL CHECK
        // ---------------------------------------------

        if (wallCheck != null)
        {
            Gizmos.color =
                Color.blue;


            Gizmos.DrawLine(
                wallCheck.position,
                wallCheck.position +
                Vector3.right *
                direccion *
                wallCheckDistance
            );
        }


        // ---------------------------------------------
        // GROUND CHECK
        // ---------------------------------------------

        if (groundCheck != null)
        {
            Gizmos.color =
                Color.green;


            Gizmos.DrawLine(
                groundCheck.position,
                groundCheck.position +
                Vector3.down *
                groundCheckDistance
            );
        }


        // ---------------------------------------------
        // SPAWN POINTS
        // ---------------------------------------------

        if (spawnPoints != null)
        {
            Gizmos.color =
                Color.yellow;


            foreach (
                Transform punto
                in spawnPoints
            )
            {
                if (punto != null)
                {
                    Gizmos.DrawWireSphere(
                        punto.position,
                        0.2f
                    );
                }
            }
        }


        // ---------------------------------------------
        // POSICIÓN ORIGINAL
        // ---------------------------------------------

        Gizmos.color = Color.cyan;


        Vector3 alturaInicial =
            new Vector3(
                transform.position.x,
                posicionOriginal.y,
                transform.position.z
            );


        Gizmos.DrawWireSphere(
            alturaInicial,
            0.15f
        );
    }
}


// =============================================================
// TRACKER DE KAMIKAZE
// =============================================================

public class KamikazeTracker : MonoBehaviour
{
    private MiniJefeGarra jefe;

    private bool avisado = false;


    public void Inicializar(
        MiniJefeGarra nuevoJefe
    )
    {
        jefe = nuevoJefe;
    }


    private void OnDestroy()
    {
        if (
            !avisado &&
            jefe != null
        )
        {
            avisado = true;

            jefe.KamikazeMurio();
        }
    }
}