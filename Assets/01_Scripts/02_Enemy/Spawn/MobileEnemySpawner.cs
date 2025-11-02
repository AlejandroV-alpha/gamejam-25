using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawnea enemigos dentro de una zona definida.
/// Evita generar enemigos en zonas donde haya obstáculos o jugadores (usando LayerMasks).
/// Mantiene un número máximo de enemigos activos.
/// El temporizador solo se reinicia cuando hay espacio para spawnear.
/// </summary>
public class MobileEnemySpawner : MonoBehaviour
{
    [Header("Zona de Spawn")]
    [Tooltip("Área rectangular donde se generarán los enemigos.")]
    [SerializeField] private Collider2D spawnArea;

    [Header("Configuración de Enemigos")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int maxActiveEnemies = 10;
    [SerializeField] private float timeBetweenSpawn = 3f;

    [Header("Detección de Obstáculos y Enemigos")]
    [Tooltip("Capas que representan obstáculos o enemigos donde no se debe spawnear.")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float checkRadius = 0.5f; // Radio para evitar obstáculos/enemigos

    [Header("Evitar al Jugador")]
    [Tooltip("Capa que representa al jugador.")]
    [SerializeField] private LayerMask playerLayer;
    [Tooltip("Radio de seguridad para evitar spawnear cerca del jugador.")]
    [SerializeField] private float playerAvoidRadius = 3f;

    private float spawnTimer;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool canSpawnTimerRun = true;

    private void Start()
    {
        spawnTimer = timeBetweenSpawn;
    }

    private void Update()
    {
        int previousCount = activeEnemies.Count;
        activeEnemies.RemoveAll(e => e == null);

        // Detectar si murió un enemigo
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

        // Si llegamos al máximo, detenemos el temporizador
        if (activeEnemies.Count >= maxActiveEnemies)
        {
            canSpawnTimerRun = false;
            spawnTimer = 0f;
        }
    }

    private void TrySpawnEnemy()
    {
        if (activeEnemies.Count >= maxActiveEnemies)
            return;

        if (enemyPrefab == null || spawnArea == null)
        {
            Debug.LogWarning("Falta asignar el prefab del enemigo o la zona de spawn.");
            return;
        }

        Vector2 spawnPoint;
        bool foundValidPoint = false;

        // Hasta 10 intentos de encontrar un punto válido
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
            Debug.Log("No se encontró un punto libre para spawnear enemigo (zona saturada o muy cerca del jugador).");
    }

    private void SpawnEnemy(Vector2 position)
    {
        GameObject newEnemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        activeEnemies.Add(newEnemy);
    }

    private Vector2 GetRandomPointInArea()
    {
        Bounds bounds = spawnArea.bounds;
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);
        return new Vector2(x, y);
    }

}
