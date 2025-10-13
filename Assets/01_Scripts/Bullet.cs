using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    #region Variables Generales
    [Header("Bullet Type")]
    public BulletType bulletType;

    [Header("Impact Properties")]
    [SerializeField] float knockbackForce = 0f;

    [Header("Properties of Movement")]
    [SerializeField] float moveSpeed = 15f;
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

    void Start()
    {
        Destroy(gameObject, timeToDestroy);
    }

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
        if (collision.CompareTag("Floor"))
        {
            return;
        }
        
        ITakeDamage target = collision.GetComponent<ITakeDamage>();

        if (target != null)
        {
            Vector2 hitDir = (collision.transform.position - transform.position).normalized;

            switch (bulletType)
            {
                case BulletType.EnemyDamage:
                    if (collision.gameObject.CompareTag("Player"))
                    {
                        target.TakeDamage(damage, hitDir, knockbackForce);
                    }
                    break;

                case BulletType.PlayerEnergy:
                    if (collision.gameObject.CompareTag("Nodo"))
                    {
                        target.TakeDamage(energy, hitDir, knockbackForce);
                    }
                    break;

                case BulletType.PlayerDamage:
                    if (collision.gameObject.CompareTag("Enemy"))
                    {
                        target.TakeDamage(damage, hitDir, knockbackForce);
                    }
                    break;

                default:
                    break;
            }
        }

        Destroy(gameObject);
    }
    #endregion


    #region Metodos Publicos
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