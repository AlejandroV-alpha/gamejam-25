using UnityEngine;

/// <summary>
/// Detecta cuando el jugador alcanza un punto del tutorial
/// y notifica al TutorialManager correspondiente.
/// </summary>
public class TutorialPoint : MonoBehaviour
{
    [SerializeField] private TutorialManager tutorialManager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (tutorialManager != null)
            {
                tutorialManager.OnPointReached(gameObject);
            }
            else
            {
                Debug.LogWarning("TutorialManager no asignado en TutorialPoint: " + gameObject.name);
            }
        }
    }
}
