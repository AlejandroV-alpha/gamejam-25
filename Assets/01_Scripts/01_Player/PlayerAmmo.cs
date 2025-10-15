using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerShooter))]
public class PlayerAmmo : MonoBehaviour
{
    [Header("Ammo Settings (by bullet type)")]
    [SerializeField] int[] maxAmmoPerType;
    [SerializeField] float reloadTime = 1.5f;

    [Header("Keys")]
    [SerializeField] KeyCode reloadKey = KeyCode.R;

    [SerializeField] int[] currentAmmoPerType;
    bool isReloading = false;

    PlayerShooter playerShooter;

    void Awake()
    {
        playerShooter = GetComponent<PlayerShooter>();

        InitializeAmmoArrays();
    }

    void Update()
    {
        HandleManualReloadInput();
    }

    void InitializeAmmoArrays()
    {
        if (maxAmmoPerType != null && maxAmmoPerType.Length > 0)
        {
            currentAmmoPerType = new int[maxAmmoPerType.Length];
            maxAmmoPerType.CopyTo(currentAmmoPerType, 0);
        }
    }

    #region Ammo Management
    /// <summary>
    /// Intenta consumir una unidad de municion del tipo indicado por indice.
    /// Devuelve true si se pudo disparar.
    /// </summary>
    public bool TryConsumeAmmo(int bulletTypeIndex)
    {
        if (isReloading)
        {
            return false;
        }

        if (currentAmmoPerType == null || bulletTypeIndex < 0 || bulletTypeIndex >= currentAmmoPerType.Length)
        {
            // indice invalido -> no disparar
            return false;
        }

        if (currentAmmoPerType[bulletTypeIndex] > 0)
        {
            currentAmmoPerType[bulletTypeIndex]--;
            if (currentAmmoPerType[bulletTypeIndex] == 0)
            {
                StartReloadIfNeeded(bulletTypeIndex);
            }
            return true;
        }

        // si no hay balas, iniciar recarga automatica
        StartReloadIfNeeded(bulletTypeIndex);
        return false;
    }


    /// <summary>
    /// Devuelve la cantidad de municion restante para el tipo indicado por indice.
    /// </summary>
    public int GetCurrentAmmo(int bulletTypeIndex)
    {
        if (currentAmmoPerType == null || bulletTypeIndex < 0 || bulletTypeIndex >= currentAmmoPerType.Length)
        {
            return 0;
        }

        return currentAmmoPerType[bulletTypeIndex];
    }

    /// <summary>
    /// Devuelve true si actualmente se esta recargando.
    /// </summary>
    public bool IsReloading()
    {
        return isReloading;
    }

    /// <summary>
    /// Inicia la recarga si no hay otra recarga en curso.
    /// </summary>
    void StartReloadIfNeeded(int bulletTypeIndex)
    {
        if (!isReloading)
        {
            StartCoroutine(ReloadCoroutine(bulletTypeIndex));
        }
    }
    #endregion

    #region Reload Logic
    /// <summary>
    /// Corrutina que recarga el tipo de municion indicado despues de un tiempo.
    /// </summary>
    IEnumerator ReloadCoroutine(int bulletTypeIndex)
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);

        if (maxAmmoPerType != null && bulletTypeIndex >= 0 && bulletTypeIndex < maxAmmoPerType.Length)
        {
            currentAmmoPerType[bulletTypeIndex] = maxAmmoPerType[bulletTypeIndex];
        }

        isReloading = false;
    }

    /// <summary>
    /// Detecta la entrada del jugador para iniciar una recarga manual.
    /// </summary>
    void HandleManualReloadInput()
    {
        if (Input.GetKeyDown(reloadKey) && !isReloading)
        {
            if (playerShooter == null)
            {
                playerShooter = GetComponent<PlayerShooter>();
            }
            if (playerShooter == null)
            {
                return;
            }

            int currentIndex = playerShooter.GetCurrentBulletIndex();

            if (GetCurrentAmmo(currentIndex) >= maxAmmoPerType[currentIndex])
            {
                return;
            }

            StartCoroutine(ReloadCoroutine(currentIndex));
        }
    }
    #endregion
}
