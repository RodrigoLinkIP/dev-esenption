using UnityEngine;
using System.Collections;

public class FinalBoss : MonoBehaviour
{
    private enum Estado
    {
        Idle,
        Patrullando,
        Atacando,
        Caminando,
        Subiendo,
        Volando,
        Bajando,
        Muriendo
    }

    // =========================================================
    // VIDA
    // =========================================================

    [Header("Vida")]
    public float maxHealth = 20f;

    private float currentHealth;
    private bool faseDos = false;


    // =========================================================
    // DETECCIÓN
    // =========================================================

    [Header("Detección - Fase 1")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 5f;

    [Header("Detección - Fase 2")]
    [SerializeField] private float detectionRangeFaseDos = 14f;
    [SerializeField] private float attackRangeFaseDos = 7f;


    // =========================================================
    // MOVIMIENTO TERRESTRE
    // =========================================================

    [Header("Movimiento terrestre")]
    [SerializeField] private float walkSpeed = 2f;


    // =========================================================
    // PATRULLA
    // =========================================================

    [Header("Patrulla")]
    [SerializeField] private float patrolSpeed = 1.5f;
    [SerializeField] private float patrolChangeTime = 3f;
    [SerializeField] private float patrolPauseDuration = 0.5f;

    private float patrolTimer = 0f;
    private float patrolPauseTimer = 0f;

    private int patrolDirection = 1;

    private bool patrullaPausada = false;


    // =========================================================
    // VUELO
    // =========================================================

    [Header("Vuelo")]
    [SerializeField] private float flightHeight = 3f;
    [SerializeField] private float flightSpeed = 5f;
    [SerializeField] private float flightDuration = 2.5f;
    [SerializeField] private float timeBetweenFlights = 8f;

    private float tiempoDesdeVuelo = 0f;


    // =========================================================
    // ATAQUES
    // =========================================================

    [Header("Ataques")]
    [SerializeField] private float attackDuration = 1.5f;
    [SerializeField] private float attackWalkDuration = 3f;
    [SerializeField] private float timeBetweenAttacks = 1f;

    private float tiempoDesdeAtaque = 999f;


    // =========================================================
    // DISPARO
    // =========================================================

    [Header("Disparo")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    [SerializeField] private float timeBetweenShots = 0.5f;

    private float nextShotTime = 0f;


    // =========================================================
    // PAREDES
    // =========================================================

    [Header("Paredes")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallCheckDistance = 0.2f;
    [SerializeField] private LayerMask wallLayer;


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

    private Estado estadoActual = Estado.Idle;

    private bool muerto = false;
    private bool jugadorDetectado = false;
    private bool isAttacking = false;

    private int direccion = 1;

    private Coroutine rutinaJefe;

    private Vector3 posicionSuelo;


    // =========================================================
    // INICIO
    // =========================================================

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        animator = GetComponentInChildren<Animator>();

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        currentHealth = maxHealth;

        posicionSuelo = transform.position;


        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }


        CambiarEstado(Estado.Idle);

        rutinaJefe = StartCoroutine(IAJefe());
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (muerto)
            return;


        if (player == null)
        {
            GameObject jugador =
                GameObject.FindWithTag("Player");

            if (jugador != null)
            {
                player = jugador.transform;
            }

            return;
        }


        tiempoDesdeVuelo += Time.deltaTime;
        tiempoDesdeAtaque += Time.deltaTime;


        float distancia = Vector2.Distance(
            transform.position,
            player.position
        );


        jugadorDetectado =
            distancia <= detectionRange;


        ActualizarAnimaciones();
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
            case Estado.Idle:

                if (!jugadorDetectado)
                    Patrullar();
                else
                    DetenerMovimiento();

                break;


            case Estado.Patrullando:

                Patrullar();

                break;


            case Estado.Atacando:

                DetenerMovimiento();

                break;


            case Estado.Caminando:

                MoverHaciaJugador();

                break;


            case Estado.Subiendo:

                DetenerMovimiento();

                break;


            case Estado.Volando:

                PerseguirJugadorEnElAire();

                break;


            case Estado.Bajando:

                DetenerMovimiento();

                break;


            case Estado.Muriendo:

                DetenerMovimiento();

                break;
        }
    }


