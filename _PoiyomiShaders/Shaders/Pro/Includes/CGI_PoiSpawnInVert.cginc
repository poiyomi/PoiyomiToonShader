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
    //sampler2D _SpawnInNoiseVert; float4 _SpawnInNoiseVert_ST;
    
    float calculateGradientValueVert(float3 start, float3 finish, float3 localPos)
    {
        return inverseLerp3(start, finish, localPos);
    }
    
    void applySpawnInVert(inout float4 worldPos, inout float4 localPos, float2 uv)
    {
        UNITY_BRANCH
        if (_EnableScifiSpawnIn)
        {
            float noise = 0;
            float gradient = calculateGradientValueVert(_SpawnInGradientStart, _SpawnInGradientFinish, localPos.xyz);
            float inverseGradient = 1 - gradient;
            float alpha = gradient - _SpawnInAlpha - noise;
            worldPos.xyz += saturate(inverseGradient + _SpawnInAlpha + _SpawnInVertOffsetOffset -1) * float3(0, _SpawnInVertOffset, 0);
            localPos.xyz = mul(unity_WorldToObject, worldPos).xyz;
        }
        //float noise = tex2Dlod(_SpawnInNoise, float4(TRANSFORM_TEX(uv, _SpawnInNoise))).r * _SpawnInAlpha * _SpawnInNoiseIntensity;
    }
    
#endif