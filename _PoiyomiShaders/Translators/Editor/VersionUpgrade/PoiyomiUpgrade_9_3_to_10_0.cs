using System;
using System.Collections.Generic;
using UnityEngine;
using Thry.ThryEditor.Helpers;

namespace Poi.Tools.ShaderTranslator.VersionUpgrade
{
	public class PoiyomiUpgrade_9_3_to_10_0 : ScriptedShaderTranslator, IPoiyomiVersionUpgrade
	{
		public static readonly Version SourceVersion = new Version(9, 3);
		public static readonly Version TargetVersion = new Version(10, 0);

		public Version GetSourceVersion() => SourceVersion;
		public Version GetTargetVersion() => TargetVersion;

		public override bool CanTranslateMaterial(Material sourceMaterial)
		{
			if (!PoiyomiVersionDetector.IsPoiyomiShader(sourceMaterial))
				return false;

			if (!PoiyomiVersionDetector.TryGetVersion(sourceMaterial, out Version version))
				return false;

			return version.Major == SourceVersion.Major && version.Minor == SourceVersion.Minor;
		}

		protected override Shader GetTargetShader(Material sourceMaterial, string newShaderName)
		{
			Shader effectiveShader = PoiyomiVersionDetector.GetEffectiveShader(sourceMaterial);
			string variant = PoiyomiVersionDetector.GetShaderVariant(effectiveShader);

			// 10.0 is the latest version, so use the main shader path (not Old Versions)
			string targetShaderName = $".poiyomi/{variant}";

			Shader targetShader = Shader.Find(targetShaderName);
			if (targetShader != null)
				return targetShader;

			return base.GetTargetShader(sourceMaterial, newShaderName);
		}

		protected override List<PropertyTranslation> AddProperties()
		{
			return new List<PropertyTranslation>
			{
				// Flipbook positioning: _FlipbookScaleOffset (sX, sY, oX, oY) -> _FlipbookPosition + _FlipbookScale
				new PropertyTranslation("_FlipbookScaleOffset", (prop, ctx) =>
				{
					Vector4 scaleOffset = GetSourcePropertyValue<Vector4>(ctx, prop);
					// Old: sX, sY = scale, oX, oY = offset from center
					// New: Position is center (0.5, 0.5) + offset, Scale is separate
					Vector2 position = new Vector2(scaleOffset.z + 0.5f, scaleOffset.w + 0.5f);
					Vector3 scale = new Vector3(scaleOffset.x, scaleOffset.y, 1f);
					SetTargetPropertyValue(ctx, "_FlipbookPosition", position);
					SetTargetPropertyValue(ctx, "_FlipbookScale", scale);
				}),

				// Flipbook Opacity: _FlipbookReplace -> _FlipbookAlpha.
				//
				// The slider was always a plain Opacity slider despite the
				// name. 10.0 renames it to match what it does in order to
				// fix another unrelated bug.
				new PropertyTranslation("_FlipbookReplace", "_FlipbookAlpha"),

				// Rim Lighting: _RimSharpness -> _RimBlur
				new PropertyTranslation("_RimSharpness", (prop, ctx) =>
				{
					float sharpness = GetSourcePropertyValue<float>(ctx, prop);
					float width = GetSourcePropertyValue<float>(ctx, "_RimWidth");
					SetTargetPropertyValue(ctx, "_RimBlur", RimBlurFromSharpness(width, sharpness));
				}),

				// Rim Lighting 2: _Rim2Sharpness -> _Rim2Blur
				new PropertyTranslation("_Rim2Sharpness", (prop, ctx) =>
				{
					float sharpness = GetSourcePropertyValue<float>(ctx, prop);
					float width = GetSourcePropertyValue<float>(ctx, "_Rim2Width");
					SetTargetPropertyValue(ctx, "_Rim2Blur", RimBlurFromSharpness(width, sharpness));
				}),

				// AL Spectrum Positioning: _ALUVPosition -> _ALDecalPosition
				new PropertyTranslation("_ALUVPosition", "_ALDecalPosition"),

				// AL Spectrum Rotation: _ALUVRotation -> _ALDecalRotation && _ALUVRotationSpeed -> _ALDecalRotationSpeed
				new PropertyTranslation("_ALUVRotation", "_ALDecalRotation"),
				new PropertyTranslation("_ALUVRotationSpeed", "_ALDecalRotationSpeed"),

				// AL Spectrum Scale: _ALUVScale (X, Y, Z, W) -> _ALDecalScale (X, Y)
				new PropertyTranslation("_ALUVScale", (prop, ctx) =>
				{
					Vector4 scale = GetSourcePropertyValue<Vector4>(ctx, prop);
					Vector2 decalScale = new Vector2((scale.x + scale.y) * 0.5f, (scale.z + scale.w) * 0.5f);
					SetTargetPropertyValue(ctx, "_ALDecalScale", decalScale);
				}),

				// Dissolve: _ContinuousDissolve -> _ContinuousDissolveSpeed && _ContinuousDissolveEnabled
				new PropertyTranslation("_ContinuousDissolve", (prop, ctx) =>
				{
					float speed = GetSourcePropertyValue<float>(ctx, prop);
					SetTargetPropertyValue(ctx, "_ContinuousDissolveSpeed", speed);
					SetTargetPropertyValue(ctx, "_ContinuousDissolveEnabled", speed != 0f ? 1f : 0f);
				}),

				// Dissolve: _DissolveEdgeWidth -> if (_DissolveEdgeWidth > 0) _DissolveEdgeEnabled = 1f
				new PropertyTranslation("_DissolveEdgeWidth", (prop, ctx) =>
				{
					if (GetSourcePropertyValue<float>(ctx, prop) > 0f) SetTargetPropertyValue(ctx, "_DissolveEdgeEnabled", 1f);
				}),

				// Dissolve: _DissolveDetailStrength -> if (_DissolveDetailStrength > 0) _DissolveDetailNoiseEnabled = 1f
				new PropertyTranslation("_DissolveDetailStrength", (prop, ctx) =>
				{
					if (GetSourcePropertyValue<float>(ctx, prop) > 0f) SetTargetPropertyValue(ctx, "_DissolveDetailNoiseEnabled", 1f);
				}),

				// Dissolve: 9.3 only drew the edge hue shift when BOTH _DissolveEdgeHueShiftEnabled and the master
				// _DissolveHueShiftEnabled were on. 10.0 moves it into the Edge Line section where it stands alone,
				// so a material with the edge toggle on but the master off would gain a shift it never displayed.
				new PropertyTranslation("_DissolveEdgeHueShiftEnabled", (prop, ctx) =>
				{
					bool edgeEnabled = GetSourcePropertyValue<float>(ctx, prop) > 0f;
					bool masterEnabled = GetSourcePropertyValue<float>(ctx, "_DissolveHueShiftEnabled") > 0f;
					SetTargetPropertyValue(ctx, "_DissolveEdgeHueShiftEnabled", edgeEnabled && masterEnabled ? 1f : 0f);
				}),

				// Grab Pass global mask was renamed in 10.0. The enum layout is identical, so a straight rename
				// preserves the selected channel and its blend mode exactly.
				new PropertyTranslation("_GrabPassBlendGlobalMask", "_GrabRefractionGlobalMask"),
				new PropertyTranslation("_GrabPassBlendGlobalMaskBlendType", "_GrabRefractionGlobalMaskBlendType"),

				// Chromatic Aberration: 9.3 shipped the name misspelled ("Aberattion"); 10.0 corrected the spelling.
				// Same type, range and default, so this is a pure rename - without it the user's value resets to 0.
				new PropertyTranslation("_RefractionChromaticAberattion", "_RefractionChromaticAberration"),
			};
		}

