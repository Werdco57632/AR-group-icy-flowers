using UnityEngine;
using System.Collections.Generic;

public class PlayerCombo : MonoBehaviour
{
    [Header("Combo Settings")]
    public int baseStompPoints = 100;

    [Header("Current Combo")]
    public int comboCount = 0;
    public int pendingPoints = 0;

    [Header("Score")]
    public int totalScore = 0;
    public int highestCombo = 0;

    private HashSet<EnemyHealth> stompedEnemies =
        new HashSet<EnemyHealth>();

    public void RegisterStomp(EnemyHealth enemy)
    {
        if (enemy == null)
            return;

        // Each enemy counts once per combo
        if (!stompedEnemies.Add(enemy))
            return;

        comboCount++;

        int earnedPoints =
            baseStompPoints * comboCount;

        pendingPoints += earnedPoints;

        Debug.Log(
            comboCount + "x COMBO! | " +
            pendingPoints + " pending points"
        );
    }

    public void BankCombo()
    {
        if (comboCount == 0)
            return;

        totalScore += pendingPoints;

        highestCombo = Mathf.Max(
            highestCombo, comboCount
        );

        Debug.Log(
            "COMBO COMPLETE! +" +
            pendingPoints +
            " | Total Score: " + totalScore
        );

        ResetCombo();
    }

    public void LoseCombo()
    {
        if (comboCount == 0)
            return;

        Debug.Log(
            "COMBO LOST! -" +
            pendingPoints + " pending points"
        );

        ResetCombo();
    }

    void ResetCombo()
    {
        comboCount = 0;
        pendingPoints = 0;
        stompedEnemies.Clear();
    }
}