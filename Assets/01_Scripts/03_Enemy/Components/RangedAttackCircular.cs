using UnityEngine;

/// <summary>
/// Controla el disparo circular de un enemigo, disparando varias balas en 360 grados.
/// Implementa cooldown entre cada ráfaga.
/// </summary>
public class RangedAttackCircular : MonoBehaviour
{
    #region Inspector Variables
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform firePoint;
    [SerializeField] int numProjectiles = 8;
    [SerializeField] float timeBtwShots = 5f;
    #endregion

    #region Private Fields
    float shotTimer = 0f;
    bool canShoot = true;
    #endregion

    #region Unity Methods
    void Update()
    {
        UpdateShootTimer();
    }
    #endregion

    #region Timer Logic
    /// <summary>
    /// Controla el tiempo de espera entre cada ráfaga de disparo.
    /// </summary>
    void UpdateShootTimer()
    {
        if (!canShoot)
        {
            shotTimer += Time.deltaTime;
            if (shotTimer >= timeBtwShots)
            {
                shotTimer = 0f;
                canShoot = true;
            }
        }
    }
    #endregion

    #region Shooting Logic
    /// <summary>
    /// Dispara las balas distribuidas uniformemente en círculo desde firePoint.
    /// </summary>
    public void Shoot()
    {
        if (!canShoot || bulletPrefab == null || firePoint == null)
        {
            return;
        }

        float angleStep = 360f / numProjectiles;
        float angle = 0f;

        for (int i = 0; i < numProjectiles; i++)
        {
            Vector2 dir = new Vector2(
                Mathf.Cos(Mathf.Deg2Rad * angle),
                Mathf.Sin(Mathf.Deg2Rad * angle)
            );

            LaunchBullet(dir);
            angle += angleStep;
        }

        canShoot = false;
    }

    /// <summary>
    /// Instancia la bala y llama a Launch con la direccion adecuada.
    /// </summary>
    void LaunchBullet(Vector2 dir)
    {
        GameObject instance = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        BaseBullet bulletComp = instance.GetComponent<BaseBullet>();
        if (bulletComp != null)
        {
            bulletComp.Launch(dir);
        }
    }
    #endregion

    #region Public Methods
    public bool CanShoot()
    {
        return canShoot;
    }
    #endregion
}
