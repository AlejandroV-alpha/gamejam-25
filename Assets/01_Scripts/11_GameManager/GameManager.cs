using UnityEngine;
using UnityEngine.SceneManagement; // Permite cargar o reiniciar escenas

/// <summary>
/// Controla las condiciones de victoria y derrota del juego.
/// - Ganas cuando TODOS los nodos tienen su energía al máximo.
/// - Pierdes cuando el jugador muere (energía = 0 o fue destruido).
/// </summary>
public class GameManager : MonoBehaviour
{
    // Singleton: permite acceder a este GameManager desde cualquier script mediante GameManager.Instance
    public static GameManager Instance;

    [Header("Referencias en escena")]
    [Tooltip("Referencia al objeto del jugador (asignar en el inspector).")]
    public GameObject player;

    [Tooltip("Lista de nodos a controlar (asignar en el inspector).")]
    public Node[] nodes;

    public int currentLevel = 0;

    // Estado interno del juego
    private bool gameOver = false;

    //void Awake()
    //{
    //    // Implementación del patrón Singleton
    //    if (Instance == null)
    //    {
    //        Instance = this;
    //    }
    //    else
    //    {
    //        Destroy(gameObject); // Evita duplicados en la escena
    //    }
    //}

    void Update()
    {
        // Si ya terminó la partida, no se siguen comprobando condiciones
        if (gameOver) return;

        // --- Condición de derrota ---
        // Si el jugador fue destruido o su energía llegó a 0
        if (player == null || player.GetComponent<PlayerHealth>().GetCurrentEnergy() <= 0)
        {
            LoseGame();
            return;
        }

        // --- Condición de victoria ---
        // Si todos los nodos tienen su energía al máximo
        if (AllNodesAtMaxEnergy())
        {
            WinGame();
            return;
        }
    }

    /// <summary>
    /// Comprueba si todos los nodos tienen su energía máxima.
    /// </summary>
    /// <returns>True si todos los nodos están al 100%, false si al menos uno no lo está.</returns>
    bool AllNodesAtMaxEnergy()
    {
        foreach (Node node in nodes)
        {
            // Verifica que el nodo no sea nulo (por si fue destruido)
            if (node == null) return false;

            if (node.GetCurrentEnergy() < node.GetMaxEnergy())
                return false;
        }
        return true;
    }

    /// <summary>
    /// Ejecuta la secuencia de victoria del juego.
    /// </summary>
    void WinGame()
    {
        gameOver = true;
        Debug.Log("¡Ganaste la partida!");
        if (currentLevel >= 4)
        {
            SceneManager.LoadScene("NivellMenuScene");
        }
        else
        {
            PlayerPrefs.SetInt("level", currentLevel + 1);
            Debug.Log($"level: {currentLevel + 1}");
            SceneManager.LoadScene($"CinematicLevel{currentLevel + 1}Scene");
        }
    }

    /// <summary>
    /// Ejecuta la secuencia de derrota del juego.
    /// </summary>
    void LoseGame()
    {
        gameOver = true;
        Debug.Log("Perdiste la partida...");

        SceneManager.LoadScene("NivellMenuScene");
    }
}
