using UnityEngine;

namespace Assets.Scripts.Cam.Effects
{
    [ExecuteAlways]
    [AddComponentMenu("Rendering/Posterize (URP)")]
    public class Posterize : MonoBehaviour
    {
        [Header("Colour Levels")]
        [Min(2)] public int redComponent = 8;
        [Min(2)] public int greenComponent = 8;
        [Min(2)] public int blueComponent = 8;

        [Header("URP Fullscreen Material")]
        [SerializeField] private Material posterizeMaterial;

        private static readonly int RedId =
            Shader.PropertyToID("_Red");

        private static readonly int GreenId =
            Shader.PropertyToID("_Green");

        private static readonly int BlueId =
            Shader.PropertyToID("_Blue");

        private void OnEnable()
        {
            UpdateMaterial();
        }

        private void OnValidate()
        {
            redComponent = Mathf.Max(2, redComponent);
            greenComponent = Mathf.Max(2, greenComponent);
            blueComponent = Mathf.Max(2, blueComponent);

            UpdateMaterial();
        }

        public void UpdateMaterial()
        {
            if (posterizeMaterial == null)
                return;

            posterizeMaterial.SetInt(
                RedId, Mathf.Max(2, redComponent));

            posterizeMaterial.SetInt(
                GreenId, Mathf.Max(2, greenComponent));

            posterizeMaterial.SetInt(
                BlueId, Mathf.Max(2, blueComponent));
        }
    }
}