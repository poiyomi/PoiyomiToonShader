#ifndef POI_PATHING
#define POI_PATHING

// Fill, 0, Path, 1, Loop, 2
half _PathTypeR;
half _PathTypeG;
half _PathTypeB;
half3 _PathWidth;
float3 _PathTime;
float3 _PathOffset;
float3 _PathSpeed;
float4 _PathColorR;
float4 _PathColorG;
float4 _PathColorB;
float3 _PathEmissionStrength;
float3 _PathSoftness;
float3 _PathSegments;
float3 _PathAlpha;

#ifdef POI_AUDIOLINK
	// Time Offset
	half _AudioLinkPathTimeOffsetBandR;
	half2 _AudioLinkPathTimeOffsetR;
	half _AudioLinkPathTimeOffsetBandG;
	half2 _AudioLinkPathTimeOffsetG;
	half _AudioLinkPathTimeOffsetBandB;
	half2 _AudioLinkPathTimeOffsetB;

	// Emission Offset
	half _AudioLinkPathEmissionAddBandR;
	half2 _AudioLinkPathEmissionAddR;
	half _AudioLinkPathEmissionAddBandG;
	half2 _AudioLinkPathEmissionAddG;
	half _AudioLinkPathEmissionAddBandB;
	half2 _AudioLinkPathEmissionAddB;

	// Length Offset
	half _AudioLinkPathWidthOffsetBandR;
	half2 _AudioLinkPathWidthOffsetR;
	half _AudioLinkPathWidthOffsetBandG;
	half2 _AudioLinkPathWidthOffsetG;
	half _AudioLinkPathWidthOffsetBandB;
	half2 _AudioLinkPathWidthOffsetB;
#endif

#if defined(PROP_PATHINGMAP) || !defined(OPTIMIZER_ENABLED)
	POI_TEXTURE_NOSAMPLER(_PathingMap);
#endif
#if defined(PROP_PATHINGCOLORMAP) || !defined(OPTIMIZER_ENABLED)
	POI_TEXTURE_NOSAMPLER(_PathingColorMap);
#endif

