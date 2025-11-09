using UnityEngine;
using UnityEngine.SceneManagement;

public class NivellMenu : MonoBehaviour
{
    /// <summary>
    /// Retornar al menú principal
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
            // Solo muestra el tutorial si aún no ha empezado el juego
            StartLevel0();
        }
        else if (IsCurrentLevelPlayer(1))
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

    /// <summary>
    /// Verifica si el jugador tiene desbloqueado el nivel indicado.
    /// </summary>
    bool IsCurrentLevelPlayer(int level)
    {
        // Si el jugador tiene nivel 3, puede jugar 1, 2 y 3
        return PlayerPrefs.GetInt("level", 0) >= level;
    }
}
