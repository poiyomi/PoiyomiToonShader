#ifndef BASICS
    #define BASICS
    
    //Properties
    float4 _Color;
    float _Desaturation;
    UNITY_DECLARE_TEX2D(_MainTex); float4 _MainTex_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_BumpMap); float4 _BumpMap_ST;
    float4 _GlobalPanSpeed;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_DetailNormalMap); float4 _DetailNormalMap_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_DetailNormalMask); float4 _DetailNormalMask_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_AlphaMask); float4 _AlphaMask_ST;
    float _BumpScale;
    float _DetailNormalMapScale;
    float _Clip;
    
    float3 viewDirection;
    float viewDotNormal;
    float4 mainTexture;
    float alphaMask;
    float4 albedo;
    
    void InitializeFragmentNormal(inout v2f i)
    {
        
        float3 mainNormal = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_BumpMap, _MainTex, TRANSFORM_TEX(i.uv, _BumpMap)), _BumpScale);
        float detailNormalMask = UNITY_SAMPLE_TEX2D_SAMPLER(_DetailNormalMask, _MainTex, TRANSFORM_TEX(i.uv, _DetailNormalMask));
        float3 detailNormal = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_DetailNormalMap, _MainTex, TRANSFORM_TEX(i.uv, _DetailNormalMap)), _DetailNormalMapScale * detailNormalMask);
        float3 tangentSpaceNormal = BlendNormals(mainNormal, detailNormal);
        
        #if defined(BINORMAL_PER_FRAGMENT)
            float3 binormal = CreateBinormal(i.normal, i.tangent.xyz, i.tangent.w);
        #else
            float3 binormal = i.binormal;
        #endif
        
        i.normal = normalize(
            tangentSpaceNormal.x * i.tangent +
            tangentSpaceNormal.y * binormal +
            tangentSpaceNormal.z * i.normal
        );
    }
    
    void calculateBasics(inout v2f i)
    {
        UNITY_SETUP_INSTANCE_ID(i);
        baseNormal = i.normal;
        
        #ifndef DRAG_N_DROP
            InitializeFragmentNormal(i);
        #endif
        
        viewDirection = normalize(_WorldSpaceCameraPos - i.worldPos);
        viewDotNormal = abs(dot(viewDirection, i.normal));
        
        mainTexture = UNITY_SAMPLE_TEX2D(_MainTex, TRANSFORM_TEX(i.uv, _MainTex));
        alphaMask = UNITY_SAMPLE_TEX2D_SAMPLER(_AlphaMask, _MainTex, TRANSFORM_TEX(i.uv, _AlphaMask));
        albedo = float4(lerp(mainTexture.rgb, dot(mainTexture.rgb, float3(0.3, 0.59, 0.11)), _Desaturation) * _Color.rgb, mainTexture.a * _Color.a * alphaMask);
    }
#endif