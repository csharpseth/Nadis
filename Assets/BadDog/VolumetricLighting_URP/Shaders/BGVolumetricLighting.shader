Shader "BadDog/URP/BGVolumetricLighting"
{
    Properties
    {
        _MainTex ("", 2D) = "" {}
    }

	HLSLINCLUDE

		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "./BGVolumetricLighting.hlsl"

    ENDHLSL

    SubShader
    {
        ZTest Always Cull Off ZWrite Off

        Pass
        {
			HLSLPROGRAM
                #pragma vertex VertDefault
                #pragma fragment FragPrefilter
			ENDHLSL
        }

        Pass
        {
			HLSLPROGRAM
                #pragma vertex VertDefault
                #pragma fragment FragRadialBlur
			ENDHLSL
        }

        Pass
        {
			HLSLPROGRAM
                #pragma vertex VertDefault
                #pragma fragment FragComposite
			ENDHLSL
        }

		Pass
        {
			HLSLPROGRAM
                #pragma vertex VertDefault
                #pragma fragment FragDebug
			ENDHLSL
        }
    }
}
