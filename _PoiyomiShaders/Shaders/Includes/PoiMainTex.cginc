#ifndef POI_MAINTEXTURE
    #define POI_MAINTEXTURE
    
    UNITY_DECLARE_TEX2D_NOSAMPLER(_BumpMap); float4 _BumpMap_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_DetailNormalMap); float4 _DetailNormalMap_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_DetailMask); float4 _DetailMask_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_AlphaMask); float4 _AlphaMask_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_MainFadeTexture); float4 _MainFadeTexture_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_DetailTex); float4 _DetailTex_ST;
    
    float4 _Color;
    float _MainVertexColoring;
    float _Saturation;
    float _BumpScale;
    float _DetailNormalMapScale;
    float2 _MainNormalPan;
    float2 _MainDetailNormalPan;
    float2 _MainDistanceFade;
    half _MainMinAlpha;
    half _DetailTexIntensity;
    half3 _DetailTint;
    uint _DetailTexUV;
    uint _DetailNormalUV;
    float _DetailBrightness;
    float2 _DetailTexturePan;
    float _MainHueShift;
    float _AlphaMod;
    //globals
    float alphaMask;
    half3 diffColor;
    uint _BumpMapUV;
    uint _MainTextureUV;
    #include "PoiBackFace.cginc"
    
    float3 wireframeEmission;
    
    inline FragmentCommonData SpecularSetup(float4 i_tex)
    {
        half4 specGloss = 0;
        half3 specColor = specGloss.rgb;
        half smoothness = specGloss.a;
        
        half oneMinusReflectivity;
        diffColor = EnergyConservationBetweenDiffuseAndSpecular(albedo, specColor, /*out*/ oneMinusReflectivity);
        
        FragmentCommonData o = (FragmentCommonData)0;
        o.diffColor = diffColor;
        o.specColor = specColor;
        o.oneMinusReflectivity = oneMinusReflectivity;
        o.smoothness = smoothness;
        return o;
    }
    
    inline FragmentCommonData FragmentSetup(float4 i_tex, half3 i_viewDirForParallax, float3 i_posWorld)
    {
        i_tex = i_tex;
        
        FragmentCommonData o = SpecularSetup(i_tex);
        o.normalWorld = float4(0, 0, 0, 1);
        o.eyeVec = poiCam.viewDir;
        o.posWorld = i_posWorld;
        
        // NOTE: shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)
        o.diffColor = PreMultiplyAlpha(o.diffColor, 1, o.oneMinusReflectivity, /*out*/ o.alpha);
        return o;
    }
    
    void initTextureData()
    {
        mainTexture = UNITY_SAMPLE_TEX2D(_MainTex, TRANSFORM_TEX(poiMesh.uv[_MainTextureUV], _MainTex));
        
        #if (defined(FORWARD_BASE_PASS) || defined(FORWARD_ADD_PASS))
            #ifdef POI_MIRROR
                applyMirrorTexture();
            #endif
        #endif
        
        #ifdef _ALPHABLEND_ON
            calculateDissolve();
        #endif
        
        #ifndef SIMPLE
            alphaMask = UNITY_SAMPLE_TEX2D_SAMPLER(_AlphaMask, _MainTex, TRANSFORM_TEX(poiMesh.uv[0], _AlphaMask));
        #else
            alphaMask = 1;
        #endif
        
        mainTexture *= alphaMask;
        
        #ifndef POI_SHADOW
            albedo = float4(lerp(mainTexture.rgb, dot(mainTexture.rgb, float3(0.3, 0.59, 0.11)), -_Saturation) * _Color.rgb * lerp(1, poiMesh.vertexColor.rgb, _MainVertexColoring), mainTexture.a * _Color.a);
            
            #ifdef POI_RGBMASK
                albedo.rgb = calculateRGBMask(albedo.rgb);
            #endif

            albedo.a = saturate(_AlphaMod + albedo.a);

            wireframeEmission = 0;
            #ifdef POI_WIREFRAME
                applyWireframe(wireframeEmission, albedo);
            #endif
            
            applyBackFaceTexture();
            
            UNITY_BRANCH
            if(_MainHueShift != 0)
            {
                float3 HSVAlbedo = RGBtoHSV(albedo.rgb);
                HSVAlbedo.r = frac(HSVAlbedo.r + _MainHueShift);
                albedo.rgb = HSVtoRGB(HSVAlbedo);
            }
            
            #ifdef POI_BACKFACE
                if(poiMesh.isFrontFace != 1)
                {
                    BackFaceColor = lerp(albedo.rgb, dot(albedo.rgb, float3(0.3, 0.59, 0.11)), -_Saturation);
                    albedo.rgb = BackFaceColor;
                }
            #endif
            
            float3 mainNormal = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_BumpMap, _MainTex, TRANSFORM_TEX(poiMesh.uv[_BumpMapUV], _BumpMap) + _Time.x * _MainNormalPan), _BumpScale);
            float3 detailMask = UNITY_SAMPLE_TEX2D_SAMPLER(_DetailMask, _MainTex, TRANSFORM_TEX(poiMesh.uv[0], _DetailMask));
            float3 detailNormal = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_DetailNormalMap, _MainTex, TRANSFORM_TEX(poiMesh.uv[_DetailNormalUV], _DetailNormalMap) + _Time.x * _MainDetailNormalPan), _DetailNormalMapScale * detailMask.g);
            poiMesh.tangentSpaceNormal = BlendNormals(mainNormal, detailNormal);
            
            albedo.rgb *= lerp(1, UNITY_SAMPLE_TEX2D_SAMPLER(_DetailTex, _MainTex, TRANSFORM_TEX(poiMesh.uv[_DetailTexUV], _DetailTex) + _Time.x * _DetailTexturePan).rgb * _DetailBrightness * _DetailTint * unity_ColorSpaceDouble, detailMask.r * _DetailTexIntensity);
            albedo.rgb = saturate(albedo.rgb);
            poiMesh.normals[1] = normalize(
                poiMesh.tangentSpaceNormal.x * poiMesh.tangent +
                poiMesh.tangentSpaceNormal.y * poiMesh.bitangent +
                poiMesh.tangentSpaceNormal.z * poiMesh.normals[0]
            );
            
            poiLight.nDotV = dot(poiMesh.normals[1], poiCam.viewDir);
            poiLight.vNDotV = dot(poiMesh.normals[0], poiCam.viewDir);
            poiLight.nDotL = dot(poiMesh.normals[1], poiLight.direction);
            poiLight.nDotH = dot(poiMesh.normals[1], poiLight.halfDir);
            poiLight.lDotv = dot(poiLight.direction, poiCam.viewDir);
            poiLight.lDotH = dot(poiLight.direction, poiLight.halfDir);
            
            poiCam.viewDotNormal = abs(dot(poiCam.viewDir, poiMesh.normals[1]));
            
            #ifdef POI_HOLOGRAM
                ApplyHoloAlpha(albedo);
            #endif
            
            s = FragmentSetup(float4(poiMesh.uv[0], 1, 1), poiCam.viewDir, poiMesh.worldPos);
        #endif
    }
    
    void distanceFade()
    {
        half fadeMap = UNITY_SAMPLE_TEX2D_SAMPLER(_MainFadeTexture, _MainTex, TRANSFORM_TEX(poiMesh.uv[0], _MainFadeTexture));
        if(fadeMap)
        {
            half fadeValue = max(smoothstep(_MainDistanceFade.x, _MainDistanceFade.y, poiCam.distanceToVert), _MainMinAlpha);
            albedo.a *= fadeValue;
        }
    }
#endif