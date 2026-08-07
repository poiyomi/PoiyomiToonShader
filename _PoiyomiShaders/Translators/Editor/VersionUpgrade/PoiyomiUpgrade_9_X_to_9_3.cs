// 9.3 is still bundled (.poiyomi/Old Versions/9.3/...), so this lands legacy materials on a shader that
// exists and renders, from where the user can opt in to 10.0 later via the normal 9.3 -> 10.0 pipeline.
//
// Empirically there are ZERO property remaps 9.x -> 9.3 (Toon 9.2 == 9.3; Pro only drops the removed DPS
// penetrator + legacy aniso-noise). So this is pure routing:
//   - Unlocked: assign the 9.3 shader; same-named serialized values carry over automatically.
//   - Locked:   spoof the OriginalShader recovery tags to point at 9.3, then let ShaderOptimizer.UnlockMaterials
//               restore onto 9.3. This avoids the hazard where GetOriginalShader resolves a locked 9.2 material's
//               name tag (".poiyomi/Poiyomi Toon") straight onto the 10.0 shader.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Thry;
using Thry.ThryEditor;
using Thry.ThryEditor.Helpers;

namespace Poi.Tools.ShaderTranslator.VersionUpgrade
{
	public static class PoiyomiUpgrade_9_X_to_9_3
	{
		const string Nine3Prefix = ".poiyomi/Old Versions/9.3/";

		/// <summary>Detect and, if it's a removed-version material, route it onto 9.3.</summary>
		public static bool UpgradeToNine3(Material material)
		{
			if (material == null)
				return false;

			if (!LegacyMaterialDetector.TryDetectLegacyNine(material, out LegacyMaterialInfo info))
			{
				ThryLogger.LogWarn($"Material <b>{material.name}</b> is not a removed-version (9.2 or older) Poiyomi material.");
				return false;
			}

			return UpgradeToNine3(material, info);
		}

		/// <summary>Route a material already confirmed legacy (info from the detector) onto 9.3.</summary>
		public static bool UpgradeToNine3(Material material, LegacyMaterialInfo info)
		{
			Shader target = ResolveNine3Target(info);
			if (target == null)
			{
				ThryLogger.LogErr($"Could not resolve a 9.3 target shader for <b>{material.name}</b> " +
					$"(edition: {info.Edition}, variant: {info.Variant ?? "unknown"}). Skipped.");
				return false;
			}

			WarnAboutDroppedFeatures(material, info);

			bool ok = info.IsLocked
				? UpgradeLocked(material, target)
				: UpgradeUnlocked(material, target);

			if (ok)
			{
				EditorUtility.SetDirty(material);
				ThryLogger.Log($"Upgraded <b>{material.name}</b> to <b>{target.name}</b> " +
					$"({(info.IsLocked ? "unlocked onto 9.3" : "reassigned to 9.3")}). You can update it to 10.0 whenever you're ready.");
			}
			return ok;
		}

		// --- Locked: spoof-tags-then-unlock ------------------------------------------------------------

		static bool UpgradeLocked(Material material, Shader target)
		{
			// Before unlocking: retarget any stripped-texture tag so an 8.x texture (e.g. _ClippingMask) restores
			// onto its 9.3 name, and snapshot the (non-stripped) serialized values for the by-name remaps.
			PoiyomiLegacyRemaps.RenameStrippedTextureTags(material);
			var snapshot = LegacyMaterialDetector.MaterialSerializedReader.Read(material);
			SpoofRecoveryTags(material, target);

			bool unlocked;
			try
			{
				unlocked = ShaderOptimizer.UnlockMaterials(new[] { material });
			}
			catch (Exception ex)
			{
				ThryLogger.LogErr($"Unlock-to-9.3 failed for <b>{material.name}</b>. Report this with the stack trace below.");
				Debug.LogException(ex);
				return false;
			}

			if (!unlocked)
			{
				ThryLogger.LogErr($"ShaderOptimizer could not unlock <b>{material.name}</b> onto 9.3.");
				return false;
			}

			// Now on 9.3: apply the 8.x property renames, then drop the removed DPS/aniso/8.x orphans (VRAM).
			PoiyomiLegacyRemaps.Apply(material, snapshot);
			ScriptedShaderTranslator.RemoveOrphanedProperties(material, target);
			return true;
		}