void applyPathing(inout float4 albedo, inout float3 pathEmission)
{
	#if defined(PROP_PATHINGMAP) || !defined(OPTIMIZER_ENABLED)
		float4 path = POI2D_SAMPLER_PAN(_PathingMap, _MainTex, poiMesh.uv[_PathingMapUV], _PathingMapPan);
	#else
		float4 path = float4(0,0,0,0);
		return;
	#endif
	
	#if defined(PROP_PATHINGCOLORMAP) || !defined(OPTIMIZER_ENABLED)
		float4 pathColorMap = POI2D_SAMPLER_PAN(_PathingColorMap, _MainTex, poiMesh.uv[_PathingColorMapUV], _PathingColorMapPan);
	#else
		float4 pathColorMap = float4(1, 1, 1, 1);
	#endif
	
	float3 pathAudioLinkEmission = 0;
	float3 pathTime = 0;
	float3 pathAlpha[3] = {
		float3(0.0, 0.0, 0.0), float3(0.0, 0.0, 0.0), float3(0.0, 0.0, 0.0)
	};


	#ifdef POI_AUDIOLINK
		half pathAudioLinkPathTimeOffsetBand[3] = {_AudioLinkPathTimeOffsetBandR, _AudioLinkPathTimeOffsetBandG, _AudioLinkPathTimeOffsetBandB};
		half2 pathAudioLinkTimeOffset[3] = {_AudioLinkPathTimeOffsetR.xy, _AudioLinkPathTimeOffsetG.xy, _AudioLinkPathTimeOffsetB.xy};
		half pathAudioLinkPathWidthOffsetBand[3] = {_AudioLinkPathWidthOffsetBandR, _AudioLinkPathWidthOffsetBandG, _AudioLinkPathWidthOffsetBandB};
		half2 pathAudioLinkWidthOffset[3] = {_AudioLinkPathWidthOffsetR.xy, _AudioLinkPathWidthOffsetG.xy, _AudioLinkPathWidthOffsetB.xy};

		UNITY_BRANCH
		if (poiMods.audioLinkTextureExists)
		{
			// Emission
			pathAudioLinkEmission.r = lerp(_AudioLinkPathEmissionAddR.x, _AudioLinkPathEmissionAddR.y, poiMods.audioLink[_AudioLinkPathEmissionAddBandR]);
			pathAudioLinkEmission.g = lerp(_AudioLinkPathEmissionAddG.x, _AudioLinkPathEmissionAddG.y, poiMods.audioLink[_AudioLinkPathEmissionAddBandG]);
			pathAudioLinkEmission.b = lerp(_AudioLinkPathEmissionAddB.x, _AudioLinkPathEmissionAddB.y, poiMods.audioLink[_AudioLinkPathEmissionAddBandB]);
		}
	#endif

	[unroll]
	for (int index = 0; index < 3; index++)
	{
		pathTime[index] = _PathTime[index] != -999.0f ? frac(_PathTime[index] + _PathOffset[index]): frac(_Time.x * _PathSpeed[index] + _PathOffset[index]);

		#ifdef POI_AUDIOLINK
			UNITY_BRANCH
			if (poiMods.audioLinkTextureExists)
			{
				pathTime[index] += lerp(pathAudioLinkTimeOffset[index].x, pathAudioLinkTimeOffset[index].y, poiMods.audioLink[pathAudioLinkPathTimeOffsetBand[index]]);
			}
		#endif

		if (_PathSegments[index])
		{
			float pathSegments = abs(_PathSegments[index]);
			pathTime = (ceil(pathTime * pathSegments) - .5) / pathSegments;
		}

		if (path[index])
		{
			// Cutting it in half because it goes out in both directions for now
			half pathWidth = _PathWidth[index] * .5;
			#ifdef POI_AUDIOLINK
				UNITY_BRANCH
				if (poiMods.audioLinkTextureExists)
				{
					pathWidth += lerp(pathAudioLinkWidthOffset[index].x, pathAudioLinkWidthOffset[index].y, poiMods.audioLink[pathAudioLinkPathWidthOffsetBand[index]]);
				}
			#endif


			//fill
			pathAlpha[index].x = pathTime[index] > path[index];
			//path
			pathAlpha[index].y = saturate((1 - abs(lerp(-pathWidth, 1 + pathWidth, pathTime[index]) - path[index])) - (1 - pathWidth)) * (1 / pathWidth);
			//loop
			pathAlpha[index].z = saturate((1 - distance(pathTime[index], path[index])) - (1 - pathWidth)) * (1 / pathWidth);
			pathAlpha[index].z += saturate(distance(pathTime[index], path[index]) - (1 - pathWidth)) * (1 / pathWidth);
			pathAlpha[index] = smoothstep(0, _PathSoftness[index] + .00001, pathAlpha[index]);
		}
	}

	// Emission
	pathEmission = 0;
	pathEmission += pathAlpha[0][_PathTypeR] * _PathColorR.rgb * (_PathEmissionStrength[0] + pathAudioLinkEmission.r);
	pathEmission += pathAlpha[1][_PathTypeG] * _PathColorG.rgb * (_PathEmissionStrength[1] + pathAudioLinkEmission.g);
	pathEmission += pathAlpha[2][_PathTypeB] * _PathColorB.rgb * (_PathEmissionStrength[2] + pathAudioLinkEmission.b);
	pathEmission *= pathColorMap.rgb * pathColorMap.a * path.a;

	float3 colorReplace = 0;
	colorReplace = pathAlpha[0][_PathTypeR] * _PathColorR.rgb * pathColorMap.rgb;
	albedo.rgb = lerp(albedo.rgb, colorReplace + albedo.rgb * 0.00001, pathColorMap.a * path.a * _PathColorR.a * pathAlpha[0][_PathTypeR]);
	colorReplace = pathAlpha[1][_PathTypeG] * _PathColorG.rgb * pathColorMap.rgb;
	albedo.rgb = lerp(albedo.rgb, colorReplace + albedo.rgb * 0.00001, pathColorMap.a * path.a * _PathColorG.a * pathAlpha[1][_PathTypeG]);
	colorReplace = pathAlpha[2][_PathTypeB] * _PathColorB.rgb * pathColorMap.rgb;
	albedo.rgb = lerp(albedo.rgb, colorReplace + albedo.rgb * 0.00001, pathColorMap.a * path.a * _PathColorB.a * pathAlpha[2][_PathTypeB]);
}

#endif