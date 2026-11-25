using UnityEngine;
using TMPro;

public class PlayerMoney : MonoBehaviour
{
    [Header("Economy Settings")]
    public float scoreToMonsRate = 0.1f;

    [Range(0f, 1f)]
    public float deathPenalty = 0.1f;

    public int maximumTimeBonus = 100;

    [Header("Money")]
    public int levelMons = 0;
    public int walletMons = 0;

    [Header("Level Results")]
    public int scoreBonus = 0;
    public int timeBonus = 0;
    public int totalMonsEarned = 0;
    public int totalMonsLost = 0;
    public int finalReward = 0;
    public bool rewardsCollected = false;

    [Header("References")]
    public PlayerCombo playerCombo;
    public PlayerHealth playerHealth;
    public TMP_Text moneyText;

    private bool wasDead;

    void Start()
    {
        if (playerCombo == null)
            playerCombo = GetComponent<PlayerCombo>();

        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();

        levelMons = 0;

        walletMons = PlayerPrefs.GetInt(
            "WalletMons", 0
        );

        UpdateMoneyUI();
    }

    void Update()
    {
        if (rewardsCollected)
            return;

        if (playerHealth != null)
        {
            if (playerHealth.isDead && !wasDead)
            {
                ApplyDeathPenalty();
            }

            wasDead = playerHealth.isDead;
        }

        UpdateMoneyUI();
    }

    public void AddMons(int amount)
    {
        if (amount <= 0 || rewardsCollected)
            return;

        levelMons += amount;
        totalMonsEarned += amount;

        Debug.Log("STONKS! +" + amount + " MONS!");

        UpdateMoneyUI();
    }

    public void ApplyDeathPenalty()
    {
        if (rewardsCollected)
            return;

        int lostMons = Mathf.CeilToInt(
            levelMons * deathPenalty
        );

        lostMons = Mathf.Clamp(
            lostMons, 0, levelMons
        );

        levelMons -= lostMons;
        totalMonsLost += lostMons;

        Debug.Log(
            "McDeath Tax: -" + lostMons + " MONS!"
        );

        UpdateMoneyUI();
    }

    public void CompleteLevel(LevelTimer timer)
    {
        if (rewardsCollected)
            return;

        // Convert score into mons
        if (playerCombo != null)
        {
            scoreBonus = Mathf.FloorToInt(
                playerCombo.totalScore *
                Mathf.Max(0f, scoreToMonsRate)
            );
        }

        // Calculate percentage-based time bonus
        float timePercentage = 0f;

        if (timer != null && timer.startingTime > 0f)
        {
            timePercentage = Mathf.Clamp01(
                timer.remainingTime / timer.startingTime
            );
        }

        timeBonus = Mathf.FloorToInt(
            Mathf.Max(0, maximumTimeBonus) *
            timePercentage
        );

        // Combine all three sources of money
        finalReward = levelMons + scoreBonus + timeBonus;

        walletMons += finalReward;

        rewardsCollected = true;
        levelMons = 0;

        PlayerPrefs.SetInt("WalletMons", walletMons);
        PlayerPrefs.Save();

        Debug.Log("COLLECTED MONS: " + totalMonsEarned);
        Debug.Log("McDEATH TAX: -" + totalMonsLost);
        Debug.Log("SCORE BONUS: +" + scoreBonus);
        Debug.Log("TIME BONUS: +" + timeBonus);
        Debug.Log("FINAL REWARD: " + finalReward);
        Debug.Log("WALLET: " + walletMons);

        UpdateMoneyUI();
    }

    void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            moneyText.text = "MONS: " + levelMons;
        }
    }

    [ContextMenu("Test Add 100 Mons")]
    void TestAddMons()
    {
        if (Application.isPlaying)
            AddMons(100);
    }
}