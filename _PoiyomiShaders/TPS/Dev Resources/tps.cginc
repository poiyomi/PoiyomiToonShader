float _TPS_AnimatedToggle;
float _TPS_PenetratorLength;
float3 _TPS_PenetratorScale;
float3 _TPS_PenetratorForward;
float3 _TPS_PenetratorRight;
float3 _TPS_PenetratorUp;
float _TPS_VertexColors;
float _TPS_MinimumOrificeDistance;
float _TPS_BezierStart;
float _TPS_SmoothStart;
float _TPS_BezierSmoothness;
float _TPS_Squeeze;
float _TPS_SqueezeDistance;
float _TPS_Buldge;
float _TPS_BuldgeDistance;
float _TPS_BuldgeFalloffDistance;

float _TPS_PumpingStrength;
float _TPS_PumpingSpeed;
float _TPS_PumpingWidth;

float _TPS_IdleSkrinkWidth;
float _TPS_IdleSkrinkLength;

float _TPS_BufferedDepth;
float _TPS_BufferedStrength;

UNITY_DECLARE_SCREENSPACE_TEXTURE(_TPS_Grabpass);
#ifdef TPS_IsSkinnedMesh
UNITY_DECLARE_TEX2D_NOSAMPLER(_TPS_BakedMesh);
#else
sampler2D _TPS_BakedMesh;
#endif


#define ID_HOLE 0.41
#define ID_RING 0.42
#define ID_NORM 0.45

#define TPS_RECIEVER_DIST 0.01

#define PENETRATORTYPE_RING 1
#define PENETRATORTYPE_HOLE 2

//https://forum.unity.com/threads/point-light-in-v-f-shader.499717/ UGA BUGA, Unity Forums pogggggggg
float LightRange(int i) {
	return (0.005 * sqrt(1000000.0 - unity_4LightAtten0[i])) / sqrt(unity_4LightAtten0[i]);
}

float4 GetLightPositionInObjectSpace(int i) {
	return mul(unity_WorldToObject, float4(unity_4LightPosX0[i], unity_4LightPosY0[i], unity_4LightPosZ0[i], 1));
}

uint4 f32touint8(float4 input)
{
	input.r = LinearToGammaSpaceExact(input.r);
	input.g = LinearToGammaSpaceExact(input.g);
	input.b = LinearToGammaSpaceExact(input.b);
	return round(input * 255);
}

float decodeFloatFromARGB8(float4 rgba)
{
	uint4 u = f32touint8(rgba);
	return asfloat(u.x + (u.y << 8) + (u.z << 16) + (u.w << 24));
}

float decodeFloatFromARGB8File(float4 rgba)
{
	uint4 u = round(rgba * 255);
	return asfloat(u.x + (u.y << 8) + (u.z << 16) + (u.w << 24));
}

#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
	#define SampleGrabpass(tex,uv) UNITY_SAMPLE_TEX2DARRAY_LOD(tex, float3(uv.xy, 0), 0)
#elif UNITY_SINGLE_PASS_STEREO
	#define SampleGrabpass(tex,uv) tex2Dlod(tex, float4(uv.x * 0.5, uv.y,0,0))
#else
	#define SampleGrabpass(tex,uv) tex2Dlod(tex, float4(uv.xy,0,0))
#endif

#define IsLightOrificeType(range,type) (abs(range - type) < 0.001)
#define IsLightAnyOrificeType(range) ((IsLightOrificeType(range,ID_RING)) || (IsLightOrificeType(range,ID_HOLE)))
#define VectorLengthIntoDirection(v,d) dot(v,d)

