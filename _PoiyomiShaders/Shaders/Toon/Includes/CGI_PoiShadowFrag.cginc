#ifndef SHADOW_FRAG
    #define SHADOW_FRAG
    
    float2 _MainDistanceFade;
    float _ForceOpaque;
    float _MainShadowClipMod;
    float2 _AlphaMaskPan;
    float _AlphaMaskUV;
    sampler3D _DitherMaskLOD;
    float2 _MainTexPan;
    float _MainTextureUV;
    
    half4 fragShadowCaster(
        #if !defined(V2F_SHADOW_CASTER_NOPOS_IS_EMPTY) || defined(UNITY_STANDARD_USE_SHADOW_UVS)
            V2FShadow i, uint facing: SV_IsFrontFace
            #endif
        ): SV_Target
        {
            poiMesh.uv[0] = i.uv;
            poiMesh.uv[1] = i.uv1;
            poiMesh.uv[2] = i.uv2;
            poiMesh.uv[3] = i.uv3;
            
            // Basically every texture relies on the maintex sampler to function and that's why this is here.
            float4 mainTexture = UNITY_SAMPLE_TEX2D(_MainTex, TRANSFORM_TEX(poiMesh.uv[_MainTextureUV], _MainTex) + _Time.x * _MainTexPan);
            
            
            //Possible Bug with clip
            float clipValue = clamp(_Cutoff + _MainShadowClipMod, - .001, 1.001);
            
            poiMesh.vertexColor = saturate(i.vertexColor);
            poiMesh.worldPos = i.worldPos;
            poiMesh.localPos = i.localPos;
            
            #ifdef POI_MIRROR
                applyMirrorRenderFrag();
            #endif
            
            #if defined(UNITY_STANDARD_USE_SHADOW_UVS)
                
                half4 alpha = mainTexture;
                
                UNITY_BRANCH
                if (_EnableMirrorTexture)
                {
                    if(IsInMirror())
                    {
                        alpha.a = UNITY_SAMPLE_TEX2D_SAMPLER(_MirrorTexture, _MainTex, TRANSFORM_TEX(i.uv, _MirrorTexture)).a;
                    }
                }
                
                
                alpha.a *= smoothstep(_MainDistanceFade.x, _MainDistanceFade.y, distance(i.modelPos, _WorldSpaceCameraPos));
                half alphaMask = POI2D_PAN(_AlphaMask, poiMesh.uv[_AlphaMaskUV], _AlphaMaskPan);
                alpha.a *= alphaMask;
                alpha.a *= _Color.a + .0001;
                alpha.a += _AlphaMod;
                alpha.a = saturate(alpha.a);
                
                UNITY_BRANCH
                if(_Mode == 0)
                {
                    alpha.a = 1;
                }
                
                UNITY_BRANCH
                if(_Mode == 1)
                {
                    applyShadowDithering(alpha.a, calcScreenUVs(i.grabPos).xy);
                }
                
                #ifdef POI_DISSOLVE
                    float3 fakeEmission = 1;
                    calculateDissolve(alpha, fakeEmission);
                #endif
                UNITY_BRANCH
                if(_Mode == 1)
                {
                    clip(alpha.a - 0.001);
                }
                
                /*
                return poiMesh.vertexColor.g;
                
                #ifdef POI_RANDOM
                    alpha.a *= i.angleAlpha;
                #endif
                
                UNITY_BRANCH
                if(_Mode >= 1)
                {
                    applySpawnInShadow(uv[0], i.localPos);
                    
                    #if defined(POI_FLIPBOOK)
                        alpha.a *= applyFlipbookAlphaToShadow(uv[_FlipbookTexArrayUV]);
                    #endif
                }
                */
                UNITY_BRANCH
                if (_Mode == 1)
                {
                    clip(alpha.a - clipValue);
                }
                
                UNITY_BRANCH
                if(_Mode > 1)
                {
                    float dither = tex3D(_DitherMaskLOD, float3(i.pos.xy * .25, alpha.a * 0.9375)).a;
                    clip(dither - 0.01);
                }
                
            #endif
            SHADOW_CASTER_FRAGMENT(i)
        }
        
    #endif