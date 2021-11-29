#ifndef POI_DATA
#define POI_DATA

float _ParallaxBias;
float _LightingAdditiveLimitIntensity;
float _LightingAdditiveMaxIntensity;
POI_TEXTURE_NOSAMPLER(_BumpMap);
#ifdef FINALPASS
	#if defined(PROP_DETAILMASK) || !defined(OPTIMIZER_ENABLED)
		POI_TEXTURE_NOSAMPLER(_DetailMask);
	#endif
	#if defined(PROP_DETAILNORMALMAP) || !defined(OPTIMIZER_ENABLED)
		POI_TEXTURE_NOSAMPLER(_DetailNormalMap);
	#endif
	float _DetailNormalMapScale;
#endif
float _BumpScale;

void calculateAttenuation(v2f i)
{
	#ifdef FORWARD_ADD_PASS
		#if defined(POINT) || defined(SPOT)
			POI_LIGHT_ATTENUATION(attenuation, shadow, i, i.worldPos.xyz)
				poiLight.additiveShadow = shadow;
		#else
			UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos.xyz)
			poiLight.additiveShadow == 0;
		#endif
	#else
		UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos.xyz)
		// fix for rare bug where light atten is 0 when there is no directional light in the scene
		#ifdef FORWARD_BASE_PASS
			if (all(_LightColor0.rgb == 0.0))
			{
				attenuation = 1.0;
			}
		#endif
	#endif
	poiLight.attenuation = attenuation;
}

void calculateVertexLightingData(in v2f i)
{
	#ifdef VERTEXLIGHT_ON
		float4 toLightX = unity_4LightPosX0 - i.worldPos.x;
		float4 toLightY = unity_4LightPosY0 - i.worldPos.y;
		float4 toLightZ = unity_4LightPosZ0 - i.worldPos.z;
		float4 lengthSq = 0;
		lengthSq += toLightX * toLightX;
		lengthSq += toLightY * toLightY;
		lengthSq += toLightZ * toLightZ;
		
		float4 lightAttenSq = unity_4LightAtten0;
		float4 atten = 1.0 / (1.0 + lengthSq * lightAttenSq);
		float4 vLightWeight = saturate(1 - (lengthSq * lightAttenSq / 25));
		poiLight.vAttenuation = min(atten, vLightWeight * vLightWeight);
		
		poiLight.vDotNL = 0;
		poiLight.vDotNL += toLightX * poiMesh.normals[1].x;
		poiLight.vDotNL += toLightY * poiMesh.normals[1].y;
		poiLight.vDotNL += toLightZ * poiMesh.normals[1].z;
		
		float4 corr = rsqrt(lengthSq);
		poiLight.vDotNL = max(0, poiLight.vDotNL * corr);
		poiLight.vAttenuationDotNL = poiLight.vAttenuation * poiLight.vDotNL;
		
		for (int index = 0; index < 4; index++)
		{
			poiLight.vPosition[index] = float3(unity_4LightPosX0[index], unity_4LightPosY0[index], unity_4LightPosZ0[index]);
			
			float3 vertexToLightSource = poiLight.vPosition[index] - poiMesh.worldPos;
			poiLight.vDirection[index] = normalize(vertexToLightSource);
			//poiLight.vAttenuationDotNL[index] = 1.0 / (1.0 + unity_4LightAtten0[index] * poiLight.vDotNL[index]);
			poiLight.vColor[index] = unity_LightColor[index].rgb;
			UNITY_BRANCH
			if (_LightingAdditiveLimitIntensity == 1)
			{
				float intensity = max(0.001, (0.299 * poiLight.vColor[index].r + 0.587 * poiLight.vColor[index].g + 0.114 * poiLight.vColor[index].b));
				poiLight.vColor[index] = min(poiLight.vColor[index], poiLight.vColor[index] / (intensity / _LightingAdditiveMaxIntensity));
			}
			poiLight.vHalfDir[index] = Unity_SafeNormalize(poiLight.vDirection[index] + poiCam.viewDir);
			poiLight.vDotNL[index] = dot(poiMesh.normals[1], -poiLight.vDirection[index]);
			poiLight.vCorrectedDotNL[index] = .5 * (poiLight.vDotNL[index] + 1);
			poiLight.vDotLH[index] = saturate(dot(poiLight.vDirection[index], poiLight.vHalfDir[index]));
			
			poiLight.vDotNH[index] = saturate(dot(poiMesh.normals[1], poiLight.vHalfDir[index]));
		}
	#endif
}

