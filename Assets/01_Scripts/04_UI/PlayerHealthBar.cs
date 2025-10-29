using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla la barra de vida/energía del jugador.
/// Actualiza el llenado de la barra según la energía actual del jugador.
/// </summary>
public class PlayerHealthBar : MonoBehaviour
{
    #region Variables

    [Header("UI")]
    [SerializeField] Image lifeEnergyBarFilling; // Imagen que representa la barra de vida/energía

    PlayerHealth playerHealth; // referencia al script de salud del jugador
    float maxLifeEnergy; // vida máxima del jugador

    #endregion

    #region Unity Methods

    /// <summary>
    /// Se ejecuta al inicio. Obtiene la referencia del jugador y su vida máxima.
    /// </summary>
    void Start()
    {
        playerHealth = GameObject.Find("Player").GetComponent<PlayerHealth>();
        maxLifeEnergy = playerHealth.GetMaxEnergy();
    }

    /// <summary>
    /// Se ejecuta una vez por frame. Actualiza el llenado de la barra según la vida actual.
    /// </summary>
    void Update()
    {
        lifeEnergyBarFilling.fillAmount = playerHealth.GetCurrentEnergy() / maxLifeEnergy;
    }

    #endregion
}
