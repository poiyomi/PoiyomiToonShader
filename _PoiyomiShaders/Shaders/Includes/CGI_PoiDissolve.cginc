#ifndef POI_DISSOLVE
    #define POI_DISSOLVE
    
    uint _DissolveType;
    float _DissolveEdgeWidth;
    float4 _DissolveEdgeColor;
    sampler2D _DissolveEdgeGradient; float4 _DissolveEdgeGradient_ST;
    float _DissolveEdgeEmission;
    float4 _DissolveTextureColor;
    POI_TEXTURE_NOSAMPLER(_DissolveToTexture);
    POI_TEXTURE_NOSAMPLER(_DissolveNoiseTexture);
    POI_TEXTURE_NOSAMPLER(_DissolveDetailNoise);
    POI_TEXTURE_NOSAMPLER(_DissolveMask);
    float _DissolveMaskInvert;
    float _DissolveAlpha;
    float _ContinuousDissolve;
    float _DissolveDetailStrength;
    float _DissolveEdgeHardness;
    float _DissolveInvertNoise;
    float _DissolveInvertDetailNoise;
    float _DissolveToEmissionStrength;
    // Point to Point
    float _DissolveP2PWorldLocal;
    float _DissolveP2PEdgeLength;
    float4 _DissolveStartPoint;
    float4 _DissolveEndPoint;
    
    // World Dissolve
    uint _DissolveWorldShape;
    float4 _DissolveShapePosition;
    float4 _DissolveShapeRotation;
    float _DissolveShapeScale;
    float _DissolveInvertShape;
    float _DissolveShapeEdgeLength;
    
    // Masking
    uint _DissolveEmissionSide;
    uint _DissolveEmission1Side;
    
    float4 edgeColor;
    float edgeAlpha;
    float dissolveAlpha;
    float4 dissolveToTexture;
    //Globals
    #ifndef POI_SHADOW
        void calculateDissolve()
        {
            float dissolveMask = POI2D_SAMPLER_PAN(_DissolveMask, _MainTex, poiMesh.uv[_DissolveMaskUV], _DissolveMaskPan).r;
            dissolveToTexture = POI2D_SAMPLER_PAN(_DissolveToTexture, _MainTex, poiMesh.uv[_DissolveToTextureUV], _DissolveToTexturePan) * _DissolveTextureColor;
            float dissolveNoiseTexture = POI2D_SAMPLER_PAN(_DissolveNoiseTexture, _MainTex, poiMesh.uv[_DissolveNoiseTextureUV], _DissolveNoiseTexturePan);
            
            #ifdef POI_BLACKLIGHT
                if (_BlackLightMaskDissolve != 4)
                {
                    dissolveMask *= blackLightMask[_BlackLightMaskDissolve];
                }
            #endif
            
            if(_DissolveMaskInvert)
            {
                dissolveMask = 1 - dissolveMask;
            }
            
            float dissolveDetailNoise = POI2D_SAMPLER_PAN(_DissolveDetailNoise, _MainTex, poiMesh.uv[_DissolveDetailNoiseUV], _DissolveDetailNoisePan);
            
            if(_DissolveInvertNoise)
            {
                dissolveNoiseTexture = 1 - dissolveNoiseTexture;
            }
            if(_DissolveInvertDetailNoise)
            {
                dissolveDetailNoise = 1 - dissolveDetailNoise;
            }
            if(_ContinuousDissolve != 0)
            {
                _DissolveAlpha = sin(_Time.y * _ContinuousDissolve) * .5 + .5;
            }
            _DissolveAlpha *= dissolveMask;
            dissolveAlpha = _DissolveAlpha;
            edgeAlpha = 0;
            
            UNITY_BRANCH
            if(_DissolveType == 1) // Basic
            {
                _DissolveAlpha = remap(_DissolveAlpha, 0, 1, -_DissolveEdgeWidth, 1);
                dissolveAlpha = _DissolveAlpha;
                //Adjust detail strength to avoid artifacts
                _DissolveDetailStrength *= smoothstep(1, .99, _DissolveAlpha);
                float noise = saturate(dissolveNoiseTexture - dissolveDetailNoise * _DissolveDetailStrength);
                
                noise = saturate(noise + 0.001);
                //noise = remap(noise, 0, 1, _DissolveEdgeWidth, 1 - _DissolveEdgeWidth);
                dissolveAlpha = dissolveAlpha >= noise;
                edgeAlpha = remapClamped(noise, _DissolveAlpha + _DissolveEdgeWidth, _DissolveAlpha, 0, 1) * (1 - dissolveAlpha);
            }
            else if (_DissolveType == 2) // Point to Point
            {
                float3 direction;
                float3 currentPos;
                float distanceTo = 0;
                direction = normalize(_DissolveEndPoint - _DissolveStartPoint);
                currentPos = lerp(_DissolveStartPoint, _DissolveEndPoint, dissolveAlpha);
                if (_DissolveP2PWorldLocal == 0)
                {
                    distanceTo = dot(poiMesh.localPos - currentPos, direction) - dissolveDetailNoise * _DissolveDetailStrength;
                    edgeAlpha = smoothstep(_DissolveP2PEdgeLength, 0, distanceTo);
                    dissolveAlpha = step(distanceTo, 0);
                    edgeAlpha *= 1 - dissolveAlpha;
                }
                else
                {
                    distanceTo = dot(poiMesh.worldPos - currentPos, direction) - dissolveDetailNoise * _DissolveDetailStrength;
                    edgeAlpha = smoothstep(_DissolveP2PEdgeLength, 0, distanceTo);
                    dissolveAlpha = step(distanceTo, 0);
                    edgeAlpha *= 1 - dissolveAlpha;
                }
            }
            
            albedo = lerp(albedo, dissolveToTexture, dissolveAlpha);
            
            if(_DissolveEdgeWidth)
            {
                edgeColor = tex2D(_DissolveEdgeGradient, TRANSFORM_TEX(float2(edgeAlpha, edgeAlpha), _DissolveEdgeGradient)) * _DissolveEdgeColor;
                albedo.rgb = lerp(albedo.rgb, edgeColor.rgb, remapClamped(edgeAlpha, 0, 1 - _DissolveEdgeHardness, 0, 1));
            }
            
            
            
            
            /*
            UNITY_BRANCH
            if (_Blend != 0)
            {
                float blendNoise = tex2D(_BlendNoiseTexture, TRANSFORM_TEX(uv, _BlendNoiseTexture));
                blendAlpha = _BlendAlpha;
                if(_AutoBlend > 0)
                {
                    blendAlpha = (clamp(sin(_Time.y * _AutoBlendSpeed / _AutoBlendDelay) * (_AutoBlendDelay + 1), -1, 1) + 1) / 2;
                }
                blendAlpha = lerp(saturate((blendNoise - 1) + blendAlpha * 2), step((1-blendAlpha) * 1.001, blendNoise), _Blend - 1);
                
                float4 blendCol = tex2D(_BlendTexture, TRANSFORM_TEX(uv, _BlendTexture)) * _BlendTextureColor;
                diffuse = lerp(diffuse, blendCol, blendAlpha);
                mainTexture.a = lerp(mainTexture.a, blendCol.a, blendAlpha);
            }
            */
        }
        
        void applyDissolveEmission(inout float3 finalEmission)
        {
            finalEmission += lerp(0, dissolveToTexture * _DissolveToEmissionStrength, dissolveAlpha);
            finalEmission += lerp(0, edgeColor.rgb * _DissolveEdgeEmission, remapClamped(edgeAlpha, 0, 1 - _DissolveEdgeHardness, 0, 1));
        }
        
    #endif
    
    float calculateShadowDissolveAlpha(float3 worldPos, float3 localPos, float2 uv)
    {
        float dissolveMask = UNITY_SAMPLE_TEX2D_SAMPLER(_DissolveMask, _MainTex, TRANSFORM_TEX(uv, _DissolveMask)).r;
        dissolveToTexture = UNITY_SAMPLE_TEX2D_SAMPLER(_DissolveToTexture, _MainTex, TRANSFORM_TEX(uv, _DissolveToTexture) + _Time.y * _DissolveToTexturePan) * _DissolveTextureColor;
        float dissolveNoiseTexture = UNITY_SAMPLE_TEX2D_SAMPLER(_DissolveNoiseTexture, _MainTex, TRANSFORM_TEX(uv, _DissolveNoiseTexture) + _Time.y * _DissolveNoiseTexturePan);
        float dissolveDetailNoise = UNITY_SAMPLE_TEX2D_SAMPLER(_DissolveDetailNoise, _MainTex, TRANSFORM_TEX(uv, _DissolveDetailNoise) + _Time.y * _DissolveDetailNoisePan);
        
        if (_DissolveInvertNoise)
        {
            dissolveNoiseTexture = 1 - dissolveNoiseTexture;
        }
        if(_DissolveInvertDetailNoise)
        {
            dissolveDetailNoise = 1 - dissolveDetailNoise;
        }
        if(_ContinuousDissolve != 0)
        {
            _DissolveAlpha = sin(_Time.y * _ContinuousDissolve) * .5 + .5;
        }
        _DissolveAlpha *= dissolveMask;
        dissolveAlpha = _DissolveAlpha;
        edgeAlpha = 0;
        
        UNITY_BRANCH
        if(_DissolveType == 1) // Basic
        {
            _DissolveAlpha = remap(_DissolveAlpha, 0, 1, -_DissolveEdgeWidth, 1);
            dissolveAlpha = _DissolveAlpha;
            //Adjust detail strength to avoid artifacts
            _DissolveDetailStrength *= smoothstep(1, .99, _DissolveAlpha);
            float noise = saturate(dissolveNoiseTexture - dissolveDetailNoise * _DissolveDetailStrength);
            
            noise = saturate(noise + 0.001);
            //noise = remap(noise, 0, 1, _DissolveEdgeWidth, 1 - _DissolveEdgeWidth);
            dissolveAlpha = dissolveAlpha >= noise;
            edgeAlpha = remapClamped(noise, _DissolveAlpha + _DissolveEdgeWidth, _DissolveAlpha, 0, 1) * (1 - dissolveAlpha);
        }
        else if (_DissolveType == 2) // Point to Point
        {
            float3 direction;
            float3 currentPos;
            float distanceTo = 0;
            direction = normalize(_DissolveEndPoint - _DissolveStartPoint);
            currentPos = lerp(_DissolveStartPoint, _DissolveEndPoint, dissolveAlpha);
            if (_DissolveP2PWorldLocal == 0)
            {
                distanceTo = dot(localPos - currentPos, direction) - dissolveDetailNoise * _DissolveDetailStrength;
                dissolveAlpha = step(distanceTo, 0);
            }
            else
            {
                distanceTo = dot(worldPos - currentPos, direction) - dissolveDetailNoise * _DissolveDetailStrength;
                dissolveAlpha = step(distanceTo, 0);
            }
        }
        
        return lerp(1, dissolveToTexture, dissolveAlpha).a;
    }
    
#endif