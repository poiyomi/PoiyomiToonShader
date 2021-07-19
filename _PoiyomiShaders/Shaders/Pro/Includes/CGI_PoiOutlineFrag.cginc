float _OutlineRimLightBlend;
float _OutlineLit;
float _OutlineTintMix;
float2 _MainTexPan;
float _MainTextureUV;
half _OutlineHueOffset;
half _OutlineHueShift;
half _OutlineHueOffsetSpeed;

float4 frag(v2f i, uint facing: SV_IsFrontFace): COLOR
{
	float4 finalColor = 1;
	UNITY_BRANCH
	if (_commentIfZero_EnableOutlinePass)
	{
		UNITY_SETUP_INSTANCE_ID(i);
		
		float3 finalEmission = 0;
		float4 albedo = 1;
		
		poiMesh.uv[0] = i.uv0.xy;
		poiMesh.uv[1] = i.uv0.zw;
		poiMesh.uv[2] = i.uv1.xy;
		poiMesh.uv[3] = i.uv1.zw;
		
		calculateAttenuation(i);
		InitializeMeshData(i, facing);
		initializeCamera(i);
		calculateTangentData();
		
		float4 mainTexture = UNITY_SAMPLE_TEX2D(_MainTex, TRANSFORM_TEX(poiMesh.uv[_MainTextureUV], _MainTex) + _Time.x * _MainTexPan);
		half3 detailMask = 1;
		calculateNormals(detailMask);
		
		#ifdef POI_DATA
		calculateLightingData(i);
	#endif
	#ifdef POI_LIGHTING
		calculateBasePassLightMaps();
	#endif
	
	float3 uselessData0;
	float3 uselessData1;
	initTextureData(albedo, mainTexture, uselessData0, uselessData1, detailMask);
	
	
	fixed4 col = mainTexture;
	float alphaMultiplier = smoothstep(_OutlineFadeDistance.x, _OutlineFadeDistance.y, distance(getCameraPosition(), i.worldPos));
	float OutlineMask = tex2D(_OutlineMask, TRANSFORM_TEX(poiMesh.uv[_OutlineMaskUV], _OutlineMask) + _Time.x * _OutlineMaskPan).r;
	clip(OutlineMask * _LineWidth - 0.001);
	
	col = col * 0.00000000001 + tex2D(_OutlineTexture, TRANSFORM_TEX(poiMesh.uv[_OutlineTextureUV], _OutlineTexture) + _Time.x * _OutlineTexturePan);
	col.a *= albedo.a;
	col.a *= alphaMultiplier;
	
	#ifdef POI_RANDOM
		col.a *= i.angleAlpha;
	#endif
	
	poiCam.screenUV = calcScreenUVs(i.grabPos);
	col.a *= _LineColor.a;
	
	UNITY_BRANCH
	if (_Mode == 1)
	{
		applyDithering(col);
	}
	
	clip(col.a - _Cutoff);
	
	#ifdef POI_MIRROR
		applyMirrorRenderFrag();
	#endif
	
	UNITY_BRANCH
	if (_OutlineMode == 1)
	{
		#ifdef POI_MIRROR
			applyMirrorTexture(mainTexture);
		#endif
		col.rgb = mainTexture.rgb;
	}
	else if (_OutlineMode == 2)
	{
		col.rgb = lerp(col.rgb, poiLight.color, _OutlineRimLightBlend);
	}
	col.rgb *= _LineColor.rgb;
	
	if (_OutlineMode == 1)
	{
		col.rgb = lerp(col.rgb, mainTexture.rgb, _OutlineTintMix);
	}
	
	finalColor = col;

	// Hue shift
	UNITY_BRANCH
	if (_OutlineHueShift)
	{
		finalColor.rgb = hueShift(finalColor.rgb, _OutlineHueOffset + _OutlineHueOffsetSpeed * _Time.x);
	}

	#ifdef POI_LIGHTING
		UNITY_BRANCH
		if (_OutlineLit)
		{
			finalColor.rgb *= calculateFinalLighting(finalColor.rgb, finalColor);
		}
	#endif
	finalColor.rgb += (col.rgb * _OutlineEmission);
}
else
{
	clip(-1);
}
return finalColor;
}