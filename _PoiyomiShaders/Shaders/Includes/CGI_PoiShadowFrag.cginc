#ifndef SHADOW_FRAG
    #define SHADOW_FRAG
    
    float2 _MainDistanceFade;
    float _ForceOpaque;
    float _MainShadowClipMod;
    float2 _AlphaMaskPan;
    uint _AlphaMaskUV;
    float _AlphaMod;
    
    #ifdef TRANSPARENT
        sampler3D _DitherMaskLOD;
    #endif
    
    half4 fragShadowCaster(
        #if !defined(V2F_SHADOW_CASTER_NOPOS_IS_EMPTY) || defined(UNITY_STANDARD_USE_SHADOW_UVS)
            V2FShadow i
            #endif
        ): SV_Target
        {
            _Clip = clamp(_Clip + _MainShadowClipMod, - .001, 1.001);
            float2 uv[4] = {
                i.uv, i.uv1, i.uv2, i.uv3,
            };
            
            #ifdef POI_MIRROR
                applyMirrorRenderFrag();
            #endif
            
            #if defined(UNITY_STANDARD_USE_SHADOW_UVS)
                
                half alpha = UNITY_SAMPLE_TEX2D(_MainTex, TRANSFORM_TEX(i.uv, _MainTex)).a;
                
                
                
                UNITY_BRANCH
                if (_EnableMirrorTexture)
                {
                    if(IsInMirror())
                    {
                        alpha = UNITY_SAMPLE_TEX2D_SAMPLER(_MirrorTexture, _MainTex, TRANSFORM_TEX(i.uv, _MirrorTexture)).a;
                    }
                }
                
                
                alpha *= smoothstep(_MainDistanceFade.x, _MainDistanceFade.y, distance(i.modelPos, _WorldSpaceCameraPos));
                half alphaMask = POI2D_PAN(_AlphaMask, uv[_AlphaMaskUV], _AlphaMaskPan);
                alpha *= alphaMask;
                alpha *= _Color.a;
                alpha += _AlphaMod;
                alpha = saturate(alpha);
                
                #ifdef OPAQUE
                    alpha = 1;
                #endif
                
                clip(alpha - 0.01);
                
                #if defined(CUTOUT)
                    applyShadowDithering(alpha, calcScreenUVs(i.grabPos).xy);
                #endif
                
                #ifdef POI_DISSOLVE
                    alpha *= calculateShadowDissolveAlpha(i.worldPos, i.localPos, i.uv);
                #endif
                
                #ifdef POI_RANDOM
                    alpha *= i.angleAlpha;
                #endif
                
                #if defined(CUTOUT) || defined(TRANSPARENT)
                    #ifndef SIMPLE
                        applySpawnInShadow(uv[0], i.localPos);
                    #endif
                    #if defined(POI_FLIPBOOK)
                        alpha *= applyFlipbookAlphaToShadow(uv[_FlipbookTexArrayUV]);
                    #endif
                #endif
                
                #if defined(CUTOUT)
                    #ifndef SIMPLE
                        clip(alpha - _Clip);
                    #endif
                #endif
                
                #if defined(TRANSPARENT)
                    float dither = tex3D(_DitherMaskLOD, float3(i.pos.xy * .25, alpha * 0.9375)).a;
                    clip(dither - 0.01);
                #endif
                
            #endif
            SHADOW_CASTER_FRAGMENT(i)
        }
        
    #endif