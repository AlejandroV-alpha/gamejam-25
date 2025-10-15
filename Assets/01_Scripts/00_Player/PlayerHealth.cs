using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Health (Energy) Settings")]
    [SerializeField] float maxEnergy = 300f;

    [Header("Movement Energy Drain")]
    [SerializeField] float movementEnergyDrain = 0.3f;
    [SerializeField] float minSpeedToDrain = 0.2f;

    [Header("Rotation Energy Drain")]
    [SerializeField] float rotationEnergyDrain = 0.2f;
    [SerializeField] float minRotateSpeedToDrain = 5f;


    float currentEnergy;
    PlayerController playerController;

    // Inicializa la salud y obtiene referencia al PlayerController
    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        currentEnergy = maxEnergy;
    }

    void Update()
    {
        HandleEnergyDrain();
    }

    #region Energy Consumption
    /// <summary>
    /// Reduce la energía del jugador mientras se mueve o rota.
    /// </summary>
    void HandleEnergyDrain()
    {
        float currentSpeed = Mathf.Abs(playerController.GetCurrentSpeed());
        float currentRotateSpeed = Mathf.Abs(playerController.GetCurrentRotateSpeed());
        float totalEnergyLoss = 0f;

        // Drena energía por movimiento
        if (currentSpeed > minSpeedToDrain)
        {
            totalEnergyLoss += movementEnergyDrain * Time.deltaTime;
        }

        // Drena energía por rotación
        if (currentRotateSpeed > minRotateSpeedToDrain)
        {
            totalEnergyLoss += rotationEnergyDrain * Time.deltaTime;
        }

        if (totalEnergyLoss > 0f)
        {
            currentEnergy = Mathf.Max(currentEnergy - totalEnergyLoss, 0f);

            if (currentEnergy <= 0f)
            {
                HandleDeath();
            }
        }
    }
    #endregion

    #region Apply Damage and Die
    /// <summary>
    /// Aplica daño al jugador y un empuje en la dirección del impacto.
    /// </summary>
    public void TakeDamage(float damage, Vector2 hitDirection, float knockbackStrength)
    {
        currentEnergy -= damage;
        playerController.ApplyKnockback(hitDirection, knockbackStrength);

        if (currentEnergy <= 0f)
        {
            HandleDeath();
        }
    }

    /// <summary>
    /// Gestiona la muerte del jugador (puede expandirse para efectos, respawn, etc.).
    /// </summary>
    void HandleDeath()
    {
        Debug.Log("Player died");
        Destroy(gameObject);
    }
    #endregion

    #region Utilities
    /// <summary>
    /// Retorna la salud actual del jugador.
    /// </summary>
    public float GetCurrentHealth()
    {
        return currentEnergy;
    }
    #endregion
}
