using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InGameUI : MonoBehaviour
{

    public static InGameUI Instance;

    public TMP_Text scoreText;
    public TMP_Text scareTimerDisplay;
    public TMP_Text scaredTimerTitle;

    private int score = 0;
    private Coroutine timerCoroutine;
    
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
    }

    // Update is called once per frame
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
        timerCoroutine = StartCoroutine(GhostTimerRoutine(duration));
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


            scareTimerDisplay.text = $"{minutes:00}:{seconds:00}:{milliseconds:00}";

            yield return null;
            remaining -= Time.deltaTime;
        }

        scaredTimerTitle.gameObject.SetActive(false);
        scareTimerDisplay.gameObject.SetActive(false);
        scareTimerDisplay.text = "00:00:00";
    }
}
