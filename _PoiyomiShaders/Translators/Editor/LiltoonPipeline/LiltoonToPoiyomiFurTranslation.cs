using System.Collections.Generic;
using Poi.Tools.ShaderTranslator;

namespace Poi.Tools.ShaderTranslator.Translations
{
	public static class LiltoonToPoiyomiFurTranslation
	{
		public static List<PropertyTranslation> GetFurPropertyTranslations()
		{
			return new List<PropertyTranslation>()
			{
				#region Fur
				new PropertyTranslation("_FurVector", "_FurVector"),
				new PropertyTranslation("_FurVectorTex", "_FurVectorTex"),
				new PropertyTranslation("_FurVectorTex_ST", "_FurVectorTex_ST"),
				new PropertyTranslation("_FurVectorScale", "_FurVectorScale"),
				new PropertyTranslation("_FurLengthMask", "_FurLengthMask"),
				new PropertyTranslation("_FurLengthMask_ST", "_FurLengthMask_ST"),
				new PropertyTranslation("_FurGravity", "_FurGravity"),
				new PropertyTranslation("_FurRandomize", "_FurRandomize"),
				new PropertyTranslation("_FurNoiseMask", "_FurNoiseMask"),
				new PropertyTranslation("_FurNoiseMask_ST", "_FurNoiseMask_ST"),
				new PropertyTranslation("_FurMask", "_FurMask"),
				new PropertyTranslation("_FurMask_ST", "_FurMask_ST"),
				new PropertyTranslation("_FurAO", "_FurAO"),
				new PropertyTranslation("_FurLayerNum", "_FurLayerNum"),
				new PropertyTranslation("_FurRootOffset", "_FurRootOffset"),
				new PropertyTranslation("_FurTouchStrength", "_FurTouchStrength"),
				new PropertyTranslation("_FurRimColor", "_FurRimColor"),
				new PropertyTranslation("_FurRimFresnelPower", "_FurRimFresnelPower"),
				new PropertyTranslation("_FurRimAntiLight", "_FurRimAntiLight"),
				new PropertyTranslation("_FurCutoutLength", "_FurCutoutLength"),
				new PropertyTranslation("_VertexColor2FurVector", "_VertexColor2FurVector"),
				new PropertyTranslation("_FurCull", "_FurCull"),
				new PropertyTranslation("_FurSrcBlend", "_FurSrcBlend"),
				new PropertyTranslation("_FurDstBlend", "_FurDstBlend"),
				new PropertyTranslation("_FurSrcBlendAlpha", "_FurSrcBlendAlpha"),
				new PropertyTranslation("_FurDstBlendAlpha", "_FurDstBlendAlpha"),
				new PropertyTranslation("_FurBlendOp", "_FurBlendOp"),
				new PropertyTranslation("_FurBlendOpAlpha", "_FurBlendOpAlpha"),
				new PropertyTranslation("_FurSrcBlendFA", "_FurSrcBlendFA"),
				new PropertyTranslation("_FurDstBlendFA", "_FurDstBlendFA"),
				new PropertyTranslation("_FurSrcBlendAlphaFA", "_FurSrcBlendAlphaFA"),
				new PropertyTranslation("_FurDstBlendAlphaFA", "_FurDstBlendAlphaFA"),
				new PropertyTranslation("_FurBlendOpFA", "_FurBlendOpFA"),
				new PropertyTranslation("_FurBlendOpAlphaFA", "_FurBlendOpAlphaFA"),
				new PropertyTranslation("_FurZWrite", "_FurZWrite"),
				new PropertyTranslation("_FurZTest", "_FurZTest"),
				new PropertyTranslation("_FurZClip", "_FurZClip"),
				new PropertyTranslation("_FurOffsetFactor", "_FurOffsetFactor"),
				new PropertyTranslation("_FurOffsetUnits", "_FurOffsetUnits"),
				new PropertyTranslation("_FurColorMask", "_FurColorMask"),
				new PropertyTranslation("_FurAlphaToMask", "_FurAlphaToMask"),
				new PropertyTranslation("_FurStencilType", "_FurStencilType"),
				new PropertyTranslation("_FurStencilRef", "_FurStencilRef"),
				new PropertyTranslation("_FurStencilReadMask", "_FurStencilReadMask"),
				new PropertyTranslation("_FurStencilWriteMask", "_FurStencilWriteMask"),
				new PropertyTranslation("_FurStencilPass", "_FurStencilPass"),
				new PropertyTranslation("_FurStencilFail", "_FurStencilFail"),
				new PropertyTranslation("_FurStencilZFail", "_FurStencilZFail"),
				new PropertyTranslation("_FurStencilComp", "_FurStencilComp"),
				new PropertyTranslation("_FurStencilBackPass", "_FurStencilBackPass"),
				new PropertyTranslation("_FurStencilBackFail", "_FurStencilBackFail"),
				new PropertyTranslation("_FurStencilBackZFail", "_FurStencilBackZFail"),
				new PropertyTranslation("_FurStencilBackComp", "_FurStencilBackComp"),
				new PropertyTranslation("_FurStencilFrontPass", "_FurStencilFrontPass"),
				new PropertyTranslation("_FurStencilFrontFail", "_FurStencilFrontFail"),
				new PropertyTranslation("_FurStencilFrontZFail", "_FurStencilFrontZFail"),
				new PropertyTranslation("_FurStencilFrontComp", "_FurStencilFrontComp"),
				#endregion
			};
		}
	}
}

