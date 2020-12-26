#ifndef POIFRAG
    #define POIFRAG
    
    float _MainEmissionStrength;
    float _IgnoreFog;
    half _GIEmissionMultiplier;
    uint _IridescenceTime;
    uint _AlphaToMask;
    float _ForceOpaque;
    
    // Built-in uniforms for "vertex lights"
    //uniform float4 unity_LightColor[4];
    // array of the colors of the 4 light sources
    //uniform float4 unity_4LightPosX0;
    // x coordinates of the 4 light sources in world space
    //uniform float4 unity_4LightPosY0;
    // y coordinates of the 4 light sources in world space
    //uniform float4 unity_4LightPosZ0;
    // z coordinates of the 4 light sources in world space
    //uniform float4 unity_4LightAtten0;
    // scale factors for attenuation with squared distance
    // uniform vec4 unity_LightPosition[4] is apparently not
    // always correctly set in Unity 3.4
    // uniform vec4 unity_LightAtten[4] is apparently not
    // always correctly set in Unity 3.4
    
    float4 frag(v2f i, uint facing: SV_IsFrontFace): SV_Target
    {
        #ifndef POI_LIGHTING
            #ifdef FORWARD_ADD_PASS
                return 0;
            #endif
        #endif
        
        float3 finalLighting = 1;
        float3 finalSpecular0 = 0;
        float3 finalSpecular1 = 0;
        float3 finalEnvironmentalRim = 0;
        float3 finalSSS = 0;
        fixed lightingAlpha = 1;
        float3 IridescenceEmission = 0;
        float bakedCubemap = 0; // Whether or not metallic should run before or after lighting multiplication
        float3 spawnInEmission = 0;
        float3 voronoiEmission = 0;
        float3 matcapEmission = 0;
        finalEmission = 0;
        poiMesh.isFrontFace = facing;
        //This has to be first because it modifies the UVs for the rest of the functions
        
        #ifdef POI_DATA
            InitData(i, facing);
        #endif
        
        #ifdef POI_BLACKLIGHT
            createBlackLightMask();
            
            UNITY_BRANCH
            if (_BlackLightMaskDebug)
            {
                return float4(blackLightMask.rgb, 1);
            }
        #endif
        
        // This has to happen in init because it alters UV data globally
        #ifdef POI_PARALLAX
            calculateandApplyParallax();
        #endif
        
        #ifdef POI_MAINTEXTURE
            initTextureData();
        #endif
        
        #ifdef POI_DECAL
            applyDecal(albedo);
        #endif
        
        #ifdef POI_DATA
            calculateLightingData(i);
        #endif
        
        
        #ifdef POI_IRIDESCENCE
            UNITY_BRANCH
            if (_IridescenceTime == 0)
            {
                IridescenceEmission = applyIridescence(albedo);
            }
        #endif
        
        #ifdef POI_VORONOI
            applyVoronoi(albedo, voronoiEmission);
        #endif
        
        #ifdef POI_MSDF
            ApplyTextOverlayColor(albedo);
        #endif
        
        #ifdef POI_LIGHTING
            finalLighting = calculateLighting(albedo.rgb);
        #endif
        
        #ifdef POI_ENVIRONMENTAL_RIM
            finalEnvironmentalRim = calculateEnvironmentalRimLighting();
        #endif
        
        #if defined(POI_METAL) || defined(POI_CLEARCOAT)
            CalculateReflectionData();
        #endif
        
        #ifdef POI_DATA
            distanceFade();
        #endif
        
        #ifdef POI_RANDOM
            albedo.a *= i.angleAlpha;
        #endif
        
        #ifdef CUTOUT
            UNITY_BRANCH
            if(_AlphaToMask == 0)
            {
                applyDithering(albedo);
            }
        #endif
        
        albedo.a = max(_ForceOpaque, albedo.a);
        
        #ifdef POI_FLIPBOOK
            calculateFlipbook();
        #endif
        
        #ifdef POI_LIGHTING
            #ifdef SUBSURFACE
                finalSSS = calculateSubsurfaceScattering();
            #endif
        #endif
        
        #ifdef POI_RIM
            calculateRimLighting();
        #endif
        
        #ifdef PANOSPHERE
            calculatePanosphere();
        #endif
        
        finalColor = albedo;
        
        
        
        applySpawnIn(finalColor, spawnInEmission, poiMesh.uv[0], poiMesh.localPos);
        
        #ifdef MATCAP
            matcapEmission = applyMatcap(finalColor);
        #endif
        
        #ifdef PANOSPHERE
            applyPanosphereColor(finalColor);
        #endif
        
        #ifdef POI_FLIPBOOK
            applyFlipbook(finalColor);
        #endif
        
        #ifndef OPAQUE
            clip(finalColor.a - _Clip);
        #endif
        
        #ifdef POI_RIM
            applyRimColor(finalColor);
        #endif
        
        #ifdef POI_DEPTH_COLOR
            applyDepthColor(finalColor, finalEmission, poiCam.screenPos, poiCam.clipPos);
        #endif
        
        #ifdef POI_IRIDESCENCE
            UNITY_BRANCH
            if(_IridescenceTime == 1)
            {
                IridescenceEmission = applyIridescence(finalColor);
            }
        #endif
        
        float4 finalColorBeforeLighting = finalColor;
        
        #ifdef POI_SPECULAR
            finalSpecular0 = calculateSpecular(finalColorBeforeLighting);
            
            //return float4(finalSpecular0, 1);
        #endif
        
        #ifdef POI_PARALLAX
            calculateAndApplyInternalParallax(finalColor);
        #endif
        
        #if defined(FORWARD_BASE_PASS)
            #ifdef POI_LIGHTING
                #ifdef POI_SPECULAR
                    //applyLightingToSpecular();
                    //applySpecular(finalColor);
                #endif
            #endif
        #endif
        #if defined(FORWARD_BASE_PASS) || defined(POI_META_PASS)
            finalEmission += finalColorBeforeLighting.rgb * _MainEmissionStrength * albedo.a;
            finalEmission += wireframeEmission;
            finalEmission += IridescenceEmission;
            finalEmission += spawnInEmission;
            finalEmission += voronoiEmission;
            finalEmission += matcapEmission;
            UNITY_BRANCH
            if (_BackFaceEnabled)
            {
                finalEmission += BackFaceColor * _BackFaceEmissionStrength;
            }
            
            #ifdef PANOSPHERE
                applyPanosphereEmission(finalEmission);
            #endif
            
            #ifdef POI_EMISSION
                finalEmission += calculateEmissionNew(finalColorBeforeLighting, finalColor);
            #endif
            
            #ifdef POI_DISSOLVE
                applyDissolveEmission(finalEmission);
            #endif
            
            #ifdef POI_RIM
                ApplyRimEmission(finalEmission);
            #endif
            
            #ifdef POI_FLIPBOOK
                applyFlipbookEmission(finalEmission);
            #endif
            
            #ifdef POI_GLITTER
                applyGlitter(finalEmission, finalColor);
            #endif
            
            #ifdef POI_MSDF
                {
                    ApplyTextOverlayEmission(finalEmission);
                }
            #endif
        #endif
        
        #ifdef POI_LIGHTING
            #if(defined(POINT) || defined(SPOT))
                #ifdef POI_METAL
                    //ApplyMetallics(finalColor, true);
                #endif
            #endif
        #endif
        
        #if defined(TRANSPARENT) && defined(FORWARD_ADD_PASS)
            finalColor.rgb *= finalColor.a;
        #endif
        
        #ifdef POI_VIDEO
            applyScreenEffect(finalColor, finalColorBeforeLighting);
            finalEmission += globalVideoEmission;
        #endif
        
        #ifdef POI_ALPHA_TO_COVERAGE
            ApplyAlphaToCoverage(finalColor);
        #endif
        
        #ifdef CUTOUT
            UNITY_BRANCH
            if (_AlphaToMask == 1)
            {
                applyDithering(finalColor);
            }
        #endif
        
        #ifdef POI_METAL
            bool probeExists = shouldMetalHappenBeforeLighting();
            UNITY_BRANCH
            if(!probeExists)
            {
                ApplyMetallicsFake(finalColor);
            }
        #endif
        
        #ifdef VERTEXLIGHT_ON
            finalColor.rgb *= finalLighting + poiLight.vFinalLighting;
        #else
            finalColor.rgb *= finalLighting;
        #endif
        
        #ifdef POI_METAL
            UNITY_BRANCH
            if(probeExists)
            {
                ApplyMetallics(finalColor);
            }
        #endif
        
        finalColor.rgb += finalSpecular0 + finalEnvironmentalRim + finalSSS;
        
        #ifdef FORWARD_BASE_PASS
            #ifdef POI_CLEARCOAT
                calculateAndApplyClearCoat(finalColor);
            #endif
        #endif
        
        #ifdef POI_DEBUG
            displayDebugInfo(finalColor);
        #endif
        
        finalColor.a = saturate(finalColor.a);
        
        
        #if defined(TRANSPARENT) || defined(CUTOUT)
            //finalEmission *= finalColor.a;
        #endif
        
        #ifdef POI_META_PASS
            UnityMetaInput meta;
            UNITY_INITIALIZE_OUTPUT(UnityMetaInput, meta);
            meta.Emission = finalEmission * _GIEmissionMultiplier;
            meta.Albedo = saturate(finalColor.rgb);
            #ifdef POI_SPECULAR
                meta.SpecularColor = poiLight.color.rgb * _SpecularTint.rgb * lerp(1, albedo.rgb, _SpecularMetallic) * _SpecularTint.a;
            #else
                meta.SpecularColor = poiLight.color.rgb * albedo.rgb;
            #endif
            return UnityMetaFragment(meta);
        #endif
        
        finalColor.rgb += finalEmission;
        
        #ifdef POI_GRAB
            applyGrabEffects(finalColor);
        #endif
        
        #ifdef POI_BLUR
            ApplyBlurToGrabPass(finalColor);
        #endif
        
        #ifdef FORWARD_BASE_PASS
            UNITY_BRANCH
            if (_IgnoreFog == 0)
            {
                UNITY_APPLY_FOG(i.fogCoord, finalColor);
            }
        #endif
        
        #ifdef OPAQUE
            finalColor.a = 1;
        #endif
        
        #ifdef FORWARD_ADD_PASS
            finalColor.rgb *= finalColor.a;
        #endif
        
        return finalColor;
    }
#endif