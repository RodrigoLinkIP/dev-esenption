using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // Datos que persisten entre escenas
    private string checkpointScene;
    private Vector3 checkpointPosition;
    private bool hasCheckpoint = false;

    void Awake()
    {
        // Singleton — solo existe un GameManager en todo el juego
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // sobrevive cambios de escena
        }
        else
        {
            Destroy(gameObject); // elimina duplicados
        }
    }

    public void SetCheckpoint(Vector3 position, string sceneName)
    {
        checkpointPosition = position;
        checkpointScene = sceneName;
        hasCheckpoint = true;
        Debug.Log($"Checkpoint guardado en {sceneName} : {position}");
    }

    public void PlayerDied()
    {
        if (hasCheckpoint)
            SceneManager.LoadScene(checkpointScene);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name); // reinicia escena actual
    }

    public bool HasCheckpoint() => hasCheckpoint;
    public Vector3 GetCheckpointPosition() => checkpointPosition;
    public string GetCheckpointScene() => checkpointScene;
}