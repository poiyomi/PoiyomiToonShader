#ifndef POI_CLEARCOAT
#define POI_CLEARCOAT

float _Clearcoat;
float _ClearcoatGlossiness;
float _ClearcoatAnisotropy;
float _ClearcoatForceFallback;
float _ClearcoatEnableReflections;
float _ClearcoatEnableSpecular;
float _ClearcoatInvertSmoothness;
#if defined(PROP_CLEARCOATMAP) || !defined(OPTIMIZER_ENABLED)
	POI_TEXTURE_NOSAMPLER(_ClearcoatMap);
#endif

samplerCUBE _ClearcoatFallback;

bool clearcoatDoesReflectionProbeExist()
{
	float4 envSample = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, poiCam.reflectionDir, UNITY_SPECCUBE_LOD_STEPS);
	bool probeExists = !(unity_SpecCube0_HDR.a == 0 && envSample.a == 0);
	return probeExists && !_ClearcoatForceFallback;
}

float3 clearcoatF_Schlick(float u, float3 f0)
{
	return f0 + (1.0 - f0) * pow(1.0 - u, 5.0);
}

float4 getClearcoatSmoothness(float4 clearcoatMap)
{
	float roughness = 1 - (_ClearcoatGlossiness * clearcoatMap.a);
	roughness = clamp(roughness, 0.0045, 1.0);
	roughness = roughness * roughness;
	
	float reflectivity = _Clearcoat * clearcoatMap.r;
	return float4(reflectivity, 0, 0, roughness);
}

float getGeometricClearCoatSpecularAA(float3 normal)
{
	float3 vNormalWsDdx = ddx(normal.xyz);
	float3 vNormalWsDdy = ddy(normal.xyz);
	float flGeometricRoughnessFactor = pow(saturate(max(dot(vNormalWsDdx.xyz, vNormalWsDdx.xyz), dot(vNormalWsDdy.xyz, vNormalWsDdy.xyz))), 0.333);
	return max(0, flGeometricRoughnessFactor);
}

float3 getClearcoatAnisotropicReflectionVector(float3 viewDir, float3 bitangent, float3 tangent, float3 normal, float roughness, float anisotropy)
{
	//_Anisotropy = lerp(-0.2, 0.2, sin(_Time.y / 20)); //This is pretty fun
	float3 anisotropicDirection = anisotropy >= 0.0 ? bitangent: tangent;
	float3 anisotropicTangent = cross(anisotropicDirection, viewDir);
	float3 anisotropicNormal = cross(anisotropicTangent, anisotropicDirection);
	float bendFactor = abs(anisotropy) * saturate(5.0 * roughness);
	float3 bentNormal = normalize(lerp(normal, anisotropicNormal, bendFactor));
	return reflect(-viewDir, bentNormal);
}

float D_GGXClearcoat(float NoH, float roughness)
{
	float a2 = roughness * roughness;
	float f = (NoH * a2 - NoH) * NoH + 1.0;
	return a2 / (UNITY_PI * f * f);
}

float D_GGXClearcoat_Anisotropic(float NoH, const float3 h, const float3 t, const float3 b, float at, float ab)
{
	float ToH = dot(t, h);
	float BoH = dot(b, h);
	float a2 = at * ab;
	float3 v = float3(ab * ToH, at * BoH, a2 * NoH);
	float v2 = dot(v, v);
	float w2 = a2 / v2;
	return a2 * w2 * w2 * (1.0 / UNITY_PI);
}

float V_SmithGGXClearcoatCorrelated(float NoV, float NoL, float a)
{
	float a2 = a * a;
	float GGXL = NoV * sqrt((-NoL * a2 + NoL) * NoL + a2);
	float GGXV = NoL * sqrt((-NoV * a2 + NoV) * NoV + a2);
	return 0.5 / (GGXV + GGXL);
}

float3 getClearcoatDirectSpecular(float roughness, float ndh, float vdn, float ndl, float ldh, float3 f0, float3 halfVector, float3 tangent, float3 bitangent, float anisotropy)
{
	#if !defined(LIGHTMAP_ON)
		float rough = max(roughness * roughness, 0.0045);
		float Dn = D_GGXClearcoat(ndh, rough);
		float3 F = clearcoatF_Schlick(ldh, f0);
		float V = V_SmithGGXClearcoatCorrelated(vdn, ndl, rough);
		float3 directSpecularNonAniso = max(0, (Dn * V) * F);
		
		anisotropy *= saturate(5.0 * roughness);
		float at = max(rough * (1.0 + anisotropy), 0.001);
		float ab = max(rough * (1.0 - anisotropy), 0.001);
		float D = D_GGXClearcoat_Anisotropic(ndh, halfVector, tangent, bitangent, at, ab);
		float3 directSpecularAniso = max(0, (D * V) * F);
		
		return lerp(directSpecularNonAniso, directSpecularAniso, saturate(abs(_ClearcoatAnisotropy * 100))) * 3; // * 100 to prevent blending, blend because otherwise tangents are fucked on lightmapped object
	#else
		return 0;
	#endif
}

float3 getClearCoatBoxProjection(float3 direction, float3 position, float4 cubemapPosition, float3 boxMin, float3 boxMax)
{
	// #if defined(UNITY_SPECCUBE_BOX_PROJECTION) // For some reason this doesn't work?
	if (cubemapPosition.w > 0)
	{
		float3 factors = ((direction > 0 ? boxMax: boxMin) - position) / direction;
		float scalar = min(min(factors.x, factors.y), factors.z);
		direction = direction * scalar + (position - cubemapPosition.xyz);
	}
	// #endif
	return direction;
}

