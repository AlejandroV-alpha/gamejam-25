using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerHealth : MonoBehaviour, ITakeDamage, ITakeEnergy
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
    /// Reduce la energia del jugador mientras se mueve o rota
    /// </summary>
    void HandleEnergyDrain()
    {
        float currentSpeed = Mathf.Abs(playerController.GetCurrentSpeed());
        float currentRotateSpeed = Mathf.Abs(playerController.GetCurrentRotateSpeed());
        float totalEnergyLoss = 0f;

        // Drena energia por movimiento
        if (currentSpeed > minSpeedToDrain)
        {
            totalEnergyLoss += movementEnergyDrain * Time.deltaTime;
        }

        // Drena energia por rotacion
        if (currentRotateSpeed > minRotateSpeedToDrain)
        {
            totalEnergyLoss += rotationEnergyDrain * Time.deltaTime;
        }

        if (totalEnergyLoss > 0f)
        {
            ChangeEnergy(-totalEnergyLoss);
        }
    }

    /// <summary>
    /// Cambia la energia actual del jugador aplicando un monto positivo o negativo
    /// Controla que la energia se mantenga entre 0 y maxEnergy y maneja la muerte si llega a 0
    /// </summary>
    /// <param name="amount">Cantidad a sumar (positiva) o restar (negativa)</param>
    private void ChangeEnergy(float amount)
    {
        currentEnergy = Mathf.Clamp(currentEnergy + amount, 0f, maxEnergy);
        if (currentEnergy <= 0f)
        {
            HandleDeath();
        }
    }

    /// <summary>
    /// Recibe energía y la aplica al jugador.
    /// </summary>
    /// <param name="amount">Cantidad de energia a recibir</param>
    public void ReceiveEnergy(float amount)
    {
        if (amount > 0f)
        {
            ChangeEnergy(amount);
        }
    }
    #endregion

    #region Apply Damage and Die
    /// <summary>
    /// Aplica dano al jugador y un empuje en la direccion del impacto
    /// </summary>
    public void TakeDamage(float damage, Vector2 hitDirection, float knockbackStrength = 0)
    {
        ChangeEnergy(-damage);
        playerController.ApplyKnockback(hitDirection, knockbackStrength);
    }

    /// <summary>
    /// Gestiona la muerte del jugador (puede expandirse para efectos, respawn, etc)
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
