using UnityEngine;

/// <summary>
/// Componente que dispara tres balas en abanico y maneja sobrecalentamiento.
/// Implementa la interfaz IShooter.
/// </summary>
public class RangedAttackTriple : MonoBehaviour, IShooter
{
    #region Inspector Variables
    [Header("Bullet Settings")]
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform firePoint;
    [SerializeField] float spreadAngle = 45f;

    [Header("Attack Settings")]
    [SerializeField] float cooldownBetweenShots = 1f;
    [SerializeField] int maxBursts = 4;
    [SerializeField] float overheatCooldown = 3f;
    #endregion

    #region Private Fields
    float lastShotTime;
    int burstsFired;
    bool isOverheated;
    float overheatTimer;
    #endregion

    #region Public Methods
    /// <summary>
    /// Retorna si puede disparar considerando cooldown y sobrecalentamiento.
    /// </summary>
    public bool CanShoot()
    {
        if (isOverheated)
        {
            overheatTimer += Time.deltaTime;
            if (overheatTimer >= overheatCooldown)
            {
                isOverheated = false;
                burstsFired = 0;
                overheatTimer = 0f;
            }
            else
            {
                return false;
            }
        }

        if (Time.time - lastShotTime >= cooldownBetweenShots)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Dispara tres balas en abanico y maneja sobrecalentamiento.
    /// </summary>
    public void Shoot()
    {
        if (!CanShoot() || bulletPrefab == null || firePoint == null)
        {
            return;
        }

        lastShotTime = Time.time;
        burstsFired++;

        Vector2 forward = firePoint.up;
        Vector2 left = Quaternion.Euler(0, 0, spreadAngle) * forward;
        Vector2 right = Quaternion.Euler(0, 0, -spreadAngle) * forward;

        FireBullet(forward);
        FireBullet(left);
        FireBullet(right);

        if (burstsFired >= maxBursts)
        {
            isOverheated = true;
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
}
