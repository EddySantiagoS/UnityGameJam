using UnityEngine;
using System.Collections;

public class RotateOnPlayerJump : MonoBehaviour
{
    [Header("Tipo de pieza")]
    public bool isCornerPiece = false;

    [Header("Rotación")]
    [SerializeField] private float rotationDuration = 0.3f;
    private bool isRotating = false;

    [Header("Puzzle")]
    [Tooltip("Tolerancia de ángulo para considerar que la pieza está correctamente orientada")]
    public float alignmentTolerance = 2f;

    private Quaternion correctRotation; // rotación inicial (la correcta)
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        correctRotation = transform.rotation;

        // Rotación inicial aleatoria pero solo en pasos de 90°
        Vector3 randomStart = new Vector3(
            90f * Random.Range(0, 4),
            90f * Random.Range(0, 4),
            90f * Random.Range(0, 4)
        );
        transform.rotation = Quaternion.Euler(randomStart);

        // Desactiva la física hasta resolver el puzzle
        if (rb != null)
            rb.isKinematic = true;
    }

    public void RotateRandom()
    {
        if (!isRotating)
            StartCoroutine(SmoothRotate(RandomRotationStep()));
    }

    private Vector3 RandomRotationStep()
    {
        int axis = Random.Range(0, 3);
        Vector3 rotation = Vector3.zero;

        switch (axis)
        {
            case 0: rotation = new Vector3(90f, 0f, 0f); break;
            case 1: rotation = new Vector3(0f, 90f, 0f); break;
            case 2: rotation = new Vector3(0f, 0f, 90f); break;
        }

        return rotation;
    }

    private IEnumerator SmoothRotate(Vector3 rotationStep)
    {
        isRotating = true;

        Quaternion startRot = transform.rotation;
        Quaternion endRot = transform.rotation * Quaternion.Euler(rotationStep);

        float elapsed = 0f;
        while (elapsed < rotationDuration)
        {
            transform.rotation = Quaternion.Lerp(startRot, endRot, elapsed / rotationDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRot;
        isRotating = false;

        PuzzleManager.Instance.CheckPuzzleCompletion();
    }

    public bool IsAligned()
    {
        // Si la pieza es de esquina, siempre está "bien"
        if (isCornerPiece)
            return true;

        // Rotaciones equivalentes válidas (cara superior o inferior)
        Quaternion correct = correctRotation;
        Quaternion invertedY = correctRotation * Quaternion.Euler(0f, 180f, 0f);
        Quaternion invertedYNeg = correctRotation * Quaternion.Euler(0f, -180f, 0f);

        // Comparar con margen de tolerancia
        float a = Quaternion.Angle(transform.rotation, correct);
        float b = Quaternion.Angle(transform.rotation, invertedY);
        float c = Quaternion.Angle(transform.rotation, invertedYNeg);

        // Si está cerca de cualquiera de las dos caras, está bien
        bool aligned = a < alignmentTolerance || b < alignmentTolerance || c < alignmentTolerance;

        return aligned;
    }

    public void ActivateGravity()
    {
        if (rb != null)
            rb.isKinematic = false;
    }
}