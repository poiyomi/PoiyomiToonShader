#if !defined(POI_OUTLINE_PASS_INCLUDED)
    #define POI_OUTLINE_PASS_INCLUDED
    
    #include "Lighting.cginc"
    #include "AutoLight.cginc"
    
    uniform float _LineWidth;
    uniform float _OutlineEmission;
    uniform float4 _LineColor;
    uniform float4 _Color;
    uniform float _Clip;
    uniform sampler2D _OutlineTexture; uniform float4 _OutlineTexture_ST;
    uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
    uniform float4 _OutlineTexturePan;
    
    struct VertexInput
    {
        float4 vertex: POSITION;
        float3 normal: NORMAL;
        float2 texcoord0: TEXCOORD0;
    };
    struct VertexOutput
    {
        float4 pos: SV_POSITION;
        float2 uv: TEXCOORD0;
    };
    
    VertexOutput vert(VertexInput v)
    {
        VertexOutput o = (VertexOutput)0;
        o.uv = v.texcoord0;
        o.pos = UnityObjectToClipPos(float4(v.vertex.xyz + v.normal * _LineWidth / 10000, 1));
        return o;
    }
    
    float4 frag(VertexOutput i, float facing: VFACE): COLOR
    {
        clip(_LineWidth - 0.001);
        #if defined(TRANSPARENT)
            fixed4 col = tex2D(_OutlineTexture, TRANSFORM_TEX((i.uv + (_OutlineTexturePan.xy * _Time.g)), _OutlineTexture)) * _LineColor;
        #else
            fixed4 col = fixed4(tex2D(_OutlineTexture, TRANSFORM_TEX((i.uv + (_OutlineTexturePan.xy * _Time.g)), _OutlineTexture)).rgb, 0) * _LineColor;
        #endif
        float attenuation = LIGHT_ATTENUATION(i) / SHADOW_ATTENUATION(i);
        float3 _flat_lighting_var = saturate(ShadeSH9(half4(float3(0, 1, 0), 1.0)) + (_LightColor0.rgb * attenuation));
        float4 finalColor = col;
        finalColor.rgb *= _flat_lighting_var;
        finalColor.rgb += (col.rgb * _OutlineEmission);
        return finalColor;
    }
    
#endif