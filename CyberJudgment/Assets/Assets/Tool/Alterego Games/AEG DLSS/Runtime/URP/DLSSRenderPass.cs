#if UNITY_URP
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AEG.DLSS
{
    public class DLSSRenderPass : ScriptableRenderPass
    {
        private CommandBuffer cmd;

        private DLSS_URP m_dlssURP;
        private readonly Vector4 flipVector = new Vector4(1, -1, 0, 1);

        public DLSSRenderPass(DLSS_URP _dlssURP) {
            renderPassEvent = RenderPassEvent.AfterRendering + 5;
            m_dlssURP = _dlssURP;
        }

        public void OnSetReference(DLSS_URP _dlssURP) {
            m_dlssURP = _dlssURP;
        }

        // The actual execution of the pass. This is where custom rendering occurs.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
#if AEG_DLSS && UNITY_STANDALONE_WIN && UNITY_64
            try {

                m_dlssURP.CameraGraphicsOutput = renderingData.cameraData.cameraTargetDescriptor.graphicsFormat;

                m_dlssURP.dlssCMD = cmd = CommandBufferPool.Get();

                m_dlssURP.state.CreateContext(m_dlssURP.dlssData, cmd, true);
                m_dlssURP.state.UpdateDispatch(m_dlssURP.m_colorBuffer, m_dlssURP.m_depthBuffer, m_dlssURP.m_motionVectorBuffer, null, m_dlssURP.m_dlssOutput, cmd);

#if UNITY_2022_1_OR_NEWER
                Blitter.BlitCameraTexture(cmd, m_dlssURP.m_dlssOutput, renderingData.cameraData.renderer.cameraColorTargetHandle, flipVector, 0, false);
#else
                Blit(cmd, m_dlssURP.m_dlssOutput, renderingData.cameraData.renderer.cameraColorTarget);
#endif
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
            catch {
            }
#endif
        }
    }

    public class DLSSBufferPass : ScriptableRenderPass
    {
        private DLSS_URP m_dlssURP;

#if !UNITY_2022_1_OR_NEWER
        private CommandBuffer cmd;
#endif

        private readonly int depthTexturePropertyID = Shader.PropertyToID("_CameraDepthTexture");
        private readonly int motionTexturePropertyID = Shader.PropertyToID("_MotionVectorTexture");

        public DLSSBufferPass(DLSS_URP _dlssURP) {
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
            ConfigureInput(ScriptableRenderPassInput.Depth);
            m_dlssURP = _dlssURP;
        }

        //2022 and up
        public void Setup() {
#if AEG_DLSS && UNITY_STANDALONE_WIN && UNITY_64
            if(!Application.isPlaying) {
                return;
            }
            if(m_dlssURP == null) {
                return;
            }

            m_dlssURP.m_depthBuffer = Shader.GetGlobalTexture(depthTexturePropertyID);
            m_dlssURP.m_motionVectorBuffer = Shader.GetGlobalTexture(motionTexturePropertyID);
#endif
        }

        public void OnSetReference(DLSS_URP _dlssURP) {
            m_dlssURP = _dlssURP;
        }

        // The actual execution of the pass. This is where custom rendering occurs.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
#if AEG_DLSS && UNITY_STANDALONE_WIN && UNITY_64

#if UNITY_2022_1_OR_NEWER
            m_dlssURP.m_colorBuffer = renderingData.cameraData.renderer.cameraColorTargetHandle;
#else
            cmd = CommandBufferPool.Get();

            Blit(cmd, renderingData.cameraData.renderer.cameraColorTarget, m_dlssURP.m_colorBuffer);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

            m_dlssURP.m_depthBuffer = Shader.GetGlobalTexture(depthTexturePropertyID);
            m_dlssURP.m_motionVectorBuffer = Shader.GetGlobalTexture(motionTexturePropertyID);
#endif

#endif
        }
    }
}
#endif