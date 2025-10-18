using UnityEngine;

/// <summary>
/// Controla el disparo triple de una torreta, con sobrecalentamiento.
/// </summary>
public class RangedAttackTriple : MonoBehaviour
{
    #region Inspector Variables
    [Header("Bullet Settings")]
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform firePoint;
    [SerializeField] float spreadAngle = 45f;

    [Header("Attack Settings")]
    [SerializeField] float timeBtwShoots = 1f;
    [SerializeField] int maxRafagas = 4;
    [SerializeField] float overheatCooldown = 3f;
    #endregion

    #region Fields
    float lastShootTime;
    int currentRafagas;
    bool isOverheated;
    float overheatTimer;
    #endregion

    #region Public Methods
    /// <summary>
    /// Verifica si puede disparar considerando sobrecalentamiento y tiempo entre disparos.
    /// </summary>
    public bool CanShoot()
    {
        if (isOverheated)
        {
            overheatTimer += Time.deltaTime;
            if (overheatTimer >= overheatCooldown)
            {
                isOverheated = false;
                currentRafagas = 0;
                overheatTimer = 0f;
            }
            else
            {
                return false;
            }
        }

        if (Time.time - lastShootTime >= timeBtwShoots)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Dispara 3 balas en forma de abanico y maneja sobrecalentamiento.
    /// </summary>
    public void Shoot()
    {
        if (!CanShoot() || bulletPrefab == null || firePoint == null)
        {
            return;
        }

        lastShootTime = Time.time;
        currentRafagas++;

        // Direcciones
        Vector2 forward = firePoint.up;
        Vector2 left = Quaternion.Euler(0, 0, spreadAngle) * forward;
        Vector2 right = Quaternion.Euler(0, 0, -spreadAngle) * forward;

        // Instanciamos y lanzamos cada bala
        LaunchBullet(forward);
        LaunchBullet(left);
        LaunchBullet(right);

        if (currentRafagas >= maxRafagas)
        {
            isOverheated = true;
        }
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
}
