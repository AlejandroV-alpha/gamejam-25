using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    /// <summary>
    /// Carga la escena principal del juego.
    /// Asegúrate de que la escena esté agregada en Build Settings.
    /// </summary>
    public void StartGame()
    {
        // Cambia "Nivel1" por el nombre de tu escena de juego
        SceneManager.LoadScene("NivellMenuScene");
    }

    /// <summary>
    /// Sale del juego (funciona en build, no en el editor).
    /// </summary>
    public void ExitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}
