#ifndef POI_BACKFACE
#define POI_BACKFACE

float _BackFaceEnabled;
float _BackFaceTextureUV;
float _BackFaceDetailIntensity;
float _BackFaceEmissionStrength;
float2 _BackFacePanning;
float _BackFaceHueShift;
float4 _BackFaceColor;
float _BackFaceReplaceAlpha;

#if defined(PROP_BACKFACETEXTURE) || !defined(OPTIMIZER_ENABLED)
	UNITY_DECLARE_TEX2D_NOSAMPLER(_BackFaceTexture); float4 _BackFaceTexture_ST;
#endif

float3 BackFaceColor;
void applyBackFaceTexture(inout float backFaceDetailIntensity, inout float mixedHueShift, inout float4 albedo, inout float3 backFaceEmission)
{
	backFaceEmission = 0;
	BackFaceColor = 0;
	UNITY_BRANCH
	if (_BackFaceEnabled)
	{
		if (!poiMesh.isFrontFace)
		{
			#if defined(PROP_BACKFACETEXTURE) || !defined(OPTIMIZER_ENABLED)
				float4 backFaceTex = POI2D_SAMPLER_PAN(_BackFaceTexture, _MainTex, poiMesh.uv[_BackFaceTextureUV], _BackFacePanning) * _BackFaceColor;
			#else
				float4 backFaceTex = _BackFaceColor;
			#endif

			albedo.rgb = backFaceTex.rgb;

			UNITY_BRANCH
			if (_BackFaceReplaceAlpha)
			{
				albedo.a = backFaceTex.a;
			}

			backFaceDetailIntensity = _BackFaceDetailIntensity;
			BackFaceColor = albedo.rgb;
			mixedHueShift = _BackFaceHueShift;
			backFaceEmission = BackFaceColor * _BackFaceEmissionStrength;
		}
	}
}

#endif