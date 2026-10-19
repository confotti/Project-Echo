using UnityEngine;

namespace Assets.Scripts.Cam.Effects
{
    [ExecuteAlways]
    [AddComponentMenu("Rendering/Retro Pixel Max (URP)")]
    public class RetroPixelMax : MonoBehaviour
    {
        [Header("Colors")]
        public Color[] colors = new Color[0];

        [Header("URP Fullscreen Material")]
        [SerializeField] protected Material paletteMaterial;

        private static readonly int ColorsId =
            Shader.PropertyToID("_Colors");

        private static readonly int ColorCountId =
            Shader.PropertyToID("_ColorCount");

        protected virtual void OnEnable()
        {
            UpdatePaletteMaterial();
        }

        protected virtual void OnValidate()
        {
            UpdatePaletteMaterial();
        }

        public void UpdatePaletteMaterial()
        {
            if (paletteMaterial == null || colors == null)
                return;

            int count = Mathf.Min(colors.Length, 256);

            paletteMaterial.SetInt("_ColorCount", count);

            if (count > 0)
            {
                Color[] palette = new Color[count];
                System.Array.Copy(colors, palette, count);

                paletteMaterial.SetColorArray(
                    ColorsId, palette);
            }
        }
    }
}