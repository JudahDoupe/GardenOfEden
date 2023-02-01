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
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_pass);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        _pass.MainCameraDepthHandle = renderer.cameraDepthTargetHandle; // use of target after allocation
    }


    private class RenderPass : ScriptableRenderPass
    {
        public RTHandle MainCameraDepthHandle;
        
        private Material _depthMaterial;
        private RTHandle _outputTexture;

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            RenderingUtils.ReAllocateIfNeeded(ref _outputTexture, cameraTextureDescriptor, FilterMode.Point, TextureWrapMode.Clamp, name: "_OutlinedGroupTexture");
            CameraUtils.DepthTexture = _outputTexture;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get("DepthTexture");

            Blit(cmd, MainCameraDepthHandle, _outputTexture, _depthMaterial);
        }
    }
}