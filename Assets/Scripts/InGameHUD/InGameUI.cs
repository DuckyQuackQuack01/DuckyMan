using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{

    public static InGameUI Instance;
    public AudioManager audioManager;

    public TMP_Text scoreText;
    public TMP_Text scareTimerDisplay;
    public TMP_Text scaredTimerTitle;
    public RectTransform livesContainer;

    public int maxLives = 3;

    private int score = 0;
    private Coroutine timerCoroutine;
    private float widthPerLife = 18f;

    void Awake()
    {
        Instance = this;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        UpdateScoreText();
        scaredTimerTitle.gameObject.SetActive(false);
        scareTimerDisplay.gameObject.SetActive(false);

        UpdateLivesDisplay(maxLives);
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        scoreText.text = "Score: " + score.ToString("D6");
    }

    public void StartGhostTimer(float duration)
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
        
        timerCoroutine = StartCoroutine(GhostTimerRoutine(duration));
    }

    public void UpdateLivesDisplay(int currentLives)
    {
        float newWidth = widthPerLife * currentLives;

        RectTransform rt = livesContainer;
        rt.sizeDelta = new Vector2(newWidth, rt.sizeDelta.y);
    }

    private IEnumerator GhostTimerRoutine(float duration)
    {
        scaredTimerTitle.gameObject.SetActive(true);
        scareTimerDisplay.gameObject.SetActive(true);

        float remaining = duration;

        while(remaining > 0)
        {
            int minutes = Mathf.FloorToInt(remaining / 60);
            int seconds = Mathf.FloorToInt(remaining % 60);
            int milliseconds = Mathf.FloorToInt((remaining * 1000) % 1000);
            int shortenedMs = (milliseconds / 10) % 100;

            scareTimerDisplay.text = $"{minutes:00}:{seconds:00}:{shortenedMs:00}";

            yield return null;
            remaining -= Time.deltaTime;
        }

        scaredTimerTitle.gameObject.SetActive(false);
        scareTimerDisplay.gameObject.SetActive(false);
        audioManager.PlayMusic(audioManager.background, true);
        scareTimerDisplay.text = "00:00:00";
        GhostStateManager.SetAllGhostsNormal();
    }
}
