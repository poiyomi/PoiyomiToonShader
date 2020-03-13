#ifndef POI_VERTEX_MANIPULATION
    #define POI_VERTEX_MANIPULATION
    
    #include "PoiMath.cginc"
    
    float4 _VertexManipulationLocalTranslation;
    float4 _VertexManipulationLocalRotation;
    float4 _VertexManipulationLocalScale;
    float4 _VertexManipulationWorldTranslation;
    
    float _VertexManipulationHeight;
    float _VertexManipulationHeightBias;
    sampler2D _VertexManipulationHeightMask; float4 _VertexManipulationHeightMask_ST;
    float2 _VertexManipulationHeightPan;
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
        #ifndef SIMPLE
            float3 heightOffset = (tex2Dlod(_VertexManipulationHeightMask, float4(TRANSFORM_TEX(uv, _VertexManipulationHeightMask) + _VertexManipulationHeightPan * _Time.x, 0, 0)).r - _VertexManipulationHeightBias) * _VertexManipulationHeight * worldNormal;
            worldPos.rgb += _VertexManipulationWorldTranslation.xyz * _VertexManipulationWorldTranslation.w + heightOffset;
            localPos.xyz = mul(unity_WorldToObject, worldPos);
        #endif
    }
    
    void applyWorldVertexTransformationShadow(inout float4 worldPos, inout float4 localPos, float3 worldNormal, float2 uv)
    {
        #ifndef SIMPLE
            float3 heightOffset = (tex2Dlod(_VertexManipulationHeightMask, float4(TRANSFORM_TEX(uv, _VertexManipulationHeightMask) + _VertexManipulationHeightPan * _Time.x, 0, 0)).r - _VertexManipulationHeightBias) * _VertexManipulationHeight * worldNormal;
            worldPos.rgb += _VertexManipulationWorldTranslation.xyz * _VertexManipulationWorldTranslation.w + heightOffset;
            localPos.xyz = mul(unity_WorldToObject, worldPos);
        #endif
    }
#endif
//