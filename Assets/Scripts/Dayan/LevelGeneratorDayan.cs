using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGeneratorDayan : MonoBehaviour
{
    public static LevelGeneratorDayan Instance;

    private struct Cell
    {
        public bool visited;
        public bool wallLeft;
        public bool wallBottom;
    }

    [Header("Referencias de Prefabs")]
    public List<GameObject> wallPrefabs;
    public GameObject enemyPrefab;
    public GameObject safeZonePrefab;

    [Header("Configuración del Laberinto")]
    public int mazeWidth = 15;
    public int mazeHeight = 15;
    public float cellSize = 5f; // Aumentar esto sigue ayudando a tener pasillos más anchos

    [Header("Configuración de Entidades")]
    public int enemyCount = 5;
    [Tooltip("Distancia MÍNIMA (en celdas) entre el jugador y la meta")]
    public float minPlayerSafeZoneDistance = 5f;

    // --- ¡AÑADE ESTA LÍNEA! ---
    [Tooltip("Distancia mínima que debe haber entre un enemigo y el jugador AL APARECER (en metros)")]
    public float minEnemyPlayerSpawnDistance = 4f;

    [Header("Configuración de Físicas (¡NUEVO!)")]
    [Tooltip("La capa que asignaste a tus prefabs de Muro")]
    public LayerMask wallLayerMask;
    [Tooltip("El radio (tamaño) de tu prefab de jugador")]
    public float playerRadius = 0.5f;
    [Tooltip("El radio (tamaño) de tu prefab de enemigo")]
    public float enemyRadius = 0.5f;
    [Tooltip("El radio (tamaño) de tu prefab de Zona Segura")]
    public float safeZoneRadius = 2f;

    [Header("Referencias de Escena")]
    public Transform player;

    private Cell[,] grid;
    private Stack<Vector2Int> pathStack = new Stack<Vector2Int>();
    private Transform wallContainer;
    private Transform enemyContainer;
    private Vector3 mazeOriginOffset;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public IEnumerator GenerateNewLevel()
    {
        WipeLevel();
        BuildEnvironment();

        yield return new WaitForFixedUpdate();
        SpawnEntities();
    }

    void WipeLevel()
    {
        if (wallContainer != null)
        {
            Destroy(wallContainer.gameObject);
        }

        // --- ¡AÑADE ESTO! ---
        if (enemyContainer != null)
        {
            Destroy(enemyContainer.gameObject);
        }

        ClearObjectsByTag("Projectile");
        ClearObjectsByTag("SafeZone");
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
        wallContainer = new GameObject("WallContainer").transform;
        enemyContainer = new GameObject("EnemyContainer").transform;
        grid = new Cell[mazeWidth, mazeHeight];
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int z = 0; z < mazeHeight; z++)
            {
                grid[x, z] = new Cell { visited = false, wallLeft = true, wallBottom = true };
            }
        }

        pathStack.Clear();
        Vector2Int startPos = new Vector2Int(Random.Range(0, mazeWidth), Random.Range(0, mazeHeight));
        grid[startPos.x, startPos.y].visited = true;
        pathStack.Push(startPos);

        while (pathStack.Count > 0)
        {
            Vector2Int currentPos = pathStack.Peek();
            List<Vector2Int> neighbors = GetUnvisitedNeighbors(currentPos);

            if (neighbors.Count > 0)
            {
                Vector2Int chosenNeighbor = neighbors[Random.Range(0, neighbors.Count)];
                KnockDownWall(currentPos, chosenNeighbor);
                grid[chosenNeighbor.x, chosenNeighbor.y].visited = true;
                pathStack.Push(chosenNeighbor);
            }
            else
            {
                pathStack.Pop();
            }
        }

        InstantiateWalls();
    }

    List<Vector2Int> GetUnvisitedNeighbors(Vector2Int pos)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        if (pos.y + 1 < mazeHeight && !grid[pos.x, pos.y + 1].visited) neighbors.Add(new Vector2Int(pos.x, pos.y + 1));
        if (pos.y - 1 >= 0 && !grid[pos.x, pos.y - 1].visited) neighbors.Add(new Vector2Int(pos.x, pos.y - 1));
        if (pos.x + 1 < mazeWidth && !grid[pos.x + 1, pos.y].visited) neighbors.Add(new Vector2Int(pos.x + 1, pos.y));
        if (pos.x - 1 >= 0 && !grid[pos.x - 1, pos.y].visited) neighbors.Add(new Vector2Int(pos.x - 1, pos.y));
        return neighbors;
    }

    void KnockDownWall(Vector2Int current, Vector2Int neighbor)
    {
        if (neighbor.x > current.x) grid[neighbor.x, neighbor.y].wallLeft = false;
        else if (neighbor.x < current.x) grid[current.x, current.y].wallLeft = false;
        else if (neighbor.y > current.y) grid[neighbor.x, neighbor.y].wallBottom = false;
        else if (neighbor.y < current.y) grid[current.x, current.y].wallBottom = false;
    }

    void InstantiateWalls()
    {
        // Pone las paredes en Y=0
        mazeOriginOffset = new Vector3(-(mazeWidth / 2f) * cellSize, 0, -(mazeHeight / 2f) * cellSize);

        for (int x = 0; x < mazeWidth; x++)
        {
            for (int z = 0; z < mazeHeight; z++)
            {
                Vector3 cellWorldPos = mazeOriginOffset + new Vector3(x * cellSize, 0, z * cellSize);
                Cell current = grid[x, z];

                if (current.wallLeft)
                {
                    Vector3 pos = cellWorldPos + new Vector3(-cellSize / 2, 0, 0);
                    InstantiateWall(pos, Quaternion.Euler(0, 90, 0));
                }
                if (current.wallBottom)
                {
                    Vector3 pos = cellWorldPos + new Vector3(0, 0, -cellSize / 2);
                    InstantiateWall(pos, Quaternion.identity);
                }
            }
        }

        // Perímetro Exterior
        for (int x = 0; x < mazeWidth; x++)
        {
            Vector3 pos = mazeOriginOffset + new Vector3(x * cellSize, 0, (mazeHeight - 1) * cellSize) + new Vector3(0, 0, cellSize / 2);
            InstantiateWall(pos, Quaternion.identity);
        }
        for (int z = 0; z < mazeHeight; z++)
        {
            Vector3 pos = mazeOriginOffset + new Vector3((mazeWidth - 1) * cellSize, 0, z * cellSize) + new Vector3(cellSize / 2, 0, 0);
            InstantiateWall(pos, Quaternion.Euler(0, 90, 0));
        }
    }

    void InstantiateWall(Vector3 position, Quaternion rotation)
    {
        if (wallPrefabs == null || wallPrefabs.Count == 0) return;
        GameObject wallPrefab = wallPrefabs[Random.Range(0, wallPrefabs.Count)];
        GameObject newWall = Instantiate(wallPrefab, position, rotation);

        // ¡CRÍTICO! Asignar el tag para limpieza Y asignar la capa para físicas
        newWall.tag = "Wall";
        newWall.layer = LayerMask.NameToLayer("Wall"); // Asigna la capa por código

        newWall.transform.SetParent(wallContainer);
    }


    // --- 3. SPAWN DE ENTIDADES (LÓGICA ACTUALIZADA) ---

    // --- 3. SPAWN DE ENTIDADES (LÓGICA ACTUALIZADA) ---
    void SpawnEntities()
    {
        // NUEVO: Lista para rastrear dónde ya hemos puesto cosas
        List<Vector3> occupiedPositions = new List<Vector3>();

        // A. Colocar la Zona Segura
        Vector3 safeZonePos = GetRandomSafePosition(safeZoneRadius);
        if (safeZonePos == Vector3.positiveInfinity)
        {
            Debug.LogError("¡No se encontró espacio para la SafeZone! Prueba un laberinto más grande o un 'safeZoneRadius' más pequeño.");
            return;
        }
        Instantiate(safeZonePrefab, safeZonePos, Quaternion.identity);
        occupiedPositions.Add(safeZonePos); // <<< AÑADIDO

        // B. Mover al jugador
        Vector3 playerPos = GetRandomPlayerSpawnPosition(safeZonePos);
        if (playerPos == Vector3.positiveInfinity)
        {
            Debug.LogError("¡No se encontró espacio para el Jugador!");
            playerPos = mazeOriginOffset + Vector3.up; // Fallback al centro
        }
        player.transform.position = playerPos;
        player.GetComponent<Rigidbody>().linearVelocity = Vector3.zero; // Respeto tu uso de 'linearVelocity'
        occupiedPositions.Add(playerPos); // <<< AÑADIDO

        // C. Colocar Enemigos
        int enemiesToSpawn = enemyCount;
        int spawnAttempts = 0; // Para evitar bucles infinitos si no hay espacio

        // Usamos 'while' para RE-INTENTAR si un spawn falla
        while (enemiesToSpawn > 0 && spawnAttempts < 100)
        {
            spawnAttempts++; // Prevenir bucle infinito

            Vector3 enemyPos = GetRandomSafePosition(enemyRadius);

            if (enemyPos == Vector3.positiveInfinity)
            {
                continue; // Esta celda no era válida (chocaba con pared)
            }

            // --- INICIO DE NUEVAS COMPROBACIONES ---
            bool validSpawn = true;

            // 1. (Tu Requisito) Comprobar distancia mínima con el JUGADOR
            if (Vector3.Distance(enemyPos, playerPos) < minEnemyPlayerSpawnDistance)
            {
                validSpawn = false;
            }

            // 2. (Tu Requisito) Comprobar superposición con OTROS OBJETOS
            if (validSpawn)
            {
                foreach (Vector3 pos in occupiedPositions)
                {
                    // Comprobamos si el radio del nuevo enemigo se solapa con el radio de otro objeto
                    // (Usamos enemyRadius * 2 como una simple comprobación de "espacio personal")
                    if (Vector3.Distance(enemyPos, pos) < (enemyRadius * 2))
                    {
                        validSpawn = false;
                        break;
                    }
                }
            }
            // --- FIN DE NUEVAS COMPROBACIONES ---

            // Si pasó ambas pruebas, lo instanciamos
            if (validSpawn)
            {
                GameObject newEnemy = Instantiate(enemyPrefab, enemyPos, Quaternion.identity);
                //newEnemy.GetComponent<EnemyShooterDayan>().playerTarget = player;

                newEnemy.transform.SetParent(enemyContainer);

                occupiedPositions.Add(enemyPos); // <<< AÑADIDO (Lo marcamos como ocupado)
                enemiesToSpawn--; // Un enemigo menos que spawnear
            }
            // Si validSpawn es falso, el bucle 'while' simplemente se repetirá y probará una nueva posición
        }

        if (enemiesToSpawn > 0)
        {
            Debug.LogWarning($"No se pudo encontrar espacio para {enemiesToSpawn} enemigos (demasiados enemigos o poco espacio).");
        }
    }

    // --- FUNCIÓN DE AYUDA (ACTUALIZADA) ---
    // Encuentra una celda aleatoria donde un objeto con 'spawnRadius' quepa

    Vector3 GetRandomSafePosition(float spawnRadius)
    {
        int attempts = 0;
        while (attempts < 50) // Intentar 50 celdas
        {
            int x = Random.Range(0, mazeWidth);
            int z = Random.Range(0, mazeHeight);

            // Posición central de la celda (en Y=1, sobre el suelo)
            // --- ¡ESTA ES LA LÍNEA CORREGIDA! ---
            Vector3 cellCenterPos = mazeOriginOffset + new Vector3(x * cellSize + (cellSize / 2f), 0, z * cellSize + (cellSize / 2f)) + Vector3.up;

            // --- ¡LA COMPROBACIÓN FÍSICA! ---
            // ¿Esta esfera de 'spawnRadius' choca con algo en la 'wallLayerMask'?
            if (!Physics.CheckSphere(cellCenterPos, spawnRadius, wallLayerMask))
            {
                // ¡No choca! Es un lugar seguro.
                return cellCenterPos;
            }

            attempts++;
        }

        // No se encontró nada después de 50 intentos
        return Vector3.positiveInfinity;
    }

    // --- FUNCIÓN DE AYUDA (ACTUALIZADA) ---
    // --- FUNCIÓN DE AYUDA (ACTUALIZADA Y MÁS ROBUSTA) ---
    Vector3 GetRandomPlayerSpawnPosition(Vector3 safeZonePos)
    {
        List<Vector3> validPositions = new List<Vector3>();
        List<Vector3> distantPositions = new List<Vector3>();

        // 1. Recorrer TODAS las celdas para encontrar TODAS las posiciones seguras
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int z = 0; z < mazeHeight; z++)
            {
                // ¡IMPORTANTE! Usar el centro de la celda (la corrección de nuestra conversación anterior)
                Vector3 cellCenterPos = mazeOriginOffset + new Vector3(x * cellSize + (cellSize / 2f), 0, z * cellSize + (cellSize / 2f)) + Vector3.up;

                // Comprobar si el JUGADOR cabe en esta celda
                if (!Physics.CheckSphere(cellCenterPos, playerRadius, wallLayerMask))
                {
                    validPositions.Add(cellCenterPos);
                }
            }
        }

        // 2. Si no se encontró NINGÚN lugar seguro (laberinto imposible / radio del jugador muy grande)
        if (validPositions.Count == 0)
        {
            Debug.LogError("¡No se encontró NINGÚN espacio para el Jugador en todo el laberinto! Revisa los radios o el tamaño del laberinto.");
            return mazeOriginOffset + Vector3.up; // Fallback al centro
        }

        // 3. Filtrar la lista para encontrar posiciones LEJANAS
        foreach (Vector3 pos in validPositions)
        {
            float cellDistance = Vector3.Distance(pos, safeZonePos) / cellSize;
            if (cellDistance >= minPlayerSafeZoneDistance)
            {
                distantPositions.Add(pos);
            }
        }

        // 4. Decidir qué posición devolver
        if (distantPositions.Count > 0)
        {
            // ¡Éxito! Devolver una posición lejana aleatoria
            return distantPositions[Random.Range(0, distantPositions.Count)];
        }
        else
        {
            // No se encontró nada lejano, pero sí lugares seguros.
            // Esto es lo que causaba tu advertencia.
            Debug.LogWarning("No se encontró posición lejana para el jugador (posiblemente laberinto pequeño). Colocando en una posición segura aleatoria...");
            return validPositions[Random.Range(0, validPositions.Count)];
        }
    }
}