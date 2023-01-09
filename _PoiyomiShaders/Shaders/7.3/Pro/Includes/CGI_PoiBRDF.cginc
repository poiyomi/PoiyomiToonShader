#ifndef POI_BRDF
#define POI_BRDF

/*
* MIT License
*
* Copyright (c) 2020 Xiexe
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

#if defined(PROP_BRDFMETALLICGLOSSMAP) || !defined(OPTIMIZER_ENABLED)
    POI_TEXTURE_NOSAMPLER(_BRDFMetallicGlossMap);
#endif
#if defined(PROP_BRDFSPECULARMAP) || !defined(OPTIMIZER_ENABLED)
    POI_TEXTURE_NOSAMPLER(_BRDFSpecularMap);
#endif
#if defined(PROP_BRDFMETALLICMAP) || !defined(OPTIMIZER_ENABLED)
    POI_TEXTURE_NOSAMPLER(_BRDFMetallicMap);
#endif

samplerCUBE _BRDFFallback;

float _BRDFMetallic;
float _BRDFGlossiness;
float _BRDFReflectance;
float _BRDFAnisotropy;
float _BRDFReflectionsEnabled;
float _BRDFSpecularEnabled;
float _BRDFInvertGlossiness;
float _BRDFForceFallback;
float _BRDFMetallicSpecIgnoresBaseColor;

bool DoesReflectionProbeExist()
{
    float4 envSample = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, poiCam.reflectionDir, UNITY_SPECCUBE_LOD_STEPS);
    bool probeExists = !(unity_SpecCube0_HDR.a == 0 && envSample.a == 0);
    return probeExists && !_BRDFForceFallback;
}

float getGeometricSpecularAA(float3 normal)
{
    float3 vNormalWsDdx = ddx(normal.xyz);
    float3 vNormalWsDdy = ddy(normal.xyz);
    float flGeometricRoughnessFactor = pow(saturate(max(dot(vNormalWsDdx.xyz, vNormalWsDdx.xyz), dot(vNormalWsDdy.xyz, vNormalWsDdy.xyz))), 0.333);
    return max(0, flGeometricRoughnessFactor);
}

float3 getAnisotropicReflectionVector(float3 viewDir, float3 bitangent, float3 tangent, float3 normal, float roughness, float anisotropy)
{
    float3 anisotropicDirection = anisotropy >= 0.0 ? bitangent : tangent;
    float3 anisotropicTangent = cross(anisotropicDirection, viewDir);
    float3 anisotropicNormal = cross(anisotropicTangent, anisotropicDirection);
    float bendFactor = abs(anisotropy) * saturate(5.0 * roughness);
    float3 bentNormal = normalize(lerp(normal, anisotropicNormal, bendFactor));
    return reflect(-viewDir, bentNormal);
}

float3 F_Schlick(float u, float3 f0)
{
    return f0 + (1.0 - f0) * pow(1.0 - u, 5.0);
}

float3 F_Schlick(const float3 f0, float f90, float VoH)
{
    // Schlick 1994, "An Inexpensive BRDF Model for Physically-Based Rendering"
    float pow5 = 1.0 - VoH;
    pow5 = pow5 * pow5 * pow5 * pow5 * pow5;
    return f0 + (f90 - f0) * pow5;
}

float D_GGX(float NoH, float roughness)
{
    float a2 = roughness * roughness;
    float f = (NoH * a2 - NoH) * NoH + 1.0;
    return a2 / (UNITY_PI * f * f);
}

float V_SmithGGXCorrelated(float NoV, float NoL, float a)
{
    float a2 = a * a;
    float GGXL = NoV * sqrt((-NoL * a2 + NoL) * NoL + a2);
    float GGXV = NoL * sqrt((-NoV * a2 + NoV) * NoV + a2);
    return 0.5 / (GGXV + GGXL);
}

float D_GGX_Anisotropic(float NoH, const float3 h, const float3 t, const float3 b, float at, float ab)
{
    float ToH = dot(t, h);
    float BoH = dot(b, h);
    float a2 = at * ab;
    float3 v = float3(ab * ToH, at * BoH, a2 * NoH);
    float v2 = dot(v, v);
    float w2 = a2 / v2;
    return a2 * w2 * w2 * (1.0 / UNITY_PI);
}

float3 getBoxProjection(float3 direction, float3 position, float4 cubemapPosition, float3 boxMin, float3 boxMax)
{
    // #if defined(UNITY_SPECCUBE_BOX_PROJECTION) // For some reason this doesn't work?
    if (cubemapPosition.w > 0)
    {
        float3 factors = ((direction > 0 ? boxMax : boxMin) - position) / direction;
        float scalar = min(min(factors.x, factors.y), factors.z);
        direction = direction * scalar + (position - cubemapPosition.xyz);
    }
    // #endif
    return direction;
}

float3 getDirectSpecular(float roughness, float ndh, float vdn, float ndl, float ldh, float3 f0, float3 halfVector, float3 tangent, float3 bitangent, float anisotropy)
{
    #if !defined(LIGHTMAP_ON)
        float rough = max(roughness * roughness, 0.0045);
        float Dn = D_GGX(ndh, rough);
        float3 F = F_Schlick(ldh, f0);
        float V = V_SmithGGXCorrelated(vdn, ndl, rough);
        float3 directSpecularNonAniso = max(0, (Dn * V) * F);
        
        anisotropy *= saturate(5.0 * roughness);
        float at = max(rough * (1.0 + anisotropy), 0.001);
        float ab = max(rough * (1.0 - anisotropy), 0.001);
        float D = D_GGX_Anisotropic(ndh, halfVector, tangent, bitangent, at, ab);
        float3 directSpecularAniso = max(0, (D * V) * F);
        
        return lerp(directSpecularNonAniso, directSpecularAniso, saturate(abs(_BRDFAnisotropy * 100))) * 3; // * 100 to prevent blending, blend because otherwise tangents are fucked on lightmapped object
    #else
        return 0;
    #endif
}

float3 getIndirectSpecular(float metallic, float roughness, float3 reflDir, float3 worldPos, float3 lightmap, float3 normal)
{
    float3 spec = float3(0, 0, 0);
    #if defined(UNITY_PASS_FORWARDBASE)
        float3 indirectSpecular;
        Unity_GlossyEnvironmentData envData;
        envData.roughness = roughness;
        envData.reflUVW = getBoxProjection(
            reflDir, worldPos,
            unity_SpecCube0_ProbePosition,
            unity_SpecCube0_BoxMin.xyz, unity_SpecCube0_BoxMax.xyz
        );
        float3 probe0 = Unity_GlossyEnvironment(UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, envData);
        float interpolator = unity_SpecCube0_BoxMin.w;
        UNITY_BRANCH
        if (interpolator < 0.99999)
        {
            envData.reflUVW = getBoxProjection(
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
        
        if (!DoesReflectionProbeExist())
        {
            indirectSpecular = texCUBElod(_BRDFFallback, float4(reflDir, roughness * UNITY_SPECCUBE_LOD_STEPS)).rgb * poiLight.finalLighting;
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

void poiBRDF(inout float4 finalColor, const float4 finalColorBeforeLighting)
{
    float4 ret = float4(1, 0, 0, 1);
    #if defined(PROP_BRDFMETALLICGLOSSMAP) || !defined(OPTIMIZER_ENABLED)
        float4 metallicGlossMap = POI2D_SAMPLER_PAN(_BRDFMetallicGlossMap, _MainTex, poiMesh.uv[_BRDFMetallicGlossMapUV], _BRDFMetallicGlossMapPan);
    #else
        float4 metallicGlossMap = 1;
    #endif
    #if defined(PROP_BRDFSPECULARMAP) || !defined(OPTIMIZER_ENABLED)
        float4 spcularTintMask = POI2D_SAMPLER_PAN(_BRDFSpecularMap, _MainTex, poiMesh.uv[_BRDFSpecularMapUV], _BRDFSpecularMapPan);
    #else
        float4 spcularTintMask = 1;
    #endif
    #if defined(PROP_BRDFMETALLICMAP) || !defined(OPTIMIZER_ENABLED)
        float4 metallicTintMask = POI2D_SAMPLER_PAN(_BRDFMetallicMap, _MainTex, poiMesh.uv[_BRDFMetallicMapUV], _BRDFMetallicMapPan);
    #else
        float4 metallicTintMask = 1;
    #endif
    UNITY_BRANCH
    if (_BRDFInvertGlossiness == 1)
    {
        metallicGlossMap.a = 1 - metallicGlossMap.a;
    }
    
    float metallic = metallicGlossMap.r * _BRDFMetallic;
    float reflectance = metallicGlossMap.g * _BRDFReflectance;
    float roughness = max(1 - (_BRDFGlossiness * metallicGlossMap.a), getGeometricSpecularAA(poiMesh.normals[1]));
    finalColor.rgb *= lerp(1, 1 - metallic, _BRDFReflectionsEnabled);
    
    float3 reflViewDir = getAnisotropicReflectionVector(poiCam.viewDir, poiMesh.binormal, poiMesh.tangent.xyz, poiMesh.normals[1], roughness, _BRDFAnisotropy);
    float3 reflLightDir = reflect(poiLight.direction, poiMesh.normals[1]);
    
    #if defined(FORWARD_BASE_PASS) || defined(POI_META_PASS)
        float attenuation = poiMax(poiLight.rampedLightMap);
    #endif
    #ifdef FORWARD_ADD_PASS
        float attenuation = saturate(poiLight.nDotL);
    #endif
    
    
    float3 f0 = 0.16 * reflectance * reflectance * (1.0 - metallic) + lerp(finalColorBeforeLighting.rgb * metallic, 1, _BRDFMetallicSpecIgnoresBaseColor);
    float3 fresnel = lerp(F_Schlick(poiLight.nDotV, f0), f0, metallic); //Kill fresnel on metallics, it looks bad.
    float3 directSpecular = getDirectSpecular(roughness, saturate(poiLight.nDotH), max(poiLight.nDotV, 0.000001), attenuation, saturate(poiLight.lDotH), f0, poiLight.halfDir, poiMesh.tangent.xyz, poiMesh.binormal, _BRDFAnisotropy) * poiLight.attenuation * attenuation * poiLight.color;
    directSpecular = min(directSpecular, poiLight.color);
    
    float3 vDirectSpecular = 0;
    #ifdef VERTEXLIGHT_ON
        for (int index = 0; index < 4; index++)
        {
            float3 v0directSpecular = getDirectSpecular(roughness, saturate(poiLight.vDotNH[index]), max(poiLight.nDotV, 0.000001), attenuation, saturate(poiLight.lDotH), f0, poiLight.vHalfDir[index], poiMesh.tangent, poiMesh.binormal, _BRDFAnisotropy) * poiLight.attenuation * poiLight.vAttenuationDotNL[index] * poiLight.vColor[index];
            vDirectSpecular += min(v0directSpecular, poiLight.vColor[index]);
        }
    #endif
    
    float3 indirectSpecular = getIndirectSpecular(metallic, roughness, reflViewDir, poiMesh.worldPos, /*directDiffuse*/ finalColor.rgb, poiMesh.normals[1]) * lerp(fresnel, f0, roughness);
    float3 specular = indirectSpecular * _BRDFReflectionsEnabled * metallicTintMask.a * metallicTintMask.rgb * poiLight.occlusion + (directSpecular + vDirectSpecular) * _BRDFSpecularEnabled * spcularTintMask.a * spcularTintMask.rgb;
    finalColor.rgb += specular;
}

#endif