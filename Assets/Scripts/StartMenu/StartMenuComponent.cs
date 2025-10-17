using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class StartMenuComponent : MonoBehaviour
{
    public TMP_Text highScoreText;
    public TMP_Text highScoreTimeText;
    
    void Start()
    {
        LoadHighScore();
    }

    public void OnStartClick()
    {
        SceneManager.LoadScene("Level1");   
    }

    public void OnExitClick()
    {
#if UNITY_EDITOR
        // Stop play mode in the Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    private void LoadHighScore()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        float highScoreTime = PlayerPrefs.GetFloat("HighScoreTime", 0f);

        highScoreText.text = PlayerPrefs.GetInt("HighScore", 0).ToString("D6");

        int minutes = Mathf.FloorToInt(highScoreTime / 60f);
        int seconds = Mathf.FloorToInt(highScoreTime  % 60);
        int milliseconds = Mathf.FloorToInt((highScoreTime * 1000f) % 1000);
        int shortenedMs = (milliseconds / 10) % 100;

        highScoreTimeText.text = $"{minutes:00}:{seconds:00}:{shortenedMs:00}";
    }
}
