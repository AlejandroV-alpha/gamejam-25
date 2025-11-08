using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [Header("UI")]
    public Text tutorialText;
    public Image keyW, keyA, keyS, keyD, keySpace, keyR, keyE;

    [Header("Puntos del mapa")]
    public GameObject[] puntos;
    private int pasoActual = 0;

    [Header("Objetos del mundo")]
    public GameObject enemigo;
    public GameObject nodo;

    private bool enemigoActivo = false;
    private bool nodoActivo = false;

    void Start()
    {
        // Desactiva todos los puntos excepto el primero
        for (int i = 0; i < puntos.Length; i++)
        {
            puntos[i].SetActive(i == 0);
        }
            

        enemigo.SetActive(false);
        nodo.SetActive(false);

        MostrarPaso();
    }

    void Update()
    {
        // Verifica si el enemigo fue destruido en su paso
        if (enemigoActivo && enemigo == null)
        {
            enemigoActivo = false;
            SiguientePaso();
        }

        // Verifica si el nodo cumplió la condición (vida = 100)
        if (nodoActivo)
        {
            Node vidaNodo = nodo.GetComponent<Node>();
            if (vidaNodo != null && vidaNodo.GetCurrentEnergy() >= vidaNodo.GetMaxEnergy())
            {
                nodoActivo = false;
                SiguientePaso();
            }
        }
    }

    public void OnPuntoAlcanzado(GameObject punto)
    {
        // Asegura que sea el punto esperado
        if (punto == puntos[pasoActual])
        {
            SiguientePaso();
        }
    }

    void MostrarPaso()
    {
        // Oculta todas las teclas
        keyW.enabled = keyA.enabled = keyS.enabled = keyD.enabled = keySpace.enabled = keyR.enabled = keyE.enabled = false;

        // Desactiva todos los puntos
        foreach (var p in puntos)
        {
            p.SetActive(false);
        }

        enemigo.SetActive(false);
        nodo.SetActive(false);

        switch (pasoActual)
        {
            case 0:
                tutorialText.text = "Muévete hacia atrás (S o Flecha Abajo)";
                puntos[0].SetActive(true);
                keyS.enabled = true;
                break;

            case 1:
                tutorialText.text = "Ahora avanza hacia adelante (W o Flecha Arriba)";
                puntos[1].SetActive(true);
                keyW.enabled = true;
                break;

            case 2:
                tutorialText.text = "Muévete en zig-zag hasta el siguiente punto (A y D)";
                puntos[2].SetActive(true);
                keyA.enabled = keyD.enabled = true;
                break;

            case 3:
                tutorialText.text = "¡Dispara al enemigo! (Espacio) — Recarga con R si te quedas sin balas";
                enemigo.SetActive(true);
                enemigoActivo = true;
                keySpace.enabled = true;
                keyR.enabled = true;
                break;

            case 4:
                tutorialText.text = "Dispara al nodo (Espacio), recarga con R y cambia de arma con E";
                nodo.SetActive(true);
                nodoActivo = true;
                keySpace.enabled = keyR.enabled = keyE.enabled = true;
                break;

            default:
                tutorialText.text = "¡Excelente! Has completado el tutorial";
                break;
        }
    }

    void SiguientePaso()
    {
        pasoActual++;
        MostrarPaso();
    }
}
