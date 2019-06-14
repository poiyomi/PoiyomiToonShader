#ifndef SHADOW_FRAG
    #define SHADOW_FRAG
    
    half4 fragShadowCaster(
        #if !defined(V2F_SHADOW_CASTER_NOPOS_IS_EMPTY) || defined(UNITY_STANDARD_USE_SHADOW_UVS)
            VertexOutputShadowCaster i
            #endif
        ): SV_Target
        {
            #ifdef FUN
                applyFunFrag();
            #endif
            
            #if defined(UNITY_STANDARD_USE_SHADOW_UVS)
                half alpha = tex2D(_MainTex, TRANSFORM_TEX(i.uv, _MainTex)).a * _Color.a;
                half alphaMask = tex2D(_AlphaMask, TRANSFORM_TEX(i.uv, _AlphaMask));
                
                #ifdef CUTOUT
                    clip(alpha * alphaMask - _Clip);
                #endif
                #ifdef TRANSPARENT
                    clip(alpha * alphaMask - 0.01);
                #endif
            #endif
            
            SHADOW_CASTER_FRAGMENT(i)
        }
        
    #endif