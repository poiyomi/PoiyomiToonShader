#ifndef POIFRAG
    #define POIFRAG
    
    float _ForceOpaque;
    float4 _GlobalPanSpeed;
    float _Clip;
    float4 frag(v2f i, float facing: VFACE): SV_Target
    {
        
        i.uv += _GlobalPanSpeed.xy * _Time.x;
        //This has to be first because it modifies the UVs for the rest of the functions
        
        #ifdef POI_DATA
            InitData(i);
        #endif
        
        // This has to happen in init because it alters UV data globally
        #ifdef POI_PARALLAX
            calculateandApplyParallax(i);
        #endif
        
        #ifdef POI_MAINTEXTURE
            initTextureData();
        #endif
        
        #ifdef REFRACTION
            calculateRefraction(i);
        #endif
        
        #ifdef POI_LIGHTING
            calculateLighting();
        #endif
        
        #ifdef POI_METAL
            calculateReflections();
        #endif
        
        #ifdef POI_RANDOM
            mainTexture.a *= i.angleAlpha;
            albedo.a *= i.angleAlpha;
        #endif

        #ifdef POI_DATA
            distanceFade();
        #endif
        
        clip(mainTexture.a * alphaMask - _Clip);
        
        #ifdef MATCAP
            calculateMatcap();
        #endif
        
        #ifdef FLIPBOOK
            calculateFlipbook();
        #endif
        
        #ifdef POI_LIGHTING
            #ifdef SUBSURFACE
                calculateSubsurfaceScattering();
            #endif
        #endif
        
        #ifdef POI_RIM
            calculateRimLighting();
        #endif
        
        #ifdef PANOSPHERE
            calculatePanosphere();
        #endif
        
        #ifdef POI_EMISSION
            calculateEmission();
        #endif
        
        finalColor = albedo;
        
        #ifdef REFRACTION
            applyRefraction(finalColor);
        #endif
        
        #ifdef POI_RIM
            applyRimColor(finalColor);
        #endif
        
        #ifdef MATCAP
            applyMatcap(finalColor);
        #endif
        
        #ifdef PANOSPHERE
            applyPanosphereColor(finalColor);
        #endif
        
        #ifdef FLIPBOOK
            applyFlipbook(finalColor);
        #endif
        
        float4 finalColorBeforeLighting = finalColor;
        
        #ifdef POI_LIGHTING
            applyLighting(finalColor);
        #endif
        
        #ifdef POI_RIM
            applyEnviroRim(finalColor);
        #endif
        
        #ifdef POI_METAL
            applyReflections(finalColor, finalColorBeforeLighting);
        #endif
        
        #ifdef POI_SPECULAR
            calculateSpecular(finalColorBeforeLighting);
        #endif
        
        #ifdef POI_PARALLAX
            calculateAndApplyInternalParallax();
        #endif
        
        #ifdef FORWARD_BASE_PASS
            #ifdef POI_LIGHTING
                #ifdef POI_SPECULAR
                    //applyLightingToSpecular();
                    applySpecular(finalColor);
                #endif
            #endif
            
            #ifdef PANOSPHERE
                applyPanosphereEmission(finalColor);
            #endif
            
            #ifdef POI_EMISSION
                applyEmission(finalColor);
            #endif
            
            #ifdef POI_DISSOLVE
                applyDissolveEmission(finalColor);
            #endif
            
            #ifdef POI_RIM
                ApplyRimEmission(finalColor);
            #endif
        #endif
        
        #ifdef POI_LIGHTING
            #if (defined(POINT) || defined(SPOT))
                #ifdef POI_METAL
                    applyAdditiveReflectiveLighting(finalColor);
                #endif
                #ifdef TRANSPARENT
                    finalColor.rgb *= finalColor.a;
                #endif
                
                #ifdef POI_SPECULAR
                    applySpecular(finalColor);
                #endif
            #endif
        #endif
        
        #ifdef POI_LIGHTING
            #ifdef SUBSURFACE
                applySubsurfaceScattering(finalColor);
            #endif
        #endif
        
        #ifdef FLIPBOOK
            applyFlipbookEmission(finalColor);
        #endif
        
        #ifdef FORWARD_BASE_PASS
            UNITY_APPLY_FOG(i.fogCoord, finalColor);
        #endif
        
        #ifdef POI_DEBUG
            displayDebugInfo(finalColor);
        #endif

        finalColor.a = max(_ForceOpaque, finalColor.a);
        return finalColor;
    }
#endif