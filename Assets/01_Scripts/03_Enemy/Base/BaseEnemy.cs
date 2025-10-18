// BaseEnemy.cs
using UnityEngine;

/// <summary>
/// Clase base abstracta para todos los enemigos.
/// Implementa ciclo de vida, detección y daño.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public abstract class BaseEnemy : MonoBehaviour, IEnemy
{
    #region Inspector Variables
    [Header("Base Stats")]
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float alertRange = 6f; // rango para alerta
    [SerializeField] protected float attackRange = 4f; // rango para disparo
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
    protected abstract void IdleBehaviour();
    protected abstract void AlertBehaviour();
    protected abstract void AttackBehaviour();
    protected abstract void DeathBehaviour();
    #endregion

    #region IEnemy Methods
    public virtual void Initialize()
    {
        stateMachine.ChangeState(EnemyState.Idle);
    }

    public virtual void OnDetectPlayer(Transform player)
    {
        Target = player;
        stateMachine.ChangeState(EnemyState.Alert);
    }

    public virtual void OnLosePlayer()
    {
        Target = null;
        stateMachine.ChangeState(EnemyState.Idle);
    }

    public virtual void TakeDamage(float damage, Vector2 hitDirection, float knockbackForce)
    {
        currentHealth -= damage;
        if (currentHealth <= 0 && !stateMachine.IsInState(EnemyState.Death))
        {
            stateMachine.ChangeState(EnemyState.Death);
            OnDeath();
        }
    }

    public virtual void OnDeath()
    {
        Destroy(gameObject);
    }
    #endregion
}

