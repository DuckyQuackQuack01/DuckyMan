using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuComponent : MonoBehaviour
{
    public void OnStartClick()
    {
        SceneManager.LoadScene("Level1");   
    }

    public void OnExitClick()
    {
        Application.Quit();
    }
}
