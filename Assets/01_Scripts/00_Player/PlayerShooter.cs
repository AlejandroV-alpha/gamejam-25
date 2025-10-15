using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerAmmo))]
public class PlayerShooter : MonoBehaviour
{
    [Header("Keys")]
    [SerializeField] KeyCode shootKey = KeyCode.Space;
    [SerializeField] KeyCode switchBulletKey = KeyCode.E;

    [Header("Bullet Settings")]
    [SerializeField] float timeBtwShoot = 0.5f;
    [SerializeField] GameObject commonBulletPrefab;
    [SerializeField] GameObject energyBulletPrefab;
    [SerializeField] Transform firePoint;

    [Header("Energy Cost")]
    [SerializeField] float energyCostPerEnergyBullet = 5f;


    BulletMode currentBullet = BulletMode.COMMON;
    float timer = 0f;
    bool canShoot = true;

    PlayerAmmo playerAmmo;
    PlayerHealth playerHealth;

    private void Awake()
    {
        playerAmmo = GetComponent<PlayerAmmo>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateShootTimer();
        HandleShootInput();
        HandleSwitchBulletInput();
    }

    #region Shooting Logic Methods
    /// <summary>
    /// Controla el tiempo de espera entre disparos.
    /// </summary>
    void UpdateShootTimer()
    {
        if (!canShoot)
        {
            timer += Time.deltaTime;
            if (timer >= timeBtwShoot)
            {
                timer = 0f;
                canShoot = true;
            }
        }
    }

    /// <summary>
    /// Detecta la tecla de disparo, verifica munición y recarga,
    /// y dispara la bala correspondiente si se cumplen las condiciones.
    /// </summary>
    void HandleShootInput()
    {
        if (!canShoot || firePoint == null || playerAmmo.IsReloading())
        {
            return;
        }

        if (Input.GetKeyDown(shootKey))
        {
            if (playerAmmo.TryConsumeAmmo(currentBullet, this))
            {
                // Si la bala es de tipo ENERGY, se resta energía al jugador
                if (currentBullet == BulletMode.ENERGY && playerHealth != null)
                {
                    playerHealth.TakeDamage(energyCostPerEnergyBullet, Vector2.zero, 0f);
                }

                ShootBullet();
                canShoot = false;
            }
        }
    }

    /// <summary>
    /// Alterna entre los tipos de bala disponibles.
    /// </summary>
    void HandleSwitchBulletInput()
    {
        if (Input.GetKeyDown(switchBulletKey))
        {
            currentBullet = (currentBullet == BulletMode.COMMON) ? BulletMode.ENERGY : BulletMode.COMMON;
        }
    }

    /// <summary>
    /// Instancia la bala correspondiente en la posición y rotación del firePoint.
    /// </summary>
    void ShootBullet()
    {
        GameObject prefab = GetCurrentBulletPrefab();
        if (prefab != null)
        {
            GameObject bullet = Instantiate(prefab, firePoint.position, firePoint.rotation);
            // bullet.GetComponent<Bullet>().SetDirection(firePoint.up);
        }
    }

    /// <summary>
    /// Devuelve el prefab de bala según el tipo de bala actualmente seleccionado.
    /// </summary>
    GameObject GetCurrentBulletPrefab()
    {
        return currentBullet == BulletMode.COMMON ? commonBulletPrefab : energyBulletPrefab;
    }
    #endregion

    #region Utilities
    public BulletMode GetCurrentBullet()
    {
        return currentBullet;
    }
    #endregion
}

/// <summary>
/// Define los tipos de bala que puede disparar el jugador.
/// </summary>
public enum BulletMode
{
    COMMON,
    ENERGY
}