#ifndef POI_SPAWN_IN_FRAG
    #define POI_SPAWN_FRAG
    
    #ifndef SPAWN_IN_VARIABLES
        #define SPAWN_IN_VARIABLES
        
        float3 _SpawnInGradientStart;
        float3 _SpawnInGradientFinish;
        fixed _SpawnInAlpha;
        fixed _SpawnInNoiseIntensity;
        float3 _SpawnInEmissionColor;
        float _SpawnInEmissionOffset;
        float _SpawnInVertOffset;
        float _SpawnInVertOffsetOffset;
        float _EnableScifiSpawnIn;
    #endif
    
    UNITY_DECLARE_TEX2D_NOSAMPLER(_SpawnInNoise); float4 _SpawnInNoise_ST;
    
    float calculateGradientValueFrag(float3 start, float3 finish, float3 localPos)
    {
        return inverseLerp3(start, finish, localPos);
    }
    
    void applySpawnIn(inout float4 finalColor, inout float3 spawnInEmission, float2 uv, float3 localPos)
    {
        UNITY_BRANCH
        if (_EnableScifiSpawnIn)
        {
            float noise = UNITY_SAMPLE_TEX2D_SAMPLER(_SpawnInNoise, _MainTex, TRANSFORM_TEX(uv, _SpawnInNoise)).r * _SpawnInAlpha * _SpawnInNoiseIntensity;
            float gradient = calculateGradientValueFrag(_SpawnInGradientStart, _SpawnInGradientFinish, localPos);
            float inverseGradient = 1 - gradient;
            float alpha = gradient - _SpawnInAlpha - noise;
            spawnInEmission = saturate(inverseGradient + _SpawnInAlpha + _SpawnInEmissionOffset +noise - 1) * _SpawnInEmissionColor;
            UNITY_BRANCH
            if(_Mode >= 1)
            {
                clip(ceil(alpha) - 0.001);
            }
        }
    }
    
    void applySpawnInShadow(float2 uv, float3 localPos)
    {
        UNITY_BRANCH
        if(_EnableScifiSpawnIn)
        {
            float noise = UNITY_SAMPLE_TEX2D_SAMPLER(_SpawnInNoise, _MainTex, TRANSFORM_TEX(uv, _SpawnInNoise)).r * _SpawnInAlpha * _SpawnInNoiseIntensity;
            float gradient = calculateGradientValueFrag(_SpawnInGradientStart, _SpawnInGradientFinish, localPos);
            float alpha = gradient - _SpawnInAlpha - noise + length(_SpawnInVertOffset);
            UNITY_BRANCH
            if(_Mode >= 1)
            {
                clip(ceil(alpha) - 0.001);
            }
        }
    }
#endif