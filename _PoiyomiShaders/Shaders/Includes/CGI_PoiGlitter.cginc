#ifndef POI_GLITTER
    #define POI_GLITTER
    
    half3 _GlitterColor;
    POI_TEXTURE_NOSAMPLER(_GlitterMask);
    float2 _GlitterPan;
    POI_TEXTURE_NOSAMPLER(_GlitterColorMap);
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
    uint _GlitterBlendType;
    
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
    
    void applyGlitter(inout float3 finalEmission, inout float4 finalColor)
    {
        
        
        // Scale
        float2 st = poiMesh.uv[0] * _GlitterFrequency;
        
        // Tile the space
        float2 i_st = floor(st);
        float2 f_st = frac(st);
        
        float m_dist = 10.;  // minimun distance
        float2 m_point = 0;        // minimum point
        float2 randoPoint = 0;
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
                    m_dist = dist;
                    m_point = pos;
                    randoPoint = rando;
                }
            }
        }
        
        // Assign a color using the closest point position
        //color += dot(m_point, float2(.3, .6));
        
        // Add distance field to closest point center
        // color.g = m_dist;
        
        // Show isolines
        //color -= abs(sin(40.0 * m_dist)) * 0.07;
        
        // Draw cell center
        half glitterAlpha = (1. - step(_GlitterSize, m_dist));
        float3 randomRotation = 0;
        UNITY_BRANCH
        if (_GlitterSpeed > 0)
        {
            randomRotation = randomFloat3WiggleRange(randoPoint, _GlitterAngleRange);
        }
        else
        {
            randomRotation = randomFloat3Range(randoPoint, _GlitterAngleRange);
        }
        float3 norm = poiMesh.normals[0];
        
        float3 glitterReflectionDirection = normalize(lerp(-poiCam.viewDir, mul(poiRotationMatrixFromAngles(randomRotation), norm), glitterAlpha));
        float3 finalGlitter = lerp(0, _GlitterMinBrightness, glitterAlpha) + max(pow(dot(lerp(glitterReflectionDirection, poiCam.viewDir, _GlitterBias), poiCam.viewDir), _GlitterContrast) * _GlitterBrightness, 0);
        _GlitterColor *= lerp(1, finalColor, _GlitterUseSurfaceColor);
        _GlitterColor *= POI2D_SAMPLER_PAN(_GlitterColorMap, _MainTex, poiMesh.uv[_GlitterColorMapUV], _GlitterColorMapPan);
        
        float glitterMask = POI2D_SAMPLER_PAN(_GlitterMask, _MainTex, poiMesh.uv[_GlitterMaskUV], _GlitterMaskPan);
        
        #ifdef POI_BLACKLIGHT
            if(_BlackLightMaskGlitter != 4)
            {
                glitterMask *= blackLightMask[_BlackLightMaskGlitter];
            }
        #endif
        
        _GlitterColor *= glitterMask;
        
        if(_GlitterRandomColors)
        {
            _GlitterColor *= RandomColorFromPoint(random2(randoPoint.x + randoPoint.y));
        }
        
        UNITY_BRANCH
        if(_GlitterBlendType == 0)
        {
            finalEmission += finalGlitter * _GlitterColor;
        }
        else
        {
            finalColor.rgb = lerp(finalColor.rgb, finalGlitter * _GlitterColor, finalGlitter);
        }
        // Draw grid
        //color.r += step(.98, f_st.x) + step(.98, f_st.y);
    }
    
#endif


/*
float2 F = inoise(poiMesh.uv[0] * _GlitterFrequency, _GlitterJitter) * .7;
float HeightMap = (F.y - F.x) * _GlitterSpeed;
float3 normal = UnityObjectToWorldNormal(float3(0, 0, 1));
normal = HeightToNormal(HeightMap, normal, poiMesh.worldPos);
normal = WorldToTangentNormalfloattor(normal);

//Unity_NormalFromHeight_World((HeightMap), test);
//poiMathMagic((F.y - F.x), test);
//test = (UnpackScaleNormal(float4(test, 1),40));

test = normalize(
    test.x * poiMesh.tangent +
    test.y * poiMesh.binormal +
    test.z * poiMesh.normals[0]
);

//finalColor.rgb = dot(test, -poiCam.viewDir) * _GlitterBrightness;
finalColor.rgb = normal;
*/
/*
half circleGradient;
half3 circleColor;
inoise(poiMesh.uv[0] * _GlitterFrequency, circleGradient, circleColor);
half circle = 1 - pow(max(0.0, abs(circleGradient) * _GlitterSpeed - 1), 2);
float2 P = poiMesh.uv[0] * _GlitterFrequency;
float2 Pi = mod(floor(P), 289.0);
float2 Pf = ceil(P) * .03;
finalColor.rgb = circle * circleColor;

void inoise(float2 P, out half cirlceGradient, out half3 circleColor)
{
    float2 Pi = mod(floor(P), 289.0);
    float2 Pf = frac(P);
    float3 oi = float3(-1.0, 0.0, 1.0);
    float3 of = float3(-0.5, 0.5, 1.5);
    float3 px = Permutation(Pi.x + oi);
    
    float3 p, ox, oy, dx, dy;
    float2 F = 1e6;
    
    for (int i = 0; i < 3; i ++)
    {
        p = Permutation(px[i] + Pi.y + oi); // pi1, pi2, pi3
        ox = frac(p * K) - Ko;
        oy = mod(floor(p * K), 7.0) * K - Ko;
        dx = Pf.x - of[i] + _GlitterJitter * ox;
        dy = Pf.y - of + _GlitterJitter * oy;
        
        float3 d = dx * dx + dy * dy; // di1, di2 and di3, squared
        
        //find the lowest and second lowest distances
        for (int n = 0; n < 3; n ++)
        {
            if (d[n] < F[0])
            {
                F[1] = F[0];
                F[0] = d[n];
            }
            else if(d[n] < F[1])
            {
                F[1] = d[n];
            }
        }
    }
    circleColor = randomFloat3(Pi, 1);
    cirlceGradient = F.x;
}
*/