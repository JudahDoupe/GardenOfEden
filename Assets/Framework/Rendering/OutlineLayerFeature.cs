using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlineLayerFeature : ScriptableRendererFeature
{
    public RenderPassEvent InjectionPoint = RenderPassEvent.AfterRenderingOpaques;
    public string LayerName = "OutlinedGroup";
    public Material ReplacementMaterial;
    public Material OutlineMaterial;

    private RenderPass _pass;

    public override void Create()
    {
        _pass = new RenderPass
        {
            renderPassEvent = InjectionPoint
        };
        _pass.ConfigureInput(ScriptableRenderPassInput.Color);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        _pass.Setup(renderingData, LayerName, ReplacementMaterial, OutlineMaterial);
        renderer.EnqueuePass(_pass);
    }
        
    protected override void Dispose(bool disposing)
    {
        _pass.Dispose();
    }

    private class RenderPass : ScriptableRenderPass
    {
        private string _layerName = "OutlinedGroup";
        private Material _replacementMaterial;
        private Material _outlineMaterial;
        private RTHandle _outlineHandle;
        private RTHandle _sourceHandle;

        public void Setup(in RenderingData renderingData, string layerName, Material replacementMaterial, Material outlineMaterial)
        {
            _layerName = layerName;
            _replacementMaterial = replacementMaterial;
            _outlineMaterial = outlineMaterial;
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = (int) DepthBits.None;
            RenderingUtils.ReAllocateIfNeeded(ref _outlineHandle, desc, name: "_TempOutlineTexture");
            RenderingUtils.ReAllocateIfNeeded(ref _sourceHandle, desc, name: "_TempColorTexture");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.isPreviewCamera) return;

            var shaderTags = new List<ShaderTagId>
            {
                new("SRPDefaultUnlit"),
                new("UniversalForward"),
                new("LightweightForward"),
                new("Universal")
            };
            var block = new RenderStateBlock(RenderStateMask.Nothing);
            var filteringSettings = new FilteringSettings(RenderQueueRange.opaque, LayerMask.GetMask(_layerName));
            var drawSettings = CreateDrawingSettings(shaderTags, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags); 
            drawSettings.overrideMaterial = _replacementMaterial;
            drawSettings.overrideMaterialPassIndex = 0;
            
            var cmd = CommandBufferPool.Get();
            var source = renderingData.cameraData.renderer.cameraColorTargetHandle;
             
            Blitter.BlitCameraTexture(cmd, source, _sourceHandle);            
            CoreUtils.SetRenderTarget(cmd, _outlineHandle);
            CoreUtils.ClearRenderTarget(cmd, ClearFlag.All, Color.black);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            
            context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filteringSettings, ref block);


            CoreUtils.SetRenderTarget(cmd, source);
            _outlineMaterial.SetTexture(Shader.PropertyToID("_OutlinedTexture"), _outlineHandle);
            _outlineMaterial.SetTexture(Shader.PropertyToID("_BlitTexture"), _sourceHandle);
            CoreUtils.DrawFullScreen(cmd, _outlineMaterial);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
        
        public void Dispose()
        {
            _outlineHandle?.Release();
        }
    }
}