using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlitOutlineFeature : ScriptableRendererFeature
{
    class RenderPass : ScriptableRenderPass
    {
        private string layerName;
        private Material renderMaterial;
        private Material outlineMaterial;
        private RenderTargetIdentifier sourceID;
        private RenderTargetHandle tempTextureHandle;
        private RenderTargetHandle outlineTextureHandle;

        public RenderPass(Material renderMaterial, Material outlineMaterial, string layerName) : base()
        {
            this.renderMaterial = renderMaterial;
            this.outlineMaterial = outlineMaterial;
            this.layerName = layerName;
            outlineTextureHandle.Init("_OutlinedGroupTexture");
            tempTextureHandle.Init("_TempOutlineBlitMaterialTexture");
        }

        public void SetSource(RenderTargetIdentifier source)
        {
            this.sourceID = source;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cmd.GetTemporaryRT(outlineTextureHandle.id, cameraTextureDescriptor, FilterMode.Bilinear);
            ConfigureTarget(outlineTextureHandle.Identifier());
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

            RenderTextureDescriptor cameraTextureDesc = renderingData.cameraData.cameraTargetDescriptor;
            cameraTextureDesc.depthBufferBits = 0;
            CommandBuffer cmd = CommandBufferPool.Get("BlitOutlineFeature");

            cmd.GetTemporaryRT(tempTextureHandle.id, cameraTextureDesc, FilterMode.Bilinear);
            Blit(cmd, sourceID, tempTextureHandle.Identifier(), renderMaterial);
            Blit(cmd, tempTextureHandle.Identifier(), sourceID);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(tempTextureHandle.id);
            cmd.ReleaseTemporaryRT(outlineTextureHandle.id); 
        }
    }

    [System.Serializable]
    public class Settings
    {
        public string layerName = "OutlinedGroup";
        public Material renderMaterial;
        public Material replacementMaterial;
        public RenderPassEvent renderEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    [SerializeField]
    private Settings settings = new Settings();

    private RenderPass renderPass;

    public override void Create()
    {
        this.renderPass = new RenderPass(settings.renderMaterial, settings.replacementMaterial, settings.layerName);
        renderPass.renderPassEvent = settings.renderEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderPass.SetSource(renderer.cameraColorTarget);
        renderer.EnqueuePass(renderPass);
    }
}