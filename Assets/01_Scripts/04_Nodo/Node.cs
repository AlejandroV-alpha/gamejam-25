using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Nodo que acumula energia al recibir impactos.
/// La intensidad de la luz Spot 2D aumenta con la energia actual.
/// </summary>
public class Node : MonoBehaviour, ITakeEnergy
{
    #region Energy Configuration
    [Header("Energy Configuration")]
    [Tooltip("Maximum amount of energy the node can have.")]
    [SerializeField] float maxEnergy = 100f;

    [Tooltip("Current energy of the node (read-only at runtime).")]
    [SerializeField] float currentEnergy = 0f;
    #endregion

    #region Visual Variables
    [Header("Visual Configuration")]
    [Tooltip("2D Spot Light indicating the node's energy level.")]
    [SerializeField] Light2D nodeLight;

    [Tooltip("Minimum light intensity.")]
    [SerializeField] float minIntensity = 0.2f;

    [Tooltip("Maximum light intensity when energy is full.")]
    [SerializeField] float maxIntensity = 50f;
    #endregion

    #region Unity Methods
    void Start()
    {
        if (nodeLight != null)
        {
            nodeLight.intensity = minIntensity;
        }
    }
    #endregion

    #region ITakeEnergy Implementation
    public void TakeEnergy(float energy)
    {
        currentEnergy += energy;
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
        UpdateLightIntensity();
        Debug.Log($"Nodo recibio {energy} de energia. Energia actual: {currentEnergy}/{maxEnergy}");
    }
    #endregion

    #region Private Methods
    void UpdateLightIntensity()
    {
        if (nodeLight == null)
        {
            return;
        }

        // t = 0 -> minIntensity, t = 1 -> maxIntensity
        float t = currentEnergy / maxEnergy;
        nodeLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
    }
    #endregion

    #region Public Methods
    public float GetCurrentEnergy()
    {
        return currentEnergy;
    }
    public float GetMaxEnergy()
    {
        return maxEnergy;
    }
    #endregion
}
