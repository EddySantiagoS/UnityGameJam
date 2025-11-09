using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;

    [Header("Referencias del Puzzle")]
    public List<RotateOnPlayerJump> pieces = new List<RotateOnPlayerJump>();
    public GameObject victoryObject;
    public CharacterController playerController;
    public GameObject player;

    [Header("Configuración de caída")]
    public float fallSpeed = 20f;
    public float fallDuration = 3f;

    private bool puzzleCompleted = false;
    private bool isPlayerFalling = false;
    private float fallTimer = 0f;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (isPlayerFalling && playerController != null)
        {
            fallTimer += Time.deltaTime;
            Vector3 fallMovement = Vector3.down * fallSpeed * Time.deltaTime;
            playerController.Move(fallMovement);

            if (fallTimer >= fallDuration)
            {
                isPlayerFalling = false;
                if (victoryObject != null)
                    victoryObject.SetActive(true);
            }
        }
    }

    public void CheckPuzzleCompletion()
    {
        if (puzzleCompleted) return;

        int alignedCount = 0;

        foreach (var piece in pieces)
        {
            if (piece.isCornerPiece)
                continue;

            if (piece.IsAligned())
                alignedCount++;
            else
                return; // Si una no está alineada, aún no se completa
        }

        // Si llegamos aquí, todas están bien
        puzzleCompleted = true;
        Debug.Log("¡Puzzle completado! (" + alignedCount + " piezas correctas)");

        // Activar gravedad en piezas
        foreach (var piece in pieces)
            piece.ActivateGravity();

        // Activar caída del jugador
        if (playerController != null)
        {
            isPlayerFalling = true;
            fallTimer = 0f;
        }
    }
}