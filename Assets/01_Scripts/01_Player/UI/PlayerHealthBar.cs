using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] Image lifeEnergyBarFilling;

    PlayerHealth playerHealth;
    float maxLifeEnergy;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerHealth = GameObject.Find("Player").GetComponent<PlayerHealth>();
        maxLifeEnergy = playerHealth.GetMaxEnergy();
    }

    // Update is called once per frame
    void Update()
    {
        lifeEnergyBarFilling.fillAmount = playerHealth.GetCurrentEnergy() / maxLifeEnergy;
    }
}
