
// Are Light Volumes enabled on scene?
uniform float _UdonLightVolumeEnabled;

// All volumes count in scene
uniform float _UdonLightVolumeCount;

// Additive volumes max overdraw count
uniform float _UdonLightVolumeAdditiveMaxOverdraw;

// Additive volumes count
uniform float _UdonLightVolumeAdditiveCount;

// Should volumes be blended with lightprobes?
uniform float _UdonLightVolumeProbesBlend;

// Should volumes be with sharp edges when not blending with each other
uniform float _UdonLightVolumeSharpBounds;

// Main 3D Texture atlas
uniform sampler3D _UdonLightVolume;

// World to Local (-0.5, 0.5) UVW Matrix
uniform float4x4 _UdonLightVolumeInvWorldMatrix[32];

// L1 SH components rotation (relative to baked rotataion)
uniform float3 _UdonLightVolumeRotation[64];

// Value that is needed to smoothly blend volumes ( BoundsScale / edgeSmooth )
uniform float3 _UdonLightVolumeInvLocalEdgeSmooth[32];

// AABB Bounds of islands on the 3D Texture atlas
uniform float3 _UdonLightVolumeUvw[192];

// Color multiplier (RGB) | If we actually need to rotate L1 components at all (A)
uniform float4 _UdonLightVolumeColor[32];

// Rotates vector by Matrix 2x3
float3 LV_MultiplyVectorByMatrix2x3(float3 v, float3 r0, float3 r1) {
    float3 r2 = cross(r0, r1);
    return float3(dot(v, r0), dot(v, r1), dot(v, r2));
}

// Checks if local UVW point is in bounds from -0.5 to +0.5
bool LV_PointLocalAABB(float3 localUVW){
    return all(abs(localUVW) <= 0.5);
}

// Calculates local UVW using volume ID
float3 LV_LocalFromVolume(uint volumeID, float3 worldPos) {
    return mul(_UdonLightVolumeInvWorldMatrix[volumeID], float4(worldPos, 1.0)).xyz;
}

// Calculates Island UVW from local UVW
float3 LV_LocalToIsland(uint volumeID, uint texID, float3 localUVW){
    // UVW bounds
    uint uvwID = volumeID * 6 + texID * 2;
    float3 uvwMin = _UdonLightVolumeUvw[uvwID].xyz;
    float3 uvwMax = _UdonLightVolumeUvw[uvwID + 1].xyz;
    // Ramapping world bounds to UVW bounds
    return clamp(lerp(uvwMin, uvwMax, localUVW + 0.5), uvwMin, uvwMax);
}

// Samples 3 SH textures and packing them into L1 channels
void LV_SampleLightVolumeTex(float3 uvw0, float3 uvw1, float3 uvw2, out float3 L0, out float3 L1r, out float3 L1g, out float3 L1b) {
    // Sampling 3D Atlas
    float4 tex0 = tex3Dlod(_UdonLightVolume, float4(uvw0, 0));
    float4 tex1 = tex3Dlod(_UdonLightVolume, float4(uvw1, 0));
    float4 tex2 = tex3Dlod(_UdonLightVolume, float4(uvw2, 0));
    // Packing final data
    L0 = tex0.rgb;
    L1r = float3(tex1.r, tex2.r, tex0.a);
    L1g = float3(tex1.g, tex2.g, tex1.a);
    L1b = float3(tex1.b, tex2.b, tex2.a);
}

// Bounds mask for a volume rotated in world space, using local UVW
float LV_BoundsMask(float3 localUVW, float3 invLocalEdgeSmooth) {
    float3 distToMin = (localUVW + 0.5) * invLocalEdgeSmooth;
    float3 distToMax = (0.5 - localUVW) * invLocalEdgeSmooth;
    float3 fade = saturate(min(distToMin, distToMax));
    return fade.x * fade.y * fade.z;
}

// Default light probes SH components
void LV_SampleLightProbe(out float3 L0, out float3 L1r, out float3 L1g, out float3 L1b) {
    L0 = float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
    L1r = unity_SHAr.xyz;
    L1g = unity_SHAg.xyz;
    L1b = unity_SHAb.xyz;
}

// Linear single SH L1 channel evaluation
float LV_EvaluateSH(float L0, float3 L1, float3 n) {
    return L0 + dot(L1, n);
}

// Samples a Volume with ID and Local UVW
void LV_SampleVolume(uint id, float3 localUVW, out float3 L0, out float3 L1r, out float3 L1g, out float3 L1b) {
    
    // Additive UVW
    float3 uvw0 = LV_LocalToIsland(id, 0, localUVW);
    float3 uvw1 = LV_LocalToIsland(id, 1, localUVW);
    float3 uvw2 = LV_LocalToIsland(id, 2, localUVW);
                
    // Sample additive
    LV_SampleLightVolumeTex(uvw0, uvw1, uvw2, L0, L1r, L1g, L1b);
    
    // Color correction
    float4 color = _UdonLightVolumeColor[id];
    L0 = L0 * color.rgb;
    L1r = L1r * color.r;
    L1g = L1g * color.g;
    L1b = L1b * color.b;
    
    // Rotate if needed
    if (color.a != 0) {
        float3 r0 = _UdonLightVolumeRotation[id * 2];
        float3 r1 = _UdonLightVolumeRotation[id * 2 + 1];
        L1r = LV_MultiplyVectorByMatrix2x3(L1r, r0, r1);
        L1g = LV_MultiplyVectorByMatrix2x3(L1g, r0, r1);
        L1b = LV_MultiplyVectorByMatrix2x3(L1b, r0, r1);
    }
                
}

