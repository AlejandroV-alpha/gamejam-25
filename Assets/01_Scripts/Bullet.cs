using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    #region Variables
    [Header("Bullet Type")]
    public BulletType bulletType;

    [Header("Properties of Movement")]
    [SerializeField] float moveSpeed = 7f;
    [SerializeField] float timeToDestroy = 5f;
    [SerializeField] Vector2 direction = Vector2.up;

    [Header("Damage/Energy Properties")]
    [SerializeField] float damage = 1f;
    [SerializeField] float energy = 1f;

    // Variables internas
    Rigidbody2D rb;
    #endregion


    /// <summary>
    /// Inicializa referencias internas.
    /// Configura Rigidbody2D como Kinematic para movimiento controlado manualmente.
    /// Uso de Continuous, evita que atraviesen otros objetos sin colisionar por la velocidad alta.
    /// </summary>
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    /// <summary>
    /// Destruye la bala automaticamente despues de timeToDestroy segundos.
    /// </summary>
    void Start()
    {
        Destroy(gameObject, timeToDestroy);
    }

    /// <summary>
    /// Mueve la bala en la direccion definida usando Rigidbody2D.
    /// Se llama en FixedUpdate para respetar la fisica.
    /// </summary>
    void FixedUpdate()
    {
        Vector2 move = direction.normalized * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
    }


    #region Colision con un objeto
    /// <summary>
    /// Aplica danio o energia a cualquier objeto que implemente ITakeDamage.
    /// La logica depende del tipo de bala y del tag del objeto.
    /// Destruye la bala al colisionar con cualquier objeto.
    /// </summary>
    /// <param name="collision">Collider del objeto que la bala ha tocado</param>
    void OnTriggerEnter2D(Collider2D collision)
    {
        ITakeDamage target = collision.GetComponent<ITakeDamage>();

        if (target != null)
        {
            Vector2 hitDir = (collision.transform.position - transform.position).normalized;

            switch (bulletType)
            {
                case BulletType.EnemyDamage:
                    if (collision.gameObject.CompareTag("Player"))
                    {
                        target.TakeDamage(damage, hitDir);
                    }
                    break;

                case BulletType.PlayerEnergy:
                    if (collision.gameObject.CompareTag("Nodo"))
                    {
                        target.TakeDamage(energy, hitDir);
                    }
                    break;

                case BulletType.PlayerDamage:
                    if (collision.gameObject.CompareTag("Enemy"))
                    {
                        target.TakeDamage(damage, hitDir);
                    }
                    break;

                default:
                    break;
            }
        }

        Destroy(gameObject);
    }
    #endregion


    #region Metodos publicos
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }
    #endregion
}

public enum BulletType
{
    EnemyDamage,
    PlayerEnergy,
    PlayerDamage
}