    // =========================================================
    // IA PRINCIPAL
    // =========================================================

    private IEnumerator IAJefe()
    {
        while (!muerto)
        {
            // =================================================
            // JUGADOR FUERA DEL ÁREA
            // =================================================

            while (!jugadorDetectado && !muerto)
            {
                isAttacking = false;

                CambiarEstado(Estado.Patrullando);

                yield return null;
            }


            if (muerto)
                yield break;


            // =================================================
            // JUGADOR DETECTADO
            // =================================================

            while (jugadorDetectado && !muerto)
            {
                float distancia = Vector2.Distance(
                    transform.position,
                    player.position
                );


                // =================================================
                // ATAQUE AÉREO
                // =================================================

                float tiempoVueloActual =
                    faseDos
                    ? timeBetweenFlights * 0.65f
                    : timeBetweenFlights;


                if (
                    tiempoDesdeVuelo >= tiempoVueloActual &&
                    tiempoDesdeAtaque >= timeBetweenAttacks
                )
                {
                    yield return StartCoroutine(
                        AtaqueAereo()
                    );


                    tiempoDesdeVuelo = 0f;
                    tiempoDesdeAtaque = 0f;

                    continue;
                }


                // =================================================
                // ATAQUE TERRESTRE
                // =================================================

                if (
                    distancia <= attackRange &&
                    tiempoDesdeAtaque >= timeBetweenAttacks
                )
                {
                    yield return StartCoroutine(
                        AtaqueTerrestre()
                    );


                    tiempoDesdeAtaque = 0f;

                    continue;
                }


                // =================================================
                // PERSEGUIR
                // =================================================

                MirarAlJugador();

                isAttacking = false;

                CambiarEstado(Estado.Caminando);

                yield return null;
            }


            // =================================================
            // JUGADOR SALIÓ
            // =================================================

            isAttacking = false;

            CambiarEstado(Estado.Patrullando);

            yield return null;
        }
    }


    // =========================================================
    // ATAQUE TERRESTRE
    // =========================================================

    private IEnumerator AtaqueTerrestre()
    {
        if (muerto || player == null)
            yield break;


        MirarAlJugador();


        bool ataqueCaminando =
            Random.value > 0.5f;


        isAttacking = true;


        // =====================================================
        // ATAQUE CAMINANDO
        // =====================================================

        if (ataqueCaminando)
        {
            CambiarEstado(Estado.Caminando);


            float tiempo = 0f;


            while (
                tiempo < attackWalkDuration &&
                !muerto &&
                jugadorDetectado
            )
            {
                tiempo += Time.deltaTime;

                MirarAlJugador();

                MoverHaciaJugador();

                yield return null;
            }
        }


        // =====================================================
        // ATAQUE QUIETO
        // =====================================================

        else
        {
            CambiarEstado(Estado.Atacando);

            yield return new WaitForSeconds(
                attackDuration
            );
        }


        isAttacking = false;


        if (muerto)
            yield break;


        CambiarEstado(Estado.Idle);
    }


    // =========================================================
    // ATAQUE AÉREO
    // =========================================================

