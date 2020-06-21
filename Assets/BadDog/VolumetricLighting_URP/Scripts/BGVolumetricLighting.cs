using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace BadDog_URP
{
    public class BGVolumetricLighting : ScriptableRendererFeature
    {
        public BGVolumetricLightingSetting settings = new BGVolumetricLightingSetting();

        private BGVolumetricLightingRenderPass m_VolumetricLighitngRenderPass = null;

        public override void Create()
        {
            m_VolumetricLighitngRenderPass = new BGVolumetricLightingRenderPass(settings);
            m_VolumetricLighitngRenderPass.renderPassEvent = settings.renderPassEvent;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            m_VolumetricLighitngRenderPass.Setup(renderer.cameraColorTarget);
            renderer.EnqueuePass(m_VolumetricLighitngRenderPass);
        }
    }
}
