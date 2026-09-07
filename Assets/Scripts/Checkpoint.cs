using UnityEngine;
using UnityEngine.SceneManagement;

public class Checkpoint : MonoBehaviour
{
    private bool activated = false;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !activated)
        {
            activated = true;
            GameManager.instance.SetCheckpoint(
                transform.position,
                SceneManager.GetActiveScene().name
            );

            if (sr != null) sr.color = Color.yellow;
        }
    }
}