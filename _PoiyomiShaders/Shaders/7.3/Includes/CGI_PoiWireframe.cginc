#ifndef POI_WIREFRAME
    #define POI_WIREFRAME
    
    UNITY_DECLARE_TEX2D_NOSAMPLER(_WireframeTexture); float4 _WireframeTexture_ST;
    float2 _WireframeTexturePan;
    float _WireframeSmoothing;
    float _WireframeThickness;
    float4 _WireframeColor;
    float _WireframeAlpha;
    float _WireframeEnable;
    float _WireframeWaveEnabled;
    float _WireframeWaveDensity;
    float _WireframeWaveSpeed;
    float _WireframeEdgeOpacity;
    float _WireframeFaceOpacity;
    half _WireframeEmissionAlpha;
    float _WireframeEmissionStrength;
    float _WireframeQuad;
    uint _WireframeUV;
    
    #ifndef POI_SHADOW
        void applyWireframe(inout float3 wireframeEmission, inout float4 albedo)
        {
            UNITY_BRANCH
            if (_WireframeEnable)
            {
                float4 colorMap = UNITY_SAMPLE_TEX2D_SAMPLER(_WireframeTexture, _MainTex, TRANSFORM_TEX(poiMesh.uv[_WireframeUV], _WireframeTexture) + _Time.x * _WireframeTexturePan);
                float size = _WireframeThickness;
                half3 width = abs(ddx(poiMesh.barycentricCoordinates)) + abs(ddy(poiMesh.barycentricCoordinates));
                half3 eF = smoothstep(0, width * size, poiMesh.barycentricCoordinates);
                half minBary = size > 0 ? min(min(eF.x, eF.y), eF.z): 1;
                
                float4 wireframeColor = _WireframeColor * colorMap;

                albedo.a *= lerp(_WireframeEdgeOpacity, _WireframeFaceOpacity, minBary);
                albedo.rgb = lerp(lerp(albedo.rgb, wireframeColor.rgb, wireframeColor.a), albedo.rgb, minBary);
                wireframeEmission = wireframeColor.rgb * _WireframeEmissionStrength * (1 - minBary) * _WireframeColor.a;
            }
        }
        
        [maxvertexcount(3)]
        void wireframeGeom(triangle v2f IN[3], inout TriangleStream < v2f > tristream)
        {
            UNITY_BRANCH
            if(_WireframeQuad)
            {
                float e1 = length(IN[0].localPos - IN[1].localPos);
                float e2 = length(IN[1].localPos - IN[2].localPos);
                float e3 = length(IN[2].localPos - IN[0].localPos);
                
                float3 quad = 0;
                if(e1 > e2 && e1 > e3)
                    quad.y = 1.;
                else if(e2 > e3 && e2 > e1)
                    quad.x = 1;
                else
                quad.z = 1;
                
                IN[0].barycentricCoordinates = fixed3(1, 0, 0) + quad;
                IN[1].barycentricCoordinates = fixed3(0, 0, 1) + quad;
                IN[2].barycentricCoordinates = fixed3(0, 1, 0) + quad;
            }
            else
            {
                IN[0].barycentricCoordinates = fixed3(1, 0, 0);
                IN[1].barycentricCoordinates = fixed3(0, 1, 0);
                IN[2].barycentricCoordinates = fixed3(0, 0, 1);
            }
            
            
            
            tristream.Append(IN[0]);
            tristream.Append(IN[1]);
            tristream.Append(IN[2]);
        }
    #else
        
        float applyShadowWireframe(float2 uv, float3 barycentricCoordinates, float3 normal, float3 worldPos)
        {
            UNITY_BRANCH
            if(_WireframeEnable)
            {
                float wireframeFadeAlpha = _WireframeAlpha;
                float3 finalWireframeColor = 0;
                
                float3 barys;
                barys.xy = barycentricCoordinates;
                barys.z = 1 - barys.x - barys.y;
                float3 deltas = fwidth(barys);
                float3 smoothing = deltas * _WireframeSmoothing;
                float wireframeThickness = _WireframeThickness;
                float3 thickness = deltas * wireframeThickness;
                barys = smoothstep(thickness, thickness + smoothing, barys);
                float minBary = min(barys.x, min(barys.y, barys.z));
                
                return lerp(_WireframeEdgeOpacity, _WireframeFaceOpacity, minBary);
            }
        }
        
        [maxvertexcount(3)]
        void wireframeGeom(triangle V2FShadow IN[3], inout TriangleStream < V2FShadow > tristream)
        {
            IN[0].barycentricCoordinates = fixed3(1, 0, 0);
            IN[1].barycentricCoordinates = fixed3(0, 1, 0);
            IN[2].barycentricCoordinates = fixed3(0, 0, 1);
            tristream.Append(IN[0]);
            tristream.Append(IN[1]);
            tristream.Append(IN[2]);
        }
    #endif
#endif