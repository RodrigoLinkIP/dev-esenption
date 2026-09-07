using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 5f;
    public float fuerzaSalto = 8f;
    public float velocidadCorrer = 9f;

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
    public bool canShoot = true;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        currentHealth = maxHealth;
        weapon = GetComponentInChildren<Weapon>();

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

        if ((Keyboard.current.jKey.isPressed || Keyboard.current.lKey.isPressed) && estaEnSuelo && canShoot)
        {
            animator.SetTrigger("Attack");
            weapon.Shoot(direccionDisparo);
            animator.SetBool("isAttacking", true);
            weapon.SetVisible(true);
        }

        if (!Keyboard.current.jKey.isPressed && !Keyboard.current.lKey.isPressed)
        {
            animator.SetBool("isAttacking", false);
            weapon.SetVisible(false);
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(0f, currentHealth);
        Debug.Log($"Vida actual: {currentHealth}");
        if (currentHealth <= 0f) Die();
    }

    void Die()
    {
        if (GameManager.instance != null)
            GameManager.instance.PlayerDied();
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void FixedUpdate()
    {
        float velocidadActual = Keyboard.current.leftCtrlKey.isPressed ? velocidadCorrer : velocidad;
        rb.linearVelocity = new Vector2(movimiento * velocidadActual, rb.linearVelocity.y);
    }
}