using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;

    public Vector3 offset;
    public Vector3 maxOffset;

    void Update()
    {
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