// Forms specular based on roughness
float LV_DistributionGGX(float NoH, float roughness) {
    float f = (roughness - 1) * ((roughness + 1) * (NoH * NoH)) + 1;
    return (roughness * roughness) / ((float) 3.141592653589793f * f * f);
}

// Faster normalize
float3 LV_Normalize(float3 v) {
    return rsqrt(dot(v, v)) * v;
}

// Calculates speculars for light volumes or any SH L1 data
float3 LightVolumeSpecular(float3 albedo, float smoothness, float metallic, float3 worldNormal, float3 viewDir, float3 L0, float3 L1r, float3 L1g, float3 L1b) {
    
    float3 specColor = max(float3(dot(reflect(-L1r, worldNormal), viewDir), dot(reflect(-L1g, worldNormal), viewDir), dot(reflect(-L1b, worldNormal), viewDir)), 0);
    
    float3 rDir = LV_Normalize(LV_Normalize(L1r) + viewDir);
    float3 gDir = LV_Normalize(LV_Normalize(L1g) + viewDir);
    float3 bDir = LV_Normalize(LV_Normalize(L1b) + viewDir);
    
    float rNh = saturate(dot(worldNormal, rDir));
    float gNh = saturate(dot(worldNormal, gDir));
    float bNh = saturate(dot(worldNormal, bDir));
    
    float roughness = 1 - smoothness;
    float roughExp = roughness * roughness;
    
    float rSpec = LV_DistributionGGX(rNh, roughExp);
    float gSpec = LV_DistributionGGX(gNh, roughExp);
    float bSpec = LV_DistributionGGX(bNh, roughExp);
    
    return max((rSpec + gSpec + bSpec) * (specColor + L0) * lerp(0.04f, albedo, metallic), 0.0) / 3;
    
}

// Calculates speculars for light volumes or any SH L1 data, but simplified, with only one dominant direction
float3 LightVolumeSpecularDominant(float3 albedo, float smoothness, float metallic, float3 worldNormal, float3 viewDir, float3 L0, float3 L1r, float3 L1g, float3 L1b) {

    float3 dominantDir = L1r + L1g + L1b;
    float3 dir = LV_Normalize(LV_Normalize(dominantDir) + viewDir);
    float nh = saturate(dot(worldNormal, dir));
    
    float roughness = 1 - smoothness;
    float roughExp = roughness * roughness;
    
    float spec = LV_DistributionGGX(nh, roughExp);
    
    return max(spec * L0 * lerp(0.04f, albedo, metallic), 0.0);
    
}

// Calculate Light Volume Color based on all SH components provided and the world normal
float3 LightVolumeEvaluate(float3 worldNormal, float3 L0, float3 L1r, float3 L1g, float3 L1b) {
    return float3(LV_EvaluateSH(L0.r, L1r, worldNormal), LV_EvaluateSH(L0.g, L1g, worldNormal), LV_EvaluateSH(L0.b, L1b, worldNormal));
}

