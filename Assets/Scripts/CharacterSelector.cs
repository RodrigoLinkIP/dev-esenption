using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

public class CharacterSelector : MonoBehaviour
{

    [Header("Alvin")]
    public SpriteRenderer alvin;
    public SpriteRenderer alvinObject;
    public GameObject alvinInfo;
    public AudioClip[] alvinSounds;

    [Header("Juliana")]
    public SpriteRenderer juliana;
    public SpriteRenderer julianaObject;
    public GameObject jualianaInfo;
    public AudioClip[] julianaSounds;

    [Header("Audio")]
    public AudioSource audioSource;

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
            alvinObject.enabled = false;
            alvin.enabled = true;
            alvinInfo.SetActive(true);

            julianaObject.enabled = true;
            juliana.enabled = false;
            jualianaInfo.SetActive(false);

            ReproducirSonidoAleatorio(alvinSounds);
        }
        else
        {
            julianaObject.enabled = false;
            juliana.enabled = true;
            jualianaInfo.SetActive(true);

            alvinObject.enabled = true;
            alvin.enabled = false;
            alvinInfo.SetActive(false);

            ReproducirSonidoAleatorio(julianaSounds);
        }
    }

    void ReproducirSonidoAleatorio(AudioClip[] sonidos)
    {
        if (sonidos == null || sonidos.Length == 0)
            return;

        int indiceAleatorio = Random.Range(0, sonidos.Length);

        audioSource.PlayOneShot(sonidos[indiceAleatorio]);
    }
}
