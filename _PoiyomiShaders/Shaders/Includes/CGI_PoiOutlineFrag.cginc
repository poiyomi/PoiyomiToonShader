float _OutlineRimLightBlend;
float _OutlineLit;
float _OutlineTintMix;

float4 frag(v2f i, uint facing: SV_IsFrontFace): COLOR
{
    poiMesh.uv[0] = i.uv0.xy;
    poiMesh.uv[1] = i.uv0.zw;
    poiMesh.uv[2] = i.uv1.xy;
    poiMesh.uv[3] = i.uv1.zw;

    #ifdef POI_DATA
        InitData(i, facing);
    #endif
    
    #ifdef POI_MAINTEXTURE
        initTextureData();
    #endif
    #ifdef POI_DATA
        calculateLightingData(i);
    #endif
    fixed4 col = mainTexture;
    float alphaMultiplier = smoothstep(_OutlineFadeDistance.x, _OutlineFadeDistance.y, distance(getCameraPosition(), i.worldPos));
    float OutlineMask = tex2D(_OutlineMask, TRANSFORM_TEX(i.uv0.xy, _OutlineMask) + _Time.x * _OutlineTexturePan.zw).r;
    clip(OutlineMask * _LineWidth - 0.001);
    
    float _alphaMask_tex_var = POI2D_SAMPLER_PAN(_AlphaMask, _MainTex, poiMesh.uv[_AlphaMaskUV], _AlphaMaskPan);
    
    col = col * 0.00000000001 + tex2D(_OutlineTexture, TRANSFORM_TEX((i.uv0.xy + (_OutlineTexturePan.xy * _Time.g)), _OutlineTexture));
    col.a *= albedo.a;
    col.a *= alphaMultiplier;
    
    #ifdef POI_RANDOM
        col.a *= i.angleAlpha;
    #endif
    
    poiCam.screenUV = calcScreenUVs(i.grabPos);
    col.a *= _alphaMask_tex_var * _LineColor.a;
    applyDithering(col);
    clip(col.a - _Clip);
    
    #ifdef POI_MIRROR
        applyMirrorRenderFrag();
    #endif
    
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
    col.rgb *= _LineColor.rgb;
    
    if(_OutlineMode == 1)
    {
        col.rgb = lerp(col.rgb, mainTexture.rgb, _OutlineTintMix);
    }
    
    float4 finalColor = col;
    
    #ifdef POI_LIGHTING
        UNITY_BRANCH
        if(_OutlineLit)
        {
            finalColor.rgb *= calculateLighting(finalColor.rgb);
        }
    #endif
    finalColor.rgb += (col.rgb * _OutlineEmission);
    return finalColor;
}