float FindTPSSystem(inout float3 orificePosition, inout float3 orificeNormal, inout float penetratorType, float penetratorLength) {
	//find lights
	float lightRanges[4];
	float3 lightPositions[4];
	float3 lightPositionsWorld[4];
	[loop] for (int f = 0; f < 4; f++) {
		lightPositions[f] = GetLightPositionInObjectSpace(f);
		lightPositionsWorld[f] = length(mul(unity_ObjectToWorld, float4(lightPositions[f], 1)));
		lightRanges[f] = LightRange(f);
	}
	//orifice hole + type
	float closestOrfDist = 100000000;
	[loop]for (int i = 0; i < 4; i++) {
		if (abs(lightRanges[i] - ID_RING) < 0.001 || abs(lightRanges[i] - ID_HOLE) < 0.001) {
			//Ceck if orifice light is in forward direction
			if (dot(_TPS_PenetratorForward, lightPositions[i]) > 0) {
				//Check if normal exisits
				float3 foundNormal = -_TPS_PenetratorForward;
				bool hasNormal = false;
				[loop] for (int n = 0; n < 4; n++) {
					//orifice normal
					if (IsLightOrificeType(lightRanges[n], ID_NORM) && distance(lightPositionsWorld[n] , lightPositionsWorld[i]) < 0.05f) {
						foundNormal = normalize(lightPositions[n] - lightPositions[i]);
						hasNormal = true;
					}
				}
				//if normal right direction and is cloest orifice
				if (dot(foundNormal, _TPS_PenetratorForward) < 0 && length(lightPositions[i]) < closestOrfDist) {
					closestOrfDist = length(lightPositions[i]);
					orificePosition = lightPositions[i];
					penetratorType = abs(lightRanges[i] - ID_HOLE) < 0.001 ? PENETRATORTYPE_HOLE : PENETRATORTYPE_RING;
					orificeNormal = (float3)0 * (1 - hasNormal) + foundNormal * hasNormal;
				}
			}
		}
	}
	//Guess normal of normal not existing
	if (length(orificeNormal) == 0) {
		orificeNormal = lerp(normalize(-orificePosition), -_TPS_PenetratorForward, max(dot(normalize(orificePosition), _TPS_PenetratorForward), 0.5));
	}

#ifdef TPS_IsSkinnedMesh
	return (closestOrfDist) < ((_TPS_SmoothStart+1) * penetratorLength);
#else
	return (closestOrfDist) < (min(_TPS_SmoothStart+1,1.5) * penetratorLength);
#endif
}

//https://vicrucann.github.io/tutorials/bezier-shader/
float3 toBezier(float t, float3 P0, float3 P1, float3 P2, float3 P3)
{
	float t2 = t * t;
	float one_minus_t = 1.0 - t;
	float one_minus_t2 = one_minus_t * one_minus_t;
	return (P0 * one_minus_t2 * one_minus_t + P1 * 3.0 * t * one_minus_t2 + P2 * 3.0 * t2 * one_minus_t + P3 * t2 * t);
}

