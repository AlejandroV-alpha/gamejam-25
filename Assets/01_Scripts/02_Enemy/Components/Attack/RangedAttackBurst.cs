using UnityEngine;

/// <summary>
/// Componente que dispara una rafaga de varias balas consecutivas.
/// Implementa la interfaz IShooter.
/// </summary>
public class RangedAttackBurst : MonoBehaviour, IShooter
{
    #region Inspector Variables
    [Header("Bullet Settings")]
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform firePoint;

    [Header("Burst Settings")]
    [SerializeField] float timeBetweenShotsInBurst = 0.2f;
    [SerializeField] float cooldownBetweenBursts = 2f;
    [SerializeField] int bulletsPerBurst = 3;
    #endregion

    #region Private Fields
    bool canShoot = true;
    bool isFiringBurst = false;
    int bulletsFiredInCurrentBurst = 0;

    float burstShotTimer = 0f;
    float burstCooldownTimer = 0f;
    #endregion

    #region Unity Methods
    void Update()
    {
        if (isFiringBurst)
        {
            HandleBurst();
        }
        else if (!canShoot)
        {
            HandleCooldown();
        }
    }
    #endregion

    #region Shooting Logic
    /// <summary>
    /// Inicia una nueva rafaga si puede disparar.
    /// </summary>
    public void Shoot()
    {
        if (canShoot && bulletPrefab != null && firePoint != null)
        {
            isFiringBurst = true;
            canShoot = false;
            bulletsFiredInCurrentBurst = 0;
            burstShotTimer = 0f;
        }
    }

    /// <summary>
    /// Maneja el disparo de cada bala en la rafaga.
    /// </summary>
    void HandleBurst()
    {
        burstShotTimer += Time.deltaTime;
        if (burstShotTimer >= timeBetweenShotsInBurst)
        {
            FireBullet(firePoint.up);
            bulletsFiredInCurrentBurst++;
            burstShotTimer = 0f;

            if (bulletsFiredInCurrentBurst >= bulletsPerBurst)
            {
                isFiringBurst = false;
                burstCooldownTimer = 0f;
            }
        }
    }

    /// <summary>
    /// Maneja el cooldown entre rafagas.
    /// </summary>
    void HandleCooldown()
    {
        burstCooldownTimer += Time.deltaTime;
        if (burstCooldownTimer >= cooldownBetweenBursts)
        {
            canShoot = true;
        }
    }

    /// <summary>
    /// Instancia la bala y la lanza en la direccion indicada.
    /// </summary>
    void FireBullet(Vector2 direction)
    {
        GameObject instance = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        BaseBullet bulletComp = instance.GetComponent<BaseBullet>();
        if (bulletComp != null)
        {
            bulletComp.Launch(direction);
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Retorna si puede disparar una nueva rafaga.
    /// </summary>
    public bool CanShoot()
    {
        return canShoot;
    }
    #endregion
}
