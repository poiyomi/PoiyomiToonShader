float2 getMatcapUV(float3 viewDirection, float3 normalDirection)
{
	half3 worldViewUp = normalize(half3(0,1,0) - viewDirection * dot(viewDirection, half3(0,1,0)));
	half3 worldViewRight = normalize(cross(viewDirection, worldViewUp));
	half2 matcapUV = half2(dot(worldViewRight, normalDirection), dot(worldViewUp, normalDirection)) * 0.5 + 0.5;
	return matcapUV;				
}