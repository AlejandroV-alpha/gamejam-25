using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawnea enemigos fijos en multiples puntos dados.
/// Cada punto tiene su propio temporizador y respawnea al morir su enemigo.
/// </summary>
public class FixedEnemySpawner : MonoBehaviour
{
    #region Spawn Settings
    [Header("Spawn Settings")]
    [Tooltip("Enemy prefab to spawn.")]
    [SerializeField] GameObject enemyPrefab;

    [Tooltip("Points where enemies will be spawned.")]
    [SerializeField] Transform[] spawnPoints;

    [Tooltip("Time to wait before respawning after the enemy dies.")]
    [SerializeField] float timeBetweenSpawn = 3f;
    #endregion

    #region Private Fields
    List<SpawnSlot> spawnSlots = new List<SpawnSlot>();
    #endregion

    #region Unity Methods
    void Start()
    {
        // Inicializa todos los spawn points
        foreach (var point in spawnPoints)
        {
            spawnSlots.Add(new SpawnSlot(point));
        }

        // Spawnea todos los enemigos al inicio
        foreach (var slot in spawnSlots)
        {
            SpawnEnemy(slot);
        }
    }

    void Update()
    {
        foreach (var slot in spawnSlots)
        {
            // Si murio el enemigo y no estamos esperando
            if (slot.currentEnemy == null && !slot.waitingRespawn)
            {
                slot.waitingRespawn = true;
                slot.timer = timeBetweenSpawn;
            }

            // Si estamos esperando, descontamos tiempo
            if (slot.waitingRespawn)
            {
                slot.timer -= Time.deltaTime;
                if (slot.timer <= 0f)
                {
                    SpawnEnemy(slot);
                    slot.waitingRespawn = false;
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;

        Gizmos.color = Color.red;
        foreach (var point in spawnPoints)
        {
            if (point != null)
            {
                Gizmos.DrawWireSphere(point.position, 0.3f);
            }
        }
    }
    #endregion

    #region Spawn Logic
    /// <summary>
    /// Instancia el enemigo en el spawn point del slot.
    /// </summary>
    void SpawnEnemy(SpawnSlot slot)
    {
        if (enemyPrefab == null || slot.spawnPoint == null)
        {
            Debug.LogWarning("Falta asignar el prefab del enemigo o el punto de spawn.");
            return;
        }

        slot.currentEnemy = Instantiate(enemyPrefab, slot.spawnPoint.position, slot.spawnPoint.rotation);
    }
    #endregion
}
