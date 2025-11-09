using UnityEngine;

public class FloorTile : MonoBehaviour
{
    public Renderer imageRenderer;
    public Texture currentTexture;

    private Material tileMat;
    private Texture hiddenTexture; // textura para cuando se oculta (puede ser gris o vacía)
    private bool isDropped = false;

    void Awake()
    {
        tileMat = imageRenderer.material;
        hiddenTexture = null; // o arrastra una textura genérica si prefieres
    }

    public void SetImage(Texture tex)
    {
        currentTexture = tex;
        tileMat.mainTexture = tex;
    }

    public void HideImage()
    {
        tileMat.mainTexture = hiddenTexture;
    }

    public void DropTile()
    {
        if (isDropped) return;
        isDropped = true;
        // Opción simple: desactivar
        gameObject.SetActive(false);
        // Si quieres animar caída, podrías añadir un rigidbody
    }

    public void ResetTile()
    {
        gameObject.SetActive(true);
        isDropped = false;
    }
}