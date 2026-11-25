
using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int maxHealth = 3;
    public float strength = 1f;

    [Header("Squish Settings")]
    public float squishDuration = 0.5f;
    public float squishScale = 0.3f;
    public float recoveryDelay = 1f;

    [Header("Recovery Animation")]
    public float unsquishDuration = 0.4f;
    public float bounceStrength = 1.70158f;

    [Header("Current Status")]
    public int currentHealth;
    public bool isSquished = false;
    public bool isTrapped = false;
    public bool isDead = false;

    private Vector3 originalScale;
    private Rigidbody trappingPlatform;

    void Start()
    {
        currentHealth = maxHealth;
        originalScale = transform.localScale;
    }

    public void Squish(Rigidbody platform, float platformWeight)
    {
        if (isDead || isSquished)
            return;

        currentHealth = Mathf.Max(0, currentHealth - 1);

        isSquished = true;

        transform.localScale = new Vector3(
            originalScale.x,
            originalScale.y * squishScale,
            originalScale.z
        );

        Debug.Log(gameObject.name + " got squished! HP: "
            + currentHealth);

        if (currentHealth <= 0)
        {
            StartCoroutine(Die());
            return;
        }

        trappingPlatform = platform;

        isTrapped = strength < platformWeight;

        StartCoroutine(Recover());
    }

    IEnumerator Recover()
    {
        yield return new WaitForSeconds(squishDuration);

        if (!isTrapped && trappingPlatform != null)
        {
            // Strong enemies push the platform upward
            trappingPlatform.AddForce(
                Vector3.up * strength * 2f,
                ForceMode.Impulse
            );
        }

        float recoveryTimer = 0f;

        while (recoveryTimer < recoveryDelay)
        {
            if (PlatformAbove())
            {
                isTrapped = true;
                recoveryTimer = 0f;
            }
            else
            {
                isTrapped = false;
                recoveryTimer += Time.deltaTime;
            }

            yield return null;
        }

        // Play bouncy recovery animation
        yield return StartCoroutine(UnsquishAnimation());

        isSquished = false;
        isTrapped = false;
        trappingPlatform = null;

        Debug.Log(gameObject.name + " recovered!");
    }

    IEnumerator UnsquishAnimation()
    {
        Vector3 startingScale = transform.localScale;

        float elapsed = 0f;

        while (elapsed < unsquishDuration)
        {
            // Stop recovering if the platform returns
            if (PlatformAbove())
            {
                transform.localScale = startingScale;

                float timer = 0f;

                while (timer < recoveryDelay)
                {
                    isTrapped = PlatformAbove();

                    if (isTrapped)
                        timer = 0f;
                    else
                        timer += Time.deltaTime;

                    yield return null;
                }

                elapsed = 0f;
            }

            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / unsquishDuration
            );

            float bounce = EaseOutBack(t);

            transform.localScale = Vector3.LerpUnclamped(
                startingScale,
                originalScale,
                bounce
            );

            yield return null;
        }

        transform.localScale = originalScale;
    }

    float EaseOutBack(float t)
    {
        float c1 = bounceStrength;
        float c3 = c1 + 1f;

        return 1f +
            c3 * Mathf.Pow(t - 1f, 3f) +
            c1 * Mathf.Pow(t - 1f, 2f);
    }

    bool PlatformAbove()
    {
        if (trappingPlatform == null)
            return false;

        Collider platformCollider =
            trappingPlatform.GetComponent<Collider>();

        Collider enemyCollider =
            GetComponent<Collider>();

        if (platformCollider == null || enemyCollider == null)
            return false;

        if (!platformCollider.enabled ||
            !trappingPlatform.gameObject.activeInHierarchy)
            return false;

        Bounds p = platformCollider.bounds;
        Bounds e = enemyCollider.bounds;

        float currentScaleRatio =
            Mathf.Abs(transform.localScale.y) /
            Mathf.Max(Mathf.Abs(originalScale.y), 0.01f);

        float recoveryHeight =
            e.size.y / Mathf.Max(currentScaleRatio, 0.01f);

        return p.max.x > e.min.x &&
               p.min.x < e.max.x &&
               p.max.z > e.min.z &&
               p.min.z < e.max.z &&
               p.center.y > e.center.y &&
               p.min.y < e.min.y + recoveryHeight;
    }

    IEnumerator Die()
    {
        isDead = true;

        Debug.Log(gameObject.name + " McDied!");

        yield return new WaitForSeconds(squishDuration);

        Destroy(gameObject);
    }
}
