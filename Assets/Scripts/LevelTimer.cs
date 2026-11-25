using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class LevelTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    public float startingTime = 180f;
    public float warningTime = 10f;
    public bool startAutomatically = true;

    [Header("Current Status")]
    public float remainingTime;
    public bool isRunning;
    public bool timeExpired;

    [Header("UI")]
    public TMP_Text timerText;
    public Color normalColor = Color.white;
    public Color warningColor = Color.red;

    [Header("Warning Animation")]
    public float pulseAmount = 0.15f;
    public float pulseSpeed = 8f;

    [Header("Events")]
    public UnityEvent onTimeExpired;

    private Vector3 originalScale;

    void Start()
    {
        remainingTime = Mathf.Max(0f, startingTime);
        timeExpired = false;
        isRunning = startAutomatically;

        if (timerText != null)
            originalScale = timerText.transform.localScale;

        UpdateTimerUI();
    }

    void Update()
    {
        if (!isRunning)
            return;

        remainingTime -= Time.deltaTime;

        remainingTime = Mathf.Max(
            0f, remainingTime
        );

        if (remainingTime <= 0f)
        {
            isRunning = false;
            timeExpired = true;

            UpdateTimerUI();

            Debug.Log("Aw, man.");

            onTimeExpired?.Invoke();

            return;
        }

        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {
        if (timerText == null)
            return;

        int totalSeconds =
            Mathf.CeilToInt(remainingTime);

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text =
            minutes.ToString("00") + ":" +
            seconds.ToString("00");

        if (remainingTime <= warningTime)
        {
            timerText.color = warningColor;

            if (isRunning)
            {
                float pulse = 1f +
                    Mathf.Sin(Time.time * pulseSpeed)
                    * pulseAmount;

                timerText.transform.localScale =
                    originalScale * pulse;
            }
            else
            {
                timerText.transform.localScale =
                    originalScale;
            }
        }
        else
        {
            timerText.color = normalColor;

            timerText.transform.localScale =
                originalScale;
        }
    }

    public void StopTimer()
    {
        isRunning = false;
        UpdateTimerUI();
    }

    public void ResumeTimer()
    {
        if (!timeExpired && remainingTime > 0f)
            isRunning = true;
    }

    public void ResetTimer()
    {
        remainingTime = Mathf.Max(0f, startingTime);
        timeExpired = false;
        isRunning = true;

        UpdateTimerUI();
    }

    public int GetRemainingSeconds()
    {
        return Mathf.FloorToInt(remainingTime);
    }
}