		// Point the OriginalShader recovery tags at 9.3 so unlock restores onto it. GetOriginalShader checks
		// GUID first then name, so set both.
		static void SpoofRecoveryTags(Material material, Shader target)
		{
			string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(target));
			material.SetOverrideTag(ShaderOptimizer.TAG_ORIGINAL_SHADER_GUID, guid);
			material.SetOverrideTag(ShaderOptimizer.TAG_ORIGINAL_SHADER, target.name);
		}

		// --- Batch --------------------------------------------------------------------------------------

		/// <summary>
		/// Upgrade many materials with a single grouped unlock (one compile pass for all locked materials)
		/// instead of one per material. Unlocked materials are reassigned directly.
		/// </summary>
		public static void UpgradeMaterials(IEnumerable<Material> materials)
		{
			var targets = new Dictionary<Material, Shader>();
			var snapshots = new Dictionary<Material, LegacyMaterialDetector.MaterialSerializedReader>();
			var lockedToUnlock = new List<Material>();
			var unlockedDirect = new List<Material>();

			try
			{
				foreach (Material material in materials)
				{
					if (material == null || !LegacyMaterialDetector.TryDetectLegacyNine(material, out LegacyMaterialInfo info))
						continue;

					Shader target = ResolveNine3Target(info);
					if (target == null)
					{
						ThryLogger.LogErr($"Could not resolve a 9.3 target for <b>{material.name}</b> (variant: {info.Variant ?? "unknown"}). Skipped.");
						continue;
					}

					WarnAboutDroppedFeatures(material, info);
					targets[material] = target;

					if (info.IsLocked)
					{
						// Retarget stripped-texture tags and snapshot values BEFORE the grouped unlock.
						PoiyomiLegacyRemaps.RenameStrippedTextureTags(material);
						snapshots[material] = LegacyMaterialDetector.MaterialSerializedReader.Read(material);
						SpoofRecoveryTags(material, target);
						lockedToUnlock.Add(material);
					}
					else
					{
						unlockedDirect.Add(material);
					}
				}

				// One grouped unlock for every locked material - restores each onto its spoofed 9.3 target.
				if (lockedToUnlock.Count > 0)
					ShaderOptimizer.UnlockMaterials(lockedToUnlock, ShaderOptimizer.ProgressBar.Cancellable);

				foreach (Material material in lockedToUnlock)
				{
					// Guard against a material that didn't actually unlock - don't remap/purge a still-locked shader.
					if (material.shader != targets[material])
					{
						ThryLogger.LogWarn($"<b>{material.name}</b> did not unlock onto 9.3; skipping remaps for it.");
						continue;
					}

					PoiyomiLegacyRemaps.Apply(material, snapshots[material]);
					ScriptedShaderTranslator.RemoveOrphanedProperties(material, targets[material]);
					EditorUtility.SetDirty(material);
				}

				foreach (Material material in unlockedDirect)
				{
					if (UpgradeUnlocked(material, targets[material]))
						EditorUtility.SetDirty(material);
				}
			}
			finally
			{
				EditorUtility.ClearProgressBar();
			}

			int total = lockedToUnlock.Count + unlockedDirect.Count;
			ThryLogger.Log($"Routed {total} legacy material(s) onto 9.3. Update them to 10.0 whenever you're ready.");
		}

		// --- Unlocked: direct reassignment -------------------------------------------------------------

		static bool UpgradeUnlocked(Material material, Shader target)
		{
			// Snapshot pre-swap values so renamed 8.x props can be read by name after the swap.
			var snapshot = LegacyMaterialDetector.MaterialSerializedReader.Read(material);

			// Swapping a shader wipes the render queue override; preserve it across the swap.
			int renderQueue = material.renderQueue;
			material.shader = target;
			material.renderQueue = renderQueue;

			// 9.x == 9.3 by name (values already carried); apply 8.x renames; fix keywords; purge orphans.
			PoiyomiLegacyRemaps.Apply(material, snapshot);
			ShaderEditor.FixKeywords(new[] { material });
			ScriptedShaderTranslator.RemoveOrphanedProperties(material, target);
			return true;
		}

