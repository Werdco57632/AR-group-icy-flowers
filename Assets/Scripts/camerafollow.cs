
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0, 2, -15);
    public float smoothSpeed = 5f;

    private bool isFrozen = false;

    void LateUpdate()
    {
        if (player == null || isFrozen) return;

        Vector3 targetPosition = player.position + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );
    }

    public void FreezeCamera()
    {
        isFrozen = true;
    }

    public void UnfreezeCamera()
    {
        isFrozen = false;
    }
}
