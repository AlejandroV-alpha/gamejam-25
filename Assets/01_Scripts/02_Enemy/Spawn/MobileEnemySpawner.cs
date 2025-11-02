using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawnea enemigos dentro de una zona definida.
/// Evita generar enemigos en zonas donde haya obstaculos o jugadores (usando LayerMasks).
/// Mantiene un numero maximo de enemigos activos.
/// El temporizador solo se reinicia cuando hay espacio para spawnear.
/// </summary>
public class MobileEnemySpawner : MonoBehaviour
{
    #region Spawn Area
    [Header("Spawn Area")]
    [Tooltip("Rectangular area where enemies will be spawned.")]
    [SerializeField] Collider2D spawnArea;
    #endregion

    #region Enemy Settings
    [Header("Enemy Settings")]
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] int maxActiveEnemies = 10;
    [SerializeField] float timeBetweenSpawn = 3f;
    #endregion

    #region Obstacle Detection
    [Header("Obstacle and Enemy Detection")]
    [Tooltip("Layers representing obstacles or enemies where enemies should not spawn.")]
    [SerializeField] LayerMask obstacleLayer;
    [SerializeField] float checkRadius = 0.5f; // Radius to avoid obstacles/enemies
    #endregion

    #region Player Avoidance
    [Header("Player Avoidance")]
    [Tooltip("Layer representing the player.")]
    [SerializeField] LayerMask playerLayer;
    [Tooltip("Safety radius to avoid spawning near the player.")]
    [SerializeField] float playerAvoidRadius = 3f;
    #endregion

    #region Private Fields
    float spawnTimer;
    List<GameObject> activeEnemies = new List<GameObject>();
    bool canSpawnTimerRun = true;
    #endregion

    #region Unity Methods
    void Start()
    {
        spawnTimer = timeBetweenSpawn;
    }

    void Update()
    {
        int previousCount = activeEnemies.Count;
        activeEnemies.RemoveAll(e => e == null);

        // Detectar si murio un enemigo
        if (activeEnemies.Count < previousCount && activeEnemies.Count < maxActiveEnemies)
        {
            if (!canSpawnTimerRun)
            {
                canSpawnTimerRun = true;
                spawnTimer = timeBetweenSpawn;
            }
        }

        // Control de temporizador
        if (canSpawnTimerRun)
        {
            spawnTimer -= Time.deltaTime;

            if (spawnTimer <= 0f)
            {
                TrySpawnEnemy();
                spawnTimer = timeBetweenSpawn;
            }
        }

        // Si llegamos al maximo, detenemos el temporizador
        if (activeEnemies.Count >= maxActiveEnemies)
        {
            canSpawnTimerRun = false;
            spawnTimer = 0f;
        }
    }
    #endregion

    #region Enemy Spawn Logic
    /// <summary>
    /// Intenta spawnear un enemigo si hay espacio disponible
    /// y se cumplen las condiciones de obstaculos y jugador.
    /// </summary>
    void TrySpawnEnemy()
    {
        if (activeEnemies.Count >= maxActiveEnemies)
        {
            return;
        }

        if (enemyPrefab == null || spawnArea == null)
        {
            Debug.LogWarning("Falta asignar el prefab del enemigo o la zona de spawn.");
            return;
        }

        Vector2 spawnPoint;
        bool foundValidPoint = false;

        // Hasta 10 intentos de encontrar un punto valido
        for (int i = 0; i < 10; i++)
        {
            spawnPoint = GetRandomPointInArea();

            bool nearObstacle = Physics2D.OverlapCircle(spawnPoint, checkRadius, obstacleLayer);
            bool nearPlayer = Physics2D.OverlapCircle(spawnPoint, playerAvoidRadius, playerLayer);

            if (!nearObstacle && !nearPlayer)
            {
                foundValidPoint = true;
                SpawnEnemy(spawnPoint);
                break;
            }
        }

        if (!foundValidPoint)
        {
            Debug.Log("No se encontro un punto libre para spawnear enemigo (zona saturada o muy cerca del jugador).");
        }
    }

    /// <summary>
    /// Instancia el enemigo en la posicion indicada.
    /// </summary>
    void SpawnEnemy(Vector2 position)
    {
        GameObject newEnemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        activeEnemies.Add(newEnemy);
    }

    /// <summary>
    /// Genera un punto aleatorio dentro de la zona de spawn.
    /// </summary>
    Vector2 GetRandomPointInArea()
    {
        Bounds bounds = spawnArea.bounds;
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);
        return new Vector2(x, y);
    }
    #endregion
}
