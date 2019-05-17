#if !defined(POI_OUTLINE_PASS_INCLUDED)
    #define POI_OUTLINE_PASS_INCLUDED
    
    #include "Lighting.cginc"
    #include "AutoLight.cginc"
    
    
    float _LineWidth;
    float _OutlineEmission;
    float4 _LineColor;
    float4 _Color;
    float _Clip;
    sampler2D _OutlineTexture; float4 _OutlineTexture_ST;
    sampler2D _MainTex; float4 _MainTex_ST;
    sampler2D _AlphaMask; float4 _AlphaMask_ST;
    float4 _OutlineTexturePan;
    sampler2D _AOMap; float4 _AOMap_ST;
    float _AOStrength;
    
    sampler2D _ToonRamp;
    sampler2D _AdditiveRamp;
    float _ForceLightDirection;
    float _ShadowStrength;
    float _ShadowOffset;
    float3 _LightDirection;
    float _ForceShadowStrength;
    float _CastedShadowSmoothing;
    float _MinBrightness;
    float _MaxBrightness;
    float _IndirectContribution;
    float _AttenuationMultiplier;
    
    float _OutlineShadowStrength;
    float4 _OutlineFadeDistance;
    float4 _OutlineGlobalPan;
    
    struct VertexInput
    {
        float4 vertex: POSITION;
        float3 normal: NORMAL;
        float2 texcoord0: TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };
    struct VertexOutput
    {
        float4 pos: SV_POSITION;
        float2 uv: TEXCOORD0;
        float3 normal: TEXCOORD1;
        float3 worldPos: TEXCOORD2;
        UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
        UNITY_SHADOW_COORDS(3)
    };
    
    #include "PoiLighting.cginc"
    
    float3 getCameraPosition()
    {
        #ifdef USING_STEREO_MATRICES
            return lerp(unity_StereoWorldSpaceCameraPos[0], unity_StereoWorldSpaceCameraPos[1], 0.5);
        #endif
        return _WorldSpaceCameraPos;
    }
    
    VertexOutput vert(VertexInput v)
    {
        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
        UNITY_TRANSFER_INSTANCE_ID(v, o);
        
        VertexOutput o = (VertexOutput)0;
        o.uv = v.texcoord0 + _OutlineGlobalPan.xy * _Time.y;
        o.pos = UnityObjectToClipPos(float4(v.vertex.xyz + v.normal * _LineWidth / 10000, 1));
        o.normal = UnityObjectToWorldNormal(v.normal);
        o.worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz + v.normal * _LineWidth / 10000, 1));
        
        UNITY_TRANSFER_SHADOW(o, o.uv);
        return o;
    }
    
    float4 frag(VertexOutput i, float facing: VFACE): COLOR
    {
        float alphaMultiplier = smoothstep(_OutlineFadeDistance.x, _OutlineFadeDistance.y, distance(getCameraPosition(), i.worldPos));
        clip(_LineWidth - 0.001);
        float _alphaMask_tex_var = tex2D(_AlphaMask, TRANSFORM_TEX(i.uv, _AlphaMask));
        fixed4 col = tex2D(_OutlineTexture, TRANSFORM_TEX((i.uv + (_OutlineTexturePan.xy * _Time.g)), _OutlineTexture));
        col.a *= alphaMultiplier;
        
        clip(col.a * _alphaMask_tex_var - _Clip);
        
        col *= _LineColor;
        
        float AOMap = tex2D(_AOMap, TRANSFORM_TEX(i.uv, _AOMap));
        AOMap = lerp(1, AOMap, _AOStrength);
        
        #ifdef LIGHTING
            float3 _flat_lighting_var = float3(0, 0, 0);
            UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos.xyz)
            attenuation = FadeShadows(attenuation, i.worldPos.xyz);
            float3 _light_direction_var = normalize(_LightDirection);
            if (!any(_WorldSpaceLightPos0) == 0 && _ForceLightDirection == 0)
            {
                _light_direction_var = _WorldSpaceLightPos0;
            }
            _flat_lighting_var = clamp(getNewPoiLighting(_light_direction_var, i.normal, _OutlineShadowStrength, attenuation, _AttenuationMultiplier, AOMap), _MinBrightness, _MaxBrightness);
        #endif
        
        float4 finalColor = col;
        
        #ifdef LIGHTING
            finalColor.rgb *= _flat_lighting_var;
        #endif
        
        finalColor.rgb += (col.rgb * _OutlineEmission);
        return finalColor;
    }
    
#endif