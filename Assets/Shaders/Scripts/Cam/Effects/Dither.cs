using UnityEngine;

namespace Assets.Scripts.Cam.Effects
{
    [ExecuteAlways]
    [AddComponentMenu("Rendering/Dither (URP)")]
    public class Dither : MonoBehaviour
    {
        [Header("Dither Pattern")]
        public Texture2D pattern;

        [Range(0f, 1f)]
        public float threshold = 0.45f;

        [Range(0f, 100f)]
        public float strength = 0.45f;

        [Header("URP Fullscreen Material")]
        [SerializeField] private Material ditherMaterial;

        private static readonly int DitherId =
            Shader.PropertyToID("_Dither");

        private static readonly int WidthId =
            Shader.PropertyToID("_Width");

        private static readonly int HeightId =
            Shader.PropertyToID("_Height");

        private static readonly int ThresholdId =
            Shader.PropertyToID("_Threshold");

        private static readonly int StrengthId =
            Shader.PropertyToID("_Strength");

        private void OnEnable()
        {
            UpdateMaterial();
        }

        private void OnValidate()
        {
            UpdateMaterial();
        }

        public void UpdateMaterial()
        {
            if (ditherMaterial == null)
                return;

            ditherMaterial.SetTexture(DitherId, pattern);
            ditherMaterial.SetFloat(ThresholdId, threshold);
            ditherMaterial.SetFloat(StrengthId, strength);

            ditherMaterial.SetInt(
                WidthId,
                pattern != null ? pattern.width : 1);

            ditherMaterial.SetInt(
                HeightId,
                pattern != null ? pattern.height : 1);
        }
    }
}