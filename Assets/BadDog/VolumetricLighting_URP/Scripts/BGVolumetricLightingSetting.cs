using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System;

namespace BadDog_URP
{
    [Serializable]
    public enum BGVolumetricDebugMode
    {
        Normal,
        RadialBlur
    }

    [Serializable]
    public class BGVolumetricLightingSetting
    {
        public string profilerTag = "BGVolumetricLighting";
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public Material material;
        public int renderTextureSize = 512;

        [Range(1, 128)]
        public int sampleNum = 9;

        [Range(0.1f, 2)]
        public float sampleDensity = 0.25f;

        [Range(1, 4)]
        public int blurNum = 3;

        public bool supportHDR = true;

        public BGVolumetricDebugMode debugMode = BGVolumetricDebugMode.Normal;
    }
}
