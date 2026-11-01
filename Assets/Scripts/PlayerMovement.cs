
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    private Rigidbody rb;
    private float horizontalInput;
    private bool isGrounded;
    public Animator animator;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
        animator.SetTrigger("Jump");

        SpriteRenderer spriteRenderer = animator.GetComponent<SpriteRenderer>();
        if (horizontalInput > 0)
        {
            spriteRenderer.flipX = false; // Facing right
        }
        else if (horizontalInput < 0)
        {
            spriteRenderer.flipX = true;  // Facing left
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }

        animator.SetBool("IsGrounded", isGrounded);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector3(
            horizontalInput * moveSpeed,
            rb.linearVelocity.y,
            0f
        );
    }

    void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                break;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
}