// Calculates SH components based on the world position
void LightVolumeSH(float3 worldPos, out float3 L0, out float3 L1r, out float3 L1g, out float3 L1b) {

    // Initializing output variables
    L0  = float3(0, 0, 0);
    L1r = float3(0, 0, 0);
    L1g = float3(0, 0, 0);
    L1b = float3(0, 0, 0);
    
    // Fallback to default light probes if Light Volume are not enabled
    if (!_UdonLightVolumeEnabled || _UdonLightVolumeCount == 0) {
        LV_SampleLightProbe(L0, L1r, L1g, L1b);
        return;
    }
    
    uint volumeID_A = -1; // Main, dominant volume ID
    uint volumeID_B = -1; // Secondary volume ID to blend main with

    float3 localUVW   = float3(0, 0, 0); // Last local UVW to use in disabled Light Probes mode
    float3 localUVW_A = float3(0, 0, 0); // Main local UVW for Y Axis and Free rotations
    float3 localUVW_B = float3(0, 0, 0); // Secondary local UVW
    
    // Are A and B volumes NOT found?
    bool isNoA = true;
    bool isNoB = true;
    
    // Additive volumes variables
    uint addVolumesCount = 0;
    float3 L0_, L1r_, L1g_, L1b_;
    
    // Iterating through all light volumes with simplified algorithm requiring Light Volumes to be sorted by weight in descending order
    [loop]
    for (uint id = 0; id < (uint) _UdonLightVolumeCount; id++) {
        localUVW = LV_LocalFromVolume(id, worldPos);
        if (LV_PointLocalAABB(localUVW)) { // Intersection test
            if (id < (uint) _UdonLightVolumeAdditiveCount) { // Sampling additive volumes
                if (addVolumesCount < (uint) _UdonLightVolumeAdditiveMaxOverdraw) {
                    LV_SampleVolume(id, localUVW, L0_, L1r_, L1g_, L1b_);
                    L0 += L0_;
                    L1r += L1r_;
                    L1g += L1g_;
                    L1b += L1b_;
                    addVolumesCount++;
                } 
            } else if (isNoA) { // First, searching for volume A
                volumeID_A = id;
                localUVW_A = localUVW;
                isNoA = false;
            } else { // Next, searching for volume B if A found
                volumeID_B = id;
                localUVW_B = localUVW;
                isNoB = false;
                break;
            }
        }
    }
    
    // Volume A SH components and mask to blend volume sides
    float3 L0_A  = float3(1, 1, 1);
    float3 L1r_A = float3(0, 0, 0);
    float3 L1g_A = float3(0, 0, 0);
    float3 L1b_A = float3(0, 0, 0);

    // If no volumes found, using Light Probes as fallback
    if (isNoA && _UdonLightVolumeProbesBlend) {
        LV_SampleLightProbe(L0_, L1r_, L1g_, L1b_);
        L0  += L0_;
        L1r += L1r_;
        L1g += L1g_;
        L1b += L1b_;
        return;
    }
        
    // Fallback to lowest weight light volume if oudside of every volume
    localUVW_A = isNoA ? localUVW : localUVW_A;
    volumeID_A = isNoA ? _UdonLightVolumeCount - 1 : volumeID_A;

    // Sampling Light Volume A
    LV_SampleVolume(volumeID_A, localUVW_A, L0_A, L1r_A, L1g_A, L1b_A);
    
    float mask = LV_BoundsMask(localUVW_A, _UdonLightVolumeInvLocalEdgeSmooth[volumeID_A]);
    if (mask == 1 || isNoA || (_UdonLightVolumeSharpBounds && isNoB)) { // Returning SH A result if it's the center of mask or out of bounds
        L0  += L0_A;
        L1r += L1r_A;
        L1g += L1g_A;
        L1b += L1b_A;
        return;
    }
    
    // Volume B SH components
    float3 L0_B  = float3(1, 1, 1);
    float3 L1r_B = float3(0, 0, 0);
    float3 L1g_B = float3(0, 0, 0);
    float3 L1b_B = float3(0, 0, 0);

    if (isNoB && _UdonLightVolumeProbesBlend) { // No Volume found and light volumes blending enabled

        // Sample Light Probes B
        LV_SampleLightProbe(L0_B, L1r_B, L1g_B, L1b_B);

    } else { // Blending Volume A and Volume B
            
        // If no volume b found, use last one found to fallback
        localUVW_B = isNoB ? localUVW : localUVW_B;
        volumeID_B = isNoB ? _UdonLightVolumeCount - 1 : volumeID_B;
            
        // Sampling Light Volume B
        LV_SampleVolume(volumeID_B, localUVW_B, L0_B, L1r_B, L1g_B, L1b_B);
        
    }
        
    // Lerping SH components
    L0  += lerp(L0_B,  L0_A,  mask);
    L1r += lerp(L1r_B, L1r_A, mask);
    L1g += lerp(L1g_B, L1g_A, mask);
    L1b += lerp(L1b_B, L1b_A, mask);

}

// Calculates SH components based on the world position but for additive volumes only
void LightVolumeAdditiveSH(float3 worldPos, out float3 L0, out float3 L1r, out float3 L1g, out float3 L1b) {

    // Initializing output variables
    L0  = float3(0, 0, 0);
    L1r = float3(0, 0, 0);
    L1g = float3(0, 0, 0);
    L1b = float3(0, 0, 0);
    
    if (!_UdonLightVolumeEnabled || _UdonLightVolumeAdditiveCount == 0) return;
    
    // Additive volumes variables
    float3 localUVW = float3(0, 0, 0);
    uint addVolumesCount = 0;
    float3 L0_, L1r_, L1g_, L1b_;
    
    // Iterating through all light volumes with simplified algorithm requiring Light Volumes to be sorted by weight in descending order
    [loop]
    for (uint id = 0; id < (uint) _UdonLightVolumeAdditiveCount && addVolumesCount < (uint) _UdonLightVolumeAdditiveMaxOverdraw; id++) {
        localUVW = LV_LocalFromVolume(id, worldPos);
        //Intersection test
        if (LV_PointLocalAABB(localUVW)) {
            LV_SampleVolume(id, localUVW, L0_, L1r_, L1g_, L1b_);
            L0 += L0_;
            L1r += L1r_;
            L1g += L1g_;
            L1b += L1b_;
            addVolumesCount++;
        }
    }

}