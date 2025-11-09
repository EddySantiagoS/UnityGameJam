using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class RotationLockerDayan : MonoBehaviour
{
    private Quaternion initialRotation;

    void Start()
    {
        // Guardamos la rotación inicial de la cámara en el mundo.
        // Esto captura la perspectiva 3/4 (ej: X:60, Y:45) que ya configuraste.
        initialRotation = transform.rotation;
    }
}