		// --- Target resolution -------------------------------------------------------------------------

		static Shader ResolveNine3Target(LegacyMaterialInfo info)
		{
			if (!string.IsNullOrEmpty(info.Variant))
			{
				// Exact variant match - true for every shared 9.2 variant except "Poiyomi Pro Geom".
				Shader exact = Shader.Find(Nine3Prefix + info.Variant);
				if (exact != null)
					return exact;

				// Fuzzy same-family fallback (e.g. 9.2 "Poiyomi Pro Geom" -> 9.3 "Poiyomi Pro Geom Wireframe").
				Shader fuzzy = FuzzyMatchNine3(info.Variant);
				if (fuzzy != null)
				{
					ThryLogger.LogWarn($"No exact 9.3 variant for <b>{info.Variant}</b>; using closest match <b>{fuzzy.name}</b>.");
					return fuzzy;
				}
			}

			// Variant unknown (unlocked error shader) - fall back to the edition's base shader.
			switch (info.Edition)
			{
				case PoiyomiEdition.Pro:  return Shader.Find(Nine3Prefix + "Poiyomi Pro");
				case PoiyomiEdition.Toon: return Shader.Find(Nine3Prefix + "Poiyomi Toon");
				default: return null;
			}
		}

		// Nearest 9.3 variant of the same edition by edit distance, accepted only if reasonably close.
		static Shader FuzzyMatchNine3(string variant)
		{
			bool wantPro = variant.IndexOf(" Pro", StringComparison.OrdinalIgnoreCase) != -1;
			string best = null;
			int bestDist = int.MaxValue;

			foreach (ShaderInfo si in ShaderUtil.GetAllShaderInfo())
			{
				if (si.name == null || !si.name.StartsWith(Nine3Prefix, StringComparison.Ordinal))
					continue;

				string candVariant = LegacyMaterialDetector.GetVariantFromName(si.name);
				if (string.IsNullOrEmpty(candVariant))
					continue;

				bool candPro = candVariant.IndexOf(" Pro", StringComparison.OrdinalIgnoreCase) != -1;
				if (candPro != wantPro)
					continue; // never cross Toon <-> Pro

				int d = Levenshtein(variant, candVariant);
				if (d < bestDist)
				{
					bestDist = d;
					best = si.name;
				}
			}

			if (best != null && bestDist < variant.Length * 0.5f)
				return Shader.Find(best);
			return null;
		}

		// --- Warnings ----------------------------------------------------------------------------------

		static void WarnAboutDroppedFeatures(Material material, LegacyMaterialInfo info)
		{
			if (info.UsesDps)
				ThryLogger.LogWarn($"<b>{material.name}</b>: the built-in DPS penetrator config can't carry to 9.3 " +
					"(DPS was removed in favour of TPS/SPS). All other settings are preserved.");

			if (info.UsesLegacyAnisoNoise)
				ThryLogger.LogWarn($"<b>{material.name}</b>: the legacy anisotropic-noise map can't carry to 9.3 " +
					"(anisotropy was reworked). All other settings are preserved.");
		}

		// --- Small edit-distance helper (kept local to avoid coupling to Thry internals) ----------------

		static int Levenshtein(string a, string b)
		{
			if (string.IsNullOrEmpty(a)) return string.IsNullOrEmpty(b) ? 0 : b.Length;
			if (string.IsNullOrEmpty(b)) return a.Length;

			int[] prev = new int[b.Length + 1];
			int[] curr = new int[b.Length + 1];
			for (int j = 0; j <= b.Length; j++) prev[j] = j;

			for (int i = 1; i <= a.Length; i++)
			{
				curr[0] = i;
				for (int j = 1; j <= b.Length; j++)
				{
					int cost = a[i - 1] == b[j - 1] ? 0 : 1;
					curr[j] = Mathf.Min(Mathf.Min(curr[j - 1] + 1, prev[j] + 1), prev[j - 1] + cost);
				}
				var tmp = prev; prev = curr; curr = tmp;
			}
			return prev[b.Length];
		}
	}
}
