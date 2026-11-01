
using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    private Vector3 spawnPosition;
    private Rigidbody rb;

    void Start()
    {
        spawnPosition = transform.position;
        rb = GetComponent<Rigidbody>();
    }

    public void Respawn()
    {
        // Stop movement before teleporting
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Return player to starting position
        rb.position = spawnPosition;

        Debug.Log("HERE I COME SAN FRANCISCO");
    }
}
