using UnityEngine;
using System.Collections;

public class RotateOnPlayerJump : MonoBehaviour
{
    [SerializeField] private float rotationDuration = 0.3f;
    private bool isRotating = false;
    void Start()
    {
        // Al iniciar el juego, rotar aleatoriamente
        StartCoroutine(SmoothRotate(RandomRotationStep()));
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
    }
}