using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace BadDog_URP
{
    [ExecuteAlways]
    public class AutoLoadPipelineAsset : MonoBehaviour
    {
        public UniversalRenderPipelineAsset pipelineAsset;

        private void OnEnable()
        {
            UpdatePipeline();
        }

        void UpdatePipeline()
        {
            if (pipelineAsset)
            {
                GraphicsSettings.renderPipelineAsset = pipelineAsset;
            }
        }
    }
}
