// Stable asset GUIDs for every shipped Poiyomi shader from 7.3 through 10.0.
//
// Why this exists: when a material references a shader version that is no longer in the project, the
// reference dangles. Unity swaps in the error shader and both the version AND the variant are lost -
// ThryEditor's ShaderOptimizer then reports "Original shader GUID <guid> could not be found" and falls
// back to Levenshtein name-guessing, which is unreliable across versions (9.0 ships
// "Poiyomi Toon Outline Early" while 9.3 ships "Poiyomi Toon Early Outline" - the words are reordered).
//
// Unity asset GUIDs are stable and ship inside each shader's .meta, so a dangling GUID still identifies
// exactly which shader the material was authored against. This table turns that dangling GUID back into a
// concrete (version, variant) pair with zero guessing, in ANY project - including one that never
// contained the removed version.
//
// The CURRENT version is listed as well, not just the removed ones. A 10.0 shader can be absent from a
// project too (a partial import, a deleted variant, Pro missing), and without its GUID here that dangling
// reference has no version attached - the legacy detector then falls back to "Poiyomi material, shader
// gone" and offers to translate a 10.0 material DOWN to 9.3.
//
// Generated from the .meta files under Shaders/<version>/. Values are literal constants: do NOT
// regenerate them from a project that has re-imported the shaders, as that can mint fresh GUIDs.
//
// Designed by BluWizard LABS - https://github.com/BluWizard10

using System;
using System.Collections.Generic;
using Version = System.Version;

namespace Poi.Tools.ShaderTranslator.VersionUpgrade
{
	/// <summary>
	/// Lookup from a Poiyomi shader asset GUID to the version and variant it belongs to, for shader versions
	/// that may no longer be present in the project.
	/// </summary>
	public static class PoiyomiLegacyShaderGuids
	{
		/// <summary>A shader identified purely from its asset GUID.</summary>
		public readonly struct LegacyShaderId
		{
			/// <summary>Version the shader shipped in, e.g. 9.0. May be the current version - see the file header.</summary>
			public readonly Version Version;
			/// <summary>Variant name only, e.g. "Poiyomi Toon World".</summary>
			public readonly string Variant;
			/// <summary>Full declared shader name, e.g. ".poiyomi/Old Versions/9.0/Poiyomi Toon World".</summary>
			public readonly string ShaderName;

			public LegacyShaderId(Version version, string variant, string shaderName)
			{
				Version = version;
				Variant = variant;
				ShaderName = shaderName;
			}
		}

		static readonly Dictionary<string, LegacyShaderId> Map = BuildMap();

		/// <summary>Number of known shader GUIDs.</summary>
		public static int Count => Map.Count;

		/// <summary>
		/// Resolves a raw asset GUID to the Poiyomi shader it identifies. Works whether or not the shader asset
		/// still exists in the project.
		/// </summary>
		public static bool TryResolve(string guid, out LegacyShaderId id)
		{
			id = default;
			if (string.IsNullOrEmpty(guid)) return false;
			return Map.TryGetValue(guid.Trim(), out id);
		}

		static void Add(Dictionary<string, LegacyShaderId> map, string guid, int major, int minor, string variant, string shaderName)
			=> map[guid] = new LegacyShaderId(new Version(major, minor), variant, shaderName);

