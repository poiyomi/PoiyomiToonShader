#ifndef POIFRAG
    #define POIFRAG
    
    float4 _GlobalPanSpeed;
    float _MainEmissionStrength;
    float _IgnoreFog;
    half _GIEmissionMultiplier;
    uint _IridescenceTime;
    
    float4 frag(v2f i, float facing: VFACE): SV_Target
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
        
        finalEmission = 0;
        poiMesh.isFrontFace = facing;
        i.uv0.xy += _GlobalPanSpeed.xy * _Time.x;
        //This has to be first because it modifies the UVs for the rest of the functions
        
        #ifdef POI_DATA
            InitData(i);
        #endif
        
        applyDistanceDithering();
        
        // This has to happen in init because it alters UV data globally
        #ifdef POI_PARALLAX
            calculateandApplyParallax();
        #endif
        
        #ifdef POI_MAINTEXTURE
            initTextureData();
        #endif
        
        #ifdef POI_IRIDESCENCE
            UNITY_BRANCH
            if (_IridescenceTime == 0)
            {
                IridescenceEmission = applyIridescence(albedo);
            }
        #endif
        
        #ifdef POI_MSDF
            ApplyTextOverlayColor(albedo);
        #endif
        
        #ifdef POI_LIGHTING
            finalLighting = calculateLighting();
        #endif
        
        #ifdef POI_ENVIRONMENTAL_RIM
            finalEnvironmentalRim = calculateEnvironmentalRimLighting();
        #endif
        
        #if defined(POI_METAL) || defined(POI_CLEARCOAT)
            CalculateReflectionData();
        #endif
        
        #ifdef POI_METAL
            //calculateReflections();
            CalculateEnvironmentalReflections(lightingAlpha, bakedCubemap);
        #endif
        
        #ifdef POI_DATA
            distanceFade();
        #endif
        
        #ifdef POI_RANDOM
            albedo.a *= i.angleAlpha;
        #endif
        
        #ifndef OPAQUE
            clip(albedo.a - _Clip);
        #endif
        
        #ifdef MATCAP
            calculateMatcap();
        #endif
        
        #ifdef POI_FLIPBOOK
            calculateFlipbook();
        #endif
        
        #ifdef POI_LIGHTING
            #ifdef SUBSURFACE
                finalSSS = max(0, getSubsurfaceLighting());
            #endif
        #endif
        
        #ifdef POI_RIM
            calculateRimLighting();
        #endif
        
        #ifdef PANOSPHERE
            calculatePanosphere();
        #endif
        
        finalColor = albedo;
        
        
        #ifdef MATCAP
            applyMatcap(finalColor);
        #endif
        
        #ifdef PANOSPHERE
            applyPanosphereColor(finalColor);
        #endif
        
        #ifdef POI_RIM
            applyRimColor(finalColor);
        #endif
        
        #ifdef POI_FLIPBOOK
            applyFlipbook(finalColor);
        #endif
        
        #ifdef POI_DEPTH_COLOR
            applyDepthColor(finalColor, finalEmission, poiCam.screenPos, poiCam.clipPos);
        #endif
        
        #ifdef POI_IRIDESCENCE
            UNITY_BRANCH
            if (_IridescenceTime == 1)
            {
                IridescenceEmission = applyIridescence(finalColor);
            }
        #endif
        
        float4 finalColorBeforeLighting = finalColor;
        
        
        
        
        #ifdef POI_SPECULAR
            finalSpecular0 = calculateSpecular0(finalColorBeforeLighting);
            finalSpecular1 = calculateSpecular1(finalColorBeforeLighting);
        #endif
        
        #ifdef POI_PARALLAX
            calculateAndApplyInternalParallax();
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
            UNITY_BRANCH
            if (_BackFaceEnabled)
            {
                finalEmission += BackFaceColor * _BackFaceEmissionStrength;
            }
            
            #ifdef PANOSPHERE
                applyPanosphereEmission(finalEmission);
            #endif
            
            #ifdef POI_EMISSION
                applyEmission(finalEmission);
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
            #ifdef MATCAP
                applyMatcapEmission(finalEmission);
            #endif
        #endif
        
        #ifdef POI_LIGHTING
            #if(defined(POINT) || defined(SPOT))
                #ifdef POI_METAL
                    applyAdditiveReflectiveLighting(finalColor);
                #endif
            #endif
        #endif
        
        #if defined(TRANSPARENT) && defined(FORWARD_ADD_PASS)
            finalColor.rgb *= finalColor.a;
        #endif
        
        #ifdef POI_LIGHTING
            #ifdef SUBSURFACE
                //applySubsurfaceScattering(finalColor);
                //finalSSS = max(0,getSubsurfaceLighting());
            #endif
        #endif
        
        #ifdef POI_VIDEO
            applyScreenEffect(finalColor, finalColorBeforeLighting);
            finalEmission += globalVideoEmission;
        #endif
        
        
        #ifdef POI_ALPHA_TO_COVERAGE
            ApplyAlphaToCoverage(finalColor);
        #endif
        
        #ifdef CUTOUT
            applyDithering(finalColor);
        #endif
        
        #ifdef POI_METAL
            UNITY_BRANCH
            if (bakedCubemap == 1)
            {
                finalColor.rgb *= lightingAlpha;
                applyReflections(finalColor, finalColorBeforeLighting);
            }
        #endif
        
        finalColor.rgb = finalColor.rgb * finalLighting;
        
        #ifdef POI_METAL
            UNITY_BRANCH
            if(bakedCubemap == 0)
            {
                finalColor.rgb *= lightingAlpha;
                applyReflections(finalColor, finalColorBeforeLighting);
            }
        #endif
        
        finalColor.rgb += finalSpecular0 + finalSpecular1 + finalEnvironmentalRim + finalSSS;
        
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
                meta.SpecularColor = poiLight.color.rgb * _SpecularTint.rgb * lerp(1, albedo.rgb, _SpecularMixAlbedoIntoTint) * _SpecularTint.a;
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
        
        return finalColor;
    }
#endif