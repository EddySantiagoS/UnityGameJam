using UnityEngine;

public class TargetTileDisplay : MonoBehaviour
{
    public Renderer displayRenderer; // Arrastra aquí el Quad o plane que muestra la fruta

    public void SetTarget(Texture tex)
    {
        displayRenderer.material.mainTexture = tex;
    }
}
