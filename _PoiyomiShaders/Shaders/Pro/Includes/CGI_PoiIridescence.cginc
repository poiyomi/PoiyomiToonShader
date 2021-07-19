#ifndef POI_IRIDESCENCE
#define POI_IRIDESCENCE
#if defined(PROP_IRIDESCENCERAMP) || !defined(OPTIMIZER_ENABLED)
	UNITY_DECLARE_TEX2D_NOSAMPLER(_IridescenceRamp); float4 _IridescenceRamp_ST;
#endif
#if defined(PROP_IRIDESCENCEMASK) || !defined(OPTIMIZER_ENABLED)
	UNITY_DECLARE_TEX2D_NOSAMPLER(_IridescenceMask); float4 _IridescenceMask_ST;
#endif
#if defined(PROP_IRIDESCENCENORMALMAP) || !defined(OPTIMIZER_ENABLED)
	UNITY_DECLARE_TEX2D_NOSAMPLER(_IridescenceNormalMap); float4 _IridescenceNormalMap_ST;
#endif
float _IridescenceNormalUV;
float _IridescenceMaskUV;
float _IridescenceNormalSelection;
float _IridescenceNormalIntensity;
float _IridescenceNormalToggle;
float _IridescenceIntensity;
fixed _IridescenceAddBlend;
fixed _IridescenceReplaceBlend;
fixed _IridescenceMultiplyBlend;
float _IridescenceEmissionStrength;
float _IridescencePanSpeed;
half _IridescenceOffset;

half _IridescenceHueShiftEnabled;
half _IridescenceHueShiftSpeed;
half _IridescenceHueShift;

#ifdef POI_AUDIOLINK
	half _IridescenceAudioLinkEmissionBand;
	half2 _IridescenceAudioLinkEmission;
#endif

//global
#if defined(PROP_IRIDESCENCENORMALMAP) || !defined(OPTIMIZER_ENABLED)
	float3 calculateNormal(float3 baseNormal)
	{
		
		float3 normal = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_IridescenceNormalMap, _MainTex, TRANSFORM_TEX(poiMesh.uv[_IridescenceNormalUV], _IridescenceNormalMap)), _IridescenceNormalIntensity);
		return normalize(
			normal.x * poiMesh.tangent +
			normal.y * poiMesh.binormal +
			normal.z * baseNormal
		);
	}
#endif

void applyIridescence(inout float4 albedo, inout float3 IridescenceEmission)
{
	float3 normal = poiMesh.normals[_IridescenceNormalSelection];
	
	#if defined(PROP_IRIDESCENCENORMALMAP) || !defined(OPTIMIZER_ENABLED)
		// Use custom normal map
		UNITY_BRANCH
		if (_IridescenceNormalToggle)
		{
			normal = calculateNormal(normal);
		}
	#endif
	
	float ndotv = abs(dot(normal, poiCam.viewDir)) + _Time.x * _IridescencePanSpeed + _IridescenceOffset;
	
	#if defined(PROP_IRIDESCENCERAMP) || !defined(OPTIMIZER_ENABLED)
		float4 iridescenceColor = UNITY_SAMPLE_TEX2D_SAMPLER(_IridescenceRamp, _MainTex, ndotv);
	#else
		float4 iridescenceColor = 1;
	#endif
	
	#if defined(PROP_IRIDESCENCEMASK) || !defined(OPTIMIZER_ENABLED)
		float4 iridescenceMask = UNITY_SAMPLE_TEX2D_SAMPLER(_IridescenceMask, _MainTex, TRANSFORM_TEX(poiMesh.uv[_IridescenceMaskUV], _IridescenceMask));
	#else
		float4 iridescenceMask = 1;
	#endif
	#ifdef POI_BLACKLIGHT
		if (_BlackLightMaskIridescence != 4)
		{
			iridescenceMask *= blackLightMask[_BlackLightMaskIridescence];
		}
	#endif

	UNITY_BRANCH
	if (_IridescenceHueShiftEnabled)
	{
		iridescenceColor.rgb = hueShift(iridescenceColor.rgb, _IridescenceHueShift + _Time.x * _IridescenceHueShiftSpeed);
	}
	
	albedo.rgb = lerp(albedo.rgb, saturate(iridescenceColor.rgb * _IridescenceIntensity), iridescenceColor.a * _IridescenceReplaceBlend * iridescenceMask);
	albedo.rgb += saturate(iridescenceColor.rgb * _IridescenceIntensity * iridescenceColor.a * _IridescenceAddBlend * iridescenceMask);
	albedo.rgb *= saturate(lerp(1, iridescenceColor.rgb * _IridescenceIntensity, iridescenceColor.a * _IridescenceMultiplyBlend * iridescenceMask));
	
	half emissionStrength = _IridescenceEmissionStrength;

	#ifdef POI_AUDIOLINK
		UNITY_BRANCH
		if (poiMods.audioLinkTextureExists)
		{
			emissionStrength += lerp(_IridescenceAudioLinkEmission.x, _IridescenceAudioLinkEmission.y, poiMods.audioLink[_IridescenceAudioLinkEmissionBand]);
		}
	#endif

	IridescenceEmission = saturate(iridescenceColor.rgb * _IridescenceIntensity) * iridescenceColor.a * iridescenceMask * emissionStrength;
}

#endif