#ifndef POI_VERTEX_MANIPULATION
    #define POI_VERTEX_MANIPULATION
    
    #include "CGI_PoiMath.cginc"
    
    float4 _VertexManipulationLocalTranslation;
    float4 _VertexManipulationLocalRotation;
    float4 _VertexManipulationLocalScale;
    float4 _VertexManipulationWorldTranslation;
    
    float _VertexManipulationHeight;
    float _VertexManipulationHeightBias;
    sampler2D _VertexManipulationHeightMask; float4 _VertexManipulationHeightMask_ST;
    float2 _VertexManipulationHeightPan;
    
    
    //Vertex Glitching
    float _EnableVertexGlitch;
    sampler2D _VertexGlitchMap;     float4 _VertexGlitchMap_ST;
    float _VertexGlitchThreshold;
    float _VertexGlitchFrequency;
    float _VertexGlitchStrength;
    // Rounding
    float _VertexRoundingDivision;
    float _VertexRoundingEnabled;
    
    void applyLocalVertexTransformation(inout float3 normal, inout float4 tangent, inout float4 vertex)
    {
        #ifndef SIMPLE
            normal = rotate_with_quaternion(normal, _VertexManipulationLocalRotation);
            tangent.xyz = rotate_with_quaternion(tangent.xyz, _VertexManipulationLocalRotation);
            vertex = transform(vertex, _VertexManipulationLocalTranslation, _VertexManipulationLocalRotation, _VertexManipulationLocalScale);
            
            //vertex = float4(vertex.x + sin(_Time.y*1.5 + vertex.y * 50) * .75 * smoothstep( .3, -1, vertex.y), vertex.y, vertex.z + cos(_Time.y*1.5 + vertex.y * 50) * .75 * smoothstep( .3, -1, vertex.y), 1);
        #endif
    }
    
    void applyLocalVertexTransformation(inout float3 normal, inout float4 vertex)
    {
        #ifndef SIMPLE
            normal = rotate_with_quaternion(normal, _VertexManipulationLocalRotation);
            vertex = transform(vertex, _VertexManipulationLocalTranslation, _VertexManipulationLocalRotation, _VertexManipulationLocalScale);
            
            //vertex = float4(vertex.x + sin(_Time.y*1.5 + vertex.y * 50) * .75 * smoothstep( .3, -1, vertex.y), vertex.y, vertex.z + cos(_Time.y*1.5 + vertex.y * 50) * .75 * smoothstep( .3, -1, vertex.y), 1);
        #endif
    }
    
    void applyWorldVertexTransformation(inout float4 worldPos, inout float4 localPos, inout float3 worldNormal, float2 uv)
    {
        float3 heightOffset = (tex2Dlod(_VertexManipulationHeightMask, float4(TRANSFORM_TEX(uv, _VertexManipulationHeightMask) + _VertexManipulationHeightPan * _Time.x, 0, 0)).r - _VertexManipulationHeightBias) * _VertexManipulationHeight * worldNormal;
        worldPos.rgb += _VertexManipulationWorldTranslation.xyz * _VertexManipulationWorldTranslation.w + heightOffset;
        localPos.xyz = mul(unity_WorldToObject, worldPos);
    }
    
    void applyWorldVertexTransformationShadow(inout float4 worldPos, inout float4 localPos, float3 worldNormal, float2 uv)
    {
        float3 heightOffset = (tex2Dlod(_VertexManipulationHeightMask, float4(TRANSFORM_TEX(uv, _VertexManipulationHeightMask) + _VertexManipulationHeightPan * _Time.x, 0, 0)).r - _VertexManipulationHeightBias) * _VertexManipulationHeight * worldNormal;
        worldPos.rgb += _VertexManipulationWorldTranslation.xyz * _VertexManipulationWorldTranslation.w + heightOffset;
        localPos.xyz = mul(unity_WorldToObject, worldPos);
    }
    
    void applyVertexRounding(inout float4 worldPos, inout float4 localPos)
    {
        UNITY_BRANCH
        if (_VertexRoundingEnabled)
        {
            worldPos.xyz = (ceil(worldPos * _VertexRoundingDivision) / _VertexRoundingDivision) - 1 / _VertexRoundingDivision * .5;
            localPos = mul(unity_WorldToObject, worldPos);
        }
    }
    
    void applyVertexGlitching(inout float4 worldPos, inout float4 localPos)
    {
        UNITY_BRANCH
        if(_EnableVertexGlitch)
        {
            float3 forward = getCameraPosition() - mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
            forward.y = 0;
            forward = normalize(forward);
            float3 glitchDirection = normalize(cross(float3(0, 1, 0), forward));
            float glitchAmount = frac(sin(dot(_Time.xy + worldPos.y, float2(12.9898, 78.233))) * 43758.5453123) * 2 - 1;
            /*
            float uvl = worldPos.y * _VertexGlitchDensity + _Time.x * _VertexGlitchMapPanSpeed;
            float uvr = worldPos.y * _VertexGlitchDensity - _Time.x * _VertexGlitchMapPanSpeed;
            float glitchAmountLeft = tex2Dlod(_VertexGlitchMap, float4(uvl, uvl, 0, 0)).r;
            float glitchAmountRight = -tex2Dlod(_VertexGlitchMap, float4(uvr, uvr, 0, 0)).r;
            float glitchAmount = glitchAmountLeft + glitchAmountRight;
            */
            float time = _Time.y * _VertexGlitchFrequency;
            float randomGlitch = (sin(time) + sin(2.2 * time + 5.52) + sin(2.9 * time + 0.93) + sin(4.6 * time + 8.94)) / 4;
            worldPos.xyz += glitchAmount * glitchDirection * (_VertexGlitchStrength * .01) * step(_VertexGlitchThreshold, randomGlitch);
            localPos = mul(unity_WorldToObject, worldPos);
        }
    }
    
#endif
//