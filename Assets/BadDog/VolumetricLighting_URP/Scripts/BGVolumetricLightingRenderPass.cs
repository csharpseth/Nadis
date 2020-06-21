using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace BadDog_URP
{
    public class BGVolumetricLightingRenderPass : ScriptableRenderPass
    {
        private BGVolumetricLightingSetting m_Settings;
        private List<BGMainLight> m_InViewLightList = new List<BGMainLight>();
        private RenderTargetIdentifier m_Source;

        private int m_TempRT1;
        private int m_TempRT2;
        private int m_TempRT3;
        private int m_TempRT4;
        private int m_DebugRT;

        private int m_TempDestRT;

        public BGVolumetricLightingRenderPass(BGVolumetricLightingSetting settings)
        {
            m_Settings = settings;

            m_TempRT1 = Shader.PropertyToID("_BGVolumetricLightingTempRT1");
            m_TempRT2 = Shader.PropertyToID("_BGVolumetricLightingTempRT2");
            m_TempRT3 = Shader.PropertyToID("_BGVolumetricLightingTempRT3");
            m_TempRT4 = Shader.PropertyToID("_BGVolumetricLightingTempRT4");
            m_DebugRT = Shader.PropertyToID("_BGVolumetricLightingDebugRT");

            m_TempDestRT = Shader.PropertyToID("_BGVolumetricLightingTempDestRT");
        }

        private RenderTextureFormat GetRenderTextureFormat()
        {
            if (m_Settings.supportHDR)
            {
                return RenderTextureFormat.DefaultHDR;
            }
            else
            {
                return RenderTextureFormat.Default;
            }
        }

        private void UpdateInViewLights(in RenderingData renderingData)
        {
            Camera camera = renderingData.cameraData.camera;

            m_InViewLightList.Clear();

            List<BGMainLight> allLights = BGMainLight.GetAllLights();

            for (int i = 0; i < allLights.Count; i++)
            {
                BGMainLight mainLight = allLights[i];

                if (mainLight.UpdateViewPosition(camera))
                {
                    m_InViewLightList.Add(mainLight);
                }
            }
        }

        private void RenderOneLight(BGMainLight mainLight, CommandBuffer cmd, RenderTargetIdentifier currentSourceRT, RenderTargetIdentifier destRT)
        {
            Material material = mainLight.UpdateMaterial(m_Settings);

            // create RT
            int rtSize = m_Settings.renderTextureSize;
            cmd.GetTemporaryRT(m_TempRT1, rtSize, rtSize, 0, FilterMode.Bilinear, GetRenderTextureFormat());
            cmd.GetTemporaryRT(m_TempRT2, rtSize, rtSize, 0, FilterMode.Bilinear, GetRenderTextureFormat());

            // prefilter
            cmd.Blit(m_Source, m_TempRT1, material, 0);

            int lastRT = -1;

            // radial blur
            for (int i = 0; i < m_Settings.blurNum; i++)
            {
                cmd.Blit(m_TempRT1, m_TempRT2, material, 1);
                lastRT = m_TempRT2;

                int temp = m_TempRT1;
                m_TempRT1 = m_TempRT2;
                m_TempRT2 = temp;
            }

            cmd.SetGlobalTexture(BGShaderIDs._VolumetricLightingTex, lastRT);

            // final composite
            cmd.Blit(currentSourceRT, destRT, material, 2);

            cmd.ReleaseTemporaryRT(m_TempRT1);
            cmd.ReleaseTemporaryRT(m_TempRT2);
        }

        public void Setup(RenderTargetIdentifier source)
        {
            m_Source = source;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cmd.GetTemporaryRT(m_TempDestRT, cameraTextureDescriptor.width, cameraTextureDescriptor.height, 0, FilterMode.Bilinear, cameraTextureDescriptor.colorFormat);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if(m_Settings.material == null)
            {
                Debug.LogError("BGVolumetricLighting.mat missing!");
                return;
            }

            UpdateInViewLights(renderingData);

            int lightCount = m_InViewLightList.Count;

            if(lightCount <= 0)
            {
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get(m_Settings.profilerTag);

            RenderTextureDescriptor renderTextureDesc = renderingData.cameraData.cameraTargetDescriptor;

            var beginRT = m_Source;

#if UNITY_EDITOR
            if (m_Settings.debugMode == BGVolumetricDebugMode.RadialBlur)
            {
                cmd.GetTemporaryRT(m_DebugRT, renderTextureDesc.width, renderTextureDesc.height, 0, FilterMode.Bilinear, renderTextureDesc.colorFormat);
                cmd.Blit(m_Source, m_DebugRT, m_Settings.material, 3);
                beginRT = m_DebugRT;
            }
#endif

            if (lightCount == 1)
            {
                RenderOneLight(m_InViewLightList[0], cmd, beginRT, m_TempDestRT);
                cmd.Blit(m_TempDestRT, m_Source);
            }
            else if (lightCount == 2)
            {
                cmd.GetTemporaryRT(m_TempRT3, renderTextureDesc.width, renderTextureDesc.height, 0, FilterMode.Bilinear, renderTextureDesc.colorFormat);

                RenderOneLight(m_InViewLightList[0], cmd, beginRT, m_TempRT3);
                RenderOneLight(m_InViewLightList[1], cmd, m_TempRT3, m_Source);

                cmd.ReleaseTemporaryRT(m_TempRT3);
            }
            else
            {
                cmd.GetTemporaryRT(m_TempRT3, renderTextureDesc.width, renderTextureDesc.height, 0, FilterMode.Bilinear, renderTextureDesc.colorFormat);
                cmd.GetTemporaryRT(m_TempRT4, renderTextureDesc.width, renderTextureDesc.height, 0, FilterMode.Bilinear, renderTextureDesc.colorFormat);

                RenderOneLight(m_InViewLightList[0], cmd, beginRT, m_TempRT3);

                var lastSourceRT = m_TempRT3;
                var lastDestRT = m_TempRT4;

                for (int i = 1; i < m_InViewLightList.Count - 1; i++)
                {
                    RenderOneLight(m_InViewLightList[i], cmd, lastSourceRT, lastDestRT);

                    var temp = lastSourceRT;
                    lastSourceRT = lastDestRT;
                    lastDestRT = temp;
                }

                RenderOneLight(m_InViewLightList[m_InViewLightList.Count - 1], cmd, lastSourceRT, m_Source);

                cmd.ReleaseTemporaryRT(m_TempRT3);
                cmd.ReleaseTemporaryRT(m_TempRT4);
            }

#if UNITY_EDITOR
            if (m_Settings.debugMode == BGVolumetricDebugMode.RadialBlur)
            {
                if (beginRT != m_Source)
                {
                    cmd.ReleaseTemporaryRT(m_DebugRT);
                }
            }
#endif
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(m_TempDestRT);
        }
    }
}
