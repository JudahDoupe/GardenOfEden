using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DepthTextureFeature : ScriptableRendererFeature
{
    class RenderPass : ScriptableRenderPass
    {
        public Material DepthMaterial;
        private RenderTargetIdentifier SourceId;

        public RenderPass(Material depthMaterial)
        {
            DepthMaterial = depthMaterial;
        }

        public void SetSource(RenderTargetIdentifier source)
        {
            SourceId = source;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("DepthTexture");
            var screen = renderingData.cameraData.cameraTargetDescriptor;
            var outputTexture = CameraUtils.DepthTexture;

            if (outputTexture == null || screen.width != outputTexture.width || screen.height != outputTexture.height)
            {
                if (outputTexture != null) outputTexture.Release();
                CameraUtils.DepthTexture = outputTexture = new RenderTexture(screen.width, screen.height, 0, RenderTextureFormat.RFloat);
            }

            Blit(cmd, SourceId, outputTexture, DepthMaterial);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    [System.Serializable]
    public class Settings
    {
        public Material depthMaterial;
        public RenderPassEvent renderEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    [SerializeField]
    private Settings settings = new Settings();
    RenderPass Pass;

    public override void Create()
    {
        Pass = new RenderPass(settings.depthMaterial);
        Pass.renderPassEvent = settings.renderEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        Pass.SetSource(renderer.cameraColorTarget);
        renderer.EnqueuePass(Pass);
    }
}