		static Dictionary<string, LegacyShaderId> BuildMap()
		{
			var map = new Dictionary<string, LegacyShaderId>(97, StringComparer.OrdinalIgnoreCase);

			// ---- 7.3 ----
			Add(map, "917b37092bae034459c28c00a3a19b54", 7, 3, "Poiyomi Toon", ".poiyomi/Old Versions/7.3/• Poiyomi Toon •");

			// ---- 8.0 ----
			Add(map, "9cdbe9241ea4aaa48944028272700834", 8, 0, "Poiyomi Outline", ".poiyomi/Old Versions/8.0/Poiyomi Outline");
			Add(map, "1513582543e8f6c44b1981e8689f7a52", 8, 0, "Poiyomi Outline Early", ".poiyomi/Old Versions/8.0/Poiyomi Outline Early");
			Add(map, "231d95ff7213dd74f82af136b256fb32", 8, 0, "Poiyomi Toon", ".poiyomi/Old Versions/8.0/Poiyomi Toon");
			Add(map, "c933bce24d9d90747a5def016febd042", 8, 0, "Poiyomi World", ".poiyomi/Old Versions/8.0/Poiyomi World");

			// ---- 8.1 ----
			Add(map, "c0aca4001823bf74e8a9538b3075bf56", 8, 1, "Poiyomi Grab Pass", ".poiyomi/Old Versions/8.1/Poiyomi Grab Pass");
			Add(map, "5ca92f1e9fc35504aba297fd1acd62df", 8, 1, "Poiyomi Toon", ".poiyomi/Old Versions/8.1/Poiyomi Toon");
			Add(map, "d0091594032d90d46b8d5eaeaf065a17", 8, 1, "Poiyomi Toon Early Z", ".poiyomi/Old Versions/8.1/Poiyomi Toon Early Z");
			Add(map, "034460dc851505a42baca2ba6514f9f0", 8, 1, "Poiyomi Toon Outline", ".poiyomi/Old Versions/8.1/Poiyomi Toon Outline");
			Add(map, "24edb77705ad65a49afb831f72747592", 8, 1, "Poiyomi Toon Outline Early", ".poiyomi/Old Versions/8.1/Poiyomi Toon Outline Early");
			Add(map, "784867d15973a4847b1a6048d42c1565", 8, 1, "Poiyomi World", ".poiyomi/Old Versions/8.1/Poiyomi World");

			// ---- 9.0 ----
			Add(map, "23f6705aff8bf964c87bfb3dd66ab345", 9, 0, "Poiyomi Toon", ".poiyomi/Old Versions/9.0/Poiyomi Toon");
			Add(map, "bdbac715113578048ab7114aa3379a1d", 9, 0, "Poiyomi Toon Grab Pass", ".poiyomi/Old Versions/9.0/Poiyomi Toon Grab Pass");
			Add(map, "aa7e588ee69e46f429e3daa4b3068311", 9, 0, "Poiyomi Toon Outline Early", ".poiyomi/Old Versions/9.0/Poiyomi Toon Outline Early");
			Add(map, "fc2c2d7b9ec11d34785997ccbb86f496", 9, 0, "Poiyomi Toon Two Pass", ".poiyomi/Old Versions/9.0/Poiyomi Toon Two Pass");
			Add(map, "68d4032c05798c44eafe02f272e6895c", 9, 0, "Poiyomi Toon World", ".poiyomi/Old Versions/9.0/Poiyomi Toon World");

			// ---- 9.1 ----
			Add(map, "cef5dc16b30fdbb46b08ab424d41d1c0", 9, 1, "Poiyomi Toon", ".poiyomi/Old Versions/9.1/Poiyomi Toon");
			Add(map, "3eefd743f727e99419fa501e889c4998", 9, 1, "Poiyomi Toon Grab Pass", ".poiyomi/Old Versions/9.1/Poiyomi Toon Grab Pass");
			Add(map, "7845a1ca48a75804c8925b2bd5680f7f", 9, 1, "Poiyomi Toon Outline Early", ".poiyomi/Old Versions/9.1/Poiyomi Toon Outline Early");
			Add(map, "18de0e547c911de4e804ceaaa97a0b26", 9, 1, "Poiyomi Toon Two Pass", ".poiyomi/Old Versions/9.1/Poiyomi Toon Two Pass");
			Add(map, "7bca35b6e5a09ee4cbdff5cc54a9bd49", 9, 1, "Poiyomi Toon World", ".poiyomi/Old Versions/9.1/Poiyomi Toon World");

			// ---- 9.2 ----
			Add(map, "104003ab15a9f7a47b6ef0597225e771", 9, 2, "Poiyomi Toon", ".poiyomi/Old Versions/9.2/Poiyomi Toon");
			Add(map, "00a679886ff24e74585a2a9d37b3c8d7", 9, 2, "Poiyomi Toon Grab Pass", ".poiyomi/Old Versions/9.2/Poiyomi Toon Grab Pass");
			Add(map, "3210d19aa4851e148b6848177a017f02", 9, 2, "Poiyomi Toon Outline Early", ".poiyomi/Old Versions/9.2/Poiyomi Toon Outline Early");
			Add(map, "73da03e2cd0afa940968d9c487e2474d", 9, 2, "Poiyomi Toon Two Pass", ".poiyomi/Old Versions/9.2/Poiyomi Toon Two Pass");
			Add(map, "1b58d372fe360874a8fb767d53f955b0", 9, 2, "Poiyomi Toon World", ".poiyomi/Old Versions/9.2/Poiyomi Toon World");

			// ---- 9.3 ----
			Add(map, "77f45810ea400c74e929db1f8ea09c47", 9, 3, "Poiyomi Pro", ".poiyomi/Old Versions/9.3/Poiyomi Pro");
			Add(map, "34127024aef6d024cbba0e4cc31212a0", 9, 3, "Poiyomi Pro + Lil Fur", ".poiyomi/Old Versions/9.3/Poiyomi Pro + Lil Fur");
			Add(map, "817a582edd559e446b741985e9f5ff64", 9, 3, "Poiyomi Pro + Lil Fur Two Pass", ".poiyomi/Old Versions/9.3/Poiyomi Pro + Lil Fur Two Pass");
			Add(map, "60e25e22ba618fb4ea697234614dea32", 9, 3, "Poiyomi Pro Fur", ".poiyomi/Old Versions/9.3/Poiyomi Pro Fur");
			Add(map, "00cf37d2dece34646b62514579918ef3", 9, 3, "Poiyomi Pro Geom Wireframe", ".poiyomi/Old Versions/9.3/Poiyomi Pro Geom Wireframe");
			Add(map, "c3a26bf5a5f93e942be1e4a78d753eaf", 9, 3, "Poiyomi Pro Geometric Dissolve", ".poiyomi/Old Versions/9.3/Poiyomi Pro Geometric Dissolve");
			Add(map, "05757319c71261a46a65943298a32f04", 9, 3, "Poiyomi Pro Grab Pass", ".poiyomi/Old Versions/9.3/Poiyomi Pro Grab Pass");
			Add(map, "6c2ebae0a71e33443a1f927d90a2703a", 9, 3, "Poiyomi Pro Outline Early", ".poiyomi/Old Versions/9.3/Poiyomi Pro Outline Early");
			Add(map, "92632e57ac725e249ae42b2fc967aa1c", 9, 3, "Poiyomi Pro Particle", ".poiyomi/Old Versions/9.3/Poiyomi Pro Particle");
			Add(map, "b4920b37bd3167d44850cedb9e5b1f92", 9, 3, "Poiyomi Pro ShatterWave", ".poiyomi/Old Versions/9.3/Poiyomi Pro ShatterWave");
			Add(map, "c840163bcf9217c47bd683bd71657ae6", 9, 3, "Poiyomi Pro Tessellated", ".poiyomi/Old Versions/9.3/Poiyomi Pro Tessellated");
			Add(map, "3940adbf318512846854163393be5f08", 9, 3, "Poiyomi Pro Tessellated Geom", ".poiyomi/Old Versions/9.3/Poiyomi Pro Tessellated Geom");
			Add(map, "ee0b3cea5aa9f8b49b47793938c72fb7", 9, 3, "Poiyomi Pro Tessellated Geometric Dissolve", ".poiyomi/Old Versions/9.3/Poiyomi Pro Tessellated Geometric Dissolve");
			Add(map, "9f53ae95ffac3e84188db42145c07ecf", 9, 3, "Poiyomi Pro Two Pass", ".poiyomi/Old Versions/9.3/Poiyomi Pro Two Pass");
			Add(map, "db5a0f4b8e49a2146b206aa07f68bff6", 9, 3, "Poiyomi Pro Two Pass Particle", ".poiyomi/Old Versions/9.3/Poiyomi Pro Two Pass Particle");
			Add(map, "3b2d04b1ef05b2748a3554ef91b6de3b", 9, 3, "Poiyomi Pro Wireframe", ".poiyomi/Old Versions/9.3/Poiyomi Pro Wireframe");
			Add(map, "a11582d439d5e494986c4b421999c03a", 9, 3, "Poiyomi Pro World", ".poiyomi/Old Versions/9.3/Poiyomi Pro World");
			Add(map, "9a208f99f4d4b8044b28a1b8715ad139", 9, 3, "Poiyomi Pro World Fur", ".poiyomi/Old Versions/9.3/Poiyomi Pro World Fur");
			Add(map, "9444ce77bf4418748b1e8591b9d97f85", 9, 3, "Poiyomi Toon", ".poiyomi/Old Versions/9.3/Poiyomi Toon");
			Add(map, "e99d2df55dac44a4c921122fe7cf6ba7", 9, 3, "Poiyomi Toon + Lil Fur", ".poiyomi/Old Versions/9.3/Poiyomi Toon + Lil Fur");
			Add(map, "47c3851517c96c84aae06d328a2581f6", 9, 3, "Poiyomi Toon + Lil Fur Two Pass", ".poiyomi/Old Versions/9.3/Poiyomi Toon + Lil Fur Two Pass");
			Add(map, "d141019541e4e44479fbd71ad42a2cf6", 9, 3, "Poiyomi Toon Grab Pass", ".poiyomi/Old Versions/9.3/Poiyomi Toon Grab Pass");
			Add(map, "21e56665a7e45184b9013fb9e4f7bc96", 9, 3, "Poiyomi Toon Outline Early", ".poiyomi/Old Versions/9.3/Poiyomi Toon Outline Early");
			Add(map, "eda2412ac7ab2db45a47a521f6d7d8a6", 9, 3, "Poiyomi Toon Two Pass", ".poiyomi/Old Versions/9.3/Poiyomi Toon Two Pass");
			Add(map, "a6839ad792f95aa41a941c7317aa1fe1", 9, 3, "Poiyomi Toon World", ".poiyomi/Old Versions/9.3/Poiyomi Toon World");

			// ---- 10.0 (current) ----
			// The shipping version is listed too, so a dangling 10.0 reference resolves as 10.0 instead of falling
			// through to the weaker name/fingerprint guesses, which read "Poiyomi material, shader gone" as legacy.
			// Keep this section in step with Shaders/10.0 whenever a variant is added, renamed or removed.
			Add(map, "a24c7f2b5aa9f75478a3abf1a4afc345", 10, 0, "Poiyomi Pro", ".poiyomi/Poiyomi Pro");
			Add(map, "2b0f5c7e9e6d7ef4ea95a82254d8d42d", 10, 0, "Poiyomi Pro + Lil Fur", ".poiyomi/Poiyomi Pro + Lil Fur");
			Add(map, "ef4d659770256ba40b893516cd4e7699", 10, 0, "Poiyomi Pro + Lil Fur Two Pass", ".poiyomi/Poiyomi Pro + Lil Fur Two Pass");
			Add(map, "028e3088845e88647bfb8100985f574e", 10, 0, "Poiyomi Pro Fur", ".poiyomi/Poiyomi Pro Fur");
			Add(map, "31f073458988e164292a056cd65c70c7", 10, 0, "Poiyomi Pro Geom", ".poiyomi/Poiyomi Pro Geom");
			Add(map, "54f7d8e1a4ffc424490f1c9281afb67e", 10, 0, "Poiyomi Pro Geometric Dissolve", ".poiyomi/Poiyomi Pro Geometric Dissolve");
			Add(map, "c5ea2891b9bbd8d4cb408a5a07b2bbef", 10, 0, "Poiyomi Pro Grab Pass", ".poiyomi/Poiyomi Pro Grab Pass");
			Add(map, "e32ba9b464d4bd147a514c82ec8977d9", 10, 0, "Poiyomi Pro Outline Early", ".poiyomi/Poiyomi Pro Outline Early");
			Add(map, "57882b4d0899ad046bcaa235926e9b8c", 10, 0, "Poiyomi Pro Particle", ".poiyomi/Poiyomi Pro Particle");
			Add(map, "b6ee8d70e15754340800fff8dd1ac39c", 10, 0, "Poiyomi Pro Self Grab", ".poiyomi/Poiyomi Pro Self Grab");
			Add(map, "bd7f1ef2d9cc40a42bcd157c0e92f6e5", 10, 0, "Poiyomi Pro ShatterWave", ".poiyomi/Poiyomi Pro ShatterWave");
			Add(map, "073de4acf517eab43aa107bab29737d2", 10, 0, "Poiyomi Pro Tessellated", ".poiyomi/Poiyomi Pro Tessellated");
			Add(map, "047b1f6427de17c4f916a93e160558c7", 10, 0, "Poiyomi Pro Tessellated Geom", ".poiyomi/Poiyomi Pro Tessellated Geom");
			Add(map, "9e3834be0ab83294a98a5da91c83d551", 10, 0, "Poiyomi Pro Tessellated Geometric Dissolve", ".poiyomi/Poiyomi Pro Tessellated Geometric Dissolve");
			Add(map, "b332e0a0d4785a542ab1a0e82867a0f5", 10, 0, "Poiyomi Pro Triplanar Projection", ".poiyomi/Poiyomi Pro Triplanar Projection");
			Add(map, "b5a2786a3d31e5745ab68d6a084f1a2d", 10, 0, "Poiyomi Pro Two Pass", ".poiyomi/Poiyomi Pro Two Pass");
			Add(map, "ccff42a00a09e9847a3e72868227aa53", 10, 0, "Poiyomi Pro Two Pass Particle", ".poiyomi/Poiyomi Pro Two Pass Particle");
			Add(map, "d5ac3d4f634d99843a03126b59d48d27", 10, 0, "Poiyomi Pro URP", ".poiyomi/Poiyomi Pro URP");
			Add(map, "922ed338d70464f4899de7a8ed233c7e", 10, 0, "Poiyomi Pro URP + Lil Fur", ".poiyomi/Poiyomi Pro URP + Lil Fur");
			Add(map, "806659fa3c0a7844ba231410b8f098bb", 10, 0, "Poiyomi Pro URP Fur", ".poiyomi/Poiyomi Pro URP Fur");
			Add(map, "8fa8fbd2c13e75441869df5fe42af9eb", 10, 0, "Poiyomi Pro URP Geom", ".poiyomi/Poiyomi Pro URP Geom");
			Add(map, "666d8c8e0f87f504bb17f838c515938d", 10, 0, "Poiyomi Pro URP Geometric Dissolve", ".poiyomi/Poiyomi Pro URP Geometric Dissolve");
			Add(map, "4568e92c51ea9634284c588e57919945", 10, 0, "Poiyomi Pro URP Grab Pass", ".poiyomi/Poiyomi Pro URP Grab Pass");
			Add(map, "240224a47ea159e4bba0113c6f29a8e0", 10, 0, "Poiyomi Pro URP Particle", ".poiyomi/Poiyomi Pro URP Particle");
			Add(map, "5a8d7251024d1b14f8cbaa5cd7a52316", 10, 0, "Poiyomi Pro URP ShatterWave", ".poiyomi/Poiyomi Pro URP ShatterWave");
			Add(map, "75985a2ed1c538749a8d7e0a07aaf04f", 10, 0, "Poiyomi Pro URP Tessellated", ".poiyomi/Poiyomi Pro URP Tessellated");
			Add(map, "a63aea0c7cf09e44e8dc108da8d53a55", 10, 0, "Poiyomi Pro URP Tessellated Geom", ".poiyomi/Poiyomi Pro URP Tessellated Geom");
			Add(map, "12119e9fbbbf7e64c90d760d81681ba8", 10, 0, "Poiyomi Pro URP Tessellated Geometric Dissolve", ".poiyomi/Poiyomi Pro URP Tessellated Geometric Dissolve");
			Add(map, "0b34b9c276a2e05418dbeb953f9ed406", 10, 0, "Poiyomi Pro URP Triplanar Projection", ".poiyomi/Poiyomi Pro URP Triplanar Projection");
			Add(map, "14d6af4b964edf54eabec4660d443ddb", 10, 0, "Poiyomi Pro URP Wireframe", ".poiyomi/Poiyomi Pro URP Wireframe");
			Add(map, "0e3df917f51b2cf409751517fe0b4910", 10, 0, "Poiyomi Pro URP World", ".poiyomi/Poiyomi Pro URP World");
			Add(map, "897e4f0318b821e489e2a7ff680b2114", 10, 0, "Poiyomi Pro URP World Fur", ".poiyomi/Poiyomi Pro URP World Fur");
			Add(map, "de86a09af9414654abf8157f356e290c", 10, 0, "Poiyomi Pro Wireframe", ".poiyomi/Poiyomi Pro Wireframe");
			Add(map, "b4ed1bbd171c63b43b8f92181c3abb29", 10, 0, "Poiyomi Pro World", ".poiyomi/Poiyomi Pro World");
			Add(map, "cacc5c04e6c6a714eb080f1c1e9dbf21", 10, 0, "Poiyomi Pro World Fur", ".poiyomi/Poiyomi Pro World Fur");
			Add(map, "e9f30272e2d12c945851329f504df73d", 10, 0, "Poiyomi Toon", ".poiyomi/Poiyomi Toon");
			Add(map, "9ea5d2df175e6f84785d5fffd69809a5", 10, 0, "Poiyomi Toon + Lil Fur", ".poiyomi/Poiyomi Toon + Lil Fur");
			Add(map, "b357c1b1407f3ad4eba47df501cc613c", 10, 0, "Poiyomi Toon + Lil Fur Two Pass", ".poiyomi/Poiyomi Toon + Lil Fur Two Pass");
			Add(map, "a19e06d869c537d4391955e43f631173", 10, 0, "Poiyomi Toon Grab Pass", ".poiyomi/Poiyomi Toon Grab Pass");
			Add(map, "fa7f9dc5d41366249bd0098f22c3c1cd", 10, 0, "Poiyomi Toon Outline Early", ".poiyomi/Poiyomi Toon Outline Early");
			Add(map, "997f84ffc30f9ca439a796739ced421c", 10, 0, "Poiyomi Toon Two Pass", ".poiyomi/Poiyomi Toon Two Pass");
			Add(map, "63f1a49520e88be41888c86298e53c28", 10, 0, "Poiyomi Toon URP", ".poiyomi/Poiyomi Toon URP");
			Add(map, "3969baf45f800a749bdd84ca97341eca", 10, 0, "Poiyomi Toon URP + Lil Fur", ".poiyomi/Poiyomi Toon URP + Lil Fur");
			Add(map, "0cd02d134f25fa046b6de9236f3cd425", 10, 0, "Poiyomi Toon URP Grab Pass", ".poiyomi/Poiyomi Toon URP Grab Pass");
			Add(map, "0a22d5524572c6244a9bdb0f23bf5f66", 10, 0, "Poiyomi Toon URP World", ".poiyomi/Poiyomi Toon URP World");
			Add(map, "aaeaef15081a5154eac3e71b584c5472", 10, 0, "Poiyomi Toon World", ".poiyomi/Poiyomi Toon World");

			return map;
		}
	}
}
