using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab; // Prefab de la bala del enemigo
    [SerializeField] private Transform firePoint;     // Punto desde donde dispara
    [SerializeField] private float timeBetweenShots = 0.5f;

    private float shootTimer;

    void Update()
    {
        shootTimer += Time.deltaTime;

        if (shootTimer >= timeBetweenShots)
        {
            Shoot();
            shootTimer = 0f;
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("Faltan referencias en EnemyShooter.");
            return;
        }

        // Instancia la bala
        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Bullet bullet = bulletObj.GetComponent<Bullet>();

        if (bullet != null)
        {
            bullet.bulletType = BulletType.EnemyDamage; // Bala enemiga
            bullet.SetDirection(firePoint.up);          // Dispara hacia adelante
        }

        Debug.Log("Enemy disparó una bala hacia adelante.");
    }
}
