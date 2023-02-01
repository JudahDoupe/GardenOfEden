using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlitMaterialFeature : ScriptableRendererFeature
{
    public Material Material;
    public RenderPassEvent RenderEvent = RenderPassEvent.AfterRenderingOpaques;
    private RenderPass _renderPass;

    public override void Create()
    {
        _renderPass = new RenderPass
        {
            renderPassEvent = RenderEvent,
            Material = Material,
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_renderPass);
    }
    
    public override void SetupRenderPasses(ScriptableRenderer renderer,
                                           in RenderingData renderingData)
    {
        _renderPass.ConfigureInput(ScriptableRenderPassInput.Color);
        _renderPass.SetTarget(renderer.cameraColorTargetHandle, Material);
    }

    private class RenderPass : ScriptableRenderPass
    {
        public Material Material;
        public RTHandle MainCameraHandle;

        public void SetTarget(RTHandle colorHandle, Material material)
        {
            MainCameraHandle = colorHandle;
            Material = material;
        }
        
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ConfigureTarget(MainCameraHandle);
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();
            Blitter.BlitCameraTexture(cmd, MainCameraHandle, MainCameraHandle, Material, 0);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CommandBufferPool.Release(cmd);
        }
    }
}