void calculateLightingData(in v2f i)
{
	poiLight.occlusion = 1;
	#ifdef FORWARD_BASE_PASS
		//poiLight.color = saturate(_LightColor0.rgb) + saturate(ShadeSH9(normalize(unity_SHAr + unity_SHAg + unity_SHAb)));
		float3 magic = max(ShadeSH9(normalize(unity_SHAr + unity_SHAg + unity_SHAb)), 0);
		float3 normalLight = _LightColor0.rgb;
		poiLight.color = magic + normalLight;
	#else
		#ifdef FORWARD_ADD_PASS
			poiLight.color = _LightColor0.rgb;
			
			UNITY_BRANCH
			if (_LightingAdditiveLimitIntensity == 1)
			{
				float additiveLightIntensity = max(0.001, (0.299 * poiLight.color.r + 0.587 * poiLight.color.g + 0.114 * poiLight.color.b));
				poiLight.color = min(poiLight.color, poiLight.color / (additiveLightIntensity / _LightingAdditiveMaxIntensity));
			}
		#endif
	#endif
	
	#ifdef FORWARD_BASE_PASS
		poiLight.direction = normalize(_WorldSpaceLightPos0.xyz + unity_SHAr.xyz + unity_SHAg.xyz + unity_SHAb.xyz);
	#else
		#if defined(POINT) || defined(SPOT)
			poiLight.direction = normalize(_WorldSpaceLightPos0.xyz - i.worldPos.xyz);
		#else
			poiLight.direction = _WorldSpaceLightPos0.xyz;
		#endif
	#endif
	
	poiLight.halfDir = normalize(poiLight.direction + poiCam.viewDir);
	
	poiLight.dotNH = saturate(dot(poiMesh.normals[1], poiLight.halfDir));
	poiLight.dotLH = saturate(dot(poiLight.direction, poiLight.halfDir));
	
	poiLight.nDotV = dot(poiMesh.normals[1], poiCam.viewDir);
	poiLight.N0DotV = dot(poiMesh.normals[0], poiCam.viewDir);
	poiLight.nDotL = dot(poiMesh.normals[1], poiLight.direction);
	poiLight.nDotH = dot(poiMesh.normals[1], poiLight.halfDir);
	poiLight.lDotv = dot(poiLight.direction, poiCam.viewDir);
	poiLight.lDotH = dot(poiLight.direction, poiLight.halfDir);
}

void initPoiMods()
{
	poiMods.audioLink = float4(0, 0, 0, 0);
	poiMods.globalMasks = float4(0, 0, 0, 0);
	#ifdef POI_AUDIOLINK
		initAudioBands();
	#endif
}

void InitializeMeshData(inout v2f i, uint facing)
{
	poiMesh.isFrontFace = facing;
	poiMesh.normals[0] = i.normal;
	poiMesh.binormal.rgb = i.binormal;
	poiMesh.tangent = i.tangent;
	
	#ifndef OUTLINE
		if (!poiMesh.isFrontFace)
		{
			poiMesh.normals[0] *= -1;
			poiMesh.tangent *= -1;
			poiMesh.binormal *= -1;
		}
	#endif
	
	poiMesh.worldPos = i.worldPos.xyz;
	poiMesh.localPos = i.localPos.xyz;
	poiMesh.barycentricCoordinates = i.barycentricCoordinates;
	poiMesh.uv[0] = i.uv0.xy;
	poiMesh.uv[1] = i.uv0.zw;
	poiMesh.uv[2] = i.uv1.xy;
	poiMesh.uv[3] = i.uv1.zw;
	
	initPoiMods();

	#ifdef POI_UV_DISTORTION
		poiMesh.uv[4] = calculateDistortionUV(i.uv0.xy);
	#else
		poiMesh.uv[4] = poiMesh.uv[0];
	#endif
	
	poiMesh.vertexColor = i.vertexColor;
	#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
		poiMesh.lightmapUV = i.lightmapUV;
	#endif
	poiMesh.modelPos = i.modelPos;
	
	#ifdef FUR
		poiMesh.furAlpha = i.furAlpha;
	#endif
}

