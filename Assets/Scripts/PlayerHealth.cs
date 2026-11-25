using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Health")]
    public int maxHealth = 3;
    public int currentHealth;

    [Header("Invincibility Settings")]
    public float invincibilityDuration = 1.5f;
    public float flashInterval = 0.1f;

    [Header("References")]
    public SpriteRenderer spriteRenderer;

    [Header("Current Status")]
    public bool isInvincible = false;
    public bool isDead = false;

    private playerPerish perish;
    private PlayerCombo playerCombo;
    private Coroutine invincibilityCoroutine;

    void Awake()
    {
        currentHealth = maxHealth;

        perish = GetComponent<playerPerish>();
        playerCombo = GetComponent<PlayerCombo>();

        if (spriteRenderer == null)
        {
            spriteRenderer =
                GetComponentInChildren<SpriteRenderer>();
        }
    }

    public bool TakeDamage(int damage)
    {
        if (isInvincible || isDead || damage <= 0)
            return false;

        if (playerCombo != null)
            playerCombo.LoseCombo();

        currentHealth = Mathf.Max(
            0, currentHealth - damage
        );

        Debug.Log("Sketchsia HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            perish.Die();
            return false;
        }

        BeginInvincibility();

        return true;
    }

    void BeginInvincibility()
    {
        StopInvincibility();

        invincibilityCoroutine =
            StartCoroutine(InvincibilityFrames());
    }

    IEnumerator InvincibilityFrames()
    {
        isInvincible = true;

        float elapsed = 0f;
        float flashTimer = 0f;

        while (elapsed < invincibilityDuration)
        {
            elapsed += Time.deltaTime;
            flashTimer += Time.deltaTime;

            if (flashTimer >= flashInterval)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled =
                        !spriteRenderer.enabled;
                }

                flashTimer = 0f;
            }

            yield return null;
        }

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        isInvincible = false;
        invincibilityCoroutine = null;
    }

    void StopInvincibility()
    {
        if (invincibilityCoroutine != null)
        {
            StopCoroutine(invincibilityCoroutine);
            invincibilityCoroutine = null;
        }

        isInvincible = false;

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;
    }

    public void OnPlayerDeath()
    {
        StopInvincibility();

        if (playerCombo != null)
            playerCombo.LoseCombo();

        isDead = true;
        currentHealth = 0;

        Debug.Log("Sketchsia McDied!");
    }

    public void OnPlayerRespawn()
    {
        currentHealth = maxHealth;
        isDead = false;

        BeginInvincibility();

        Debug.Log("Health restored!");
    }

    [ContextMenu("Test Damage")]
    void TestDamage()
    {
        if (Application.isPlaying)
            TakeDamage(1);
    }
}