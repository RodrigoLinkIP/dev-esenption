using UnityEngine;

public class DroneSpawner : MonoBehaviour
{
    [SerializeField] private GameObject dronePrefab;
    [SerializeField] private Transform[] puntosDespawn; // posiciones desde donde caen
    public int cantidadDrones = 2;

    public int CantidadDrones() => cantidadDrones;

    public void SpawnDrones(ArenaManager arena)
    {
        for (int i = 0; i < cantidadDrones; i++)
        {
            // Elegir punto de spawn (alternar si hay varios)
            Transform punto = puntosDespawn[i % puntosDespawn.Length];

            GameObject drone = Instantiate(dronePrefab, punto.position, Quaternion.identity);

            // Notificar al ArenaManager cuando muera
            drone.GetComponent<Enemy>()?.SetArena(arena);
        }
    }
}