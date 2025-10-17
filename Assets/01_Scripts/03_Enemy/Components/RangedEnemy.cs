// RangedEnemy.cs
using UnityEngine;

/// <summary>
/// Componente que maneja disparos a distancia con cooldown.
/// </summary>
public class RangedEnemy : MonoBehaviour
{
    #region Inspector
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Transform firePoint;
    [SerializeField, Min(0.1f)] float timeBtwShoot = 1f;
    #endregion

    #region Private Fields
    float timer = 0f;
    Transform target;
    #endregion

    #region Public Methods
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void Tick()
    {
        if (target == null || projectilePrefab == null || firePoint == null) return;

        timer += Time.deltaTime;
        if (timer > timeBtwShoot)
        {
            Shoot();
            timer = 0f;
        }
    }

    void Shoot()
    {
        GameObject instance = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        BaseBullet bulletComp = instance.GetComponent<BaseBullet>();
        if (bulletComp != null)
        {
            bulletComp.Launch(firePoint.up);
        }
    }
    #endregion
}
