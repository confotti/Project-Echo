using UnityEngine;

namespace Assets.Scripts.Cam.Effects
{
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Rendering/Retro Size (URP)")]
    public class RetroSize : MonoBehaviour
    {
        [Header("Resolution")]
        [Range(1, 2048)]
        public int horizontalResolution = 160;

        [Range(1, 2048)]
        public int verticalResolution = 144;

        [Header("URP Fullscreen Material")]
        [SerializeField] private Material retroSizeMaterial;

        private static readonly int HorizontalResolutionId =
            Shader.PropertyToID("_HorizontalResolution");

        private static readonly int VerticalResolutionId =
            Shader.PropertyToID("_VerticalResolution");

        private void OnEnable()
        {
            UpdateMaterial();
        }

        private void OnValidate()
        {
            horizontalResolution =
                Mathf.Clamp(horizontalResolution, 1, 2048);

            verticalResolution =
                Mathf.Clamp(verticalResolution, 1, 2048);

            UpdateMaterial();
        }

        public void UpdateMaterial()
        {
            if (retroSizeMaterial == null)
                return;

            retroSizeMaterial.SetInt(
                HorizontalResolutionId,
                Mathf.Clamp(horizontalResolution, 1, 2048));

            retroSizeMaterial.SetInt(
                VerticalResolutionId,
                Mathf.Clamp(verticalResolution, 1, 2048));
        }
    }
}