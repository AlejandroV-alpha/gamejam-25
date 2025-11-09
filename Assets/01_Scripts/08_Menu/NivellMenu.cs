using UnityEngine;
using UnityEngine.SceneManagement;

public class NivellMenu : MonoBehaviour
{
    /// <summary>
    /// Retornar al main menu
    /// </summary>
    public void ReturnMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    void StartLevel0()
    {
        SceneManager.LoadScene("Level0Scene");
    }

    public void StartLevel1()
    {
        if (PlayerPrefs.GetInt("level", 0) == 0)
        {
            StartLevel0();
        }
        else
        {
            SceneManager.LoadScene("CinematicLevel1Scene");
        }
    }

    public void StartLevel2()
    {
        if (IsCurrentLevelPlayer(2))
        {
            SceneManager.LoadScene("CinematicLevel2Scene");
        }
    }

    public void StartLevel3()
    {
        if (IsCurrentLevelPlayer(3))
        {
            SceneManager.LoadScene("CinematicLevel3Scene");
        }
    }

    public void StartLevel4()
    {
        if (IsCurrentLevelPlayer(4))
        {
            SceneManager.LoadScene("CinematicLevel4Scene");
        }
    }

    bool IsCurrentLevelPlayer(int level)
    {
        return PlayerPrefs.GetInt("level", 0) <= level;
    }
}
