#ifndef POI_MAINTEXTURE
    #define POI_MAINTEXTURE
    
    float2 _MainTexPan;
    uint _MainTextureUV;
    POI_TEXTURE_NOSAMPLER(_BumpMap);
    POI_TEXTURE_NOSAMPLER(_AlphaMask);
    POI_TEXTURE_NOSAMPLER(_DetailMask);
    POI_TEXTURE_NOSAMPLER(_DetailNormalMap);
    POI_TEXTURE_NOSAMPLER(_DetailTex);
    POI_TEXTURE_NOSAMPLER(_MainFadeTexture);
    POI_TEXTURE_NOSAMPLER(_MainHueShiftMask);
    float4 _Color;
    float _MainVertexColoring;
    float _Saturation;
    float _BumpScale;
    float _DetailNormalMapScale;
    float2 _MainDistanceFade;
    half _MainMinAlpha;
    half _DetailTexIntensity;
    half3 _DetailTint;
    float _DetailBrightness;
    float _MainHueShiftToggle;
    float _MainHueShift;
    float _MainHueShiftSpeed;
    float _MainHueShiftReplace;
    //globals
    float alphaMask;
    half3 diffColor;
    
    #include "CGI_PoiBackFace.cginc"
    
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
        mainTexture = UNITY_SAMPLE_TEX2D(_MainTex, TRANSFORM_TEX(poiMesh.uv[_MainTextureUV], _MainTex) + _Time.x * _MainTexPan);
        
        #if (defined(FORWARD_BASE_PASS) || defined(FORWARD_ADD_PASS))
            #ifdef POI_MIRROR
                applyMirrorTexture();
            #endif
        #endif
        
        
        #ifndef SIMPLE
            alphaMask = POI2D_SAMPLER_PAN(_AlphaMask, _MainTex, poiMesh.uv[_AlphaMaskUV], _AlphaMaskPan);
        #else
            alphaMask = 1;
        #endif
        
        mainTexture.a *= alphaMask;
        
        #ifndef POI_SHADOW
            albedo = float4(lerp(mainTexture.rgb, dot(mainTexture.rgb, float3(0.3, 0.59, 0.11)), -_Saturation) * _Color.rgb * lerp(1, GammaToLinearSpace(poiMesh.vertexColor.rgb), _MainVertexColoring), mainTexture.a * _Color.a);
            
            #ifdef POI_RGBMASK
                albedo.rgb = calculateRGBMask(albedo.rgb);
            #endif
            
            albedo.a = saturate(_AlphaMod + albedo.a);
            
            wireframeEmission = 0;
            #ifdef POI_WIREFRAME
                applyWireframe(wireframeEmission, albedo);
            #endif
            
            applyBackFaceTexture();
            
            #ifdef POI_FUR
                calculateFur();
            #endif
            
            UNITY_BRANCH
            if(_MainHueShiftToggle)
            {
                float hueShiftAlpha = POI2D_SAMPLER_PAN(_MainHueShiftMask, _MainTex, poiMesh.uv[_MainHueShiftMaskUV], _MainHueShiftMaskPan).r;
                
                if(_MainHueShiftReplace)
                {
                    albedo.rgb = lerp(albedo.rgb, hueShift(albedo.rgb, _MainHueShift + _MainHueShiftSpeed * _Time.x), hueShiftAlpha);
                }
                else
                {
                    albedo.rgb = hueShift(albedo.rgb, frac((_MainHueShift - (1 - hueShiftAlpha) + _MainHueShiftSpeed * _Time.x)));
                }
            }
            
            half3 mainNormal = UnpackScaleNormal(POI2D_SAMPLER_PAN(_BumpMap, _MainTex, poiMesh.uv[_BumpMapUV], _BumpMapPan), _BumpScale);
            half3 detailMask = POI2D_SAMPLER_PAN(_DetailMask, _MainTex, poiMesh.uv[_DetailMaskUV], _DetailMaskPan);
            half3 detailNormal = UnpackScaleNormal(POI2D_SAMPLER_PAN(_DetailNormalMap, _MainTex, poiMesh.uv[_DetailNormalMapUV], _DetailNormalMapPan), _DetailNormalMapScale * detailMask.g);
            poiMesh.tangentSpaceNormal = poiMesh.tangentSpaceNormal = BlendNormals(mainNormal, detailNormal);
            
            //float4 detailTexture = UNITY_SAMPLE_TEX2D_SAMPLER(_DetailTex, _MainTex, TRANSFORM_TEX(poiMesh.uv[_DetailTexUV], _DetailTex) + _Time.x * _DetailTexPan);
            half3 detailTexture = POI2D_SAMPLER_PAN(_DetailTex, _MainTex, poiMesh.uv[_DetailTexUV], _DetailTexPan).rgb * _DetailTint.rgb;
            albedo.rgb *= LerpWhiteTo(detailTexture * _DetailBrightness * unity_ColorSpaceDouble.rgb, detailMask.r * _DetailTexIntensity);
            albedo.rgb = saturate(albedo.rgb);
            
            poiMesh.normals[1] = normalize(
                poiMesh.tangentSpaceNormal.x * poiMesh.tangent +
                poiMesh.tangentSpaceNormal.y * poiMesh.binormal +
                poiMesh.tangentSpaceNormal.z * poiMesh.normals[0]
            );
            
            poiCam.viewDotNormal = abs(dot(poiCam.viewDir, poiMesh.normals[1]));
            
            #ifdef POI_HOLOGRAM
                ApplyHoloAlpha(albedo);
            #endif
            
            s = FragmentSetup(float4(poiMesh.uv[0], 1, 1), poiCam.viewDir, poiMesh.worldPos);
        #endif
        
        #ifdef DISTORT
            calculateDissolve();
        #endif
    }
    
    void distanceFade()
    {
        half fadeMap = POI2D_SAMPLER_PAN(_MainFadeTexture, _MainTex, poiMesh.uv[_MainFadeTextureUV], _MainFadeTexturePan);
        if (fadeMap)
        {
            half fadeValue = max(smoothstep(_MainDistanceFade.x, _MainDistanceFade.y, poiCam.distanceToVert), _MainMinAlpha);
            albedo.a *= fadeValue;
        }
    }
#endif