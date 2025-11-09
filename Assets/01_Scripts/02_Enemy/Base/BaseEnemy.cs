// BaseEnemy.cs
using UnityEngine;

/// <summary>
/// Clase base abstracta para todos los enemigos.
/// Implementa ciclo de vida, detección y manejo de daño.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public abstract class BaseEnemy : MonoBehaviour, IEnemy, ITakeDamage
{
    #region Inspector Variables
    [Header("Base Stats")]
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float alertRange = 6f;
    [SerializeField] protected float attackRange = 4f;

    [Header("Energy")]
    [SerializeField] protected GameObject energyPrefab;

    #endregion

    #region Protected Fields
    [SerializeField] protected float currentHealth;
    protected EnemyStateMachine stateMachine;
    #endregion

    #region Public Properties
    public Transform Target { get; set; }
    #endregion

    #region Unity Methods
    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        stateMachine = new EnemyStateMachine();
    }

    protected virtual void Start()
    {
        if (Target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                Target = playerObj.transform;
            }
        }
    }

    protected virtual void Update()
    {
        Tick();
    }
    #endregion

    #region Behaviour Cycle
    /// <summary>
    /// Actualiza el estado actual del enemigo y ejecuta el comportamiento correspondiente.
    /// </summary>
    public virtual void Tick()
    {
        UpdateTargetState();

        switch (stateMachine.CurrentState)
        {
            case EnemyState.Idle:
                IdleBehaviour();
                break;
            case EnemyState.Alert:
                AlertBehaviour();
                break;
            case EnemyState.Attack:
                AttackBehaviour();
                break;
            case EnemyState.Death:
                DeathBehaviour();
                break;
        }
    }

    /// <summary>
    /// Verifica la distancia al objetivo y actualiza la maquina de estados.
    /// </summary>
    protected virtual void UpdateTargetState()
    {
        if (Target == null)
        {
            return;
        }

        float distance = Vector2.Distance(transform.position, Target.position);

        if (distance <= attackRange)
        {
            stateMachine.ChangeState(EnemyState.Attack);
        }
        else if (distance <= alertRange)
        {
            stateMachine.ChangeState(EnemyState.Alert);
        }
        else
        {
            stateMachine.ChangeState(EnemyState.Idle);
        }
    }
    #endregion

    #region Abstract Behaviours
    /// <summary>
    /// Define el comportamiento cuando el enemigo esta en estado Idle.
    /// </summary>
    protected abstract void IdleBehaviour();

    /// <summary>
    /// Define el comportamiento cuando el enemigo esta en estado Alert.
    /// </summary>
    protected abstract void AlertBehaviour();

    /// <summary>
    /// Define el comportamiento cuando el enemigo esta en estado Attack.
    /// </summary>
    protected abstract void AttackBehaviour();

    /// <summary>
    /// Define el comportamiento cuando el enemigo esta en estado Death.
    /// </summary>
    protected abstract void DeathBehaviour();
    #endregion

    #region IEnemy Methods
    /// <summary>
    /// Inicializa el enemigo en estado Idle.
    /// </summary>
    public virtual void Initialize()
    {
        stateMachine.ChangeState(EnemyState.Idle);
    }

    /// <summary>
    /// Se llama cuando el enemigo detecta al jugador.
    /// </summary>
    public virtual void OnDetectPlayer(Transform player)
    {
        Target = player;
        stateMachine.ChangeState(EnemyState.Alert);
    }

    /// <summary>
    /// Se llama cuando el enemigo pierde de vista al jugador.
    /// </summary>
    public virtual void OnLosePlayer()
    {
        Target = null;
        stateMachine.ChangeState(EnemyState.Idle);
    }

    /// <summary>
    /// Acciones al morir (animacion, efectos, loot, etc.).
    /// </summary>
    public virtual void OnDeath()
    {
        Instantiate(energyPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
    #endregion

    #region ITakeDamage Methods
    /// <summary>
    /// Reduce la vida al recibir daño y activa la muerte si la vida llega a cero.
    /// </summary>
    public virtual void TakeDamage(float damage, Vector2 hitDirection, float knockbackForce)
    {
        currentHealth -= damage;
        if (currentHealth <= 0 && !stateMachine.IsInState(EnemyState.Death))
        {
            stateMachine.ChangeState(EnemyState.Death);
            OnDeath();
        }
    }
    #endregion

    #region Debug Gizmos
    /// <summary>
    /// Dibuja los rangos de alerta y ataque en el editor.
    /// </summary>
    protected void DrawRanges()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, alertRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    #endregion
}
