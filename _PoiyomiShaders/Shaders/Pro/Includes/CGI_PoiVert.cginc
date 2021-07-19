#ifndef POI_VERT
#define POI_VERT

float _VertexManipulationHeightUV;
float _VertexUnwrap;

#define PM UNITY_MATRIX_P

inline float4 CalculateFrustumCorrection()
{
	float x1 = -PM._31 / (PM._11 * PM._34);
	float x2 = -PM._32 / (PM._22 * PM._34);
	return float4(x1, x2, 0, PM._33 / PM._34 + x1 * PM._13 + x2 * PM._23);
}

float3 CreateBinormal(half3 normal, half3 tangent, half tangentSign)
{
	half sign = tangentSign * unity_WorldTransformParams.w;
	return cross(normal, tangent) * sign;
}

v2f vert(appdata v)
{
	UNITY_SETUP_INSTANCE_ID(v);
	v2f o;
	
	#ifdef _COLOROVERLAY_ON
		v.vertex.xyz = lerp(v.vertex.xyz, float3(v.uv0.x - .5, v.uv0.y - .5, 0), _VertexUnwrap);
	#endif

	#ifdef RALIV_PENETRATION
		applyRalivDynamicOrifaceSystem(v);
	#endif

	#ifdef AUTO_EXPOSURE
		applyLocalVertexTransformation(v.normal, v.tangent, v.vertex);
	#endif
	

	UNITY_INITIALIZE_OUTPUT(v2f, o);
	UNITY_TRANSFER_INSTANCE_ID(v, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	
	
	#ifdef _REQUIRE_UV2 //POI_MIRROR
		applyMirrorRenderVert(v.vertex);
	#endif
	
	TANGENT_SPACE_ROTATION;
	o.localPos = v.vertex;
	o.worldPos = mul(unity_ObjectToWorld, o.localPos);
	o.normal = UnityObjectToWorldNormal(v.normal);
	
	#ifdef RALIV_PENETRATION
		applyRalivDynamicPenetrationSystem(o.localPos.rgb, o.normal.rgb, v);
	#endif
	
	//o.localPos.x *= -1;
	//o.localPos.xz += sin(o.localPos.y * 100 + _Time.y * 5) * .0025;
	
	float2 uvToUse = 0;
	UNITY_BRANCH
	if (_VertexManipulationHeightUV == 0)
	{
		uvToUse = v.uv0.xy;
	}
	UNITY_BRANCH
	if (_VertexManipulationHeightUV == 1)
	{
		uvToUse = v.uv1.xy;
	}
	UNITY_BRANCH
	if (_VertexManipulationHeightUV == 2)
	{
		uvToUse = v.uv2.xy;
	}
	UNITY_BRANCH
	if (_VertexManipulationHeightUV == 3)
	{
		uvToUse = v.uv3.xy;
	}
	#ifdef AUTO_EXPOSURE
		applyWorldVertexTransformation(o.worldPos, o.localPos, o.normal, uvToUse);
	#endif
	applyVertexGlitching(o.worldPos, o.localPos);
	applySpawnInVert(o.worldPos, o.localPos, v.uv0.xy);
	#ifdef AUTO_EXPOSURE
		applyVertexRounding(o.worldPos, o.localPos);
	#endif
	o.pos = UnityObjectToClipPos(o.localPos);
	o.grabPos = ComputeGrabScreenPos(o.pos);
	o.uv0.xy = v.uv0.xy;
	o.uv0.zw = v.uv1.xy;
	o.uv1.xy = v.uv2.xy;
	o.uv1.zw = v.uv3.xy;
	o.vertexColor = v.color;
	o.modelPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
	o.tangent = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
	
    o.binormal.rgb = CreateBinormal(o.normal.xyz, o.tangent.xyz, o.tangent.w);

	#ifdef POI_BULGE
		bulgyWolgy(o);
	#endif
	
	
	o.angleAlpha = 1;
	#ifdef _SUNDISK_NONE //POI_RANDOM
		o.angleAlpha = ApplyAngleBasedRendering(o.modelPos, o.worldPos);
	#endif
	
	#if defined(LIGHTMAP_ON)
		o.lightmapUV.xy = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
	#endif
	#ifdef DYNAMICLIGHTMAP_ON
		o.lightmapUV.zw = v.uv2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
	#endif
	
	UNITY_TRANSFER_SHADOW(o, o.uv0.xy);
	UNITY_TRANSFER_FOG(o, o.pos);
	
	v.tangent.xyz = normalize(v.tangent.xyz);
	v.normal = normalize(v.normal);
	float3x3 objectToTangent = float3x3(
		v.tangent.xyz,
		cross(v.normal, v.tangent.xyz) * v.tangent.w,
		v.normal
	);
	o.tangentViewDir = mul(objectToTangent, ObjSpaceViewDir(v.vertex));
	
	#ifdef POI_META_PASS
		o.pos = UnityMetaVertexPosition(v.vertex, v.uv1.xy, v.uv2.xy, unity_LightmapST, unity_DynamicLightmapST);
	#endif
	
	#if defined(GRAIN)
		float4 worldDirection;
		
		worldDirection.xyz = o.worldPos.xyz - _WorldSpaceCameraPos;
		worldDirection.w = dot(o.pos, CalculateFrustumCorrection());
		o.worldDirection = worldDirection;
	#endif
	
	return o;
}
#endif