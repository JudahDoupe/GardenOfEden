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
        private RTHandle _tmpHandle;

        public void Setup(in RenderingData renderingData, string layerName, Material replacementMaterial, Material outlineMaterial)
        {
            _layerName = layerName;
            _replacementMaterial = replacementMaterial;
            _outlineMaterial = outlineMaterial;
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = (int) DepthBits.None;
            RenderingUtils.ReAllocateIfNeeded(ref _tmpHandle, desc, name: "_TempOutlineBlitMaterialTexture");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.isPreviewCamera) return;

            var shaderTags = new List<ShaderTagId>
            {
                new("SRPDefaultUnlit"),
                new("UniversalForward"),
                new("LightweightForward")
            };
            var filteringSettings = new FilteringSettings(RenderQueueRange.opaque, LayerMask.GetMask(_layerName));
            var drawSettings = CreateDrawingSettings(shaderTags, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
            drawSettings.overrideMaterial = _replacementMaterial;
            context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filteringSettings);

            var cmd = CommandBufferPool.Get();
            var source = renderingData.cameraData.renderer.cameraColorTargetHandle;
            Blitter.BlitCameraTexture(cmd, source, _tmpHandle); 

            CoreUtils.SetRenderTarget(cmd, source);
            _outlineMaterial.SetTexture(Shader.PropertyToID("_BlitTexture"), _tmpHandle);
            CoreUtils.DrawFullScreen(cmd, _outlineMaterial);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
        
        public void Dispose()
        {
            _tmpHandle?.Release();
        }
    }
}