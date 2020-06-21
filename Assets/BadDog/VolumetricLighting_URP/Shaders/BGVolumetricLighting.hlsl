#ifndef __BGVOLUMETRICLIGHTING__
#define __BGVOLUMETRICLIGHTING__

TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
TEXTURE2D(_VolumetricLightingTex); SAMPLER(sampler_VolumetricLightingTex);
TEXTURE2D(_CameraDepthTexture); SAMPLER(sampler_CameraDepthTexture);

float2 _MainLightViewPosition;
half _DepthThreshold;
half _LightingRadius;
half _SampleNum;
half _SampleDensity;
half _LightingSampleWeight;
half _LightingDecay;
half _LightingIntensity;
half4 _LightingColor;

struct VertexInput
{
    float4 vertex : POSITION;
    float4 texcoord : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutput
{
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
	UNITY_VERTEX_OUTPUT_STEREO
};

VertexOutput VertDefault(VertexInput v)
{
    VertexOutput o;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	o.pos = TransformObjectToHClip(v.vertex.xyz);
	o.uv = v.texcoord;

    return o;
}

half4 FragPrefilter(VertexOutput i) : SV_Target
{
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

	float2 uv = UnityStereoTransformScreenSpaceTex(i.uv);

	half3 mainColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).rgb;

	// luminance = max(r, g, b)
	half lum = max(mainColor.x, max(mainColor.y, mainColor.z));

	// depth
	float depth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, uv).r;
	depth = Linear01Depth(depth, _ZBufferParams);

	half isSky = depth > _DepthThreshold;
	depth = lerp(0, 1, isSky);
	lum *= isSky;

	float2 distance = UnityStereoTransformScreenSpaceTex(_MainLightViewPosition.xy) - uv;
	distance.y *= _ScreenParams.y / _ScreenParams.x;
	float distanceDecay = saturate(_LightingRadius - length(distance));

	depth *= distanceDecay;

	return half4(depth, depth, depth, lum);
}

half4 FragRadialBlur(VertexOutput i) : SV_Target
{
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

	float2 uv = UnityStereoTransformScreenSpaceTex(i.uv);

	half3 finalColor = half3(0, 0, 0);

	float2 uvDelta = uv - UnityStereoTransformScreenSpaceTex(_MainLightViewPosition);
	uvDelta *= 1.0f / _SampleNum * _SampleDensity;

	half lightingDecay = 1.0f;

	for (int i = 0; i < _SampleNum; i++)
	{
		uv -= uvDelta;

		half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
		color.rgb *= lightingDecay * (_LightingSampleWeight/ _SampleNum) * color.a;

		finalColor.rgb += color.rgb;

		lightingDecay *= _LightingDecay;
	}

	return float4(finalColor.rgb, 1);
}

half4 FragComposite(VertexOutput i) : SV_Target
{
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
	float2 uv = UnityStereoTransformScreenSpaceTex(i.uv);

	half3 mainColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).rgb;

	half3 volumetricColor = SAMPLE_TEXTURE2D(_VolumetricLightingTex, sampler_VolumetricLightingTex, uv).rgb;
	volumetricColor = volumetricColor * _LightingColor.rgb * _LightingIntensity;

#ifdef UNITY_COLORSPACE_GAMMA
	mainColor = mainColor * mainColor;
	volumetricColor = volumetricColor * volumetricColor;
#endif

	half3 finalColor = mainColor + volumetricColor;

#ifdef UNITY_COLORSPACE_GAMMA
	finalColor = sqrt(finalColor);
#endif

	return half4(finalColor, 1);
}

half4 FragDebug(VertexOutput i) : SV_Target
{
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
	return half4(0, 0, 0, 1);
}

#endif // __BGVOLUMETRICLIGHTING__
