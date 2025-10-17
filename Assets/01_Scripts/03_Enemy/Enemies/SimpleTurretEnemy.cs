// SimpleTurretEnemy.cs
using UnityEngine;

/// <summary>
/// Enemigo fijo que rota hacia el jugador y dispara ráfagas.
/// Combina BaseEnemy + RotatorTowardsTarget + RangedEnemy.
/// </summary>
[RequireComponent(typeof(RotatorTowardsTarget))]
[RequireComponent(typeof(RangedEnemy))]
public class SimpleTurretEnemy : BaseEnemy
{
    #region Inspector
    [Tooltip("Transform that rotates to fire")]
    [SerializeField] Transform turretHead;
    #endregion

    #region Components
    RotatorTowardsTarget rotatorComponent;
    RangedEnemy rangedComponent;
    #endregion

    #region Unity Methods
    protected override void Awake()
    {
        base.Awake();
        rotatorComponent = GetComponent<RotatorTowardsTarget>();
        rangedComponent = GetComponent<RangedEnemy>();

        if (turretHead == null)
        {
            turretHead = transform;
        }
        rotatorComponent.SetPartToRotate(turretHead);
    }
    #endregion

    #region Behaviour Methods
    protected override void IdleBehaviour()
    {
        if (rotatorComponent.CurrentTarget() != null)
        {
            rotatorComponent.SetTarget(null);
        }

        rotatorComponent.Tick();
    }

    protected override void AlertBehaviour()
    {
        if (Target == null)
        {
            return;
        }

        rotatorComponent.SetTarget(Target);
        rotatorComponent.Tick();
    }

    protected override void AttackBehaviour()
    {
        if (Target == null)
        {
            return;
        }

        rotatorComponent.SetTarget(Target);
        rotatorComponent.Tick();

        rangedComponent.SetTarget(Target);
        rangedComponent.Tick();
    }

    protected override void DeathBehaviour()
    {
        base.OnDeath();
    }
    #endregion

    #region Public Methods
    public override void Initialize()
    {
        base.Initialize();
    }
    #endregion
}