void ApplyTPSPenetrator(inout float4 vertex, inout float3 normal, inout float4 vertexColor, uint vertexId, float2 uv) {
	float orificeType = (float3)0;
	float3 orificePosition = (float3)0;
	float3 orificeNormal = (float3)0;

	// SMR Scale. Might be something to look into in the future, although fucked normals or different scaling into axis would mess with this
	// scale = length(normal);

	// Adjust pen length for scaled non skinned renderers
	float penetratorLength = _TPS_PenetratorLength;
#ifndef TPS_IsSkinnedMesh
	/*float3 scale = float3(
		length(float3(unity_ObjectToWorld[0].x, unity_ObjectToWorld[1].x, unity_ObjectToWorld[2].x)), // scale x axis
		length(float3(unity_ObjectToWorld[0].y, unity_ObjectToWorld[1].y, unity_ObjectToWorld[2].y)), // scale y axis
		length(float3(unity_ObjectToWorld[0].z, unity_ObjectToWorld[1].z, unity_ObjectToWorld[2].z))  // scale z axis
		);*/
	// Mesh Renderers arent getting skinned => verticies are still in object space => _TPS_PenetratorLength needs to be scaled to object space
	penetratorLength = _TPS_PenetratorLength / abs(VectorLengthIntoDirection(_TPS_PenetratorScale, _TPS_PenetratorForward));
#endif

	//Idle shrinkage
	float2 shrinkage = float2(_TPS_IdleSkrinkWidth, _TPS_IdleSkrinkLength);

	//Idle Position
	//Fix idle gravity
	float3 targetPosition = _TPS_PenetratorForward * penetratorLength;
	float3 targetNormal = -_TPS_PenetratorForward;

	//Default values
	float tpsSmoothStart = 0;
	float tpsSmoothStart2 = 0;

	float bezierSmoothness = _TPS_BezierSmoothness * penetratorLength;
	float penetrationDepth = 0;

	//Find tps system, calculate values
	[branch] if (FindTPSSystem(orificePosition, orificeNormal, orificeType, penetratorLength) )
	{
#ifdef TPS_IsSkinnedMesh
		float smoothStartStart = _TPS_SmoothStart;
#else
		float smoothStartStart = min(_TPS_SmoothStart,0.5);
#endif
		tpsSmoothStart = saturate((penetratorLength * (1+smoothStartStart) - length(orificePosition)) / (penetratorLength * smoothStartStart));
		tpsSmoothStart2 = saturate((penetratorLength - length(orificePosition)) / penetratorLength * 10);

		float3 orifceTargetPos = normalize(orificePosition) * min(penetratorLength,length(orificePosition));
		targetPosition = lerp(targetPosition, orifceTargetPos, tpsSmoothStart);
		targetNormal = lerp(targetNormal, orificeNormal, tpsSmoothStart);

		penetrationDepth = 1 - saturate(length(orificePosition) / penetratorLength);
		//smoothness goes to 0 when close to oriface to prevent weird batching
		bezierSmoothness = lerp(bezierSmoothness, 0, penetrationDepth);

		shrinkage = lerp(float2(_TPS_IdleSkrinkWidth, _TPS_IdleSkrinkLength), float2(1, 1), saturate(tpsSmoothStart * 2));
	}

	//Vertex for skinned penetrators is read from texture
	float3 penetratorVertex = vertex;
	float3 penetratorNormal = normal;
#ifdef TPS_IsSkinnedMesh
	//_TPS_BakedMesh = 
	int uvY = (vertexId * 6) / 8190.0;
	int baseX = (vertexId * 6) % 8190;
	penetratorVertex.x = decodeFloatFromARGB8File(_TPS_BakedMesh.Load(int3(baseX + 0, uvY, 0)));
	penetratorVertex.y = decodeFloatFromARGB8File(_TPS_BakedMesh.Load(int3(baseX + 1, uvY, 0)));
	penetratorVertex.z = decodeFloatFromARGB8File(_TPS_BakedMesh.Load(int3(baseX + 2, uvY, 0)));
	penetratorNormal.x = decodeFloatFromARGB8File(_TPS_BakedMesh.Load(int3(baseX + 3, uvY, 0)));
	penetratorNormal.y = decodeFloatFromARGB8File(_TPS_BakedMesh.Load(int3(baseX + 4, uvY, 0)));
	penetratorNormal.z = decodeFloatFromARGB8File(_TPS_BakedMesh.Load(int3(baseX + 5, uvY, 0)));
	penetratorVertex = penetratorVertex * _TPS_PenetratorScale;
#endif

	float penZ = VectorLengthIntoDirection(penetratorVertex, _TPS_PenetratorForward);
	vertexColor.a = penZ / penetratorLength;
	 
	float3 bezier0 = _TPS_PenetratorForward * _TPS_BezierStart;
	float3 bezier0Out = bezier0 + _TPS_PenetratorForward * bezierSmoothness;
	float3 bezier1 = targetPosition;
	float3 bezier1In = bezier1 + targetNormal * bezierSmoothness;

	float bezierStrengthUncapped = ((penZ - _TPS_BezierStart) / distance(bezier0, bezier1));
	float bezierStrength = saturate(bezierStrengthUncapped);

	//Bezier curve, calculate two points, calculate forward, right, up vectors
	float3 bezierPoint = toBezier(bezierStrength, bezier0, bezier0Out, bezier1In, bezier1);
	float3 bezierPoint2 = toBezier(bezierStrength + 0.01f, bezier0, bezier0Out, bezier1In, bezier1);

	if (orificeType == PENETRATORTYPE_RING && bezierStrength == 1 && tpsSmoothStart == 1) {
		bezierPoint = orificePosition + (penZ - _TPS_BezierStart - length(orificePosition.xyz)) * -orificeNormal;
		bezierPoint2 = bezierPoint - orificeNormal;
	}

	float3 penDirectionRight = normalize(cross(_TPS_PenetratorUp, normalize(bezier1 - bezier0)));
	float3 penDirectionUp = normalize(cross(normalize(bezier1 - bezier0), penDirectionRight));

	float3 bezierForward = normalize(bezierPoint2 - bezierPoint);
	float3 bezierRight = normalize(cross(penDirectionUp, bezierForward));
	float3 bezierUp = normalize(cross(bezierForward, bezierRight));	

//Mask for skinned mesh renderer
#ifdef TPS_IsSkinnedMesh
	if(length(penetratorNormal) == 0){
		bezierStrength = 0;
		vertexColor.a = 0;
	}
#else
	float4 mask = tex2Dlod(_TPS_BakedMesh, float4(uv.xy, 0, 0));
	bezierStrength *= mask.r;
	vertexColor.a *= mask.r;
#endif
	if(_TPS_AnimatedToggle == 0){
		bezierStrength = 0;
	}

	//calculate new position and normal
	if (bezierStrength > 0) {
		//pumping
		float sizeChange = 1;
		if (_TPS_PumpingStrength > 0) {
			sizeChange *= lerp(1 - _TPS_PumpingStrength * tpsSmoothStart2, 1 + _TPS_PumpingStrength * tpsSmoothStart2, abs(sin(_Time.y * -_TPS_PumpingSpeed + bezierStrengthUncapped / _TPS_PumpingWidth)));
		}
		//buldging + squeezing
		float buldgeLerp = 0;
		if (bezierStrength < 1 - _TPS_BuldgeFalloffDistance)
			buldgeLerp = saturate((bezierStrength - 1 + _TPS_BuldgeDistance + _TPS_BuldgeFalloffDistance) / _TPS_BuldgeDistance);
		else
			buldgeLerp = saturate((-bezierStrength + 1) / _TPS_BuldgeFalloffDistance);

		//Squeezing while outside oriface makes penetrator looks weirly streched
		float squeeze = lerp(0, _TPS_Squeeze, saturate(penetrationDepth * 20));
		sizeChange *= lerp(1, 1 - squeeze, saturate(1 - abs(bezierStrengthUncapped - 1) / _TPS_SqueezeDistance) * tpsSmoothStart);
		sizeChange *= lerp(1, 1 + _TPS_Buldge, buldgeLerp * tpsSmoothStart2);

		//Calc idle shrinkage
		shrinkage = lerp(1, shrinkage, saturate((penZ - _TPS_BezierStart) * 20));
		//Apply vertex + normal
		float x = VectorLengthIntoDirection(penetratorVertex, _TPS_PenetratorRight);
		float y = VectorLengthIntoDirection(penetratorVertex, _TPS_PenetratorUp);
		float3 normalX = VectorLengthIntoDirection(normal, _TPS_PenetratorRight);
		float3 normalY = VectorLengthIntoDirection(normal, _TPS_PenetratorUp);
		float3 normalZ = VectorLengthIntoDirection(normal, _TPS_PenetratorForward);

		//TODO This is technically not correct for skinned meshes. Might want to revisit idle stuff in the future
		float3 vertexZ = _TPS_PenetratorForward * VectorLengthIntoDirection(vertex, _TPS_PenetratorForward);
		float3 vertexXY = vertex.xyz - vertexZ;
		vertex.xyz = shrinkage.y * vertexZ + shrinkage.x * vertexXY;

		vertex.xyz = lerp(vertex.xyz, bezierPoint + x * bezierRight * sizeChange + y * bezierUp * sizeChange, tpsSmoothStart); //for dynamic bones, lerp between original and skinned vertecies
		normal.xyz = lerp(normal.xyz, normalX * bezierRight + normalY * bezierUp + normalZ * bezierForward, tpsSmoothStart); //for dynamic bones, lerp between original and skinned vertecies
	}
}

float TPSBufferedDepth(float3 vertex, float4 vertexColor) {
	return saturate((vertexColor.a - (1 - _TPS_BufferedDepth)) * 10) * _TPS_BufferedStrength;
}