using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;

    public Vector3 offset;
    public Vector3 maxOffset;

    void Update()
    {
        if (player == null)
        {
            GameObject jugador = GameObject.FindWithTag("Player");

            if (jugador != null)
            {
                player = jugador.transform;
            }

            return;
        }

        if (player == null)
            return;

        float posX = player.position.x;
        float posY = player.position.y;

        posX = Mathf.Clamp(posX, offset.x, maxOffset.x);
        posY = Mathf.Clamp(posY, offset.y, maxOffset.y);

        transform.position = new Vector3(
            posX,
            posY,
            offset.z
        );
    }
}
