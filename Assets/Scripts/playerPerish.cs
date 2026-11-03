
using System.Collections;
using UnityEngine;

public class playerPerish : MonoBehaviour
{
    public Animator animator;
    public CameraFollow cameraFollow;

    public float bounceForce = 8f;
    public float respawnDelay = 2f;

    private Rigidbody rb;
    private PlayerRespawn playerRespawn;
    private PlayerMovement playerMovement;
    private Collider playerCollider;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerRespawn = GetComponent<PlayerRespawn>();
        playerMovement = GetComponent<PlayerMovement>();
        playerCollider = GetComponent<Collider>();
    }

    public void Die()
    {
        if (isDead) return;

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        isDead = true;

        // Stop player controls
        playerMovement.enabled = false;

        // Play death animation
        animator.SetBool("IsDead", true);

        // Freeze camera at the death location
        cameraFollow.FreezeCamera();

        // Allow the player to fall through platforms
        playerCollider.enabled = false;

        // Stop existing momentum
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // SONIC-STYLE DEATH BOUNCE!
        rb.AddForce(Vector3.up * bounceForce, ForceMode.Impulse);

        // Wait while she flies upward and falls
        yield return new WaitForSeconds(respawnDelay);

        // Restore player
        playerRespawn.Respawn();

        playerCollider.enabled = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        animator.SetBool("IsDead", false);
        animator.ResetTrigger("Jump");
        animator.SetFloat("Speed", 0f);
        animator.SetBool("IsGrounded", true);

        playerMovement.enabled = true;
        cameraFollow.UnfreezeCamera();

        isDead = false;
    }
}
