float _OutlineRimLightBlend;
float _OutlineLit;
float4 frag(v2f i): COLOR
{
    
    #ifdef POI_DATA
        InitData(i);
    #endif
    
    #ifdef POI_MAINTEXTURE
        initTextureData();
    #endif
    
    fixed4 col = mainTexture;
    float alphaMultiplier = smoothstep(_OutlineFadeDistance.x, _OutlineFadeDistance.y, distance(getCameraPosition(), i.worldPos));
    clip(_LineWidth - 0.001);
    #ifndef SIMPLE
        float _alphaMask_tex_var = UNITY_SAMPLE_TEX2D_SAMPLER(_AlphaMask, _MainTex, TRANSFORM_TEX(i.uv0.xy, _AlphaMask));
    #else
        float _alphaMask_tex_var = 1;
    #endif
    col = col * 0.00000000001 + tex2D(_OutlineTexture, TRANSFORM_TEX((i.uv0.xy + (_OutlineTexturePan.xy * _Time.g)), _OutlineTexture));
    col.a *= albedo.a;
    col.a *= alphaMultiplier;
    
    #ifdef POI_RANDOM
        col.a *= i.angleAlpha;
    #endif
    
    clip(col.a * _alphaMask_tex_var - _Clip);
    
    UNITY_BRANCH
    if (_OutlineMode == 1)
    {
        #ifdef POI_MIRROR
            applyMirrorTexture();
        #endif
        col.rgb = mainTexture.rgb;
    }
    else if(_OutlineMode == 2)
    {
        col.rgb = lerp(col.rgb, poiLight.color, _OutlineRimLightBlend);
    }
    col *= _LineColor;
    
    
    #ifdef POI_LIGHTING
        UNITY_BRANCH
        if(_OutlineLit)
        {
            calculateLighting();
        }
    #endif
    
    float4 finalColor = col;
    
    #ifdef POI_LIGHTING
        UNITY_BRANCH
        if(_OutlineLit)
        {
            finalColor.rgb *= calculateLighting();
        }
    #endif
    
    finalColor.rgb += (col.rgb * _OutlineEmission);
    return finalColor;
}