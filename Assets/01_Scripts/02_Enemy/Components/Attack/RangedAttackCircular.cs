using UnityEngine;

/// <summary>
/// Componente que dispara balas en 360 grados.
/// Implementa cooldown entre rafagas.
/// </summary>
public class RangedAttackCircular : MonoBehaviour, IShooter
{
    #region Inspector Variables
    [Header("Bullet Settings")]
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform firePoint;

    [Header("Burst Settings")]
    [SerializeField] int bulletsPerBurst = 8;
    [SerializeField] float cooldownBetweenBursts = 5f;
    #endregion

    #region Private Fields
    bool canShoot = true;
    float burstCooldownTimer = 0f;
    #endregion

    #region Unity Methods
    void Update()
    {
        UpdateBurstCooldown();
    }
    #endregion

    #region Timer Logic
    /// <summary>
    /// Maneja el cooldown entre rafagas.
    /// </summary>
    void UpdateBurstCooldown()
    {
        if (!canShoot)
        {
            burstCooldownTimer += Time.deltaTime;
            if (burstCooldownTimer >= cooldownBetweenBursts)
            {
                burstCooldownTimer = 0f;
                canShoot = true;
            }
        }
    }
    #endregion

    #region Shooting Logic
    /// <summary>
    /// Dispara las balas en 360 grados uniformemente.
    /// </summary>
    public void Shoot()
    {
        if (!canShoot || bulletPrefab == null || firePoint == null)
        {
            return;
        }

        float angleStep = 360f / bulletsPerBurst;
        float angle = 0f;

        for (int i = 0; i < bulletsPerBurst; i++)
        {
            Vector2 direction = new Vector2(
                Mathf.Cos(Mathf.Deg2Rad * angle),
                Mathf.Sin(Mathf.Deg2Rad * angle)
            );

            FireBullet(direction);
            angle += angleStep;
        }

        canShoot = false;
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
