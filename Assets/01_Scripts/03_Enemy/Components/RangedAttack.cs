using UnityEngine;

/// <summary>
/// Componente para manejar disparos a distancia de un enemigo.
/// Implementa la interfaz IShooter.
/// </summary>
public class RangedAttack : MonoBehaviour, IShooter
{
    #region Inspector Variables
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform firePoint;
    [SerializeField] float timeBtwShoot = 1f;
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
    /// Controla el tiempo de espera entre disparos.
    /// </summary>
    void UpdateShootTimer()
    {
        if (!canShoot)
        {
            shotTimer += Time.deltaTime;
            if (shotTimer >= timeBtwShoot)
            {
                shotTimer = 0f;
                canShoot = true;
            }
        }
    }
    #endregion

    #region Shooting Logic
    /// <summary>
    /// Dispara el proyectil asignado hacia la dirección del firePoint.
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

    public bool CanShoot()
    {
        return canShoot;
    }
}
