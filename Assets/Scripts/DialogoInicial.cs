using UnityEngine;

public class DialogoInicial : MonoBehaviour
{
    [SerializeField] private Dialogue dialogo;
    [SerializeField] private string arenaID = "Arena_1";

    void Start()
    {
        // Solo mostrar si la arena no ha sido completada
        if (GameManager.instance != null &&
            GameManager.instance.ArenaEstaCompletada(arenaID))
            return;

        if (dialogo != null)
            dialogo.StartDialogue(true);
    }
}