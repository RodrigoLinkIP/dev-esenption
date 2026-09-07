using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject playerA;
    public GameObject playerB;

    void Start()
    {
        GameObject jugador;

        if (CharacterSelectionData.personajeSeleccionado == 0)
        {
            jugador = Instantiate(
                playerA,
                transform.position,
                transform.rotation
            );
        }
        else
        {
            jugador = Instantiate(
                playerB,
                transform.position,
                transform.rotation
            );
        }

        SpawnPoint[] spawnPoints = FindObjectsByType<SpawnPoint>();

        foreach (SpawnPoint spawnPoint in spawnPoints)
        {
            if (spawnPoint.spawnPointName ==
                PlayerSpawnManager.spawnPointName)
            {
                jugador.transform.position =
                    spawnPoint.transform.position;

                break;
            }
        }
    }
}