    private IEnumerator AtaqueAereo()
    {
        if (player == null)
            yield break;


        isAttacking = true;


        // =====================================================
        // SUBIR
        // =====================================================

        CambiarEstado(Estado.Subiendo);


        float alturaObjetivo =
            posicionSuelo.y + flightHeight;


        while (
            transform.position.y <
            alturaObjetivo - 0.05f &&
            !muerto
        )
        {
            Vector2 nuevaPosicion =
                rb.position;


            nuevaPosicion.y =
                Mathf.MoveTowards(
                    nuevaPosicion.y,
                    alturaObjetivo,
                    flightSpeed *
                    Time.fixedDeltaTime
                );


            rb.MovePosition(nuevaPosicion);


            yield return new WaitForFixedUpdate();
        }


        if (muerto)
            yield break;


        // =====================================================
        // VUELO
        // =====================================================

        CambiarEstado(Estado.Volando);


        float tiempoVuelo = 0f;


        while (
            tiempoVuelo < flightDuration &&
            !muerto &&
            jugadorDetectado
        )
        {
            tiempoVuelo += Time.deltaTime;

            yield return null;
        }


        if (muerto)
            yield break;


        // =====================================================
        // BAJAR
        // =====================================================

        CambiarEstado(Estado.Bajando);


        while (
            transform.position.y >
            posicionSuelo.y + 0.05f &&
            !muerto
        )
        {
            Vector2 nuevaPosicion =
                rb.position;


            nuevaPosicion.y =
                Mathf.MoveTowards(
                    nuevaPosicion.y,
                    posicionSuelo.y,
                    flightSpeed *
                    Time.fixedDeltaTime
                );


            rb.MovePosition(nuevaPosicion);


            yield return new WaitForFixedUpdate();
        }


        if (muerto)
            yield break;


        rb.position = new Vector2(
            rb.position.x,
            posicionSuelo.y
        );


        isAttacking = false;

        CambiarEstado(Estado.Idle);


        yield return new WaitForSeconds(0.5f);
    }


    // =========================================================
    // MOVIMIENTO TERRESTRE
    // =========================================================

    private void MoverHaciaJugador()
    {
        if (player == null)
            return;


        if (player.position.x < transform.position.x)
        {
            direccion = -1;
        }
        else
        {
            direccion = 1;
        }


        // Detectar pared

        if (wallCheck != null)
        {
            bool hayPared =
                Physics2D.Raycast(
                    wallCheck.position,
                    Vector2.right * direccion,
                    wallCheckDistance,
                    wallLayer
                );


            if (hayPared)
            {
                direccion *= -1;
            }
        }


        float velocidadActual =
            faseDos
            ? walkSpeed * 1.25f
            : walkSpeed;


        rb.linearVelocity =
            new Vector2(
                direccion * velocidadActual,
                rb.linearVelocity.y
            );


        ActualizarRotacion();
    }


    // =========================================================
    // PATRULLA
    // =========================================================

    private void Patrullar()
    {
        if (muerto || rb == null)
            return;


        // =====================================================
        // PAUSA
        // =====================================================

        if (patrullaPausada)
        {
            patrolPauseTimer +=
                Time.fixedDeltaTime;


            DetenerMovimiento();


            if (
                patrolPauseTimer >=
                patrolPauseDuration
            )
            {
                patrullaPausada = false;

                patrolPauseTimer = 0f;

                patrolTimer = 0f;

                patrolDirection *= -1;
            }


            return;
        }


        patrolTimer +=
            Time.fixedDeltaTime;


        // =====================================================
        // CAMBIAR DIRECCIÓN
        // =====================================================

        if (
            patrolTimer >=
            patrolChangeTime
        )
        {
            patrolTimer = 0f;

            patrullaPausada = true;

            DetenerMovimiento();

            return;
        }


        // =====================================================
        // PARED
        // =====================================================

        if (wallCheck != null)
        {
            bool hayPared =
                Physics2D.Raycast(
                    wallCheck.position,
                    Vector2.right *
                    patrolDirection,
                    wallCheckDistance,
                    wallLayer
                );


            if (hayPared)
            {
                patrolDirection *= -1;

                patrolTimer = 0f;

                patrullaPausada = true;

                DetenerMovimiento();

                return;
            }
        }


        float velocidadActual =
            faseDos
            ? patrolSpeed * 1.2f
            : patrolSpeed;


        rb.linearVelocity =
            new Vector2(
                patrolDirection *
                velocidadActual,
                rb.linearVelocity.y
            );


        direccion = patrolDirection;

        ActualizarRotacion();
    }


    // =========================================================
    // MOVIMIENTO AÉREO
    // =========================================================

