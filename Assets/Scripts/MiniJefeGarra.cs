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
    // MUERTE
    // =========================================================

    [Header("Muerte")]

    [Tooltip("Tiempo que se deja reproducir la animación de muerte antes de destruir al jefe.")]
    [SerializeField] private float tiempoAnimacionMuerte = 2f;


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

    private Vector2 posicionOriginal;


    // =========================================================
    // DETECCIÓN DEL JUGADOR
    // =========================================================

    [Header("Zona de detección del jugador")]

    [Tooltip("Ancho de la zona donde Garra detecta al jugador.")]
    [SerializeField] private float anchoDeteccion = 4f;

    [Tooltip("Altura de la zona de detección.")]
    [SerializeField] private float altoDeteccion = 8f;

    [Tooltip("Desplazamiento vertical de la zona respecto a Garra.")]
    [SerializeField] private float offsetDeteccionY = -4f;

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
    // ARENA
    // =========================================================

    [Header("Arena")]
    public GameObject paredes;


    // =========================================================
    // REFERENCIAS
    // =========================================================

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private EstadoMinijefe estadoActual =
        EstadoMinijefe.Volando;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        rb =
            GetComponent<Rigidbody2D>();


        animator =
            GetComponentInChildren<Animator>();


        spriteRenderer =
            GetComponentInChildren<SpriteRenderer>();


        currentHealth =
            maxHealth;


        // ---------------------------------------------
        // GUARDAR ALTURA ORIGINAL
        // ---------------------------------------------

        if (rb != null)
        {
            posicionOriginal =
                rb.position;
        }
        else
        {
            posicionOriginal =
                transform.position;
        }


        // ---------------------------------------------
        // BUSCAR JUGADOR
        // ---------------------------------------------

        BuscarJugador();


        // ---------------------------------------------
        // ANIMACIÓN INICIAL
        // ---------------------------------------------

        if (animator != null)
        {
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
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (muerto)
            return;


        // ---------------------------------------------
        // BUSCAR PLAYER SI SE PERDIÓ REFERENCIA
        // ---------------------------------------------

        if (player == null)
        {
            BuscarJugador();
        }


        // ---------------------------------------------
        // SPAWN
        // ---------------------------------------------

        // Mientras cae no genera kamikazes
        if (
            estadoActual !=
            EstadoMinijefe.Cayendo
        )
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
            // ---------------------------------------------
            // VOLANDO
            // ---------------------------------------------

            case EstadoMinijefe.Volando:

                Volar();

                DetectarJugador();

                break;


            // ---------------------------------------------
            // CAYENDO
            // ---------------------------------------------

            case EstadoMinijefe.Cayendo:

                Caer();

                DetectarSuelo();

                break;


            // ---------------------------------------------
            // SUBIENDO
            // ---------------------------------------------

            case EstadoMinijefe.Subiendo:

                Subir();

                break;


            // ---------------------------------------------
            // MURIENDO
            // ---------------------------------------------

            case EstadoMinijefe.Muriendo:

                rb.linearVelocity =
                    Vector2.zero;

                break;
        }
    }


    // =========================================================
    // BUSCAR JUGADOR
    // =========================================================

    private void BuscarJugador()
    {
        GameObject jugadorObject =
            GameObject.FindWithTag("Player");


        if (jugadorObject != null)
        {
            player =
                jugadorObject.transform;
        }
    }


    // =========================================================
    // VOLAR
    // =========================================================

    private void Volar()
    {
        // ---------------------------------------------
        // DETECTAR PARED
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
        // VELOCIDAD SEGÚN FASE
        // ---------------------------------------------

        float velocidadActual =
            faseDos
                ? velocidadVueloFase2
                : velocidadVuelo;


        // ---------------------------------------------
        // MOVIMIENTO HORIZONTAL
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

    private void DetectarJugador()
    {
        if (player == null)
            return;


        // ---------------------------------------------
        // CENTRO DEL GIZMO
        // ---------------------------------------------

        Vector2 centroDeteccion =
            new Vector2(
                rb.position.x,
                rb.position.y +
                offsetDeteccionY
            );


        // ---------------------------------------------
        // TAMAÑO DEL GIZMO
        // ---------------------------------------------

        Vector2 tamañoDeteccion =
            new Vector2(
                anchoDeteccion,
                altoDeteccion
            );


        // ---------------------------------------------
        // BUSCAR PLAYER
        // ---------------------------------------------

        Collider2D jugadorDetectado =
            Physics2D.OverlapBox(
                centroDeteccion,
                tamañoDeteccion,
                0f,
                playerLayer
            );


        if (jugadorDetectado == null)
            return;


        PlayerController jugador =
            jugadorDetectado
                .GetComponentInParent<PlayerController>();


        if (
            jugadorDetectado.CompareTag("Player") ||
            jugador != null
        )
        {
            CambiarEstado(
                EstadoMinijefe.Cayendo
            );
        }
    }


    // =========================================================
    // CAER
    // =========================================================

    private void Caer()
    {
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
            Vector2.down *
            groundCheckDistance,
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
        Vector2 posicionActual =
            rb.position;


        float nuevaY =
            posicionActual.y +
            velocidadSubida *
            Time.fixedDeltaTime;


        // ---------------------------------------------
        // EVITAR SOBREPASAR ALTURA ORIGINAL
        // ---------------------------------------------

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


        rb.MovePosition(
            nuevaPosicion
        );


        // ---------------------------------------------
        // LLEGÓ A LA ALTURA ORIGINAL
        // ---------------------------------------------

        if (
            Mathf.Abs(
                nuevaY -
                posicionOriginal.y
            ) <= 0.01f
        )
        {
            rb.position =
                new Vector2(
                    rb.position.x,
                    posicionOriginal.y
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
        if (
            estadoActual ==
            nuevoEstado
        )
        {
            return;
        }


        if (
            muerto &&
            nuevoEstado !=
            EstadoMinijefe.Muriendo
        )
        {
            return;
        }


        estadoActual =
            nuevoEstado;


        // ---------------------------------------------
        // LIMPIAR VELOCIDAD
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
            // ---------------------------------------------
            // VOLANDO
            // ---------------------------------------------

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


            // ---------------------------------------------
            // CAYENDO
            // ---------------------------------------------

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


            // ---------------------------------------------
            // SUBIENDO
            // ---------------------------------------------

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


            // ---------------------------------------------
            // MURIENDO
            // ---------------------------------------------

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
                direccion < 0
                    ? 180f
                    : 0f,
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

            tiempoDesdeSpawn =
                0f;
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


        if (puntoSpawn == null)
            return;


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


        tracker.Inicializar(
            this
        );
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

    public void TakeDamage(
        float cantidad
    )
    {
        if (muerto)
            return;


        currentHealth -=
            cantidad;


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


        // ---------------------------------------------
        // FASE 2
        // ---------------------------------------------

        if (
            !faseDos &&
            currentHealth <=
            maxHealth * 0.5f
        )
        {
            ActivarFaseDos();
        }


        // ---------------------------------------------
        // MUERTE
        // ---------------------------------------------

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


        faseDos =
            true;


        Debug.Log(
            "🔥 MINIJEFE GARRA ENTRA EN FASE 2"
        );


        tiempoDesdeSpawn =
            0f;
    }


    // =========================================================
    // MORIR
    // =========================================================

    private void Morir()
    {
        if (muerto)
            return;


        // ---------------------------------------------
        // PRIMERO CAMBIAMOS ESTADO
        // ---------------------------------------------

        CambiarEstado(
            EstadoMinijefe.Muriendo
        );


        // ---------------------------------------------
        // MARCAR COMO MUERTO
        // ---------------------------------------------

        muerto =
            true;


        // ---------------------------------------------
        // DETENER MOVIMIENTO
        // ---------------------------------------------

        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;

            rb.angularVelocity =
                0f;
        }


        // ---------------------------------------------
        // ASEGURAR ANIMACIÓN DE MUERTE
        // ---------------------------------------------

        if (animator != null)
        {
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
        }


        // ---------------------------------------------
        // AUDIO DE VICTORIA
        // ---------------------------------------------

        if (player != null)
        {
            PlayerController jugador =
                player.GetComponent<PlayerController>();


            if (jugador == null)
            {
                jugador =
                    player.GetComponentInParent<PlayerController>();
            }


            if (jugador != null)
            {
                jugador.PlayVictoriaAudios();
            }
        }


        // ---------------------------------------------
        // ESPERAR ANIMACIÓN
        // ---------------------------------------------

        StartCoroutine(
            DestruirDespuesDeMorir()
        );
    }


    // =========================================================
    // DESTRUIR DESPUÉS DE MORIR
    // =========================================================

    private IEnumerator DestruirDespuesDeMorir()
    {
        // ---------------------------------------------
        // ESPERAR A QUE TERMINE LA ANIMACIÓN
        // ---------------------------------------------

        yield return
            new WaitForSeconds(
                tiempoAnimacionMuerte
            );


        // ---------------------------------------------
        // ABRIR PAREDES
        // ---------------------------------------------

        if (paredes != null)
        {
            paredes.SetActive(
                false
            );
        }


        // ---------------------------------------------
        // DESTRUIR JEFE
        // ---------------------------------------------

        Destroy(
            gameObject
        );
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


        // Solo hace daño de aplastamiento
        // mientras está cayendo
        if (
            estadoActual !=
            EstadoMinijefe.Cayendo
        )
        {
            return;
        }


        // ---------------------------------------------
        // BUSCAR PLAYER
        // ---------------------------------------------

        PlayerController jugador =
            collision.gameObject
                .GetComponent<PlayerController>();


        if (jugador == null)
        {
            jugador =
                collision.gameObject
                    .GetComponentInParent<PlayerController>();
        }


        // ---------------------------------------------
        // DAÑO POR APLASTAMIENTO
        // ---------------------------------------------

        if (jugador != null)
        {
            jugador.TakeDamage(
                dañoAplastamiento
            );
        }


        // IMPORTANTE:
        //
        // No cambiamos aquí a "Subiendo".
        //
        // DetectarSuelo() es el encargado
        // de detectar el piso.
    }


    // =========================================================
    // GIZMOS
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        // ---------------------------------------------
        // ZONA DE DETECCIÓN DEL PLAYER
        // ---------------------------------------------

        Gizmos.color =
            Color.red;


        Vector3 centroDeteccion =
            new Vector3(
                transform.position.x,
                transform.position.y +
                offsetDeteccionY,
                transform.position.z
            );


        Vector3 tamañoDeteccion =
            new Vector3(
                anchoDeteccion,
                altoDeteccion,
                0f
            );


        Gizmos.DrawWireCube(
            centroDeteccion,
            tamañoDeteccion
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
        // ALTURA ORIGINAL
        // ---------------------------------------------

        if (Application.isPlaying)
        {
            Gizmos.color =
                Color.cyan;


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
}


// =============================================================
// TRACKER DE KAMIKAZE
// =============================================================

public class KamikazeTracker : MonoBehaviour
{
    private MiniJefeGarra jefe;

    private bool avisado =
        false;


    public void Inicializar(
        MiniJefeGarra nuevoJefe
    )
    {
        jefe =
            nuevoJefe;
    }


    private void OnDestroy()
    {
        if (
            !avisado &&
            jefe != null
        )
        {
            avisado =
                true;


            jefe.KamikazeMurio();
        }
    }
}