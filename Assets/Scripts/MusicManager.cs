using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    [Header("Música normal")]
    public AudioClip[] musicasNormales;

    [Header("Música de jefe")]
    public AudioClip musicaJefe;

    [Header("Configuración")]
    [Range(0f, 2f)]
    public float volumen = 0.5f;

    private AudioSource audioSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

            DontDestroyOnLoad(gameObject);

            audioSource = GetComponent<AudioSource>();

            audioSource.loop = true;
            audioSource.volume = volumen;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ReproducirMusicaNormal(int indice)
    {
        if (indice < 0 || indice >= musicasNormales.Length)
        {
            Debug.LogWarning("Índice de música inválido.");
            return;
        }

        AudioClip nuevaMusica = musicasNormales[indice];

        // Si ya está sonando esta música, no la reiniciamos
        if (audioSource.clip == nuevaMusica)
        {
            return;
        }

        audioSource.clip = nuevaMusica;
        audioSource.Play();
    }

    public void ReproducirMusicaJefe()
    {
        if (audioSource.clip == musicaJefe)
        {
            return;
        }

        audioSource.clip = musicaJefe;
        audioSource.Play();
    }

    public void DetenerMusica()
    {
        audioSource.Stop();
    }
}