    private void PerseguirJugadorEnElAire()
    {
        if (player == null)
            return;


        float diferenciaX =
            player.position.x -
            transform.position.x;


        if (Mathf.Abs(diferenciaX) > 0.1f)
        {
            direccion =
                diferenciaX < 0
                ? -1
                : 1;
        }


        if (wallCheck != null)
        {
            bool hayPared =
                Physics2D.Raycast(
                    wallCheck.position,
                    Vector2.right * direccion,
                    wallCheckDistance,
                    wallLayer
                );


            if (hayPared)
            {
                direccion *= -1;
            }
        }


        float velocidadActual =
            faseDos
            ? flightSpeed * 1.2f
            : flightSpeed;


        rb.linearVelocity =
            new Vector2(
                direccion *
                velocidadActual,
                0f
            );


        ActualizarRotacion();
    }


    // =========================================================
    // MIRAR AL JUGADOR
    // =========================================================

    private void MirarAlJugador()
    {
        if (player == null)
            return;


        if (
            player.position.x <
            transform.position.x
        )
        {
            direccion = -1;
        }
        else
        {
            direccion = 1;
        }


        ActualizarRotacion();
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
    // DETENER MOVIMIENTO
    // =========================================================

    private void DetenerMovimiento()
    {
        rb.linearVelocity =
            new Vector2(
                0f,
                rb.linearVelocity.y
            );
    }


    // =========================================================
    // CAMBIO DE ESTADO
    // =========================================================

    private void CambiarEstado(
        Estado nuevoEstado
    )
    {
        if (muerto)
            return;


        if (
            estadoActual ==
            nuevoEstado
        )
            return;


        estadoActual =
            nuevoEstado;


        if (animator == null)
            return;


        switch (nuevoEstado)
        {
            case Estado.Idle:

                animator.SetBool(
                    "isWalking",
                    false
                );

                animator.SetBool(
                    "isAttacking",
                    false
                );

                break;


            case Estado.Patrullando:

                animator.SetBool(
                    "isWalking",
                    true
                );

                animator.SetBool(
                    "isAttacking",
                    false
                );

                break;


            case Estado.Caminando:

                animator.SetBool(
                    "isWalking",
                    true
                );

                animator.SetBool(
                    "isAttacking",
                    isAttacking
                );

                break;


            case Estado.Subiendo:

                animator.SetBool(
                    "isWalking",
                    false
                );

                animator.SetBool(
                    "isAttacking",
                    false
                );

                animator.SetTrigger(
                    "FlyUp"
                );

                break;


            case Estado.Volando:

                animator.SetBool(
                    "isWalking",
                    false
                );

                animator.SetBool(
                    "isAttacking",
                    false
                );

                break;


            case Estado.Bajando:

                animator.SetBool(
                    "isWalking",
                    false
                );

                animator.SetBool(
                    "isAttacking",
                    false
                );

                animator.SetTrigger(
                    "FlyDown"
                );

                break;


            case Estado.Atacando:

                animator.SetBool(
                    "isWalking",
                    false
                );

                animator.SetBool(
                    "isAttacking",
                    true
                );

                animator.SetTrigger(
                    "Attack"
                );

                break;


            case Estado.Muriendo:

                animator.SetBool(
                    "isWalking",
                    false
                );

                animator.SetBool(
                    "isAttacking",
                    false
                );

                animator.SetTrigger(
                    "Death"
                );

                break;
        }
    }


    // =========================================================
    // ANIMACIONES
    // =========================================================

    private void ActualizarAnimaciones()
    {
        if (animator == null)
            return;


        bool caminando =
            estadoActual == Estado.Caminando ||
            estadoActual == Estado.Patrullando;


        animator.SetBool(
            "isWalking",
            caminando
        );


        animator.SetBool(
            "isAttacking",
            isAttacking
        );
    }


    // =========================================================
    // DISPARO DESDE ANIMATION EVENT
    // =========================================================

    public void DispararDesdeAnimacion()
    {
        if (muerto)
            return;


        // =====================================================
        // COOLDOWN DEL DISPARO
        // =====================================================

        if (Time.time < nextShotTime)
            return;


        if (bulletPrefab == null)
        {
            Debug.LogWarning(
                "FinalBoss: bulletPrefab no asignado."
            );

            return;
        }


        if (firePoint == null)
        {
            Debug.LogWarning(
                "FinalBoss: firePoint no asignado."
            );

            return;
        }


        if (player == null)
            return;


        Vector2 direccionDisparo =
            (
                player.position -
                firePoint.position
            ).normalized;


        GameObject bullet =
            Instantiate(
                bulletPrefab,
                firePoint.position,
                Quaternion.identity
            );


        Bullet bulletScript =
            bullet.GetComponent<Bullet>();


        if (bulletScript != null)
        {
            bulletScript.SetOwner(
                gameObject
            );


            bulletScript.SetDirection(
                direccionDisparo
            );
        }


        float cooldownActual =
            faseDos
            ? timeBetweenShots * 0.7f
            : timeBetweenShots;


        nextShotTime =
            Time.time +
            cooldownActual;
    }


    // =========================================================
    // VIDA
    // =========================================================

    public void TakeDamage(float amount)
    {
        if (muerto)
            return;


        currentHealth -= amount;


        currentHealth =
            Mathf.Max(
                0f,
                currentHealth
            );


        Debug.Log(
            "Final Boss recibió " +
            amount +
            " de daño. Vida: " +
            currentHealth
        );


        // =====================================================
        // FASE 2
        // =====================================================

        if (
            !faseDos &&
            currentHealth <=
            maxHealth * 0.5f
        )
        {
            ActivarFaseDos();
        }


        // =====================================================
        // MUERTE
        // =====================================================

        if (currentHealth <= 0f)
        {
            Morir();

            return;
        }


        // =====================================================
        // REACCIÓN AL DAÑO
        // =====================================================

        if (spriteRenderer != null)
        {
            StartCoroutine(
                FlashDaño()
            );
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
            "🔥 FINAL BOSS ENTRA EN FASE 2"
        );


        // =====================================================
        // AUMENTAR RANGOS DE DETECCIÓN
        // =====================================================

        detectionRange = detectionRangeFaseDos;
        attackRange = attackRangeFaseDos;


        // =====================================================
        // REINICIAR TEMPORIZADORES
        // =====================================================

        tiempoDesdeAtaque =
            timeBetweenAttacks;


        tiempoDesdeVuelo =
            timeBetweenFlights;
    }


    // =========================================================
    // FLASH DE DAÑO
    // =========================================================

    private IEnumerator FlashDaño()
    {
        if (spriteRenderer == null)
            yield break;


        Color colorOriginal =
            spriteRenderer.color;


        spriteRenderer.color =
            Color.white;


        yield return new WaitForSeconds(
            0.08f
        );


        if (!muerto)
        {
            spriteRenderer.color =
                colorOriginal;
        }
    }


    // =========================================================
    // MUERTE
    // =========================================================

    private void Morir()
    {
        if (muerto)
            return;


        muerto = true;


        estadoActual =
            Estado.Muriendo;


        isAttacking = false;


        if (rutinaJefe != null)
        {
            StopCoroutine(
                rutinaJefe
            );
        }


        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;
        }


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


            animator.SetTrigger(
                "Death"
            );
        }


        StartCoroutine(
            DestruirDespuesDeMorir()
        );
    }


    private IEnumerator DestruirDespuesDeMorir()
    {
        yield return new WaitForSeconds(
            2f
        );


        Destroy(gameObject);
    }


    // =========================================================
    // GIZMOS
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        // =====================================================
        // DETECCIÓN
        // =====================================================

        Gizmos.color =
            Color.yellow;


        Gizmos.DrawWireSphere(
            transform.position,
            detectionRange
        );


        // =====================================================
        // ATAQUE
        // =====================================================

        Gizmos.color =
            Color.red;


        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );


        // =====================================================
        // PARED
        // =====================================================

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


        // =====================================================
        // ALTURA DE VUELO
        // =====================================================

        Gizmos.color =
            Color.cyan;


        Vector3 posicionVuelo =
            Application.isPlaying
            ? posicionSuelo
            : transform.position;


        posicionVuelo.y +=
            flightHeight;


        Gizmos.DrawLine(
            new Vector3(
                transform.position.x - 1f,
                posicionVuelo.y,
                transform.position.z
            ),
            new Vector3(
                transform.position.x + 1f,
                posicionVuelo.y,
                transform.position.z
            )
        );
    }
}