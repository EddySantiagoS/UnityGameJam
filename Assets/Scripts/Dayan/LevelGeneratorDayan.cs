using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGeneratorDayan : MonoBehaviour
{
    public static LevelGeneratorDayan Instance;

    [Header("Referencias de Prefabs")]
    public GameObject wallPrefab;
    public GameObject enemyPrefab;
    public GameObject safeZonePrefab;

    [Header("Configuración del Nivel")]
    [Tooltip("El objeto 'Ground' (Suelo) que define los límites del mapa")]
    public Renderer groundRenderer; // Arrastra tu objeto 'Ground' aquí
    public float spawnPadding = 2f; // Margen desde el borde del suelo
    private Vector2 minBounds;
    private Vector2 maxBounds;
    public int enemyCount = 3;
    public int obstacleCount = 5; // Obstáculos internos

    [Header("Referencias de Escena")]
    public Transform player; // Arrastra al jugador aquí

    public float minPlayerSafeZoneDistance = 15f; // Distancia mínima entre jugador y meta

    // Capas de físicas para comprobar colisiones
    public LayerMask obstacleLayerMask;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // --- Calcular los límites basado en el tamaño del suelo ---
        if (groundRenderer != null)
        {
            Bounds groundBounds = groundRenderer.bounds;
            minBounds = new Vector2(groundBounds.min.x + spawnPadding, groundBounds.min.z + spawnPadding);
            maxBounds = new Vector2(groundBounds.max.x - spawnPadding, groundBounds.max.z - spawnPadding);

            Debug.Log($"Límites del mapa calculados: {minBounds} a {maxBounds}");
        }
        else
        {
            Debug.LogError("¡No se asignó un 'Ground Renderer' al LevelGenerator!");
        }
    }

    // --- Punto de Entrada Principal ---
    public void GenerateNewLevel()
    {
        // 1. Limpiar el nivel anterior
        WipeLevel();

        // 2. Construir el nuevo entorno
        BuildEnvironment();

        // 3. Colocar entidades (Jugador, Meta, Enemigos)
        SpawnEntities();
    }

    // --- 1. LIMPIEZA ---
    void WipeLevel()
    {
        // Buscar todos los objetos con estos tags y destruirlos
        ClearObjectsByTag("Enemy");
        ClearObjectsByTag("Projectile");
        ClearObjectsByTag("Wall");

        // También destruimos la zona segura (si la etiquetamos)
        GameObject safeZone = GameObject.FindGameObjectWithTag("SafeZone"); // Asegúrate de que el Prefab de SafeZone tenga este tag
        if (safeZone != null) Destroy(safeZone);
    }

    void ClearObjectsByTag(string tag)
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject obj in objects)
        {
            Destroy(obj);
        }
    }

    // --- 2. CONSTRUCCIÓN ---
    void BuildEnvironment()
    {
        // A. Construir el perímetro exterior
        // (Nota: Este bucle 'for' crea muchas paredes. Es funcional pero no muy optimizado. 
        // Más adelante podríamos reemplazarlo instanciando 4 cubos escalados).
        for (float x = minBounds.x; x <= maxBounds.x; x++)
        {
            Instantiate(wallPrefab, new Vector3(x, 1, minBounds.y), Quaternion.identity); // Pared inferior
            Instantiate(wallPrefab, new Vector3(x, 1, maxBounds.y), Quaternion.identity); // Pared superior
        }
        for (float z = minBounds.y + 1; z < maxBounds.y; z++)
        {
            Instantiate(wallPrefab, new Vector3(minBounds.x, 1, z), Quaternion.identity); // Pared izquierda
            Instantiate(wallPrefab, new Vector3(maxBounds.x, 1, z), Quaternion.identity); // Pared derecha
        }

        // B. Construir obstáculos internos
        for (int i = 0; i < obstacleCount; i++)
        {
            Vector3 randomPos = GetRandomSafePosition(1f); // Pedir pos segura
            if (randomPos != Vector3.positiveInfinity)
            {
                Instantiate(wallPrefab, randomPos, Quaternion.identity);
            }
        }
    }

    // --- 3. SPAWN DE ENTIDADES ---
    void SpawnEntities()
    {
        // A. Colocar la Zona Segura PRIMERO
        Vector3 safeZonePos = GetRandomSafePosition(2f);
        if (safeZonePos == Vector3.positiveInfinity)
        {
            Debug.LogError("No se pudo encontrar posición para la SafeZone. Abortando spawn.");
            return;
        }
        Instantiate(safeZonePrefab, safeZonePos, Quaternion.identity);

        // B. Mover al jugador lejos de la zona segura
        Vector3 playerPos = GetRandomPlayerSpawnPosition(safeZonePos);
        player.transform.position = playerPos;
        player.GetComponent<Rigidbody>().linearVelocity = Vector3.zero; // Detenerlo

        // C. Colocar Enemigos
        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 enemyPos = GetRandomSafePosition(1.5f);
            if (enemyPos != Vector3.positiveInfinity)
            {
                // Instanciamos y asignamos el target al jugador
                GameObject newEnemy = Instantiate(enemyPrefab, enemyPos, Quaternion.identity);
                newEnemy.GetComponent<EnemyShooterDayan>().playerTarget = player;
            }
        }
    }

    // --- FUNCIÓN DE AYUDA: Encontrar lugar para el jugador ---
    Vector3 GetRandomPlayerSpawnPosition(Vector3 safeZonePos)
    {
        int attempts = 0;
        while (attempts < 50)
        {
            // 1. Encontrar un punto vacío aleatorio (reusamos la función existente)
            Vector3 randomPos = GetRandomSafePosition(1f); // 1f es el radio del jugador

            if (randomPos != Vector3.positiveInfinity)
            {
                // 2. Comprobar la distancia a la zona segura
                float distance = Vector3.Distance(randomPos, safeZonePos);

                if (distance >= minPlayerSafeZoneDistance)
                {
                    // ¡Encontrado! Es un punto vacío Y está lejos
                    return randomPos;
                }
            }

            attempts++;
        }

        Debug.LogWarning("No se encontró posición lejana para el jugador. Colocando en el centro.");

        // --- ¡LÍNEA CORREGIDA! ---
        return Vector3.up; // Fallback al centro (0, 1, 0)
    }


    // --- FUNCIÓN DE AYUDA: Encontrar un lugar vacío ---
    Vector3 GetRandomSafePosition(float spawnRadius)
    {
        int attempts = 0;
        while (attempts < 50) // Intentar 50 veces
        {
            // Elige un punto aleatorio dentro de los límites
            float x = Random.Range(minBounds.x, maxBounds.x);
            float z = Random.Range(minBounds.y, maxBounds.y);
            Vector3 randomPos = new Vector3(x, 1, z); // Asumimos Y=1

            // Comprobar si está vacío usando una esfera
            // (Asegúrate de que obstacleLayerMask incluya "Obstacles" y "Player")
            if (!Physics.CheckSphere(randomPos, spawnRadius, obstacleLayerMask))
            {
                // ¡Espacio libre! Devolver esta posición.
                return randomPos;
            }

            attempts++;
        }

        Debug.LogWarning("No se encontró posición segura para instanciar.");
        return Vector3.positiveInfinity; // Fallback
    }
}