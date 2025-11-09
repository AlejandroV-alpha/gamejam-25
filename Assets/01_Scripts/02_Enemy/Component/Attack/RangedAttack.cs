using UnityEngine;

/// <summary>
/// Componente para manejar disparos a distancia de un enemigo.
/// Implementa la interfaz IShooter.
/// </summary>
public class RangedAttack : MonoBehaviour, IShooter
{
    #region Inspector Variables
    [Header("Bullet Settings")]
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform firePoint;

    [Header("Attack Settings")]
    [SerializeField] float cooldownBetweenShots = 1f;
    #endregion

    #region Private Fields
    bool canShoot = true;
    float shotTimer = 0f;
    #endregion

    #region Unity Methods
    void Update()
    {
        UpdateCooldown();
    }
    #endregion

    #region Timer Logic
    /// <summary>
    /// Maneja el cooldown entre disparos.
    /// </summary>
    void UpdateCooldown()
    {
        if (!canShoot)
        {
            shotTimer += Time.deltaTime;
            if (shotTimer >= cooldownBetweenShots)
            {
                canShoot = true;
                shotTimer = 0f;
            }
        }
    }
    #endregion

    #region Shooting Logic
    /// <summary>
    /// Dispara la bala hacia la direccion del firePoint.
    /// </summary>
    public void Shoot()
    {
        if (!canShoot || bulletPrefab == null || firePoint == null)
        {
            return;
        }

        GameObject instance = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        BaseBullet bulletComp = instance.GetComponent<BaseBullet>();
        if (bulletComp != null)
        {
            bulletComp.Launch(firePoint.up);
        }

        canShoot = false;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Retorna si puede disparar.
    /// </summary>
    public bool CanShoot()
    {
        return canShoot;
    }
    #endregion
}