void initializeCamera(v2f i)
{
	poiCam.viewDir = normalize(_WorldSpaceCameraPos - i.worldPos.xyz);
	poiCam.forwardDir = getCameraForward();
	poiCam.worldPos = _WorldSpaceCameraPos;
	poiCam.distanceToModel = distance(poiMesh.modelPos, poiCam.worldPos);
	poiCam.distanceToVert = distance(poiMesh.worldPos, poiCam.worldPos);
	poiCam.grabPos = i.grabPos;
	poiCam.screenUV = calcScreenUVs(i.grabPos);
	poiCam.clipPos = i.pos;
	#if defined(GRAIN)
		poiCam.worldDirection = i.worldDirection;
	#endif
	
	poiCam.tangentViewDir = normalize(i.tangentViewDir);
	poiCam.decalTangentViewDir = poiCam.tangentViewDir;
	poiCam.tangentViewDir.xy /= (poiCam.tangentViewDir.z + _ParallaxBias);
}

void calculateTangentData()
{
	poiTData.tangentTransform = float3x3(poiMesh.tangent.xyz, poiMesh.binormal, poiMesh.normals[0]);
	poiTData.tangentToWorld = transpose(float3x3(poiMesh.tangent.xyz, poiMesh.binormal, poiMesh.normals[0]));
}

void CalculateReflectionData()
{
	#if defined(_METALLICGLOSSMAP) || defined(_COLORCOLOR_ON)
		poiCam.reflectionDir = reflect(-poiCam.viewDir, poiMesh.normals[1]);
		poiCam.vertexReflectionDir = reflect(-poiCam.viewDir, poiMesh.normals[0]);
	#endif
}

void calculateNormals(inout half3 detailMask)
{
	half3 mainNormal = UnpackScaleNormal(POI2D_SAMPLER_PAN(_BumpMap, _MainTex, poiMesh.uv[_BumpMapUV], _BumpMapPan), _BumpScale);
	
	#ifdef FINALPASS
		#if defined(PROP_DETAILMASK) || !defined(OPTIMIZER_ENABLED)
			detailMask = POI2D_SAMPLER_PAN(_DetailMask, _MainTex, poiMesh.uv[_DetailMaskUV], _DetailMaskPan).rgb;
		#else
			detailMask = 1;
		#endif
		UNITY_BRANCH
		if (_DetailNormalMapScale > 0)
		{
			#if defined(PROP_DETAILNORMALMAP) || !defined(OPTIMIZER_ENABLED)
				half3 detailNormal = UnpackScaleNormal(POI2D_SAMPLER_PAN(_DetailNormalMap, _MainTex, poiMesh.uv[_DetailNormalMapUV], _DetailNormalMapPan), _DetailNormalMapScale * detailMask.g);
				poiMesh.tangentSpaceNormal = BlendNormals(mainNormal, detailNormal);
			#else
				poiMesh.tangentSpaceNormal = mainNormal;
			#endif
		}
		else
		{
			poiMesh.tangentSpaceNormal = mainNormal;
		}
	#else
		poiMesh.tangentSpaceNormal = mainNormal;
	#endif
	
	#ifdef POI_RGBMASK
		calculateRGBNormals(poiMesh.tangentSpaceNormal);
	#endif
	
	poiMesh.normals[1] = normalize(
		poiMesh.tangentSpaceNormal.x * poiMesh.tangent.xyz +
		poiMesh.tangentSpaceNormal.y * poiMesh.binormal +
		poiMesh.tangentSpaceNormal.z * poiMesh.normals[0]
	);
	
	poiCam.viewDotNormal = abs(dot(poiCam.viewDir, poiMesh.normals[1]));
}
#endif