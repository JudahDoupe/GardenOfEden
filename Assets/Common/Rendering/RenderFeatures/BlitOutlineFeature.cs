using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlitOutlineFeature : ScriptableRendererFeature
{
    internal class RenderPass : ScriptableRenderPass
    {
        public RTHandle MainCameraHandle;
        
        private string layerName;
        private Material renderMaterial;
        private Material outlineMaterial;
        private RTHandle  tempTextureHandle;
        private RTHandle  outlineTextureHandle;

        public RenderPass(Material renderMaterial, Material outlineMaterial, string layerName) : base()
        {
            this.renderMaterial = renderMaterial;
            this.outlineMaterial = outlineMaterial;
            this.layerName = layerName;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            RenderingUtils.ReAllocateIfNeeded(ref outlineTextureHandle, cameraTextureDescriptor, FilterMode.Point, TextureWrapMode.Clamp, name: "_OutlinedGroupTexture");
            ConfigureTarget(outlineTextureHandle);
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

            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0; // Color and depth cannot be combined in RTHandles
            RenderingUtils.ReAllocateIfNeeded(ref tempTextureHandle, desc, FilterMode.Point, TextureWrapMode.Clamp, name: "_TempOutlineBlitMaterialTexture");
            
            CommandBuffer cmd = CommandBufferPool.Get("BlitOutlineFeature");
            Blit(cmd, MainCameraHandle, tempTextureHandle, renderMaterial);
            Blit(cmd, tempTextureHandle, MainCameraHandle);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            outlineTextureHandle.Release();
            tempTextureHandle.Release();
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
        renderer.EnqueuePass(renderPass);
    }
    
    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        renderPass.MainCameraHandle = renderer.cameraColorTargetHandle;  // use of target after allocation
    }
}