using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 5f;
    public float fuerzaSalto = 8f;
    public float velocidadCorrer = 9f;
    public bool canShoot = true;
    private bool isDying = false;

    [Header("Dash")]
    public bool canDash = false;
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    private bool isDashing = false;
    private float dashCooldownTimer = 0f;

    [Header("Vida")]
    public float maxHealth = 3f;
    public float currentHealth;

    private Rigidbody2D rb;
    private float movimiento;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool estaEnSuelo;
    private Vector2 direccionDisparo = Vector2.right;
    private Weapon weapon;
    private AudioSource audioSource;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("Audioa Daño")]
    public AudioClip[] dannioSounds;

    [Header("Audioa Victoria")]
    public AudioClip[] victoriaSounds;

    [Header("Audioa Muerte")]
    public AudioClip[] muerteSounds;

    [System.Serializable]
    public class RangoVida
    {
        public int escenaInicial;
        public int escenaFinal;
        public float vidaMaxima;
    }

    [Header("Vida por escena")]
    [SerializeField] private RangoVida[] rangosVida;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        ConfigurarVidaPorEscena();
        weapon = GetComponentInChildren<Weapon>();
        audioSource = GetComponent<AudioSource>();

        // Respawn en checkpoint si existe
        if (GameManager.instance != null && GameManager.instance.HasCheckpoint())
        {
            transform.position = GameManager.instance.GetCheckpointPosition();
        }
        else
        {
            SpawnPoint[] spawnPoints = FindObjectsByType<SpawnPoint>();
            foreach (SpawnPoint spawnPoint in spawnPoints)
            {
                if (spawnPoint.spawnPointName == PlayerSpawnManager.spawnPointName)
                {
                    transform.position = spawnPoint.transform.position;
                    break;
                }
            }
        }
    }

    void Update()
    {
        movimiento = 0f;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            movimiento = -1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            movimiento = 1f;

        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        if (Keyboard.current.kKey.wasPressedThisFrame && canDash && !isDashing && dashCooldownTimer <= 0f)
        {
            StartCoroutine(Dash());
        }

        bool corriendo = Keyboard.current.leftCtrlKey.isPressed && movimiento != 0;
        animator.SetBool("isRunning", corriendo);
        animator.SetBool("isWalking", movimiento != 0 && !corriendo);

        if (Keyboard.current.spaceKey.wasPressedThisFrame && estaEnSuelo)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
            animator.SetTrigger("Jump");
        }

        if (movimiento != 0)
        {
            bool mirandoIzquierda = movimiento < 0;
            transform.rotation = Quaternion.Euler(0f, mirandoIzquierda ? 180f : 0f, 0f);
            direccionDisparo = mirandoIzquierda ? Vector2.left : Vector2.right;
        }

        estaEnSuelo = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        animator.SetBool("isGrounded", estaEnSuelo);
        animator.SetBool("isFalling", rb.linearVelocity.y < 0 && !estaEnSuelo);

        if ((Keyboard.current.jKey.isPressed) && estaEnSuelo && canShoot)
        {
            animator.SetTrigger("Attack");
            weapon.Shoot(direccionDisparo);
            animator.SetBool("isAttacking", true);
            weapon.SetVisible(true);
        }

        if (!Keyboard.current.jKey.isPressed)
        {
            animator.SetBool("isAttacking", false);
            weapon.SetVisible(false);
        }
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        dashCooldownTimer = dashCooldown;

        animator.SetTrigger("Dash");

        float direccionDash;

        if (movimiento != 0)
        {
            direccionDash = movimiento;
        }
        else
        {
            direccionDash = transform.eulerAngles.y == 180f ? -1f : 1f;
        }

        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < dashDuration)
        {
            rb.linearVelocity = new Vector2(
                direccionDash * dashSpeed,
                rb.linearVelocity.y
            );

            tiempoTranscurrido += Time.deltaTime;

            yield return null;
        }

        isDashing = false;
    }

    private void ConfigurarVidaPorEscena()
    {
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;

        foreach (RangoVida rango in rangosVida)
        {
            if (sceneIndex >= rango.escenaInicial &&
                sceneIndex <= rango.escenaFinal)
            {
                maxHealth = rango.vidaMaxima;
                currentHealth = maxHealth;
                return;
            }
        }

        // Si la escena no pertenece a ningún rango,
        // conserva el maxHealth configurado originalmente.
        currentHealth = maxHealth;
    }

    private IEnumerator FlashDamage()
    {
        spriteRenderer.color = Color.red;

        yield return new WaitForSeconds(0.15f);

        spriteRenderer.color = Color.white;
    }

    public void TakeDamage(float amount)
    {
        if (isDying)
            return;

        StartCoroutine(FlashDamage());
        ReproducirSonidoAleatorio(dannioSounds);

        currentHealth -= amount;
        currentHealth = Mathf.Max(0f, currentHealth);

        Debug.Log($"Vida actual: {currentHealth}");

        if (currentHealth <= 0f)
        {
            isDying = true;
            StartCoroutine(Muerte());
        }
    }

    private IEnumerator Muerte()
    {
        ReproducirSonidoAleatorio(muerteSounds);

        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        yield return new WaitForSeconds(2f);

        Die();
    }

    void Die()
    {
        if (GameManager.instance != null)
            GameManager.instance.PlayerDied();
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void PlayVictoriaAudios()
    {
        ReproducirSonidoAleatorio(victoriaSounds);
    }

    void ReproducirSonidoAleatorio(AudioClip[] sonidos)
    {
        if (sonidos == null || sonidos.Length == 0)
            return;

        int indiceAleatorio = UnityEngine.Random.Range(0, sonidos.Length);

        audioSource.PlayOneShot(sonidos[indiceAleatorio]);
    }

    void FixedUpdate()
    {
        if (isDashing)
            return;

        float velocidadActual = Keyboard.current.leftCtrlKey.isPressed
            ? velocidadCorrer
            : velocidad;

        rb.linearVelocity = new Vector2(
            movimiento * velocidadActual,
            rb.linearVelocity.y
        );
    }
}