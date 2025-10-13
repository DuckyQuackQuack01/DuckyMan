    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using TMPro;
    using UnityEngine.UI;
    using UnityEngine.SceneManagement;


public class InGameUI : MonoBehaviour
{

    public static InGameUI Instance;
    public AudioManager audioManager;

    public TMP_Text scoreText;
    public TMP_Text scareTimerDisplay;
    public TMP_Text scaredTimerTitle;
    public RectTransform livesContainer;

    public TMP_Text screenText;
    public Image overlayImage;

    public TMP_Text gameTimerText;

    public int maxLives = 3;

    private int score = 0;
    private Coroutine timerCoroutine;
    private float widthPerLife = 18f;
    private float remainingScaredTime = 0f;
    private float elapsedTime = 0f;
    private bool gameTimerRunning = false;


    private PacStudentController pacStudent;

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

        pacStudent = FindFirstObjectByType<PacStudentController>();

        gameTimerText.text = "00:00:00";

        StartCoroutine(RoundStartCountdown());
    }
    
    void Update()
    {
        if (gameTimerRunning)
        {
            elapsedTime += Time.deltaTime;
            UpdateGameTimerDisplay(elapsedTime);
        }
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
        remainingScaredTime = 0f;
        GhostStateManager.SetAllGhostsNormal();
    }

    public float GetRemainingGhostTimer()
    {
        return remainingScaredTime;
    }

    private IEnumerator RoundStartCountdown()
    {
        PacStudentController.globallyFrozen = true;
        
        pacStudent.FreezeMovement();

        overlayImage.gameObject.SetActive(true);
        screenText.gameObject.SetActive(true);

        screenText.text = "3";
        yield return new WaitForSeconds(1f);

        screenText.text = "2";
        yield return new WaitForSeconds(1f);

        screenText.text = "1";
        yield return new WaitForSeconds(1f);

        screenText.text = "GO!";
        yield return new WaitForSeconds(1f);

        overlayImage.gameObject.SetActive(false);
        screenText.gameObject.SetActive(false);

        PacStudentController.globallyFrozen = false;
        pacStudent.UnFreezeMovement();

        elapsedTime = 0f;   
        gameTimerRunning = true;
    }
    private void UpdateGameTimerDisplay(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        int milliseconds = Mathf.FloorToInt((time * 1000) % 1000);
        int shortenedMs = (milliseconds / 10) % 100;

        gameTimerText.text = $"{minutes:00}:{seconds:00}:{shortenedMs:00}";
    }


    public void OnExitClick()
    {
        SceneManager.LoadScene("StartScene");
    }

    public void SaveHighScore()
    {
        int previousHighScore = PlayerPrefs.GetInt("HighScore", 0);
        float previousHighScoreTime = PlayerPrefs.GetFloat("HighScoreTime", float.MaxValue);

        if (score > previousHighScore || (score == previousHighScore && elapsedTime < previousHighScoreTime))
        {
            PlayerPrefs.SetInt("HighScore", score);
            PlayerPrefs.SetFloat("HighScoreTime", elapsedTime);
            PlayerPrefs.Save();
        }
    }

    public void GameOver()
    {
        gameTimerRunning = false;

        PacStudentController.globallyFrozen = true;
        pacStudent.FreezeMovement();

        overlayImage.gameObject.SetActive(true);
        screenText.gameObject.SetActive(true);
        screenText.text = "GAME OVER";

        SaveHighScore();

        StartCoroutine(GameOverRoutine());
    }

    private IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("StartScene");
    }
}
