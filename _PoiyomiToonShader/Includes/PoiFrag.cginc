#ifndef POIFRAG
    #define POIFRAG
    float4 frag(v2f i, float facing: VFACE): SV_Target
    {
        float Pi = 3.141592654;
        UNITY_SETUP_INSTANCE_ID(i);
        InitializeFragmentNormal(i);
        float3 _camera_to_vert_var = normalize(getCameraPosition() - i.worldPos);
        float3 _camera_to_vert_vr_var = normalize(_WorldSpaceCameraPos - i.worldPos);
        float3 _camera_vert_dot_var = abs(dot(_camera_to_vert_var, i.normal));
        
        #ifdef LIGHTING
            float3 _flat_lighting_var = float3(0, 0, 0);
            UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos.xyz)
            attenuation = FadeShadows(attenuation, i.worldPos.xyz);
            float AOMap = UNITY_SAMPLE_TEX2D_SAMPLER(_AOMap, _MainTex, TRANSFORM_TEX(i.uv, _AOMap));
            AOMap = lerp(1, AOMap, _AOStrength);
            #ifdef FORWARD_BASE_PASS
                float3 _light_direction_var = normalize(_LightDirection);
                if (!any(_WorldSpaceLightPos0) == 0 && _ForceLightDirection == 0)
                {
                    _light_direction_var = _WorldSpaceLightPos0;
                }
                _flat_lighting_var = clamp(getNewPoiLighting(_light_direction_var, i.normal, _ShadowStrength, attenuation, _AttenuationMultiplier, AOMap), _MinBrightness, _MaxBrightness);
            #else
                #if defined(POINT) || defined(SPOT)
                    float3 _light_direction_var = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);
                    float nDotL = dot(i.normal, _light_direction_var);
                    _flat_lighting_var = _LightColor0.rgb * attenuation * tex2D(_AdditiveRamp, .5 * nDotL + .5);
                #elif defined(DIRECTIONAL)
                    return 0;
                    float3 _light_direction_var = _WorldSpaceLightPos0;
                #endif
            #endif
        #endif
        
        // diffuse
        float4 _main_tex_var = UNITY_SAMPLE_TEX2D(_MainTex, TRANSFORM_TEX(i.uv, _MainTex));
        float _alphaMask_tex_var = UNITY_SAMPLE_TEX2D_SAMPLER(_AlphaMask, _MainTex, TRANSFORM_TEX(i.uv, _AlphaMask));
        float4 _diffuse_var = float4(lerp(_main_tex_var.rgb, dot(_main_tex_var.rgb, float3(0.3, 0.59, 0.11)), _Desaturation) * _Color.rgb, _main_tex_var.a * _Color.a * _alphaMask_tex_var);
        
        // Texture Blending
        #ifdef TEXTURE_BLENDING
            UNITY_BRANCH
            if (_Blend != 0)
            {
                float4 blendCol = tex2D(_BlendTexture, TRANSFORM_TEX(i.uv, _BlendTexture)) * _BlendTextureColor;
                float blendNoise = tex2D(_BlendNoiseTexture, TRANSFORM_TEX(i.uv, _BlendNoiseTexture));
                if(_AutoBlend > 0)
                {
                    _BlendAlpha = (clamp(sin(_Time.y * _AutoBlendSpeed / _AutoBlendDelay) * (_AutoBlendDelay + 1), -1, 1) + 1) / 2;
                }
                float blendAlpha = lerp(saturate((blendNoise - 1) + _BlendAlpha * 2), step(_BlendAlpha * 1.001, blendNoise), _Blend - 1);
                _diffuse_var = lerp(_diffuse_var, blendCol, blendAlpha);
                _main_tex_var.a = lerp(_main_tex_var.a, blendCol.a, blendAlpha);
            }
        #endif
        
        // cutout
        clip(_main_tex_var.a * _alphaMask_tex_var - _Clip);
        
        // math
        
        #ifdef METALLIC
            float _metallic_map_var = tex2D(_MetallicMap, TRANSFORM_TEX(i.uv, _MetallicMap)) * _Metallic;
            float _Smoothness_map_var = (tex2D(_SmoothnessMap, TRANSFORM_TEX(i.uv, _SmoothnessMap)));
            if (_InvertSmoothness == 1)
            {
                _Smoothness_map_var = 1 - _Smoothness_map_var;
            }
            _Smoothness_map_var *= _Smoothness;
            float roughness = 1 - _Smoothness_map_var;
            roughness *= 1.7 - 0.7 * roughness;
            float3 reflectedDir = reflect(-_camera_to_vert_vr_var, i.normal);
            float3 reflection = float3(0, 0, 0);
            
            float4 envSample = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflectedDir, roughness * UNITY_SPECCUBE_LOD_STEPS);
            
            float interpolator = unity_SpecCube0_BoxMin.w;
            UNITY_BRANCH
            if(interpolator < 0.99999)
            {
                //Probe 1
                float4 reflectionData0 = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflectedDir, roughness * UNITY_SPECCUBE_LOD_STEPS);
                float3 reflectionColor0 = DecodeHDR(reflectionData0, unity_SpecCube0_HDR);
                
                //Probe 2
                float4 reflectionData1 = UNITY_SAMPLE_TEXCUBE_SAMPLER_LOD(unity_SpecCube1, unity_SpecCube0, reflectedDir, roughness * UNITY_SPECCUBE_LOD_STEPS);
                float3 reflectionColor1 = DecodeHDR(reflectionData1, unity_SpecCube1_HDR);
                
                reflection = lerp(reflectionColor1, reflectionColor0, interpolator);
            }
            else
            {
                float4 reflectionData = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflectedDir, roughness * UNITY_SPECCUBE_LOD_STEPS);
                reflection = DecodeHDR(reflectionData, unity_SpecCube0_HDR);
            }
            
            bool no_probe = unity_SpecCube0_HDR.a == 0 && envSample.a == 0;
                float lighty_boy_uwu_var = 0;
            if (no_probe || _SampleWorld)
            {
                lighty_boy_uwu_var = 1;
                reflection = texCUBElod(_CubeMap, float4(reflectedDir, roughness * UNITY_SPECCUBE_LOD_STEPS));
            }
        #endif
        
        #ifdef MATCAP
            float2 matcapUV = getMatcapUV(_camera_to_vert_vr_var, i.normal);
            float3 _matcap_var = UNITY_SAMPLE_TEX2D_SAMPLER(_Matcap, _MainTex, matcapUV) * _MatcapColor * _MatcapStrength;
            float _matcapMap_var = UNITY_SAMPLE_TEX2D_SAMPLER(_MatcapMap, _MainTex, TRANSFORM_TEX(i.uv, _MatcapMap));
        #endif
        
        #ifdef RIM_LIGHTING
            float4 rimColor = UNITY_SAMPLE_TEX2D_SAMPLER(_RimTex, _MainTex, TRANSFORM_TEX(i.uv, _RimTex) + (_Time.y * _RimTexPanSpeed.xy)) * _RimLightColor;
            float rim = pow((1 - _camera_vert_dot_var), (1 - _RimWidth) * 10);
            _RimSharpness /= 2;
            rim = (smoothstep(_RimSharpness, 1 - _RimSharpness, rim));
        #endif
        
        //return float4(_flat_lighting_var,1);
        #ifdef PANOSPHERE
            float2 _StereoEnabled_var = StereoPanoProjection(normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz) * - 1);
            float3 _pano_var = tex2D(_PanosphereTexture, TRANSFORM_TEX(_StereoEnabled_var, _PanosphereTexture)) * _PanosphereColor.rgb;
            float _panoMap_var = tex2D(_PanoMapTexture, TRANSFORM_TEX(i.uv, _PanoMapTexture));
        #endif
        
        // emission
        #ifdef EMISSION
            float4 _Emissive_Tex_var = tex2D(_EmissionMap, TRANSFORM_TEX(i.uv, _EmissionMap) + _Time.y * _EmissionScrollSpeed);
            float4 _emission_var = _Emissive_Tex_var * _EmissionColor * _EmissionStrength;
            
            // scrolling emission
            if (_ScrollingEmission == 1)
            {
                float phase = dot(i.localPos, _EmissiveScroll_Direction);
                phase -= _Time.y * _EmissiveScroll_Velocity;
                phase /= _EmissiveScroll_Interval;
                phase -= floor(phase);
                float width = _EmissiveScroll_Width;
                phase = (pow(phase, width) + pow(1 - phase, width * 4)) * 0.5;
                _emission_var *= phase;
            }
            
            // blinking emission
            float amplitude = (_EmissiveBlink_Max - _EmissiveBlink_Min) * 0.5f;
            float base = _EmissiveBlink_Min + amplitude;
            float emissiveBlink = sin(_Time.y * _EmissiveBlink_Velocity) * amplitude + base;
            _emission_var *= emissiveBlink;
            
            float _Emission_mask_var = tex2D(_EmissionMask, TRANSFORM_TEX(i.uv, _EmissionMask));
            _emission_var *= _Emission_mask_var;
        #endif
        
        // add it all up
        float4 finalColor = _diffuse_var;
        
        #ifdef RIM_LIGHTING
            float3 _rim_color_var = lerp(finalColor.rgb, rimColor, _RimLightColorBias);
            float rimMask = UNITY_SAMPLE_TEX2D_SAMPLER(_RimMask, _MainTex, TRANSFORM_TEX(i.uv, _RimMask));
            finalColor.rgb = lerp(finalColor.rgb, _rim_color_var, rim * _RimLightColor.a * rimColor.a * rimMask);
        #endif
        
        #ifdef MATCAP
            finalColor.rgb = lerp(finalColor, _matcap_var, _ReplaceWithMatcap * _matcapMap_var);
            finalColor.rgb *= lerp(1, _matcap_var, _MultiplyMatcap * _matcapMap_var);
            finalColor.rgb += _matcap_var * _AddMatcap * _matcapMap_var;
        #endif
        
        #ifdef PANOSPHERE
            finalColor.rgb = lerp(finalColor.rgb, _pano_var, _PanoBlend * _panoMap_var);
        #endif
        
        float4 finalColorBeforeLighting = finalColor;
        
        #ifdef LIGHTING
            finalColor.rgb *= _flat_lighting_var;
        #endif
        
        #ifdef METALLIC
            #ifdef FORWARD_BASE_PASS
                float3 finalreflections = reflection.rgb * lerp(finalColorBeforeLighting.rgb, 1, _PurelyAdditive);
                finalColor.rgb = finalColor.rgb * (1 - _metallic_map_var);
                finalColor.rgb += (finalreflections * ((1 - roughness + _metallic_map_var) / 2)) * lerp(1, _flat_lighting_var, lighty_boy_uwu_var);
            #endif
        #endif
        
        #ifdef SPECULAR
            #if (defined(POINT) || defined(SPOT))
                _SpecularColor.rgb = _LightColor0.rgb;
                _SpecularBias = 0;
            #endif
            float specular_map_var = tex2D(_SpecularMap, TRANSFORM_TEX(i.uv, _SpecularMap));
            float3 specularColor = ((finalColor.a * _SpecularStrength) * lerp(finalColor.rgb * _LightColor0.rgb, _SpecularColor.rgb, _SpecularBias));
            float specPow = exp2(_Gloss * 20.0 + 1.0);
            float normTerm = (specPow + 10) / (10 * Pi);
            float3 halfDirection = normalize(_camera_to_vert_vr_var + _light_direction_var);
            float3 _specular_var = float3(0, 0, 0);
            if(_HardSpecular == 1)
            {
                _specular_var = step(1 - (.5 * dot(halfDirection, i.normal) + .5), _SpecularSize) * _SpecularColor * _SpecularBias * specular_map_var;
            }
            else
            {
                _specular_var = pow(max(0, dot(halfDirection, i.normal)), specPow) * normTerm * specularColor * _SpecularStrength * specular_map_var;
            }
        #endif
        
        #if defined(FORWARD_BASE_PASS)
            #ifdef SPECULAR
                finalColor.rgb += _specular_var * _flat_lighting_var;
            #endif
            
            #ifdef PANOSPHERE
                finalColor.rgb += _pano_var * _PanoBlend * _panoMap_var * _PanoEmission;
            #endif
            
            #ifdef EMISSION
                finalColor.rgb += _emission_var;
            #endif
            
            #ifdef RIM_LIGHTING
                finalColor.rgb += rim * _rim_color_var * _RimStrength * rimColor.a * rimMask;
            #endif
        #else
            #ifdef SPECULAR
                finalColor.rgb += _specular_var;
            #endif
        #endif
        
        #ifdef LIGHTING
            #if(defined(POINT) || defined(SPOT))
                #ifdef METALLIC
                    finalColor *= (1 - _metallic_map_var);
                #endif
                #ifdef TRANSPARENT
                    finalColor.rgb *= finalColor.a;
                #endif
            #endif
        #endif
        return finalColor;
    }
#endif