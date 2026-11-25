using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Contact Damage")]
    public int contactDamage = 1;

    [Header("Knockback")]
    public float horizontalKnockback = 4f;
    public float verticalKnockback = 4f;

    [Header("Stomp Settings")]
    public float stompBounceForce = 8f;
    public float minimumFallSpeed = 0.1f;

    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;
    private PlayerCombo playerCombo;
    private Rigidbody rb;

    private float previousVerticalSpeed;

    void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerMovement = GetComponent<PlayerMovement>();
        playerCombo = GetComponent<PlayerCombo>();
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        previousVerticalSpeed = rb.linearVelocity.y;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (playerHealth.isDead)
            return;

        EnemyHealth enemy =
            collision.collider.GetComponentInParent<EnemyHealth>();

        if (enemy == null || enemy.isDead)
            return;

        if (IsStomp(collision))
        {
            if (!enemy.isSquished)
            {
                enemy.Squish(null, 0f);

                if (playerCombo != null)
                    playerCombo.RegisterStomp(enemy);

                Debug.Log("ENEMY GOT BONKED!");
            }

            playerMovement.StompBounce(
                stompBounceForce
            );

            return;
        }

        CheckEnemyContact(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        CheckEnemyContact(collision);
    }

    bool IsStomp(Collision collision)
    {
        if (previousVerticalSpeed >= -minimumFallSpeed)
            return false;

        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
                return true;
        }

        return false;
    }

    void CheckEnemyContact(Collision collision)
    {
        if (playerHealth.isDead ||
            playerHealth.isInvincible)
            return;

        EnemyHealth enemy =
            collision.collider.GetComponentInParent<EnemyHealth>();

        if (enemy == null)
            return;

        if (enemy.isDead || enemy.isSquished)
            return;

        bool damageTaken =
            playerHealth.TakeDamage(contactDamage);

        if (damageTaken)
        {
            float direction =
                transform.position.x >=
                enemy.transform.position.x ? 1f : -1f;

            playerMovement.ApplyKnockback(
                direction,
                horizontalKnockback,
                verticalKnockback
            );

            Debug.Log("SKETCHSIA GOT BONKED!");
        }
    }
}