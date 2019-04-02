
#if !defined(MY_LIGHTING_INCLUDED)
    #define MY_LIGHTING_INCLUDED

    #include "UnityCG.cginc"
    #include "Lighting.cginc"
    #include "UnityPBSLighting.cginc"
    #include "AutoLight.cginc"
    #include "PoiHelpers.cginc"

    //Structs
    struct appdata
    {
        float4 vertex: POSITION;
        float3 normal: NORMAL;
        float4 tangent: TANGENT;
        float2 texcoord: TEXCOORD0;
        float2 texcoord1: TEXCOORD1;
    };

    struct v2f
    {
        float2 uv: TEXCOORD0;
        float3 normalDir: TEXCOORD4;
        float3 tangentDir: TEXCOORD6;
        float3 bitangentDir: TEXCOORD7;
        float4 pos: SV_POSITION;
        float4 worldPos: TEXCOORD8;
        float4 localPos: TEXCOORD9;
        SHADOW_COORDS(5)
    };

    //Properties
    float4 _Color;
    float _Desaturation;
    sampler2D _MainTex; float4 _MainTex_ST;
    sampler2D _NormalMap; float4 _NormalMap_ST;
    float _NormalIntensity;

    samplerCUBE _CubeMap;
    float _SampleWorld;
    float _AdditiveClearCoat;
    float _PurelyAdditive;
    sampler2D _MetallicMap; float4 _MetallicMap_ST;
    float _Metallic;
    sampler2D _RoughnessMap; float4 _RoughnessMap_ST;
    float _Roughness;

    sampler2D _Matcap;
    sampler2D _MatcapMap; float4 _MatcapMap_ST;
    float4 _MatcapColor;
    float  _MatcapStrength;
    float _ReplaceWithMatcap;
    float _MultiplyMatcap;
    float _AddMatcap;

    sampler2D _SpecularMap; float4 _SpecularMap_ST;
    float _Gloss;
    float4 _EmissionColor;
    sampler2D _EmissionMap; float4 _EmissionMap_ST;
    float _EmissionStrength;

    uniform float4 _EmissiveScroll_Direction;
    float4 _EmissionScrollSpeed;
    uniform float _EmissiveScroll_Width;
    uniform float _EmissiveScroll_Velocity;
    uniform float _EmissiveScroll_Interval;
    uniform float _EmissiveBlink_Min;
    uniform float _EmissiveBlink_Max;
    uniform float _EmissiveBlink_Velocity;
    float _ScrollingEmission;

    sampler2D _Ramp;
    float _ForceLightDirection;
    float _ShadowStrength;
    float _ShadowOffset;
    float3 _LightDirection;
    float _ForceShadowStrength;
    float _MinBrightness;
    float _MaxDirectionalIntensity;
    sampler2D _AdditiveRamp;
    float _FlatOrFullAmbientLighting;

    float4 _SpecularColor;
    float _SpecularBias;
    float _SpecularStrength;
    float _SpecularSize;
    float _HardSpecular;

    float4 _RimLightColor;
    float _RimWidth;
    float _RimStrength;
    float _RimSharpness;
    float _RimLightColorBias;
    float4 _RimTexPanSpeed;
    sampler2D _RimTex; float4 _RimTex_ST;

    float _Clip;

    float3 getCameraPosition()
    {
        #ifdef USING_STEREO_MATRICES
            return lerp(unity_StereoWorldSpaceCameraPos[0], unity_StereoWorldSpaceCameraPos[1], 0.5);
        #endif
        return _WorldSpaceCameraPos;
    }

    float3 getCameraForward()
    {
        #if UNITY_SINGLE_PASS_STEREO
            float3 p1 = mul(unity_StereoCameraToWorld[0], float4(0, 0, 1, 1));
            float3 p2 = mul(unity_StereoCameraToWorld[0], float4(0, 0, 0, 1));
        #else
            float3 p1 = mul(unity_CameraToWorld, float4(0, 0, 1, 1));
            float3 p2 = mul(unity_CameraToWorld, float4(0, 0, 0, 1));
        #endif
        return normalize(p2 - p1);
    }

    v2f vert(appdata v)
    {
        v2f o;
        TANGENT_SPACE_ROTATION;
        o.localPos = v.vertex;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.worldPos = mul(unity_ObjectToWorld, v.vertex);
        o.normalDir = normalize(mul(unity_ObjectToWorld, v.normal));
        o.uv = float4(v.texcoord.xy, 0, 0);
        TRANSFER_SHADOW(i);
        o.tangentDir = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0.0)).xyz);
        o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
        return o;
    }

    float4 frag(v2f i, float facing: VFACE): SV_Target
    {
        float Pi = 3.141592654;
        #ifdef FORWARD_BASE_PASS
            float3 _light_direction_var = normalize(_LightDirection);
            if (!any(_WorldSpaceLightPos0) == 0 && _ForceLightDirection == 0)
            {

                _light_direction_var = _WorldSpaceLightPos0;
            }
        #else
            #if defined(POINT) || defined(SPOT)
                float3 _light_direction_var = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);
            #elif defined(DIRECTIONAL)
                return 0;
                float3 _light_direction_var = _WorldSpaceLightPos0;
            #endif
        #endif
        
        // diffuse
        float4 _main_tex_var = tex2D(_MainTex, TRANSFORM_TEX(i.uv, _MainTex));
        float4 _diffuse_var = float4(lerp(_main_tex_var.rgb, dot(_main_tex_var.rgb, float3(0.3, 0.59, 0.11)), _Desaturation) * _Color.rgb, _main_tex_var.a * _Color.a);
        // cutout
        #ifndef TRANSPARENT
            clip(_diffuse_var.a - _Clip);
        #endif
        
        // math
        float3 _camera_to_vert_var = normalize(getCameraPosition() - i.worldPos);
        float3 _camera_to_vert_vr_var = normalize(_WorldSpaceCameraPos - i.worldPos);
        float3x3 tangentTransform = float3x3(i.tangentDir, i.bitangentDir, i.normalDir);
        float3 normal = UnpackNormal(tex2D(_NormalMap, TRANSFORM_TEX(i.uv, _NormalMap)));
        float3 normalLocal = lerp(float3(0, 0, 1), normal, _NormalIntensity);
        float3 _normal_var = normalize(mul(normalLocal, tangentTransform));
        float3 _camera_vert_dot_var = abs(dot(_camera_to_vert_var, _normal_var));
        
        // metal
        float _metallic_map_var = tex2D(_MetallicMap, TRANSFORM_TEX(i.uv, _MetallicMap));
        float _final_metalic_var = _metallic_map_var * _Metallic;
        float _roughness_map_var = tex2D(_RoughnessMap, TRANSFORM_TEX(i.uv, _RoughnessMap));
        float roughness = (1 - _final_metalic_var * _Roughness * _roughness_map_var);
        roughness *= 1.7 - 0.7 * roughness;
        float3 reflectedDir = reflect(-_camera_to_vert_vr_var, _normal_var);
        float3 reflection = float3(0, 0, 0);

        float4 envSample = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflectedDir, roughness * UNITY_SPECCUBE_LOD_STEPS);
        //reflection = DecodeHDR(envSample, unity_SpecCube0_HDR);
        
        float interpolator = unity_SpecCube0_BoxMin.w;
        UNITY_BRANCH
        if (interpolator < 0.99999)
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
        
        // matcap / spehere textures
        half2 matcapUV = getMatcapUV(_camera_to_vert_var, _normal_var);
        float _matcapMap_var = tex2D(_MatcapMap, TRANSFORM_TEX(i.uv, _MatcapMap));
        float3 _matcap_var = tex2D(_Matcap, matcapUV) * _MatcapColor * _MatcapStrength;

        //rim lighting
        float4 rimColor = tex2D(_RimTex, TRANSFORM_TEX(i.uv, _RimTex) + (_Time.y * _RimTexPanSpeed.xy)) * _RimLightColor;
        float rim = pow((1 - _camera_vert_dot_var), (1 - _RimWidth) * 10);
        _RimSharpness /= 2;
        rim = (smoothstep(_RimSharpness, 1 - _RimSharpness, rim));

        // lighting
        UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos.xyz);
        float nDotL = dot(_normal_var, _light_direction_var);
        float FakeLight = clamp((nDotL + 1) / 2 + _ShadowOffset, 0, 1);
        float4 LightingRamp = tex2D(_Ramp, float2(FakeLight, FakeLight));
        #if defined(FORWARD_BASE_PASS)
            //return float4(ShadeSH9(half4(0.0, 0.0, 0.0, 1.0)),1);
            float3 _flat_lighting_var = 1;
            float3 ambient = ShadeSH9(float4(_normal_var * _FlatOrFullAmbientLighting, 1));
            if (any(_LightColor0.rgb))
            {
                float4 lightZero = min(_LightColor0, _MaxDirectionalIntensity);

                if(_ForceShadowStrength == 0)
                {
                    _flat_lighting_var = clamp(ambient + lightZero.rgb * lerp(1, LightingRamp, _ShadowStrength), _MinBrightness, max(lightZero.rgb, ambient));
                }
                else
                {
                    _flat_lighting_var = clamp((ambient + lightZero.rgb) * lerp(1, LightingRamp, _ShadowStrength), _MinBrightness, max(lightZero.rgb, ambient));
                }
            }
            else
            {
                _flat_lighting_var = max(clamp(ambient + ambient * lerp(1, LightingRamp, _ShadowStrength) - ambient * (_ShadowStrength * lerp(.75, 1, _ForceShadowStrength)), 0, ambient), _MinBrightness);
            }
            //return float4(_flat_lighting_var, 1);
        #else
            float3 _flat_lighting_var = _LightColor0.rgb * attenuation * tex2D(_AdditiveRamp, .5 * nDotL + .5);
        #endif

        // emission
        float4 _Emissive_Tex_var = tex2D(_EmissionMap, TRANSFORM_TEX(i.uv, _EmissionMap) + _Time.y * _EmissionScrollSpeed);
        ///
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
        
        // add it all up
        float4 finalColor = _diffuse_var;

        float3 _rim_color_var = lerp(finalColor.rgb, rimColor, _RimLightColorBias);

        finalColor.rgb = lerp(finalColor.rgb, _rim_color_var, rim * _RimLightColor.a * rimColor.a);


        finalColor.rgb = lerp(finalColor, _matcap_var, _ReplaceWithMatcap * _matcapMap_var);
        finalColor.rgb *= lerp(1, _matcap_var, _MultiplyMatcap * _matcapMap_var);
        finalColor.rgb += _matcap_var * _AddMatcap * _matcapMap_var;
        float4 finalColorBeforeLighting = finalColor;

        finalColor.rgb *= _flat_lighting_var;
        
        float3 finalreflections = reflection.rgb * lerp(finalColorBeforeLighting.rgb, 1, _PurelyAdditive);
        finalColor.rgb = finalColor.rgb * lerp((1 - _final_metalic_var), 1, _AdditiveClearCoat);
        finalColor.rgb += (finalreflections * ((1 - roughness + _final_metalic_var) / 2)) * lerp(1, _flat_lighting_var, lighty_boy_uwu_var);

        // specular
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
            _specular_var = step(1 - (.5 * dot(halfDirection, _normal_var) + .5), _SpecularSize) * _SpecularColor * _SpecularBias * specular_map_var;
        }
        else
        {
            _specular_var = pow(max(0, dot(halfDirection, _normal_var)), specPow) * normTerm * specularColor * _SpecularStrength * specular_map_var;
        }

        #if defined(FORWARD_BASE_PASS)
            finalColor.rgb += _specular_var * _flat_lighting_var;
            finalColor.rgb += _emission_var + ((rim * _rim_color_var * _RimStrength) * rimColor.a);
        #else
            finalColor.rgb += _specular_var;
        #endif

        return finalColor;
    }
#endif