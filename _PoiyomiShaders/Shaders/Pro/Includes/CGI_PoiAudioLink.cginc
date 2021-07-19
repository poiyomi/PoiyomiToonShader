#ifndef POI_AUDIOLINK
#define POI_AUDIOLINK

UNITY_DECLARE_TEX2D(_AudioTexture);
float4 _AudioTexture_ST;
fixed _AudioLinkDelay;
fixed _AudioLinkAveraging;
fixed _AudioLinkAverageRange;

// Debug
fixed _EnableAudioLinkDebug;
fixed _AudioLinkDebugTreble;
fixed _AudioLinkDebugHighMid;
fixed _AudioLinkDebugLowMid;
fixed _AudioLinkDebugBass;
fixed _AudioLinkDebugAnimate;
fixed _AudioLinkTextureVisualization;
fixed _AudioLinkAnimToggle;

void AudioTextureExists()
{
	half testw = 0;
	half testh = 0;
	_AudioTexture.GetDimensions(testw, testh);
	poiMods.audioLinkTextureExists = testw >= 32;
	poiMods.audioLinkTextureExists *= _AudioLinkAnimToggle;
	switch(testw)
	{
		case 32: // V1
		poiMods.audioLinkVersion = 1;
		break;
		case 128: // V2
		poiMods.audioLinkVersion = 2;
		break;
		default:
		poiMods.audioLinkVersion = 1;
		break;
	}
}

float getBandAtTime(float band, fixed time, fixed width)
{
	float versionUvMultiplier = 1;

	if (poiMods.audioLinkVersion == 2)
	{
		versionUvMultiplier = 0.0625;
	}
	return UNITY_SAMPLE_TEX2D(_AudioTexture, float2(time * width, (band * .25 + .125) * versionUvMultiplier)).r;
}

void initAudioBands()
{
	AudioTextureExists();

	float versionUvMultiplier = 1;

	if (poiMods.audioLinkVersion == 2)
	{
		versionUvMultiplier = 0.0625;
	}

	if (poiMods.audioLinkTextureExists)
	{
		poiMods.audioLink.x = UNITY_SAMPLE_TEX2D(_AudioTexture, float2(_AudioLinkDelay, .125 * versionUvMultiplier));
		poiMods.audioLink.y = UNITY_SAMPLE_TEX2D(_AudioTexture, float2(_AudioLinkDelay, .375 * versionUvMultiplier));
		poiMods.audioLink.z = UNITY_SAMPLE_TEX2D(_AudioTexture, float2(_AudioLinkDelay, .625 * versionUvMultiplier));
		poiMods.audioLink.w = UNITY_SAMPLE_TEX2D(_AudioTexture, float2(_AudioLinkDelay, .875 * versionUvMultiplier));

		UNITY_BRANCH
		if (_AudioLinkAveraging)
		{
			float uv = saturate(_AudioLinkDelay + _AudioLinkAverageRange * .25);
			poiMods.audioLink.x += UNITY_SAMPLE_TEX2D(_AudioTexture, float2(uv, .125 * versionUvMultiplier));
			poiMods.audioLink.y += UNITY_SAMPLE_TEX2D(_AudioTexture, float2(uv, .375 * versionUvMultiplier));
			poiMods.audioLink.z += UNITY_SAMPLE_TEX2D(_AudioTexture, float2(uv, .625 * versionUvMultiplier));
			poiMods.audioLink.w += UNITY_SAMPLE_TEX2D(_AudioTexture, float2(uv, .875 * versionUvMultiplier));

			uv = saturate(_AudioLinkDelay + _AudioLinkAverageRange * .5);
			poiMods.audioLink.x += UNITY_SAMPLE_TEX2D(_AudioTexture, float2(uv, .125 * versionUvMultiplier));
			poiMods.audioLink.y += UNITY_SAMPLE_TEX2D(_AudioTexture, float2(uv, .375 * versionUvMultiplier));
			poiMods.audioLink.z += UNITY_SAMPLE_TEX2D(_AudioTexture, float2(uv, .625 * versionUvMultiplier));
			poiMods.audioLink.w += UNITY_SAMPLE_TEX2D(_AudioTexture, float2(uv, .875 * versionUvMultiplier));

			uv = saturate(_AudioLinkDelay + _AudioLinkAverageRange * .75);
			poiMods.audioLink.x += UNITY_SAMPLE_TEX2D(_AudioTexture, float2(uv, .125 * versionUvMultiplier));
			poiMods.audioLink.y += UNITY_SAMPLE_TEX2D(_AudioTexture, float2(uv, .375 * versionUvMultiplier));
			poiMods.audioLink.z += UNITY_SAMPLE_TEX2D(_AudioTexture, float2(uv, .625 * versionUvMultiplier));
			poiMods.audioLink.w += UNITY_SAMPLE_TEX2D(_AudioTexture, float2(uv, .875 * versionUvMultiplier));

			uv = saturate(_AudioLinkDelay + _AudioLinkAverageRange);
			poiMods.audioLink.x += UNITY_SAMPLE_TEX2D(_AudioTexture, float2(uv, .125 * versionUvMultiplier));
			poiMods.audioLink.y += UNITY_SAMPLE_TEX2D(_AudioTexture, float2(uv, .375 * versionUvMultiplier));
			poiMods.audioLink.z += UNITY_SAMPLE_TEX2D(_AudioTexture, float2(uv, .625 * versionUvMultiplier));
			poiMods.audioLink.w += UNITY_SAMPLE_TEX2D(_AudioTexture, float2(uv, .875 * versionUvMultiplier));

			poiMods.audioLink /= 5;
		}
	}

	#ifndef OPTIMIZER_ENABLED
		UNITY_BRANCH
		if (_EnableAudioLinkDebug)
		{
			poiMods.audioLink.x = _AudioLinkDebugBass;
			poiMods.audioLink.y = _AudioLinkDebugLowMid;
			poiMods.audioLink.z = _AudioLinkDebugHighMid;
			poiMods.audioLink.w = _AudioLinkDebugTreble;

			if (_AudioLinkDebugAnimate)
			{
				poiMods.audioLink.x *= (sin(_Time.w * 3.1) + 1) * .5;
				poiMods.audioLink.y *= (sin(_Time.w * 3.2) + 1) * .5;
				poiMods.audioLink.z *= (sin(_Time.w * 3.3) + 1) * .5;
				poiMods.audioLink.w *= (sin(_Time.w * 3) + 1) * .5;
			}
			poiMods.audioLinkTextureExists = 1;
		}
		
		UNITY_BRANCH
		if (_AudioLinkTextureVisualization)
		{
			poiMods.audioLinkTexture = UNITY_SAMPLE_TEX2D(_AudioTexture, poiMesh.uv[0]);
		}
	#endif
}

#endif