using UnityEngine;

public class SceneMusic : MonoBehaviour
{
    [Header("Música de esta escena")]
    public int musicaNormal;

    private void Start()
    {
        if (MusicManager.instance != null)
        {
            MusicManager.instance.ReproducirMusicaNormal(musicaNormal);
        }
    }
}
