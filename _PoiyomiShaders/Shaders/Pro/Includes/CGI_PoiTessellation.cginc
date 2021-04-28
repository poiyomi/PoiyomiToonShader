#ifndef POI_TESSELLATION
    #define POI_TESSELLATION
    
    float _TessellationPhongStrength;
    float _TessellationEdgeLength;
    float _TessellationExtrusionAmount;
    float _TessellationUniform;
    
    struct TessellationControlPoint
    {
        float4 vertex: INTERNALTESSPOS;
        float3 normal: NORMAL;
        float4 tangent: TANGENT;
        float4 color: COLOR;
        float2 uv0: TEXCOORD0;
        float2 uv1: TEXCOORD1;
        float2 uv2: TEXCOORD2;
        float2 uv3: TEXCOORD3;
    };
    
    struct TessellationFactors
    {
        float edge[3]: SV_TessFactor;
        float inside: SV_InsideTessFactor;
    };
    
    TessellationControlPoint poiTessellationVert(appdata v)
    {
        TessellationControlPoint p;
        p.vertex = v.vertex;
        p.normal = v.normal;
        p.tangent = v.tangent;
        p.color = v.color;
        p.uv0 = v.uv0;
        p.uv1 = v.uv1;
        p.uv2 = v.uv2;
        p.uv3 = v.uv3;
        return p;
    }
    
    float TessellationEdgeFactor(float3 p0, float3 p1)
    {
        #ifndef _FADING_ON
            float edgeLength = distance(p0, p1);
            
            float3 edgeCenter = (p0 + p1) * 0.5;
            float viewDistance = distance(edgeCenter, _WorldSpaceCameraPos);
            
            return edgeLength * _ScreenParams.y /
            (_TessellationEdgeLength * viewDistance);
        #else
            return _TessellationUniform;
        #endif
    }
    
    TessellationFactors poiPatchConst(
        InputPatch < TessellationControlPoint, 3 > patch
    )
    {
        
        TessellationFactors f;
        float3 p0 = mul(unity_ObjectToWorld, patch[0].vertex).xyz;
        float3 p1 = mul(unity_ObjectToWorld, patch[1].vertex).xyz;
        float3 p2 = mul(unity_ObjectToWorld, patch[2].vertex).xyz;
        f.edge[0] = TessellationEdgeFactor(p1, p2);
        f.edge[1] = TessellationEdgeFactor(p2, p0);
        f.edge[2] = TessellationEdgeFactor(p0, p1);
        f.inside = (TessellationEdgeFactor(p1, p2) +
        TessellationEdgeFactor(p2, p0) +
        TessellationEdgeFactor(p0, p1)) * (1 / 3.0);
        return f;
    }
    
    [UNITY_domain("tri")]
    [UNITY_outputcontrolpoints(3)]
    [UNITY_outputtopology("triangle_cw")]
    [UNITY_partitioning("fractional_odd")]
    [UNITY_patchconstantfunc("poiPatchConst")]
    TessellationControlPoint poiHull(
        InputPatch < TessellationControlPoint, 3 > patch,
        uint id: SV_OutputControlPointID
    )
    {
        return patch[id];
    }
    
    [UNITY_domain("tri")]
    v2f poiDomain(
        TessellationFactors factors,
        OutputPatch < TessellationControlPoint, 3 > patch,
        float3 barycentricCoordinates: SV_DomainLocation
    )
    {
        appdata data;
        
        #define MY_DOMAIN_PROGRAM_INTERPOLATE(fieldName) data.fieldName = patch[0].fieldName * barycentricCoordinates.x + patch[1].fieldName * barycentricCoordinates.y + patch[2].fieldName * barycentricCoordinates.z;
        
        MY_DOMAIN_PROGRAM_INTERPOLATE(vertex)
        float3 pp[3];
        for (int i = 0; i < 3; ++ i)
        {
            pp[i] = data.vertex.xyz - patch[i].normal * (dot(data.vertex.xyz, patch[i].normal) - dot(patch[i].vertex.xyz, patch[i].normal));
        }
        data.vertex.xyz = _TessellationPhongStrength * (pp[0] * barycentricCoordinates.x + pp[1] * barycentricCoordinates.y + pp[2] * barycentricCoordinates.z) + (1.0f - _TessellationPhongStrength) * data.vertex.xyz;
        MY_DOMAIN_PROGRAM_INTERPOLATE(normal)
        data.vertex.xyz += data.normal.xyz * _TessellationExtrusionAmount;
        MY_DOMAIN_PROGRAM_INTERPOLATE(tangent)
        MY_DOMAIN_PROGRAM_INTERPOLATE(color)
        MY_DOMAIN_PROGRAM_INTERPOLATE(uv0)
        MY_DOMAIN_PROGRAM_INTERPOLATE(uv1)
        MY_DOMAIN_PROGRAM_INTERPOLATE(uv2)
        MY_DOMAIN_PROGRAM_INTERPOLATE(uv3)
        
        return vert(data);
    }
    
#endif