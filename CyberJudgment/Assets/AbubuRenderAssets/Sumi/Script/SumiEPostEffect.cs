using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SumiEPostEffect : ScriptableRendererFeature
{
    class SumiERenderPass : ScriptableRenderPass
    {
        public Material effectMaterial = null;
        private RenderTargetIdentifier source;
        private RenderTargetHandle temporaryTexture;

        public SumiERenderPass()
        {
            temporaryTexture.Init("_TemporaryColorTexture");
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            source = renderingData.cameraData.renderer.cameraColorTargetHandle;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (effectMaterial == null)
            {
                return;
            }
            CommandBuffer cmd = CommandBufferPool.Get("SumiEPostEffect");

            cmd.SetGlobalTexture("_OutlineMask", new RenderTargetIdentifier("_OutlineMask"));

            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            cmd.GetTemporaryRT(temporaryTexture.id, descriptor);

            Blit(cmd, source, temporaryTexture.Identifier(), effectMaterial, 0);
            Blit(cmd, temporaryTexture.Identifier(), source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    public Material effectMaterial;
    private SumiERenderPass pass;

    public override void Create()
    {
        pass = new SumiERenderPass();
        pass.effectMaterial = effectMaterial;
        pass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (effectMaterial != null)
        {
            renderer.EnqueuePass(pass);
        }
    }
}
