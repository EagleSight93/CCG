using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class NavigationManager : MonoBehaviour
{

    public void LoadScene(int i)
    {
        SceneManager.LoadSceneAsync(i);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
