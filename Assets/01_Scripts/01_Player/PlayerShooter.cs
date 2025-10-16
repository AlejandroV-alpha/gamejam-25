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
    /// Detecta la tecla de disparo, comprueba la municion y recarga.
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

                if (prefab != null)
                {
                    BaseBullet bulletData = prefab.GetComponent<BaseBullet>();

                    // Pregunta a la bala si requiere energía para dispararse.
                    if (bulletData != null && playerHealth != null)
                    {
                        float energyCost = bulletData.GetEnergyCost();
                        if (energyCost > 0f)
                        {
                            playerHealth.TakeDamage(energyCost, Vector2.zero);
                        }
                    }

                    ShootBullet(prefab);
                    canShoot = false;
                }
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

        BaseBullet bulletComp = instance.GetComponent<BaseBullet>();
        if (bulletComp != null)
        {
            bulletComp.Launch(firePoint.up);
        }
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