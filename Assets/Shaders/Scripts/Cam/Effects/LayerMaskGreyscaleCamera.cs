using UnityEngine;

namespace Assets.Scripts.Cam.Effects
{
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Rendering/Layer Masked Grayscale Camera (URP)")]
    public class LayerMaskedGrayscaleCamera : MonoBehaviour
    {
        [Header("Mask Camera")]
        [SerializeField] private Camera maskCamera;

        [SerializeField] private LayerMask colorLayer;

        [Header("Fullscreen Material")]
        [SerializeField] private Material grayscaleMaterial;

        private RenderTexture maskTexture;
        private Camera mainCamera;

        private static readonly int MaskTextureId =
            Shader.PropertyToID("_MaskTexture");

        private void OnEnable()
        {
            mainCamera = GetComponent<Camera>();
            ConfigureMaskCamera();
            UpdateMaskTexture();
        }

        private void LateUpdate()
        {
            if (mainCamera == null)
                mainCamera = GetComponent<Camera>();

            ConfigureMaskCamera();
            UpdateMaskTexture();
            SyncMaskCamera();
            UpdateMaterial();

            Debug.Log(
                $"Mask camera: {maskCamera != null}, " +
                $"Mask texture: {maskTexture != null}, " +
                $"Material: {grayscaleMaterial != null}"
            );
        }

        private void ConfigureMaskCamera()
        {
            if (maskCamera == null || mainCamera == null)
                return;

            maskCamera.enabled = true;
            maskCamera.targetDisplay = mainCamera.targetDisplay;
            maskCamera.clearFlags = CameraClearFlags.SolidColor;
            maskCamera.backgroundColor = Color.black; 
            maskCamera.cullingMask = colorLayer;
            maskCamera.allowHDR = false;
            maskCamera.allowMSAA = false;
            maskCamera.depthTextureMode = DepthTextureMode.None;
        }

        private void SyncMaskCamera()
        {
            if (maskCamera == null || mainCamera == null)
                return;

            // Keep the cameras aligned.
            maskCamera.transform.SetPositionAndRotation(
                mainCamera.transform.position,
                mainCamera.transform.rotation
            );

            maskCamera.orthographic = mainCamera.orthographic;
            maskCamera.orthographicSize = mainCamera.orthographicSize;
            maskCamera.fieldOfView = mainCamera.fieldOfView;
            maskCamera.aspect = mainCamera.aspect;
            maskCamera.nearClipPlane = mainCamera.nearClipPlane;
            maskCamera.farClipPlane = mainCamera.farClipPlane;
            maskCamera.lensShift = mainCamera.lensShift;
            maskCamera.usePhysicalProperties =
                mainCamera.usePhysicalProperties;

            maskCamera.allowHDR = false;
            maskCamera.allowMSAA = false;
            maskCamera.cullingMask = colorLayer;
        }

        private void UpdateMaskTexture()
        {
            if (mainCamera == null || maskCamera == null)
                return;

            int width = Mathf.Max(1, mainCamera.pixelWidth);
            int height = Mathf.Max(1, mainCamera.pixelHeight);

            if (maskTexture != null &&
                maskTexture.width == width &&
                maskTexture.height == height)
                return;

            ReleaseMaskTexture();

            maskTexture = new RenderTexture(
                width,
                height,
                24,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Linear
            );

            maskTexture.name = "Layer Mask Texture";
            maskTexture.filterMode = FilterMode.Point;
            maskTexture.wrapMode = TextureWrapMode.Clamp;
            maskTexture.antiAliasing = 1;
            maskTexture.useMipMap = false;
            maskTexture.autoGenerateMips = false;
            maskTexture.Create();

            maskCamera.targetTexture = maskTexture;
            UpdateMaterial();
        }

        private void UpdateMaterial()
        {
            if (grayscaleMaterial == null || maskTexture == null)
                return;

            grayscaleMaterial.SetTexture(
                MaskTextureId,
                maskTexture
            );
        }

        private void OnDisable()
        {
            ReleaseMaskTexture();
        }

        private void OnDestroy()
        {
            ReleaseMaskTexture();
        }

        private void ReleaseMaskTexture()
        {
            if (maskCamera != null &&
                maskCamera.targetTexture == maskTexture)
            {
                maskCamera.targetTexture = null;
            }

            if (maskTexture != null)
            {
                maskTexture.Release();

                if (Application.isPlaying)
                    Destroy(maskTexture);
                else
                    DestroyImmediate(maskTexture);

                maskTexture = null;
            }
        }
    }
}