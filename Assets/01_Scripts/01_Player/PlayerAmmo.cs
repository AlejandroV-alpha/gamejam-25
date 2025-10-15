using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerShooter))]
public class PlayerAmmo : MonoBehaviour
{
    [Header("Ammo Settings")]
    [SerializeField] int maxCommonAmmo = 10;
    [SerializeField] int maxEnergyAmmo = 5;
    [SerializeField] float reloadTime = 1.5f;

    [Header("Keys")]
    [SerializeField] KeyCode reloadKey = KeyCode.R;

    [SerializeField] int currentCommonAmmo;
    [SerializeField] int currentEnergyAmmo;
    bool isReloading = false;

    void Awake()
    {
        currentCommonAmmo = maxCommonAmmo;
        currentEnergyAmmo = maxEnergyAmmo;
    }

    void Update()
    {
        HandleManualReloadInput();
    }

    #region Ammo Management
    /// <summary>
    /// Intenta consumir una bala del tipo especificado.
    /// Devuelve true si fue posible disparar.
    /// Dispara recarga automática si se dispara la última bala.
    /// </summary>
    public bool TryConsumeAmmo(BulletMode bulletType, MonoBehaviour owner)
    {
        if (isReloading)
        {
            return false;
        }

        switch (bulletType)
        {
            case BulletMode.COMMON:
                if (currentCommonAmmo > 0)
                {
                    currentCommonAmmo--;
                    if (currentCommonAmmo == 0)
                    {
                        StartReloadIfNeeded(BulletMode.COMMON, owner);
                    }
                    return true;
                }
                break;

            case BulletMode.ENERGY:
                if (currentEnergyAmmo > 0)
                {
                    currentEnergyAmmo--;
                    if (currentEnergyAmmo == 0)
                    {
                        StartReloadIfNeeded(BulletMode.ENERGY, owner);
                    }
                    return true;
                }
                break;
        }

        // Si no hay balas, intenta recargar automáticamente
        StartReloadIfNeeded(bulletType, owner);
        return false;
    }

    /// <summary>
    /// Devuelve la cantidad de munición restante para el tipo de bala indicado.
    /// </summary>
    public int GetCurrentAmmo(BulletMode bulletType)
    {
        return bulletType == BulletMode.COMMON ? currentCommonAmmo : currentEnergyAmmo;
    }

    /// <summary>
    /// Devuelve si la recarga está en curso.
    /// </summary>
    public bool IsReloading()
    {
        return isReloading;
    }

    /// <summary>
    /// Inicia la recarga solo si no hay otra en curso.
    /// </summary>
    void StartReloadIfNeeded(BulletMode bulletType, MonoBehaviour owner)
    {
        if (!isReloading)
        {
            owner.StartCoroutine(Reload(bulletType));
        }
    }
    #endregion

    #region Reload Logic
    /// <summary>
    /// Recarga el cargador del tipo de bala especificado después del tiempo definido.
    /// </summary>
    IEnumerator Reload(BulletMode bulletType)
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);

        switch (bulletType)
        {
            case BulletMode.COMMON:
                currentCommonAmmo = maxCommonAmmo;
                break;
            case BulletMode.ENERGY:
                currentEnergyAmmo = maxEnergyAmmo;
                break;
        }

        isReloading = false;
    }

    /// <summary>
    /// Detecta input de recarga manual y dispara la recarga si no está en curso.
    /// </summary>
    void HandleManualReloadInput()
    {
        if (Input.GetKeyDown(reloadKey) && !isReloading)
        {
            PlayerShooter shooter = GetComponent<PlayerShooter>();
            if (shooter == null)
            {
                return;
            }

            BulletMode bulletToReload = shooter.GetCurrentBullet();

            // Solo recarga si no está lleno
            if ((bulletToReload == BulletMode.COMMON && currentCommonAmmo >= maxCommonAmmo) ||
                (bulletToReload == BulletMode.ENERGY && currentEnergyAmmo >= maxEnergyAmmo))
            {
                return;
            }

            StartCoroutine(Reload(bulletToReload));
        }
    }
    #endregion
}
