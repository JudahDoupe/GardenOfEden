using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlineFeature : ScriptableRendererFeature
{
    class RenderPass : ScriptableRenderPass
    {
        private string layerName;
        private Material outlineMaterial;
        private RenderTargetHandle destinationHandle;

        public RenderPass(Material material, string layerName)
        {
            this.outlineMaterial = material;
            this.layerName = layerName;
            destinationHandle.Init("_OutlinedObjectsTexture");
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cmd.GetTemporaryRT(destinationHandle.id, cameraTextureDescriptor, FilterMode.Bilinear);
            ConfigureTarget(destinationHandle.Identifier());
            ConfigureClear(ClearFlag.All, Color.black);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var shaderTags = new List<ShaderTagId>
            {
                new ShaderTagId("SRPDefaultUnlit"),
                new ShaderTagId("UniversalForward"),
                new ShaderTagId("LightweightForward"),
            };
            var filteringSettings = new FilteringSettings(RenderQueueRange.opaque, LayerMask.GetMask(layerName));
            var drawSettings = CreateDrawingSettings(shaderTags, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
            drawSettings.overrideMaterial = outlineMaterial;
            context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filteringSettings);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(destinationHandle.id);
        }
    }

    [System.Serializable]
    public class Settings
    {
        public string layerName = "OutlinedObjects";
        public Material outlineMaterial;
        public RenderPassEvent renderEvent = RenderPassEvent.AfterRenderingPrePasses;
    }

    [SerializeField]
    private Settings settings = new Settings();

    private RenderPass renderPass;

    public override void Create()
    {
        renderPass = new RenderPass(settings.outlineMaterial, settings.layerName);
        renderPass.renderPassEvent = settings.renderEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(renderPass);
    }
}