using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering;

public class DefaultLayerMaskFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public Material maskMaterial;
        public LayerMask layerMask = 1;
    }

    public Settings settings = new Settings();
    private MaskPass pass;

    public override void Create()
    {
        pass = new MaskPass(settings);
        pass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.maskMaterial == null || renderingData.cameraData.cameraType != CameraType.Game)
            return;

        pass.UpdateSettings(settings.maskMaterial, settings.layerMask);
        renderer.EnqueuePass(pass);
    }

    private class MaskPass : ScriptableRenderPass
    {
        private static readonly int MaskID = Shader.PropertyToID("_DefaultLayerMask");
        private Material material;
        private LayerMask layerMask;

        private class PassData
        {
            public RendererListHandle rendererList;
        }

        public MaskPass(Settings settings)
        {
            material = settings.maskMaterial;
            layerMask = settings.layerMask;
        }

        public void UpdateSettings(Material newMaterial, LayerMask newLayerMask)
        {
            material = newMaterial;
            layerMask = newLayerMask;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (material == null) return;

            var cameraData = frameData.Get<UniversalCameraData>();
            var renderingData = frameData.Get<UniversalRenderingData>();
            var resources = frameData.Get<UniversalResourceData>();

            var desc = cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;
            desc.msaaSamples = 1;
            desc.graphicsFormat = GraphicsFormat.R8_UNorm;
            desc.useMipMap = false;
            desc.autoGenerateMips = false;

            TextureHandle mask = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph, desc, "_DefaultLayerMask", false);

            var tags = new ShaderTagId[] {
                new ShaderTagId("UniversalForward"),
                new ShaderTagId("UniversalForwardOnly"),
                new ShaderTagId("SRPDefaultUnlit")
            };

            var listDesc = new RendererListDesc(tags, renderingData.cullResults, cameraData.camera)
            {
                layerMask = layerMask,
                renderQueueRange = RenderQueueRange.opaque,
                sortingCriteria = SortingCriteria.CommonOpaque,
                overrideMaterial = material,
                overrideMaterialPassIndex = 0
            };

            using (var builder = renderGraph.AddRasterRenderPass<PassData>(
                "Default Layer Mask", out var passData))
            {
                passData.rendererList = renderGraph.CreateRendererList(listDesc);

                builder.UseRendererList(passData.rendererList);
                builder.SetRenderAttachment(mask, 0, AccessFlags.Write);
                builder.SetRenderAttachmentDepth(resources.activeDepthTexture, AccessFlags.Read);
                builder.SetGlobalTextureAfterPass(mask, MaskID);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    context.cmd.ClearRenderTarget(false, true, Color.black);
                    context.cmd.DrawRendererList(data.rendererList);
                });
            }
        }
    }
}