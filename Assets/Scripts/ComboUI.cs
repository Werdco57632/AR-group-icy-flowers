using UnityEngine;
using TMPro;

public class ComboUI : MonoBehaviour
{
    [Header("References")]
    public PlayerCombo playerCombo;

    public TMP_Text scoreText;
    public TMP_Text comboText;
    public TMP_Text comboPointsText;

    [Header("Animation")]
    public float popScale = 1.4f;
    public float popSpeed = 8f;
    public float messageDuration = 1.5f;

    private int previousCombo;
    private int previousPoints;
    private int previousScore;

    private float messageTimer;
    private Vector3 originalScale;

    void Start()
    {
        originalScale = comboText.transform.localScale;

        previousScore = playerCombo.totalScore;

        comboText.gameObject.SetActive(false);
        comboPointsText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (playerCombo == null)
            return;

        int count = playerCombo.comboCount;
        int points = playerCombo.pendingPoints;
        int score = playerCombo.totalScore;

        scoreText.text = "SCORE: " + score.ToString("D6");

        if (count > 0)
        {
            messageTimer = 0f;

            comboText.gameObject.SetActive(true);
            comboPointsText.gameObject.SetActive(true);

            comboText.text = GetComboName(count);
            comboText.color = GetComboColor(count);

            comboPointsText.text = "+" + points;

            if (count > previousCombo)
            {
                comboText.transform.localScale =
                    originalScale * popScale;
            }
        }
        else if (previousCombo > 0)
        {
            // Only show combo results for 2+ stomps
            if (previousCombo >= 2)
            {
                comboText.gameObject.SetActive(true);
                comboPointsText.gameObject.SetActive(true);

                messageTimer = messageDuration;

                if (score > previousScore)
                {
                    comboText.text = "COMBO COMPLETE!";
                    comboText.color = Color.green;

                    comboPointsText.text =
                        "+" + previousPoints;
                }
                else
                {
                    comboText.text = "COMBO LOST!";
                    comboText.color = Color.red;

                    comboPointsText.text =
                        "-" + previousPoints;
                }

                comboText.transform.localScale =
                    originalScale * popScale;
            }
            else
            {
                // A single stomp is not a combo
                messageTimer = 0f;

                comboText.gameObject.SetActive(false);
                comboPointsText.gameObject.SetActive(false);
            }
        }
        else if (messageTimer > 0f)
        {
            messageTimer -= Time.deltaTime;

            if (messageTimer <= 0f)
            {
                comboText.gameObject.SetActive(false);
                comboPointsText.gameObject.SetActive(false);
            }
        }

        comboText.transform.localScale = Vector3.Lerp(
            comboText.transform.localScale,
            originalScale,
            Time.deltaTime * popSpeed
        );

        previousCombo = count;
        previousPoints = points;
        previousScore = score;
    }

    string GetComboName(int count)
    {
        if (count >= 10)
            return count + "x GRAVITY IS OPTIONAL!";

        if (count >= 5)
            return count + "x YOOOOOOOOO!";

        if (count == 4)
            return "4x WOWZERS";

        if (count == 3)
            return "3x THREE OF THEM???";

        if (count == 2)
            return "2x BONK: THE SEQUEL!";

        return "BONK!";
    }

    Color GetComboColor(int count)
    {
        if (count >= 10)
            return Color.red;

        if (count >= 5)
            return new Color(1f, 0.5f, 0f);

        if (count >= 4)
            return new Color(0.8f, 0.4f, 1f);

        if (count >= 3)
            return Color.cyan;

        if (count >= 2)
            return Color.yellow;

        return Color.white;
    }
}