using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerHealth))]
public class PlayerEnergyDrain : MonoBehaviour
{
    [Header("Movement Energy Drain")]
    [SerializeField] float movementEnergyDrain = 0.3f;
    [SerializeField] float minSpeedToDrain = 0.2f;

    [Header("Rotation Energy Drain")]
    [SerializeField] float rotationEnergyDrain = 0.2f;
    [SerializeField] float minRotateSpeedToDrain = 5f;

    PlayerController playerController;
    PlayerHealth playerHealth;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        HandleEnergyDrain();
    }

    #region Energy Drain
    /// <summary>
    /// Reduce la energia del jugador segun movimiento y rotacion
    /// </summary>
    void HandleEnergyDrain()
    {
        float currentSpeed = Mathf.Abs(playerController.GetCurrentSpeed());
        float currentRotateSpeed = Mathf.Abs(playerController.GetCurrentRotateSpeed());
        float totalEnergyLoss = 0f;

        // Energia por movimiento
        if (currentSpeed > minSpeedToDrain)
        {
            totalEnergyLoss += movementEnergyDrain * Time.deltaTime;
        }

        // Energia por rotacion
        if (currentRotateSpeed > minRotateSpeedToDrain)
        {
            totalEnergyLoss += rotationEnergyDrain * Time.deltaTime;
        }

        if (totalEnergyLoss > 0f)
        {
            playerHealth.ChangeEnergy(-totalEnergyLoss);
        }
    }
    #endregion
}
