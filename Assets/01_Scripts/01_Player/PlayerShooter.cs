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
    [SerializeField] Transform firePoint;

    [Header("Bullets (prefabs)")]
    [Tooltip("Orden: 0 = COMMON, 1 = ENERGY")]
    [SerializeField] GameObject[] bulletPrefabs;

    [Header("Energy Cost")]
    [SerializeField] float energyCostPerEnergyBullet = 5f;


    int currentBulletIndex = 0;
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
            if (playerAmmo.TryConsumeAmmo(currentBulletIndex))
            {
                GameObject prefab = GetCurrentBulletPrefab();
                // Si la bala es de tipo ENERGY, se resta energía al jugador
                if (prefab != null && prefab.GetComponent<BulletEnergy>() != null && playerHealth != null)
                {
                    playerHealth.TakeDamage(energyCostPerEnergyBullet, Vector2.zero, 0f);
                }

                ShootBullet(prefab);
                canShoot = false;
            }
            else
            {
                // Si no pudo disparar (sin munición), playerAmmo habrá iniciado recarga automática.
                // Aquí podrías reproducir un sonido "click".
            }
        }
    }

    /// <summary>
    /// Alterna entre los tipos de bala disponibles.
    /// </summary>
    void HandleSwitchBulletInput()
    {
        if (Input.GetKeyDown(switchBulletKey) && bulletPrefabs != null && bulletPrefabs.Length > 0)
        {
            currentBulletIndex = (currentBulletIndex + 1) % bulletPrefabs.Length;
        }
    }

    /// <summary>
    /// Instancia la bala correspondiente en la posición y rotación del firePoint.
    /// </summary>
    void ShootBullet(GameObject prefab)
    {
        if (prefab == null)
        {
            return;
        }

        GameObject instance = Instantiate(prefab, firePoint.position, firePoint.rotation);

        //BulletBase bulletComp = instance.GetComponent<BulletBase>();
        //if (bulletComp != null)
        //{
        //    // Nota: uso firePoint.up para que tu rotación actual siga funcionando como antes.
        //    bulletComp.Initialize(firePoint.up);
        //}
    }
    #endregion

    #region Utilities
    /// <summary>
    /// Devuelve el prefab de bala actualmente seleccionado.
    /// Si la lista esta vacia o el indice es invalido, devuelve null.
    /// </summary>
    public GameObject GetCurrentBulletPrefab()
    {
        if (bulletPrefabs == null || bulletPrefabs.Length == 0)
        {
            return null;
        }
        if (currentBulletIndex < 0 || currentBulletIndex >= bulletPrefabs.Length)
        {
            return null;
        }

        return bulletPrefabs[currentBulletIndex];
    }

    /// <summary>
    /// Devuelve la cantidad total de tipos de bala disponibles.
    /// </summary>
    public int GetBulletTypesCount()
    {
        return bulletPrefabs != null ? bulletPrefabs.Length : 0;
    }

    /// <summary>
    /// Devuelve el indice del tipo de bala actualmente seleccionado.
    /// </summary>
    public int GetCurrentBulletIndex()
    {
        return currentBulletIndex;
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