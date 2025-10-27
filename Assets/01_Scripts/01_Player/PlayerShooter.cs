using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerAmmo))]
public class PlayerShooter : MonoBehaviour
{
    [Header("Keys")]
    [SerializeField] private KeyCode shootKey = KeyCode.Space;
    [SerializeField] private KeyCode switchBulletKey = KeyCode.E;

    [Header("Bullet Settings")]
    [SerializeField] private float timeBtwShoot = 0.5f;
    [SerializeField] private Transform firePoint;

    [Header("Bullets (prefabs)")]
    [Tooltip("Orden: 0 = COMMON, 1 = ENERGY")]
    [SerializeField] private GameObject[] bulletPrefabs;

    private int currentBulletIndex = 0;
    private float timer = 0f;
    private bool canShoot = true;

    private PlayerAmmo playerAmmo;
    private PlayerHealth playerHealth;

    // ===== RECOIL integrado =====
    [Header("Recoil")]
    [SerializeField] private Transform recoilTarget;          // normalmente Graphics/Turret
    [SerializeField] private Axis recoilAxis = Axis.Up;       // ¿tu cañón “mira” con Up o Right?
    [SerializeField] private bool recoilInvert = true;        // true = hacia atrás
    [SerializeField] private float recoilDistance = 0.08f;    // cuánto retrocede
    [SerializeField] private float recoilOutTime = 0.06f;     // ida
    [SerializeField] private float recoilInTime = 0.10f;     // vuelta
    [SerializeField] private AnimationCurve recoilOutCurve = null;
    [SerializeField] private AnimationCurve recoilInCurve = null;

    private enum Axis { Up, Right }
    private Coroutine recoilCo;
    private Vector3 recoilBaseLocalPos;

    private void Awake()
    {
        playerAmmo = GetComponent<PlayerAmmo>();
        playerHealth = GetComponent<PlayerHealth>();

        // Autovincular recoilTarget si está vacío
        if (!recoilTarget)
        {
            var t = transform.Find("Graphics/Turret");
            if (t) recoilTarget = t;
        }

        if (recoilOutCurve == null) recoilOutCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        if (recoilInCurve == null) recoilInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        if (recoilTarget) recoilBaseLocalPos = recoilTarget.localPosition;
    }

    private void Update()
    {
        UpdateShootTimer();
        HandleShootInput();
        HandleSwitchBulletInput();
    }

    #region Shooting Logic Methods
    private void UpdateShootTimer()
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

    private void HandleShootInput()
    {
        if (!canShoot || firePoint == null || playerAmmo.IsReloading())
            return;

        if (Input.GetKeyDown(shootKey))
        {
            if (playerAmmo.TryConsumeAmmo(currentBulletIndex))
            {
                GameObject prefab = GetCurrentBulletPrefab();
                if (prefab != null)
                {
                    BaseBullet bulletData = prefab.GetComponent<BaseBullet>();

                    // Coste/beneficio de energía por disparo:
                    //  energyCost > 0 => gasta (resta)
                    //  energyCost < 0 => cura (suma)
                    if (bulletData != null && playerHealth != null)
                    {
                        float energyCost = bulletData.GetEnergyCost();
                        if (energyCost != 0f)
                            playerHealth.ChangeEnergy(-energyCost);
                    }

                    ShootBullet(prefab);
                    canShoot = false;
                }
            }
        }
    }

    private void HandleSwitchBulletInput()
    {
        if (Input.GetKeyDown(switchBulletKey) && bulletPrefabs != null && bulletPrefabs.Length > 0)
        {
            currentBulletIndex = (currentBulletIndex + 1) % bulletPrefabs.Length;
        }
    }

    private void ShootBullet(GameObject prefab)
    {
        if (prefab == null) return;

        GameObject instance = Instantiate(prefab, firePoint.position, firePoint.rotation);

        BaseBullet bulletComp = instance.GetComponent<BaseBullet>();
        if (bulletComp != null)
        {
            // Usa el MISMO eje que configuras en recoilAxis
            Vector2 dir = (recoilAxis == Axis.Up) ? (Vector2)firePoint.up : (Vector2)firePoint.right;
            bulletComp.Launch(dir);
        }

        // Retroceso del turret integrado
        PlayRecoil();
    }
    #endregion

    #region Utilities
    public GameObject GetCurrentBulletPrefab()
    {
        if (bulletPrefabs == null || bulletPrefabs.Length == 0) return null;
        if (currentBulletIndex < 0 || currentBulletIndex >= bulletPrefabs.Length) return null;
        return bulletPrefabs[currentBulletIndex];
    }

    public int GetBulletTypesCount()
    {
        return bulletPrefabs != null ? bulletPrefabs.Length : 0;
    }

    public int GetCurrentBulletIndex()
    {
        return currentBulletIndex;
    }
    #endregion

    #region Recoil Animation
    private void OnDisable()
    {
        if (recoilTarget) recoilTarget.localPosition = recoilBaseLocalPos;
        recoilCo = null;
    }

    private void PlayRecoil()
    {
        if (!recoilTarget || !gameObject.activeInHierarchy) return;
        if (recoilCo != null) StopCoroutine(recoilCo);
        recoilCo = StartCoroutine(CoRecoil());
    }

    private System.Collections.IEnumerator CoRecoil()
    {
        // Dirección mundial según orientación del cañón
        Vector3 worldDir = (recoilAxis == Axis.Up) ? recoilTarget.up : recoilTarget.right;
        if (recoilInvert) worldDir = -worldDir;

        // Convertir a espacio local del recoilTarget
        Vector3 localDir = recoilTarget.InverseTransformDirection(worldDir);

        Vector3 start = recoilBaseLocalPos;
        Vector3 back = start + localDir * recoilDistance;

        // Ida
        float t = 0f;
        float outT = Mathf.Max(0.01f, recoilOutTime);
        while (t < outT)
        {
            float k = recoilOutCurve.Evaluate(t / outT);
            recoilTarget.localPosition = Vector3.LerpUnclamped(start, back, k);
            t += Time.deltaTime;
            yield return null;
        }
        recoilTarget.localPosition = back;

        // Vuelta
        t = 0f;
        float inT = Mathf.Max(0.01f, recoilInTime);
        while (t < inT)
        {
            float k = recoilInCurve.Evaluate(t / inT);
            recoilTarget.localPosition = Vector3.LerpUnclamped(back, start, k);
            t += Time.deltaTime;
            yield return null;
        }
        recoilTarget.localPosition = start;
        recoilCo = null;
    }
    #endregion
}
