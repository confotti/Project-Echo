using UnityEngine;

public class ColorRestorationController : MonoBehaviour
{
    public Transform player;
    public float colorRadius = 5f;
    public float edgeWidth = 2f;

    private static readonly int ColorSource =
        Shader.PropertyToID("_ColorSource");

    private static readonly int ColorRadius =
        Shader.PropertyToID("_ColorRadius");

    private static readonly int EdgeWidth =
        Shader.PropertyToID("_EdgeWidth");

    void Update()
    {
        if (player == null)
            return;

        Shader.SetGlobalVector(
            ColorSource,
            new Vector4(
                player.position.x,
                player.position.y,
                player.position.z,
                0f
            )
        );

        Shader.SetGlobalFloat(ColorRadius, colorRadius);
        Shader.SetGlobalFloat(EdgeWidth, edgeWidth);
    }
}