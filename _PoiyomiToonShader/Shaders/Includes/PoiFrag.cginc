#ifndef POIFRAG
    #define POIFRAG
    
    float4 frag(v2f i, float facing: VFACE): SV_Target
    {
        
        #ifdef POI_PARALLAX
            calculateandApplyParallax(i);
        #endif
        
        #ifdef POI_DATA
            calculateData(i);
        #endif
        
        //return float4(poiMesh.vertexNormal,1);
        
        #ifdef BASICS
            calculateBasics(i);
        #endif

        #ifdef LIGHTING
            calculateLighting(i);
        #endif
        
        #ifdef DND_LIGHTING
            calculateDNDLighting(i);
        #endif
        
        
        #ifdef FORWARD_BASE_PASS
            #ifdef REFRACTION
                calculateRefraction(i);
            #endif
        #endif
        
        
        #ifdef METAL
            calculateReflections(i.uv, poiMesh.fragmentNormal, poiCam.viewDir);
        #endif
        
        #ifdef TEXTURE_BLENDING
            calculateTextureBlending(blendAlpha, mainTexture, albedo, i.uv);
        #endif
        
        clip(mainTexture.a * alphaMask - _Clip);
        
        #ifdef MATCAP
            calculateMatcap(poiCam.viewDir, poiMesh.fragmentNormal, i.uv);
        #endif
        
        #ifdef FLIPBOOK
            calculateFlipbook(i.uv);
        #endif
        
        #ifdef LIGHTING
            #ifdef SUBSURFACE
                calculateSubsurfaceScattering(i);
            #endif
        #endif
        
        #ifdef RIM_LIGHTING
            calculateRimLighting(i.uv, poiCam.viewDotNormal);
        #endif
        
        #ifdef PANOSPHERE
            calculatePanosphere(i.worldPos, i.uv);
        #endif
        
        #ifdef SCROLLING_LAYERS
            calculateScrollingLayers(i.uv);
        #endif
        
        #ifdef EMISSION
            calculateEmission(i.uv, i.localPos);
        #endif
        
        float4 finalColor = albedo;
        
        #ifdef RIM_LIGHTING
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
        
        #ifdef FORWARD_BASE_PASS
            #ifdef REFRACTION
                applyRefraction(finalColor);
            #endif
        #endif
        
        float4 finalColorBeforeLighting = finalColor;
        
        #ifdef LIGHTING
            applyLighting(finalColor);
        #endif
        
        #ifdef DND_LIGHTING
            applyDNDLighting(finalColor);
        #endif
        
        #ifdef METAL
            applyReflections(finalColor, finalColorBeforeLighting);
        #endif
        
        #ifdef SPECULAR
            calculateSpecular(finalColorBeforeLighting, i.uv);
        #endif
        
        #ifdef FORWARD_BASE_PASS
            #ifdef LIGHTING
                #ifdef SPECULAR
                    //applyLightingToSpecular();
                    applySpecular(finalColor);
                #endif
            #endif
            
            #ifdef PANOSPHERE
                applyPanosphereEmission(finalColor);
            #endif
            
            #ifdef EMISSION
                applyEmission(finalColor);
            #endif
            
            #ifdef RIM_LIGHTING
                ApplyRimEmission(finalColor);
            #endif
        #endif
        
        #ifdef LIGHTING
            #if (defined(POINT) || defined(SPOT))
                #ifdef METAL
                    applyAdditiveReflectiveLighting(finalColor);
                #endif
                #ifdef TRANSPARENT
                    finalColor.rgb *= finalColor.a;
                #endif
                
                #ifdef SPECULAR
                    applySpecular(finalColor);
                #endif
            #endif
        #endif
        
        #ifdef LIGHTING
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
        
        return finalColor;
    }
#endif