		protected override void DoAfterTranslation(TranslationContext context)
		{
			SetTargetRenderQueue(context, context.originalRenderQueue);
			WarnIfGrabPass(context);
		}

		/// <summary>
		/// Grab Pass was overhauled between 9.3 and 10.0. Blend, Refraction, Chromatic Aberration and the Global Masks
		/// carry across, but the "Use Material Alpha" toggle is gone (refraction now respects the alpha mask, which
		/// changes how the Base Color reads) and a few options were dropped (Hue Shift "Replace", multi-directional
		/// blur, and the per-channel Blend Mask selector - the blend map is RGBA-packed now).
		/// </summary>
		void WarnIfGrabPass(TranslationContext context)
		{
			if (SourceShader?.Shader == null || !PoiyomiVersionDetector.IsGrabPassShaderName(SourceShader.Shader.name)) return;

			ThryLogger.LogWarn($"<b>{context.Material.name}</b>: Grab Pass was overhauled between 9.3 and 10.0. Blend, " +
				"Refraction, Chromatic Aberration and the Global Masks were carried over. However, the Base Color and Alpha " +
				"handling changed (\"Use Material Alpha\" is now driven by the Alpha Mask) and some options were non-transferrable " +
				"(Hue Shift Replace, multi-directional Blur, the per-channel Blend Mask). Please inspect your material's " +
				"Grab Pass and Refraction settings.");
		}

		/// <summary>
		/// Converts a 9.3 rim "sharpness" into the equivalent 10.0 rim "blur", preserving the visual width of the
		/// soft transition band.
		/// 9.3: rim = 1 - smoothstep(min(sharpness, rimWidth), rimWidth, vDotN), where rimWidth = lerp(-0.05, 1, _RimWidth).
		///       The transition band spans [min(sharpness, rimWidth), rimWidth], i.e. its width is rimWidth - sharpness.
		/// 10.0: the transition band width is exactly _RimBlur (borderMin = saturate(rimWidth - _RimBlur)).
		/// So the matching blur is the old band width: saturate(lerp(-0.05, 1, _RimWidth) - sharpness).
		/// </summary>
		static float RimBlurFromSharpness(float rimWidth, float sharpness)
		{
			float effectiveWidth = Mathf.Lerp(-0.05f, 1f, rimWidth);
			return Mathf.Clamp01(effectiveWidth - sharpness);
		}
	}
}
