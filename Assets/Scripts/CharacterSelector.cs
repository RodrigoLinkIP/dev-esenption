using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CharacterSelector : MonoBehaviour
{
    public SpriteRenderer characterDisplay;

    public Sprite personajeA;
    public Sprite personajeB;

    private int personajeActual = 0;

    void Start()
    {
        ActualizarPersonaje();
    }

    void Update()
    {
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            personajeActual--;

            if (personajeActual < 0)
            {
                personajeActual = 1;
            }

            ActualizarPersonaje();
        }

        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            personajeActual++;

            if (personajeActual > 1)
            {
                personajeActual = 0;
            }

            ActualizarPersonaje();
        }

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            CharacterSelectionData.personajeSeleccionado = personajeActual;

            SceneManager.LoadScene(0);
        }
    }

    void ActualizarPersonaje()
    {
        if (personajeActual == 0)
        {
            characterDisplay.sprite = personajeA;
        }
        else
        {
            characterDisplay.sprite = personajeB;
        }
    }
}
