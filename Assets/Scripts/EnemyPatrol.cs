using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    public float speed = 2f;
    public float patrolDistance = 4f;

    private Vector3 startPosition;
    private int direction = 1;
    private Rigidbody2D rb;

    void Start()
    {
        startPosition = transform.position;
        rb = GetComponent<Rigidbody2D>(); // gets the Rigidbody2D attached to this GameObject
    }

    void FixedUpdate()
    {
        
        // Move horizontally using physics, keep existing vertical velocity (gravity)
        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);

        float distanceTraveled = transform.position.x - startPosition.x;

        if (distanceTraveled > patrolDistance) direction = -1;
        else if (distanceTraveled < -patrolDistance) direction = 1;

        transform.rotation = Quaternion.Euler(0f, direction < 0 ? 180f : 0f, 0f);
    }
}