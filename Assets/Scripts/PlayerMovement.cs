using UnityEngine;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    [Header("Knockback")]
    public float knockbackDuration = 0.2f;

    private Rigidbody rb;
    private PlayerCombo playerCombo;
    private PlayerHealth playerHealth;

    private float horizontalInput;
    private bool isGrounded;
    private float knockbackTimer;

    private HashSet<Collider> groundContacts =
        new HashSet<Collider>();

    public Animator animator;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerCombo = GetComponent<PlayerCombo>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        animator.SetFloat("Speed",
            Mathf.Abs(horizontalInput));

        SpriteRenderer spriteRenderer =
            animator.GetComponent<SpriteRenderer>();

        if (horizontalInput > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (horizontalInput < 0)
        {
            spriteRenderer.flipX = true;
        }

        if (Input.GetKeyDown(KeyCode.Space) &&
            isGrounded && knockbackTimer <= 0)
        {
            rb.AddForce(
                Vector3.up * jumpForce,
                ForceMode.Impulse
            );

            groundContacts.Clear();
            isGrounded = false;

            animator.SetTrigger("Jump");
        }

        animator.SetBool("IsGrounded", isGrounded);
    }

    void FixedUpdate()
    {
        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            return;
        }

        rb.linearVelocity = new Vector3(
            horizontalInput * moveSpeed,
            rb.linearVelocity.y,
            0f
        );
    }

    public void ApplyKnockback(
        float direction,
        float horizontalForce,
        float verticalForce)
    {
        knockbackTimer = knockbackDuration;

        groundContacts.Clear();
        isGrounded = false;

        rb.linearVelocity = new Vector3(
            direction * horizontalForce,
            verticalForce,
            0f
        );
    }

    public void StompBounce(float bounceForce)
    {
        groundContacts.Clear();
        isGrounded = false;

        knockbackTimer = 0f;

        rb.linearVelocity = new Vector3(
            rb.linearVelocity.x,
            bounceForce,
            0f
        );

        animator.SetTrigger("Jump");
    }

    void OnCollisionEnter(Collision collision)
    {
        CheckGround(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        CheckGround(collision);
    }

    void CheckGround(Collision collision)
    {
        if (playerHealth != null && playerHealth.isDead)
            return;

        if (collision.collider
            .GetComponentInParent<EnemyHealth>() != null)
            return;

        if (knockbackTimer > 0 ||
            rb.linearVelocity.y > 0.1f)
        {
            groundContacts.Remove(collision.collider);
            isGrounded = groundContacts.Count > 0;
            return;
        }

        bool validGround = false;

        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                validGround = true;
                break;
            }
        }

        if (validGround)
        {
            groundContacts.Add(collision.collider);
        }
        else
        {
            groundContacts.Remove(collision.collider);
        }

        isGrounded = groundContacts.Count > 0;
    }

    void LateUpdate()
    {
        // Bank after collision callbacks have finished.
        // Damage during the landing can still cancel the combo.
        if (isGrounded &&
            playerCombo != null &&
            (playerHealth == null || !playerHealth.isDead))
        {
            playerCombo.BankCombo();
        }
    }

    void OnCollisionExit(Collision collision)
    {
        groundContacts.Remove(collision.collider);
        isGrounded = groundContacts.Count > 0;
    }

    void OnDisable()
    {
        knockbackTimer = 0f;
        horizontalInput = 0f;

        groundContacts.Clear();
        isGrounded = false;
    }
}