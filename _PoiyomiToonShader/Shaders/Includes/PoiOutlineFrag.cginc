float4 frag(v2f i): COLOR
{
    #ifdef POI_DATA
        calculateData(i);
    #endif
    
    fixed4 _main_tex_var = UNITY_SAMPLE_TEX2D(_MainTex, TRANSFORM_TEX(i.uv, _MainTex));
    fixed4 col = _main_tex_var;
    float alphaMultiplier = smoothstep(_OutlineFadeDistance.x, _OutlineFadeDistance.y, distance(getCameraPosition(), i.worldPos));
    clip(_LineWidth - 0.001);
    float _alphaMask_tex_var = UNITY_SAMPLE_TEX2D_SAMPLER(_AlphaMask, _MainTex, TRANSFORM_TEX(i.uv, _AlphaMask));
    col = col * 0.00000000001 + tex2D(_OutlineTexture, TRANSFORM_TEX((i.uv + (_OutlineTexturePan.xy * _Time.g)), _OutlineTexture));
    col.a *= alphaMultiplier;
    
    clip(col.a * _alphaMask_tex_var - _Clip);
    
    col *= _LineColor;
    
    #ifdef LIGHTING
        calculateLighting(i);
    #endif
    
    float4 finalColor = col;
    
    #ifdef LIGHTING
        applyLighting(finalColor);
    #endif
    
    finalColor.rgb += (col.rgb * _OutlineEmission);
    return finalColor;
}