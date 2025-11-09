using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefab del enemigo/misil")]
    public GameObject enemyPrefab;

    [Header("Spawn Settings")]
    public float spawnInterval = 0.40f;
    public int maxEnemies = 50;

    [Header("Distancia mínima alrededor del player")]
    public float minDistanceFromPlayer = 20f;

    [Header("Sesgo hacia el centro del cubo (0 = aleatorio, 1 = casi todo al centro)")]
    [Range(0f, 1f)]
    public float centerBias = 0.65f;

    private float timer = 0f;
    private int currentEnemies = 0;

    private BoxCollider box;
    private Transform player;

    void Start()
    {
        box = GetComponent<BoxCollider>();
        if (!box.isTrigger) box.isTrigger = true;

        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (player == null) return;

        timer -= Time.deltaTime;

        if (timer <= 0f && currentEnemies < maxEnemies)
        {
            SpawnEnemy();
            timer = spawnInterval;
        }
    }

    void SpawnEnemy()
    {
        Vector3 worldSize = Vector3.Scale(box.size, transform.lossyScale);

        Vector3 randomPos;
        Vector3 center = transform.position;

        // Intentamos varias veces asegurar no spawnear encima del player
        int attempts = 0;

        do
        {
            // Posición totalmente aleatoria en el cubo
            Vector3 rand =
                new Vector3(
                    UnityEngine.Random.Range(-worldSize.x / 2f, worldSize.x / 2f),
                    UnityEngine.Random.Range(-worldSize.y / 2f, worldSize.y / 2f),
                    UnityEngine.Random.Range(-worldSize.z / 2f, worldSize.z / 2f)
                );

            // Aplicar sesgo hacia el centro
            randomPos = Vector3.Lerp(center + rand, center, centerBias);

            attempts++;

            if (attempts > 20) break;

        } while (Vector3.Distance(randomPos, player.position) < minDistanceFromPlayer);

        GameObject enemy = Instantiate(enemyPrefab, randomPos, Quaternion.identity);

        currentEnemies++;

        enemy.GetComponent<FlyingEnemy>().onDestroyed += () =>
        {
            currentEnemies--;
        };
    }
}
