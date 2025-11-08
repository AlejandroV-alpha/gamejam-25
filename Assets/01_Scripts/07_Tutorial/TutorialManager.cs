using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestiona la logica completa del tutorial paso a paso,
/// mostrando instrucciones, teclas en pantalla y activando
/// enemigos o nodos segun el progreso del jugador.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    #region UI
    [Header("UI")]
    [SerializeField] TMP_Text tutorialText;

    [SerializeField] Image keyW;
    [SerializeField] Image keyA;
    [SerializeField] Image keyS;
    [SerializeField] Image keyD;
    [SerializeField] Image keySpace;
    [SerializeField] Image keyR;
    [SerializeField] Image keyE;

    [SerializeField] Image arrowUp;
    [SerializeField] Image arrowDown;
    [SerializeField] Image arrowLeft;
    [SerializeField] Image arrowRight;
    #endregion

    #region Points
    [Header("Map Points")]
    [SerializeField] GameObject[] points;
    int currentStep = 0;
    #endregion

    #region WorldObjects
    [Header("World Objects")]
    [SerializeField] GameObject enemy;
    [SerializeField] GameObject node;

    bool enemyActive = false;
    bool nodeActive = false;
    #endregion

    #region MainControl
    void Start()
    {
        for (int i = 0; i < points.Length; i++)
        {
            points[i].SetActive(i == 0);
        }

        if (enemy != null)
        {
            enemy.SetActive(false);
        }

        if (node != null)
        {
            node.SetActive(false);
        }

        ShowStep();
    }

    void Update()
    {
        if (enemyActive && enemy == null)
        {
            enemyActive = false;
            NextStep();
        }

        if (nodeActive)
        {
            Node nodeLife = node.GetComponent<Node>();
            if (nodeLife != null && nodeLife.GetCurrentEnergy() >= nodeLife.GetMaxEnergy())
            {
                nodeActive = false;
                NextStep();
            }
        }
    }

    /// <summary>
    /// Se ejecuta cuando el jugador llega a un punto del tutorial.
    /// </summary>
    public void OnPointReached(GameObject point)
    {
        if (point == points[currentStep])
        {
            NextStep();
        }
    }

    /// <summary>
    /// Muestra el paso actual del tutorial y activa los elementos necesarios.
    /// </summary>
    private void ShowStep()
    {
        HideKeys();
        HideArrows();
        DeactivatePoints();

        if (enemy != null)
        {
            enemy.SetActive(false);
        }

        if (node != null)
        {
            node.SetActive(false);
        }

        string stepText = "";

        switch (currentStep)
        {
            case 0:
                stepText = "¡Bienvenido, comandante! Usa S o Flecha Abajo para retroceder y posicionarte.";
                ActivatePoint(0);
                keyS.enabled = true;
                arrowDown.enabled = true;
                break;

            case 1:
                stepText = "¡Excelente! Ahora avanza con W o Flecha Arriba, mantén el control del terreno.";
                ActivatePoint(1);
                keyW.enabled = true;
                arrowUp.enabled = true;
                break;

            case 2:
                stepText = "¡Perfecto! Muévete en zig-zag usando A y D o las flechas Izquierda y Derecha para esquivar obstaculos.";
                ActivatePoint(2);
                keyA.enabled = true;
                keyD.enabled = true;
                arrowLeft.enabled = true;
                arrowRight.enabled = true;
                break;

            case 3:
                stepText = "¡Hora de la acción! Dispara al enemigo con Espacio y recarga con R si te quedas sin balas. ¡Demuestra tu puntería!";
                if (enemy != null)
                {
                    enemy.SetActive(true);
                }
                enemyActive = true;
                keySpace.enabled = true;
                keyR.enabled = true;
                break;

            case 4:
                stepText = "Misión de soporte: cambia a la munición de energía con E y dispara al nodo con Espacio para restaurarlo. ¡Tu tanque es clave para la victoria!";
                if (node != null)
                {
                    node.SetActive(true);
                }
                nodeActive = true;
                keySpace.enabled = true;
                keyR.enabled = true;
                keyE.enabled = true;
                break;

            default:
                stepText = "¡Fantástico, comandante! Has completado el tutorial. ¡Ahora estás listo para enfrentarte a cualquier misión y llevar la victoria a tu escuadrón!";
                break;

        }

        // Start typewriter effect
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeText(stepText));
    }

    /// <summary>
    /// Pasa al siguiente paso del tutorial.
    /// </summary>
    private void NextStep()
    {
        currentStep++;
        ShowStep();
    }
    #endregion

    #region Utilities
    /// <summary>
    /// Desactiva todos los puntos del mapa.
    /// </summary>
    private void DeactivatePoints()
    {
        foreach (var p in points)
        {
            if (p != null)
            {
                p.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Activa un punto especifico segun su indice.
    /// </summary>
    private void ActivatePoint(int index)
    {
        if (points != null && index >= 0 && index < points.Length && points[index] != null)
        {
            points[index].SetActive(true);
        }
    }

    /// <summary>
    /// Oculta todas las teclas del HUD.
    /// </summary>
    private void HideKeys()
    {
        keyW.enabled = false;
        keyA.enabled = false;
        keyS.enabled = false;
        keyD.enabled = false;
        keySpace.enabled = false;
        keyR.enabled = false;
        keyE.enabled = false;
    }

    /// <summary>
    /// Oculta todas las flechas del HUD.
    /// </summary>
    private void HideArrows()
    {
        arrowUp.enabled = false;
        arrowDown.enabled = false;
        arrowLeft.enabled = false;
        arrowRight.enabled = false;
    }

    // Coroutine for typewriter effect
    private Coroutine typingCoroutine;

    /// <summary>
    /// Muestra el texto letra por letra, como subtitulo.
    /// </summary>
    private IEnumerator TypeText(string textToShow)
    {
        tutorialText.text = "";
        foreach (char c in textToShow)
        {
            tutorialText.text += c;
            yield return new WaitForSeconds(0.05f); // velocidad del efecto
        }
    }
    #endregion
}
