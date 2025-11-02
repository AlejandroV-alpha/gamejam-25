using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawnea enemigos fijos en múltiples puntos dados.
/// Cada punto tiene su propio temporizador y respawnea al morir su enemigo.
/// </summary>
public class FixedEnemySpawner : MonoBehaviour
{
    [Header("Configuración del Spawn")]
    [Tooltip("Prefab del enemigo a spawnear.")]
    [SerializeField] private GameObject enemyPrefab;

    [Tooltip("Puntos donde se generarán los enemigos.")]
    [SerializeField] private Transform[] spawnPoints;

    [Tooltip("Tiempo de espera entre respawns después de que el enemigo muere.")]
    [SerializeField] private float timeBetweenSpawn = 3f;

    // Clase interna para manejar cada punto de spawn
    private class SpawnSlot
    {
        public Transform spawnPoint;
        public GameObject currentEnemy;
        public float timer;
        public bool waitingRespawn;

        public SpawnSlot(Transform point)
        {
            spawnPoint = point;
            currentEnemy = null;
            timer = 0f;
            waitingRespawn = false;
        }
    }

    private List<SpawnSlot> spawnSlots = new List<SpawnSlot>();

    private void Start()
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

    private void Update()
    {
        foreach (var slot in spawnSlots)
        {
            // Si murió el enemigo y no estamos esperando
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

    private void SpawnEnemy(SpawnSlot slot)
    {
        if (enemyPrefab == null || slot.spawnPoint == null)
        {
            Debug.LogWarning("Falta asignar el prefab del enemigo o el punto de spawn.");
            return;
        }

        slot.currentEnemy = Instantiate(enemyPrefab, slot.spawnPoint.position, slot.spawnPoint.rotation);
    }

    private void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;

        Gizmos.color = Color.red;
        foreach (var point in spawnPoints)
        {
            if (point != null)
                Gizmos.DrawWireSphere(point.position, 0.3f);
        }
    }
}
