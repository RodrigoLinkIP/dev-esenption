using UnityEngine;

public class ArenaManager : MonoBehaviour
{
    [Header("Puerta")]
    [SerializeField] private ChangerScene puerta;

    [Header("Spawner")]
    [SerializeField] private DroneSpawner droneSpawner;

    [Header("Música")]
    [SerializeField] private AudioSource musicaArena;

    [Header("NPC oculto")]
    [SerializeField] private GameObject npcAtrapado;

    [Header("Identificador")]
    [SerializeField] private string arenaID = "Arena_1";

    [Header("Diálogo inicial")]
    [SerializeField] private GameObject dialogoAyuda;

    private bool arenaActiva = false;
    private bool arenaCompletada = false;
    private int dronesVivos = 0;

    void Start()
    {
        // NPC atrapado invisible al inicio
        if (npcAtrapado != null)
        {
            // Si la arena ya fue completada, mostrar NPC directamente
            if (GameManager.instance != null &&
                GameManager.instance.ArenaEstaCompletada(arenaID))
            {
                npcAtrapado.SetActive(true);
            }
            else
            {
                npcAtrapado.SetActive(false);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !arenaActiva && !arenaCompletada)
        {
            ActivarArena();
        }
    }

    void ActivarArena()
    {



        // Si ya fue completada, no hacer nada
        if (GameManager.instance != null &&
            GameManager.instance.ArenaEstaCompletada(arenaID))
        {
            arenaCompletada = true;
            return;
        }

        arenaActiva = true;

        // 1. Bloquear puerta
        if (puerta != null)
            puerta.bloqueado = true;

        // 2. Música
        if (musicaArena != null)
            musicaArena.Play();

        // 3. Spawnear drones
        if (droneSpawner != null)
        {
            dronesVivos = droneSpawner.CantidadDrones();
            droneSpawner.SpawnDrones(this);
        }
    }

    public void NotificarDroneMuerto()
    {
        dronesVivos--;
        Debug.Log($"Drones restantes: {dronesVivos}");

        if (dronesVivos <= 0)
            CompletarArena();
    }

    void CompletarArena()
    {
        arenaCompletada = true;
        arenaActiva = false;

        if (GameManager.instance != null)
            GameManager.instance.MarcarArenaCompletada(arenaID);

        // 1. Desbloquear puerta
        if (puerta != null)
            puerta.bloqueado = false;

        // 2. Detener música
        if (musicaArena != null)
            musicaArena.Stop();

        // 3. Aparecer NPC atrapado
        if (npcAtrapado != null)
            npcAtrapado.SetActive(true);

        if (dialogoAyuda != null) 
            dialogoAyuda.SetActive(false);
    }
}