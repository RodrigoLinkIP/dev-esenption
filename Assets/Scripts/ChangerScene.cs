using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ChangerScene : MonoBehaviour
{
    public int sceneBuildIndex;

    [Tooltip("Nombre del Spawn donde aparecerá el Player en la siguiente escena")]
    public string spawnPointName;

    [Tooltip("Si está activo, el jugador debe presionar E para cambiar de escena")]
    public bool requiereTecla = false;

    private bool playerDentro = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerDentro = true;

            if (!requiereTecla)
            {
                CambiarEscena();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerDentro = false;
        }
    }

    private void Update()
    {
        if (requiereTecla && playerDentro)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                CambiarEscena();
            }
        }
    }

    private void CambiarEscena()
    {
        PlayerSpawnManager.spawnPointName = spawnPointName;

        SceneManager.LoadScene(
            sceneBuildIndex,
            LoadSceneMode.Single
        );
    }
}
