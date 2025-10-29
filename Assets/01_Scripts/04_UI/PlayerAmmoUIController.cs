using UnityEngine;
using UnityEngine.UI;

public class PlayerAmmoUIController : MonoBehaviour
{
    [Header("Simple Bullets")]
    public Image[] simpleBulletIcons; // arrastra aquí los 10 íconos
    public int maxSimpleAmmo = 10;
    private int currentSimpleAmmo;

    [Header("Energy Bullets")]
    public Image[] energyBulletIcons; // arrastra aquí los 5 íconos
    public int maxEnergyAmmo = 5;
    private int currentEnergyAmmo;

    [Header("Colores")]
    public Color activeColor = Color.white; // íconos visibles
    public Color inactiveColor = new Color(1f, 1f, 1f, 0.3f); // semitransparente

    void Start()
    {
        currentSimpleAmmo = maxSimpleAmmo;
        currentEnergyAmmo = maxEnergyAmmo;
        UpdateAmmoUI();
    }

    void UseSimpleBullet()
    {
        if (currentSimpleAmmo > 0)
        {
            currentSimpleAmmo--;
            UpdateAmmoUI();
        }
    }

    void UseEnergyBullet()
    {
        if (currentEnergyAmmo > 0)
        {
            currentEnergyAmmo--;
            UpdateAmmoUI();
        }
    }

    public void Reload()
    {
        currentSimpleAmmo = maxSimpleAmmo;
        currentEnergyAmmo = maxEnergyAmmo;
        UpdateAmmoUI();
    }

    void UpdateAmmoUI()
    {
        // Balas simples
        for (int i = 0; i < simpleBulletIcons.Length; i++)
        {
            if (simpleBulletIcons[i] != null)
                simpleBulletIcons[i].color = i < currentSimpleAmmo ? activeColor : inactiveColor;
        }

        // Balas de energía
        for (int i = 0; i < energyBulletIcons.Length; i++)
        {
            if (energyBulletIcons[i] != null)
                energyBulletIcons[i].color = i < currentEnergyAmmo ? activeColor : inactiveColor;
        }
    }

    public void UseBullet(int bulletTypeIndex)
    {
        if (bulletTypeIndex < 0) return;

        switch (bulletTypeIndex)
        {
            case 0: 
                UseSimpleBullet(); 
                break;
            case 1: 
                UseEnergyBullet(); 
                break;
            default: Debug.LogWarning("Tipo de bala no definido en UI: " + bulletTypeIndex); 
                break;
        }
    }

    public void Reload(int bulletTypeIndex)
    {
        switch (bulletTypeIndex)
        {
            case 0: // bala simple
                currentSimpleAmmo = maxSimpleAmmo;
                break;
            case 1: // bala de energía
                currentEnergyAmmo = maxEnergyAmmo;
                break;
            default:
                Debug.LogWarning("Tipo de bala no definido en UI: " + bulletTypeIndex);
                break;
        }
        UpdateAmmoUI();
    }

}
