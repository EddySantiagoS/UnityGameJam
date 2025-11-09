using UnityEngine;
using TMPro;
using System.Collections;

public class BoardGenerator : MonoBehaviour
{
    [Header("Configuración del tablero")]
    public GameObject tilePrefab;
    public int gridSize = 3;
    public float spacing = 1.1f;
    public Texture[] possibleImages;

    private FloorTile[,] tiles;
    private Texture targetFruit;

    [Header("Referencias")]
    public TargetTileDisplay targetDisplay;

    [Header("Tiempos del juego")]
    public float showTime = 4f;       // Tiempo mostrando frutas
    public float countdownTime = 7f;  // Tiempo para que el jugador elija
    public float resetDelay = 2f;     // Tiempo antes de nueva ronda

    private bool gameActive = false;

    [Header("UI TextMeshPro")]
    public TextMeshProUGUI infoText;   // Mensaje principal
    public TextMeshProUGUI timerText;  // Temporizador

    void Start()
    {
        GenerateBoard();
        if (infoText) infoText.text = " Jump on any tile to start...";
        if (timerText) timerText.text = "";
    }

    public void StartGame()
    {
        if (!gameActive)
        {
            gameActive = true;
            StartCoroutine(GameLoop());
        }
    }

    void GenerateBoard()
{
    tiles = new FloorTile[gridSize, gridSize];
    float offset = (gridSize - 1) * spacing * 0.5f;

    for (int x = 0; x < gridSize; x++)
    {
        for (int z = 0; z < gridSize; z++)
        {
            Vector3 localPos = new Vector3(x * spacing - offset, 0, z * spacing - offset);
            GameObject tileGO = Instantiate(tilePrefab, transform);
            tileGO.transform.localPosition = localPos; // 👈 posición relativa al tablero
            tileGO.name = $"Tile_{x}_{z}";
            tiles[x, z] = tileGO.GetComponent<FloorTile>();
        }
    }
}

    IEnumerator GameLoop()
    {
        while (true)
        {
            // 1️⃣ Mostrar todas las frutas
            AssignImages();
            if (infoText) infoText.text = " Remember the fruits....";
            yield return StartCoroutine(UpdateTimer(showTime));

            // 2️⃣ Ocultar frutas
            HideAllTiles();
            if (infoText) infoText.text = " Preparing the target fruit...";
            if (timerText) timerText.text = "";
            yield return new WaitForSeconds(1f);

            // 3️⃣ Mostrar fruta objetivo
           targetFruit = GetRandomFruitFromBoard();
            targetDisplay.SetTarget(targetFruit);
            if (infoText) infoText.text = " Find this fruit!";
            yield return StartCoroutine(UpdateTimer(countdownTime));

            // 4️⃣ Revisar baldosas
            if (infoText) infoText.text = " Reviewing answers...";
            if (timerText) timerText.text = "";
            CheckTiles();

            yield return new WaitForSeconds(resetDelay);

            // 5️⃣ Resetear tablero
            ResetTiles();
            if (infoText) infoText.text = " New round!...";
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator UpdateTimer(float duration)
    {
        float t = duration;
        while (t > 0)
        {
            if (timerText)
                timerText.text = $"⏱ Tiempo: {t:F1}s";
            t -= Time.deltaTime;
            yield return null;
        }
        if (timerText) timerText.text = " ¡Tiempo!";
    }

    void AssignImages()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                Texture chosen = possibleImages[Random.Range(0, possibleImages.Length)];
                tiles[x, z].SetImage(chosen);
            }
        }
    }

    void HideAllTiles()
    {
        foreach (var tile in tiles)
            tile.HideImage();
    }

    void CheckTiles()
    {
        foreach (var tile in tiles)
        {
            if (tile.currentTexture != targetFruit)
                tile.DropTile();
        }
    }

    void ResetTiles()
    {
        foreach (var tile in tiles)
            tile.ResetTile();
    }

    Texture GetRandomFruitFromBoard()
{
    // Guardamos todas las frutas actualmente en el tablero
    System.Collections.Generic.List<Texture> used = new System.Collections.Generic.List<Texture>();

    for (int x = 0; x < gridSize; x++)
    {
        for (int z = 0; z < gridSize; z++)
        {
            if (tiles[x, z].currentTexture != null)
                used.Add(tiles[x, z].currentTexture);
        }
    }

    // Devolvemos una fruta al azar de las que están presentes
    if (used.Count > 0)
        return used[Random.Range(0, used.Count)];

    // Si por alguna razón no hay ninguna (no debería pasar)
    return possibleImages[Random.Range(0, possibleImages.Length)];
}
}