float3 getClearcoatIndirectSpecular(float metallic, float roughness, float3 reflDir, float3 worldPos, float3 lightmap, float3 normal)
{
	float3 spec = float3(0, 0, 0);
	#if defined(UNITY_PASS_FORWARDBASE)
		float3 indirectSpecular;
		Unity_GlossyEnvironmentData envData;
		envData.roughness = roughness;
		envData.reflUVW = getClearCoatBoxProjection(
			reflDir, worldPos,
			unity_SpecCube0_ProbePosition,
			unity_SpecCube0_BoxMin.xyz, unity_SpecCube0_BoxMax.xyz
		);
		float3 probe0 = Unity_GlossyEnvironment(UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, envData);
		float interpolator = unity_SpecCube0_BoxMin.w;
		UNITY_BRANCH
		if (interpolator < 0.99999)
		{
			envData.reflUVW = getClearCoatBoxProjection(
				reflDir, worldPos,
				unity_SpecCube1_ProbePosition,
				unity_SpecCube1_BoxMin.xyz, unity_SpecCube1_BoxMax.xyz
			);
			float3 probe1 = Unity_GlossyEnvironment(UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube1, unity_SpecCube0), unity_SpecCube0_HDR, envData);
			indirectSpecular = lerp(probe1, probe0, interpolator);
		}
		else
		{
			indirectSpecular = probe0;
		}
		
		if (!clearcoatDoesReflectionProbeExist())
		{
			indirectSpecular = texCUBElod(_ClearcoatFallback, float4(envData.reflUVW, roughness * UNITY_SPECCUBE_LOD_STEPS)).rgb * poiLight.finalLighting;
		}
		
		float horizon = min(1 + dot(reflDir, normal), 1);
		indirectSpecular *= horizon * horizon;
		
		spec = indirectSpecular;
		#if defined(LIGHTMAP_ON)
			float specMultiplier = max(0, lerp(1, pow(length(lightmap), _SpecLMOcclusionAdjust), _SpecularLMOcclusion));
			spec *= specMultiplier;
		#endif
	#endif
	return spec;
}

void calculateAndApplyClearCoat(inout float4 finalColor)
{
	#if defined(PROP_CLEARCOATMAP) || !defined(OPTIMIZER_ENABLED)
		float4 clearCoatMap = POI2D_SAMPLER_PAN(_ClearcoatMap, _MainTex, poiMesh.uv[_ClearcoatMapUV], _ClearcoatMapPan);
	#else
		float4 clearCoatMap = 1;
	#endif
	
	float4 clearcoatReflectivitySmoothness = getClearcoatSmoothness(clearCoatMap);
	float clearcoatReflectivity = clearcoatReflectivitySmoothness.r;
	float clearcoatRoughness = clearcoatReflectivitySmoothness.a;
	UNITY_BRANCH
	if (_ClearcoatInvertSmoothness)
	{
		clearcoatRoughness = 1 - clearcoatRoughness;
	}
	float3 creflViewDir = getClearcoatAnisotropicReflectionVector(poiCam.viewDir, poiMesh.binormal, poiMesh.tangent.xyz, poiMesh.normals[0], clearcoatRoughness, _ClearcoatAnisotropy);
	float cndl = saturate(dot(poiLight.direction, poiMesh.normals[0]));
	float cvdn = abs(dot(poiCam.viewDir, poiMesh.normals[0]));
	float cndh = saturate(dot(poiMesh.normals[0], poiLight.halfDir));
	
	float3 clearcoatf0 = 0.16 * clearcoatReflectivity * clearcoatReflectivity;
	float3 clearcoatFresnel = clearcoatF_Schlick(cvdn, clearcoatf0);
	
	#if defined(FORWARD_BASE_PASS) || defined(POI_META_PASS)
		float attenuation = poiLight.rampedLightMap;
	#endif
	#ifdef FORWARD_ADD_PASS
		float attenuation = saturate(poiLight.nDotL);
	#endif
	
	float3 vDirectSpecular = 0;
	#ifdef VERTEXLIGHT_ON
		for (int index = 0; index < 4; index++)
		{
			float vcndh = saturate(dot(poiMesh.normals[0], poiLight.vHalfDir[index]));
			float vcndl = saturate(dot(poiLight.vDirection[index], poiMesh.normals[0]));
			float3 v0directSpecular = getClearcoatDirectSpecular(clearcoatRoughness, vcndh, max(cvdn, 0.000001), vcndl, saturate(poiLight.vDotLH[index]), clearcoatf0, poiLight.halfDir, poiMesh.tangent, poiMesh.binormal, _ClearcoatAnisotropy) * poiLight.vAttenuation * vcndl * poiLight.vColor[index];
			vDirectSpecular += v0directSpecular;
		}
	#endif
	
	float3 clearcoatDirectSpecular = getClearcoatDirectSpecular(clearcoatRoughness, cndh, max(cvdn, 0.000001), attenuation, saturate(poiLight.lDotH), clearcoatf0, poiLight.halfDir, poiMesh.tangent, poiMesh.binormal, _ClearcoatAnisotropy) * poiLight.attenuation * attenuation * poiLight.color;
	float3 clearcoatIndirectSpecular = getClearcoatIndirectSpecular(0, clearcoatRoughness, creflViewDir, poiMesh.worldPos, finalColor, poiMesh.normals[0]);
	float3 clearcoat = ((clearcoatDirectSpecular + vDirectSpecular) * clearCoatMap.g * _ClearcoatEnableSpecular + clearcoatIndirectSpecular * clearCoatMap.b * _ClearcoatEnableReflections) * clearcoatReflectivity * clearcoatFresnel;
	finalColor.rgb += clearcoat;
}

#endif