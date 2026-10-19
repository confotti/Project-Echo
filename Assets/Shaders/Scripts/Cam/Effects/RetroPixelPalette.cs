using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Cam.Effects
{
    [Serializable]
    public class PaletteGrading
    {
        public int size = 6;

        [Range(0f, 0.5f)]
        public float blending = 0.25f;

        public Color c1 = Color.white;
        public Color c2 = Color.black;

        [HideInInspector] public Color c1B;
        [HideInInspector] public Color c2B;
    }

    public class RetroPixelPalette : RetroPixelMax
    {
        public List<PaletteGrading> gradings =
            new List<PaletteGrading>();

        protected override void OnEnable()
        {
            base.OnEnable();
            BlendStops();
            GeneratePalette();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            BlendStops();
            GeneratePalette();
        }

        public void BlendStops()
        {
            if (gradings == null)
                return;

            for (int i = 0; i < gradings.Count; i++)
            {
                PaletteGrading current = gradings[i];

                if (current == null)
                    continue;

                PaletteGrading previous =
                    i > 0 ? gradings[i - 1] : null;

                PaletteGrading next =
                    i < gradings.Count - 1
                        ? gradings[i + 1]
                        : null;

                current.c1B = previous != null
                    ? Color.Lerp(
                        previous.c2,
                        current.c1,
                        1f - current.blending)
                    : current.c1;

                current.c2B = next != null
                    ? Color.Lerp(
                        current.c2,
                        next.c1,
                        current.blending)
                    : current.c2;
            }
        }

        public void GeneratePalette()
        {
            if (gradings == null)
                return;

            List<Color> generated = new List<Color>();

            foreach (PaletteGrading grading in gradings)
            {
                if (grading == null)
                    continue;

                int size = Mathf.Max(0, grading.size);

                for (int j = 0; j < size; j++)
                {
                    float t = (float)j / size;

                    generated.Add(Color.Lerp(
                        grading.c1B,
                        grading.c2B,
                        t));
                }
            }

            colors = generated.ToArray();

            UpdatePaletteMaterial();
        }
    }
}