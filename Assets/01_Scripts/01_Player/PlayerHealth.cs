using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerHealth : MonoBehaviour, ITakeDamage, ITakeEnergy
{
    #region Inspector Variables
    [Header("Health (Energy) Settings")]
    [SerializeField] float maxEnergy = 500f;
    #endregion

    #region Private Fields
    [SerializeField] float currentEnergy;
    PlayerController playerController;
    #endregion

    #region Unity Methods
    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        currentEnergy = maxEnergy;
    }
    #endregion

    #region Energy Management
    /// <summary>
    /// Cambia la energia actual del jugador aplicando un monto positivo o negativo.
    /// Controla que la energia se mantenga entre 0 y maxEnergy y maneja la muerte si llega a 0.
    /// </summary>
    /// <param name="amount">Cantidad a sumar (positiva) o restar (negativa)</param>
    public void ChangeEnergy(float amount)
    {
        currentEnergy = Mathf.Clamp(currentEnergy + amount, 0f, maxEnergy);

        if (currentEnergy <= 0f)
        {
            HandleDeath();
        }
    }

    /// <summary>
    /// Recibe energia y la aplica al jugador.
    /// </summary>
    /// <param name="energy">Cantidad de energia a recibir</param>
    public void TakeEnergy(float energy)
    {
        if (energy > 0f)
        {
            ChangeEnergy(energy);
        }
    }
    #endregion

    #region Damage Handling
    /// <summary>
    /// Aplica dano al jugador y un empuje en la direccion del impacto.
    /// </summary>
    public void TakeDamage(float damage, Vector2 hitDirection, float knockbackStrength = 0)
    {
        ChangeEnergy(-damage);
        playerController.ApplyKnockback(hitDirection, knockbackStrength);
    }
    #endregion

    #region Death Handling
    /// <summary>
    /// Gestiona la muerte del jugador.
    /// </summary>
    private void HandleDeath()
    {
        Debug.Log("Player died");
        Destroy(gameObject);
    }
    #endregion

    #region Utilities
    /// <summary>
    /// Retorna la energia actual del jugador.
    /// </summary>
    public float GetCurrentEnergy()
    {
        return currentEnergy;
    }
    #endregion
}
