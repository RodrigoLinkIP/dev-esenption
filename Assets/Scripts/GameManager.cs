using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // Datos que persisten entre escenas
    private string checkpointScene;
    private Vector3 checkpointPosition;
    private bool hasCheckpoint = false;
    private HashSet<string> npcsCompletados = new HashSet<string>();
    private HashSet<string> arenasCompletadas = new HashSet<string>();

    public void MarcarNPCCompletado(string npcID)
    {
        npcsCompletados.Add(npcID);
    }

    public bool NPCEstaCompletado(string npcID)
    {
        return npcsCompletados.Contains(npcID);
    }

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
            SceneManager.LoadScene(0); // reinicia escena del lobby
    }

    public void MarcarArenaCompletada(string arenaID)
    {
        arenasCompletadas.Add(arenaID);
    }

    public bool ArenaEstaCompletada(string arenaID)
    {
        return arenasCompletadas.Contains(arenaID);
    }

    public bool HasCheckpoint() => hasCheckpoint;
    public Vector3 GetCheckpointPosition() => checkpointPosition;
    public string GetCheckpointScene() => checkpointScene;
}