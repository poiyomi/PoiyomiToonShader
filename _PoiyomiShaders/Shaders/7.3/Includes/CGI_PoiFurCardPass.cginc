#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

struct appdata
{
    float4 vertex: POSITION;
    float3 normal: NORMAL;
    float4 tangent: TANGENT;
    float2 uv0: TEXCOORD0;
    uint id: SV_VertexID;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2g
{
    float4 pos: SV_POSITION;
    float2 uv0: TEXCOORD0;
    float3 normal: TEXCOORD4;
    float3 tangent: TEXCOORD5;
    uint vid: TEXCOORD6;
    float3 worldPos: TEXCOORD7;
    float3 binormal: TEXCOORD8;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

struct g2f
{
    float4 pos: SV_POSITION;
    float4 uv: TEXCOORD0;
    float3 worldPos: TEXCOORD1;
    float3 normal: TEXCOORD4;
    float3 tangent: TEXCOORD5;
    float startToEndGradient: TEXCOORD6;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

float3 CreateBinormal(float3 normal, float3 tangent, float binormalSign)
{
    return cross(normal, tangent.xyz) *
    (binormalSign * unity_WorldTransformParams.w);
}

v2g furVert(appdata v)
{
    v2g o;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
    o.pos = UnityObjectToClipPos(v.vertex);
    o.normal = UnityObjectToWorldNormal(v.normal);
    o.tangent = UnityObjectToWorldDir(v.tangent.xyz);
    o.binormal = CreateBinormal(o.normal, o.tangent, v.tangent.w);
    o.uv0 = v.uv0.xy;
    o.vid = v.id;
    return o;
}

float random(float2 vids)
{
    return frac(sin(dot(vids, float2(12.9898, 78.2383))) * 43758.5453123);
}

sampler2D _FurCombMap; float4 _FurCombMap_ST;
float _FurWidth;
float _FurCardLength;
float _FurRaised;

#include "PoiMath.cginc"

[maxvertexcount(6)]
void furGeom(triangle v2g IN[3], inout TriangleStream < g2f > tristream)
{
    float randomValueA = random(float2(IN[0].vid + IN[2].vid, IN[2].vid + IN[0].vid));
    float randomValueB = random(float2(IN[1].vid + IN[2].vid, IN[1].vid + IN[0].vid));
    float randomValueC = random(float2(IN[2].vid + IN[2].vid, IN[0].vid + IN[0].vid));
    
    float3 polySideA = IN[1].worldPos.xyz - IN[0].worldPos.xyz;
    float3 polySideB = IN[2].worldPos.xyz - IN[0].worldPos.xyz;
    float3 c = cross(polySideA, polySideB);
    float3 outDir = normalize(c);
    float3 furRootPos = float4(IN[0].worldPos.xyz + (polySideA * randomValueA) + (polySideB * randomValueB), 1);
    
    float3 rootTangent = normalize((IN[0].tangent + IN[1].tangent + IN[2].tangent) * .3333333);
    float3 rootNormal = normalize((IN[0].normal + IN[1].normal + IN[2].normal) * .3333333);
    float3 rootBinormal = normalize((IN[0].binormal + IN[1].binormal + IN[2].binormal) * .3333333);
    float3 furRight = normalize(cross(IN[0].binormal, outDir));
    float2 rootUV = (IN[0].uv0 + IN[1].uv0 + IN[2].uv0) * .333333;
    half3 tangentSpaceNormal = UnpackNormal(tex2Dlod(_FurCombMap, float4(TRANSFORM_TEX(rootUV, _FurCombMap), 0, 0)));
    float3 furForward = normalize(
        tangentSpaceNormal.x * rootTangent +
        tangentSpaceNormal.y * rootBinormal +
        tangentSpaceNormal.z * rootNormal
    );
    
    furForward = (lerp(furForward, outDir, _FurRaised));
    //furForward = rotate_with_quaternion(furForward, float3(_FurRaised,0,0));
    
    g2f o;
    float4 worldPos[4];
    float4 pos[4];
    float4 uv[4];
    uv[0] = float4(0, 0, rootUV);
    uv[1] = float4(0, 1, rootUV);
    uv[2] = float4(1, 1, rootUV);
    uv[3] = float4(1, 0, rootUV);
    
    _FurWidth *= .01;
    _FurCardLength *= .01;
    
    worldPos[3] = mul(unity_WorldToObject, float4(furRootPos + (furRight * _FurWidth) + (furForward * _FurCardLength), 1));
    worldPos[2] = mul(unity_WorldToObject, float4(furRootPos + (furRight * _FurWidth), 1));
    worldPos[1] = mul(unity_WorldToObject, float4(furRootPos + (-furRight * _FurWidth), 1));
    worldPos[0] = mul(unity_WorldToObject, float4(furRootPos + (-furRight * _FurWidth) + (furForward * _FurCardLength), 1));
    
    pos[3] = UnityObjectToClipPos(worldPos[3]);
    pos[2] = UnityObjectToClipPos(worldPos[2]);
    pos[1] = UnityObjectToClipPos(worldPos[1]);
    pos[0] = UnityObjectToClipPos(worldPos[0]);
    
    o.normal = rootNormal;
    o.tangent = furForward;
    o.worldPos = worldPos[0];
    o.pos = pos[0];
    o.uv = uv[0];
    o.startToEndGradient = 1;
    tristream.Append(o);
    o.worldPos = worldPos[1];
    o.pos = pos[1];
    o.uv = uv[1];
    o.startToEndGradient = 0;
    tristream.Append(o);
    o.worldPos = worldPos[2];
    o.pos = pos[2];
    o.uv = uv[2];
    o.startToEndGradient = 0;
    tristream.Append(o);
    
    tristream.RestartStrip();
    o.worldPos = worldPos[2];
    o.pos = pos[2];
    o.uv = uv[2];
    o.startToEndGradient = 0;
    tristream.Append(o);
    o.worldPos = worldPos[3];
    o.pos = pos[3];
    o.uv = uv[3];
    o.startToEndGradient = 1;
    tristream.Append(o);
    o.worldPos = worldPos[0];
    o.pos = pos[0];
    o.uv = uv[0];
    o.startToEndGradient = 1;
    tristream.Append(o);
    
    tristream.RestartStrip();
}

sampler2D _FurCardTexture; float4 _FurCardTexture_ST;  float4 _FurCardTexture_TexelSize;
sampler2D _FurCardAlphaTexture; float4 _FurCardAlphaTexture_ST;  float4 _FurCardAlphaTexture_TexelSize;
sampler2D _FurPattern; float4 _FurPattern_ST;
float _FurClip;
fixed _FurAoStrength;
float CalcMipLevel(float2 uv)
{
    float2 dx = ddx(uv * _FurCardTexture_TexelSize.zw);
    float2 dy = ddy(uv * _FurCardTexture_TexelSize.zw);
    float delta_max_sqr = max(dot(dx, dx), dot(dy, dy));
    
    return max(0.0, 0.5 * log2(delta_max_sqr));
}

#include "CGI_FurLighting.cginc"

void ApplyAlphaToCoverage(inout float4 finalColor, float2 uv)
{
    // rescale alpha by mip level (if not using preserved coverage mip maps)
    finalColor.a *= 1 + max(0, CalcMipLevel(uv)) * .25;
    // rescale alpha by partial derivative
    finalColor.a = (finalColor.a - _FurClip) / fwidth(finalColor.a) + 0.5;
}

fixed4 FurFag(g2f i): SV_Target
{
    float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
    
    UNITY_SETUP_INSTANCE_ID(i);
    float4 finalColor = 1;
    finalColor.rgb = tex2D(_FurPattern, TRANSFORM_TEX(i.uv.zw, _FurPattern)).rgb;
    finalColor.rgb *= tex2D(_FurCardTexture, TRANSFORM_TEX(i.uv.xy, _FurCardTexture)).rgb;
    finalColor.a = tex2D(_FurCardAlphaTexture, TRANSFORM_TEX(i.uv.xy, _FurCardAlphaTexture)).r;
    ApplyAlphaToCoverage(finalColor, i.uv.xy);
    applyFurLighting(finalColor, i.uv, 1 /*attenuation*/, i.normal, viewDir, i.worldPos);
    finalColor.rgb *= lerp(1, i.startToEndGradient, _FurAoStrength);
    return finalColor;
}