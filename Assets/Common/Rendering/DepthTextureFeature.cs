using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DepthTextureFeature : ScriptableRendererFeature
{
    public RenderPassEvent RenderEvent = RenderPassEvent.AfterRenderingOpaques;
    
    private RenderPass _pass;

    public override void Create()
    {
        _pass = new RenderPass
        {
            renderPassEvent = RenderEvent
        };
        _pass.ConfigureInput(ScriptableRenderPassInput.Depth);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        _pass.Setup(renderingData);
        renderer.EnqueuePass(_pass);
    }
        
    protected override void Dispose(bool disposing)
    {
        _pass.Dispose();
    }

    private class RenderPass : ScriptableRenderPass
    {
        private RTHandle _outputTexture;

        public void Setup(in RenderingData renderingData)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = (int) DepthBits.Depth32;
            RenderingUtils.ReAllocateIfNeeded(ref _outputTexture, desc, FilterMode.Point, TextureWrapMode.Clamp, name: "_CPUDepthTexture");
            CameraUtils.DepthTexture = _outputTexture;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get("DepthTexture");
            var source = renderingData.cameraData.renderer.cameraDepthTargetHandle;
            Blitter.BlitCameraTexture(cmd, source, _outputTexture); 
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
        
        public void Dispose()
        {
            _outputTexture?.Release();
        }
    }
}