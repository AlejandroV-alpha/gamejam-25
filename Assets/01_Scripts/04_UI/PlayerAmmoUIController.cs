using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla la interfaz de usuario de munición del jugador.
/// Maneja dos tipos de balas: simples y de energía.
/// Permite usar y recargar balas, y actualiza los íconos de UI
/// mostrando las balas activas e inactivas con colores configurables.
/// </summary>
public class PlayerAmmoUIController : MonoBehaviour
{
    #region Variables

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

    #endregion

    #region Unity Methods
    void Start()
    {
        currentSimpleAmmo = maxSimpleAmmo;
        currentEnergyAmmo = maxEnergyAmmo;
        UpdateAmmoUI();
    }
    #endregion

    #region Ammo Usage Methods
    /// <summary>
    /// Dispara una bala simple, decrementando el contador y actualizando la UI.
    /// </summary>
    void UseSimpleBullet()
    {
        if (currentSimpleAmmo > 0)
        {
            currentSimpleAmmo--;
            UpdateAmmoUI();
        }
    }

    /// <summary>
    /// Dispara una bala de energía, decrementando el contador y actualizando la UI.
    /// </summary>
    void UseEnergyBullet()
    {
        if (currentEnergyAmmo > 0)
        {
            currentEnergyAmmo--;
            UpdateAmmoUI();
        }
    }

    /// <summary>
    /// Dispara una bala según el tipo indicado.
    /// 0 = bala simple, 1 = bala de energía.
    /// </summary>
    /// <param name="bulletTypeIndex">Índice del tipo de bala</param>
    public void UseBullet(int bulletTypeIndex)
    {
        if (bulletTypeIndex < 0)
        {
            return;
        }

        switch (bulletTypeIndex)
        {
            case 0:
                {
                    UseSimpleBullet();
                    break;
                }
            case 1:
                {
                    UseEnergyBullet();
                    break;
                }
            default:
                {
                    Debug.LogWarning("Tipo de bala no definido en UI: " + bulletTypeIndex);
                    break;
                }
        }
    }
    #endregion

    #region Reload Methods
    /// <summary>
    /// Recarga todas las balas (simples y de energía) al máximo.
    /// </summary>
    public void Reload()
    {
        currentSimpleAmmo = maxSimpleAmmo;
        currentEnergyAmmo = maxEnergyAmmo;
        UpdateAmmoUI();
    }

    /// <summary>
    /// Recarga un tipo específico de bala según el índice.
    /// 0 = bala simple, 1 = bala de energía.
    /// </summary>
    /// <param name="bulletTypeIndex">Índice del tipo de bala</param>
    public void Reload(int bulletTypeIndex)
    {
        switch (bulletTypeIndex)
        {
            case 0:
                {
                    currentSimpleAmmo = maxSimpleAmmo;
                    break;
                }
            case 1:
                {
                    currentEnergyAmmo = maxEnergyAmmo;
                    break;
                }
            default:
                {
                    Debug.LogWarning("Tipo de bala no definido en UI: " + bulletTypeIndex);
                    break;
                }
        }

        UpdateAmmoUI();
    }
    #endregion

    #region UI Update
    /// <summary>
    /// Actualiza la UI de las balas mostrando las que están activas e inactivas.
    /// </summary>
    void UpdateAmmoUI()
    {
        // Balas simples
        for (int i = 0; i < simpleBulletIcons.Length; i++)
        {
            if (simpleBulletIcons[i] != null)
            {
                simpleBulletIcons[i].color = i < currentSimpleAmmo ? activeColor : inactiveColor;
            }
        }

        // Balas de energía
        for (int i = 0; i < energyBulletIcons.Length; i++)
        {
            if (energyBulletIcons[i] != null)
            {
                energyBulletIcons[i].color = i < currentEnergyAmmo ? activeColor : inactiveColor;
            }
        }
    }
    #endregion
}
