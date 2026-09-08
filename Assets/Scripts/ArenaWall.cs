using UnityEngine;

public class ArenaWall : MonoBehaviour
{
    [SerializeField] private string arenaID = "Arena_1";

    void Start()
    {
        if (GameManager.instance != null &&
            GameManager.instance.ArenaEstaCompletada(arenaID))
        {
            gameObject.SetActive(false);
        }
    }
}