#ifndef POI_GLITTER
    #define POI_GLITTER
    
    half3 _GlitterColor;
    float2 _GlitterPan;
    half _GlitterSpeed;
    half _GlitterBrightness;
    float _GlitterFrequency;
    float _GlitterJitter;
    half _GlitterSize;
    half _GlitterContrast;
    half _GlitterAngleRange;
    half _GlitterMinBrightness;
    half _GlitterBias;
    float _GlitterRandomColors;
    float2 _GlitterMinMaxSaturation;
    float2 _GlitterMinMaxBrightness;
    fixed _GlitterUseSurfaceColor;
    float _GlitterBlendType;
    float _GlitterMode;
    float _GlitterShape;
    float _GlitterCenterSize;
    float _glitterFrequencyLinearEmissive;
    float _GlitterJaggyFix;
    float _GlitterRandomRotation;
    float _GlitterTextureRotation;
    float4 _GlitterMinMaxSize;
    float _GlitterRandomSize;
    float2 _GlitterUVPanning;
    
    float _GlitterHueShiftEnabled;
    float _GlitterHueShiftSpeed;
    float _GlitterHueShift;
    float _GlitterHideInShadow;
    
    #if defined(PROP_GLITTERMASK) || !defined(OPTIMIZER_ENABLED)
        POI_TEXTURE_NOSAMPLER(_GlitterMask);
    #endif
    #if defined(PROP_GLITTERCOLORMAP) || !defined(OPTIMIZER_ENABLED)
        POI_TEXTURE_NOSAMPLER(_GlitterColorMap);
    #endif
    #if defined(PROP_GLITTERTEXTURE) || !defined(OPTIMIZER_ENABLED)
        POI_TEXTURE_NOSAMPLER(_GlitterTexture);
    #endif
    
    float3 randomFloat3(float2 Seed, float maximum)
    {
        return(.5 + float3(
            frac(sin(dot(Seed.xy, float2(12.9898, 78.233))) * 43758.5453),
            frac(sin(dot(Seed.yx, float2(12.9898, 78.233))) * 43758.5453),
            frac(sin(dot(float2(Seed), float2(12.9898, 78.233))) * 43758.5453)
        ) * .5) * (maximum);
    }
    
    float3 randomFloat3Range(float2 Seed, float Range)
    {
        return(float3(
            frac(sin(dot(Seed.xy, float2(12.9898, 78.233))) * 43758.5453),
            frac(sin(dot(Seed.yx, float2(12.9898, 78.233))) * 43758.5453),
            frac(sin(dot(float2(Seed.x * Seed.y, Seed.y + Seed.x), float2(12.9898, 78.233))) * 43758.5453)
        ) * 2 - 1) * Range;
    }
    
    float3 randomFloat3WiggleRange(float2 Seed, float Range)
    {
        float3 rando = (float3(
            frac(sin(dot(Seed.xy, float2(12.9898, 78.233))) * 43758.5453),
            frac(sin(dot(Seed.yx, float2(12.9898, 78.233))) * 43758.5453),
            frac(sin(dot(float2(Seed.x * Seed.y, Seed.y + Seed.x), float2(12.9898, 78.233))) * 43758.5453)
        ) * 2 - 1);
        float speed = 1 + _GlitterSpeed;
        return float3(sin((_Time.x + rando.x * pi) * speed), sin((_Time.x + rando.y * pi) * speed), sin((_Time.x + rando.z * pi) * speed)) * Range;
    }
    
    void Unity_RandomRange_float(float2 Seed, float Min, float Max, out float Out)
    {
        float randomno = frac(sin(dot(Seed, float2(12.9898, 78.233))) * 43758.5453);
        Out = lerp(Min, Max, randomno);
    }
    
    float3 RandomColorFromPoint(float2 rando)
    {
        fixed hue = random2(rando.x + rando.y).x;
        fixed saturation = lerp(_GlitterMinMaxSaturation.x, _GlitterMinMaxSaturation.y, rando.x);
        fixed value = lerp(_GlitterMinMaxBrightness.x, _GlitterMinMaxBrightness.y, rando.y);
        float3 hsv = float3(hue, saturation, value);
        return HSVtoRGB(hsv);
    }
    
    void applyGlitter(inout float4 albedo, inout float3 glitterEmission)
    {
        
        
        // Scale
        float2 st = frac(poiMesh.uv[0] + _GlitterUVPanning.xy * _Time.x) * _GlitterFrequency;
        
        // Tile the space
        float2 i_st = floor(st);
        float2 f_st = frac(st);
        
        float m_dist = 10.;  // minimun distance
        float2 m_point = 0;        // minimum point
        float2 randoPoint = 0;
        float2 dank;
        for (int j = -1; j <= 1; j ++)
        {
            for (int i = -1; i <= 1; i ++)
            {
                float2 neighbor = float2(i, j);
                float2 pos = random2(i_st + neighbor);
                float2 rando = pos;
                pos = 0.5 + 0.5 * sin(_GlitterJitter * 6.2831 * pos);
                float2 diff = neighbor + pos - f_st;
                float dist = length(diff);
                
                if (dist < m_dist)
                {
                    dank = diff;
                    m_dist = dist;
                    m_point = pos;
                    randoPoint = rando;
                }
            }
        }
        
        float randomFromPoint = random(randoPoint);
        
        float size = _GlitterSize;
        UNITY_BRANCH
        if(_GlitterRandomSize)
        {
            size = remapClamped(randomFromPoint, 0, 1, _GlitterMinMaxSize.x, _GlitterMinMaxSize.y);
        }
        
        
        // Assign a color using the closest point position
        //color += dot(m_point, float2(.3, .6));
        
        // Add distance field to closest point center
        // color.g = m_dist;
        
        // Show isolines
        //color -= abs(sin(40.0 * m_dist)) * 0.07;
        
        // Draw cell center
        half glitterAlpha = 1;
        switch(_GlitterShape)
        {
            case 0: //circle
            glitterAlpha = (1. - step(size, m_dist));
            break;
            case 1: //sqaure
            float jaggyFix = pow(poiCam.distanceToVert, 2) * _GlitterJaggyFix;
            
            UNITY_BRANCH
            if (_GlitterRandomRotation == 1 || _GlitterTextureRotation != 0)
            {
                float2 center = float2(0, 0);
                float randomBoy = 0;
                UNITY_BRANCH
                if(_GlitterRandomRotation)
                {
                    randomBoy = random(randoPoint);
                }
                float theta = radians((randomBoy + _Time.x * _GlitterTextureRotation) * 360);
                float cs = cos(theta);
                float sn = sin(theta);
                dank = float2((dank.x - center.x) * cs - (dank.y - center.y) * sn + center.x, (dank.x - center.x) * sn + (dank.y - center.y) * cs + center.y);
                glitterAlpha = (1. - smoothstep(size - .1 * jaggyFix, size, abs(dank.x))) * (1. - smoothstep(size - .1 * jaggyFix, size, abs(dank.y)));
            }
            else
            {
                glitterAlpha = (1. - smoothstep(size - .1 * jaggyFix, size, abs(dank.x))) * (1. - smoothstep(size - .1 * jaggyFix, size, abs(dank.y)));
            }
            break;
        }
        
        float3 finalGlitter = 0;
        
        switch(_GlitterMode)
        {
            case 0:
            float3 randomRotation = 0;
            UNITY_BRANCH
            if(_GlitterSpeed > 0)
            {
                randomRotation = randomFloat3WiggleRange(randoPoint, _GlitterAngleRange);
            }
            else
            {
                randomRotation = randomFloat3Range(randoPoint, _GlitterAngleRange);
            }
            float3 norm = poiMesh.normals[0];
            
            float3 glitterReflectionDirection = normalize(mul(poiRotationMatrixFromAngles(randomRotation), norm));
            finalGlitter = lerp(0, _GlitterMinBrightness * glitterAlpha, glitterAlpha) + max(pow(saturate(dot(lerp(glitterReflectionDirection, poiCam.viewDir, _GlitterBias), poiCam.viewDir)), _GlitterContrast), 0);
            finalGlitter *= glitterAlpha;
            break;
            case 1:
            float offset = random(randoPoint);
            float brightness = sin((_Time.x + offset) * _GlitterSpeed) * _glitterFrequencyLinearEmissive - (_glitterFrequencyLinearEmissive - 1);
            finalGlitter = max(_GlitterMinBrightness * glitterAlpha, brightness * glitterAlpha * smoothstep(0, 1, 1 - m_dist * _GlitterCenterSize * 10));
            break;
        }
        
        
        half3 glitterColor = _GlitterColor;
        glitterColor *= lerp(1, albedo, _GlitterUseSurfaceColor);
        #if defined(PROP_GLITTERCOLORMAP) || !defined(OPTIMIZER_ENABLED)
            glitterColor *= POI2D_SAMPLER_PAN(_GlitterColorMap, _MainTex, poiMesh.uv[_GlitterColorMapUV], _GlitterColorMapPan).rgb;
        #endif
        float2 uv = remapClamped(dank, -size, size, 0, 1);
        UNITY_BRANCH
        if(_GlitterRandomRotation == 1 || _GlitterTextureRotation != 0 && !_GlitterShape)
        {
            float2 fakeUVCenter = float2(.5, .5);
            float randomBoy = 0;
            UNITY_BRANCH
            if(_GlitterRandomRotation)
            {
                randomBoy = random(randoPoint);
            }
            float theta = radians((randomBoy + _Time.x * _GlitterTextureRotation) * 360);
            float cs = cos(theta);
            float sn = sin(theta);
            uv = float2((uv.x - fakeUVCenter.x) * cs - (uv.y - fakeUVCenter.y) * sn + fakeUVCenter.x, (uv.x - fakeUVCenter.x) * sn + (uv.y - fakeUVCenter.y) * cs + fakeUVCenter.y);
        }
        
        #if defined(PROP_GLITTERTEXTURE) || !defined(OPTIMIZER_ENABLED)
            float4 glitterTexture = POI2D_SAMPLER_PAN(_GlitterTexture, _MainTex, uv, _GlitterTexturePan);
        #else
            float4 glitterTexture = 1;
        #endif
        //float4 glitterTexture = _GlitterTexture.SampleGrad(sampler_MainTex, frac(uv), ddx(uv), ddy(uv));
        glitterColor *= glitterTexture.rgb;
        #if defined(PROP_GLITTERMASK) || !defined(OPTIMIZER_ENABLED)
            float glitterMask = POI2D_SAMPLER_PAN(_GlitterMask, _MainTex, poiMesh.uv[_GlitterMaskUV], _GlitterMaskPan);
        #else
            float glitterMask = 1;
        #endif
        
        glitterMask *= lerp(1, poiLight.rampedLightMap, _GlitterHideInShadow);
        
        #ifdef POI_BLACKLIGHT
            if (_BlackLightMaskGlitter != 4)
            {
                glitterMask *= blackLightMask[_BlackLightMaskGlitter];
            }
        #endif
        
        if(_GlitterRandomColors)
        {
            glitterColor *= RandomColorFromPoint(random2(randoPoint.x + randoPoint.y));
        }
        
        UNITY_BRANCH
        if(_GlitterHueShiftEnabled)
        {
            glitterColor.rgb = hueShift(glitterColor.rgb, _GlitterHueShift + _Time.x * _GlitterHueShiftSpeed);
        }
        
        UNITY_BRANCH
        if(_GlitterBlendType == 1)
        {
            albedo.rgb = lerp(albedo.rgb, finalGlitter * glitterColor * _GlitterBrightness, finalGlitter * glitterTexture.a * glitterMask);
            glitterEmission = finalGlitter * glitterColor * max(0, (_GlitterBrightness - 1) * glitterTexture.a) * glitterMask;
        }
        else
        {
            glitterEmission = finalGlitter * glitterColor * _GlitterBrightness * glitterTexture.a * glitterMask;
        }
    }
    
#endif