#ifndef SHADOW_FRAG
    #define SHADOW_FRAG
    
    float2 _MainDistanceFade;
    float _ForceOpaque;
    half4 fragShadowCaster(
        #if !defined(V2F_SHADOW_CASTER_NOPOS_IS_EMPTY) || defined(UNITY_STANDARD_USE_SHADOW_UVS)
            VertexOutputShadowCaster i
            #endif
        ): SV_Target
        {
            #ifdef MIRROR
                applyMirrorRenderFrag();
            #endif
            
            #if defined(UNITY_STANDARD_USE_SHADOW_UVS)
                half alpha = UNITY_SAMPLE_TEX2D(_MainTex, TRANSFORM_TEX(i.uv, _MainTex)).a;
                alpha *= smoothstep(_MainDistanceFade.x, _MainDistanceFade.y, distance(i.modelPos, _WorldSpaceCameraPos));
                half alphaMask = tex2D(_AlphaMask, TRANSFORM_TEX(i.uv, _AlphaMask));
                
                #ifdef POI_DISSOLVE
                    alpha *= calculateShadowDissolveAlpha(i.worldPos, i.localPos, i.uv);
                #endif
                
                #ifdef POI_RANDOM
                    alpha *= i.angleAlpha;
                #endif
                
                #if defined(CUTOUT) || defined(OPAQUE) 
                    clip(alpha * alphaMask - _Clip);
                    UNITY_BRANCH
                    if (!_ForceOpaque)
                    {
                        clip(_Color.a - .75);
                    }
                #endif
                #ifdef TRANSPARENT
                    clip(alpha * alphaMask * _Color.a - 0.99);
                #endif
            #endif
            SHADOW_CASTER_FRAGMENT(i)
        }
        
    #endif