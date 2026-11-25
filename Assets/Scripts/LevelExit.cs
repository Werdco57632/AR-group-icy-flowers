using UnityEngine;
using UnityEngine.Events;

public class LevelExit : MonoBehaviour
{
    [Header("References")]
    public LevelTimer levelTimer;

    [Header("Status")]
    public bool levelCompleted = false;

    [Header("Results")]
    public int finalScore;
    public int remainingSeconds;
    public int collectedMons;
    public int scoreBonus;
    public int timeBonus;
    public int monsLost;
    public int finalMons;
    public int walletBalance;

    [Header("Events")]
    public UnityEvent onLevelCompleted;

    void OnTriggerEnter(Collider other)
    {
        if (levelCompleted)
            return;

        PlayerHealth playerHealth =
            other.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null || playerHealth.isDead)
            return;

        PlayerCombo playerCombo =
            playerHealth.GetComponent<PlayerCombo>();

        PlayerMoney playerMoney =
            playerHealth.GetComponent<PlayerMoney>();

        if (levelTimer == null)
        {
            Debug.LogError("Level Timer is not assigned!");
            return;
        }

        if (playerMoney == null)
        {
            Debug.LogError("PlayerMoney is missing!");
            return;
        }

        if (playerCombo == null)
        {
            Debug.LogError("PlayerCombo is missing!");
            return;
        }

        if (levelTimer.timeExpired)
            return;

        levelCompleted = true;

        // Stop the timer
        levelTimer.StopTimer();
        remainingSeconds = levelTimer.GetRemainingSeconds();

        // Bank any remaining combo
        playerCombo.BankCombo();
        finalScore = playerCombo.totalScore;

        // Record collected money before depositing
        collectedMons = playerMoney.levelMons;

        // Calculate and deposit rewards
        playerMoney.CompleteLevel(levelTimer);

        // Record final results
        scoreBonus = playerMoney.scoreBonus;
        timeBonus = playerMoney.timeBonus;
        monsLost = playerMoney.totalMonsLost;
        finalMons = playerMoney.finalReward;
        walletBalance = playerMoney.walletMons;

        Debug.Log("===== LEVEL COMPLETE =====");
        Debug.Log("Final Score: " + finalScore);
        Debug.Log("Remaining Time: " + remainingSeconds);
        Debug.Log("Collected Mons: " + collectedMons);
        Debug.Log("Score Bonus: +" + scoreBonus);
        Debug.Log("Time Bonus: +" + timeBonus);
        Debug.Log("McDeath Tax: -" + monsLost);
        Debug.Log("Final Reward: " + finalMons);
        Debug.Log("Wallet Balance: " + walletBalance);
        Debug.Log("==========================");

        onLevelCompleted?.Invoke();
    }
}