using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Thry.ThryEditor.Helpers;

namespace Poi.Tools.ShaderTranslator.VersionUpgrade
{
	public interface IPoiyomiVersionUpgrade
	{
		Version GetSourceVersion();
		Version GetTargetVersion();
	}

	public static class PoiyomiVersionUpgradeController
	{
		static readonly List<ScriptedShaderTranslator> Translators = new List<ScriptedShaderTranslator>
		{
			new PoiyomiUpgrade_9_3_to_10_0()
		};

		public static bool UpgradeToLatest(Material material, bool deferFinalShaderSwap, out Shader finalShader)
		{
			finalShader = null;

			if (material == null)
				return false;

			if (!PoiyomiVersionDetector.IsPoiyomiShader(material))
			{
				ThryLogger.LogWarn($"Material <b>{material.name}</b> is not using a Poiyomi shader.");
				return false;
			}

			if (!PoiyomiVersionDetector.TryGetVersion(material, out Version startVersion))
			{
				ThryLogger.LogWarn($"Could not detect version for material <b>{material.name}</b>.");
				return false;
			}

			if (startVersion >= PoiyomiVersionDetector.LatestVersion)
			{
				ThryLogger.LogWarn($"Material <b>{material.name}</b> is already on the latest version ({startVersion}).");
				return false;
			}

			ThryLogger.Log($"Upgrading material <b>{material.name}</b> from version {startVersion} to {PoiyomiVersionDetector.LatestVersion}");

			// Locked (optimized) materials carry a generated Hidden/Locked shader; translating against that is unsafe,
			// so unlock first. The values we need are preserved on the material across the unlock.
			UnlockIfNeeded(material);

			List<ScriptedShaderTranslator> translatorChain = BuildChain(startVersion, PoiyomiVersionDetector.LatestVersion);

			if (translatorChain.Count == 0)
			{
				ThryLogger.LogWarn($"No translators available for material <b>{material.name}</b> at version {startVersion}.");
				return false;
			}

			// The final shader is the target of the last step in the chain.
			finalShader = translatorChain[translatorChain.Count - 1].ResolveTargetShader(material, string.Empty);

			// Run all translations with deferred shader swap to avoid repeated compilation. The material starts on its
			// current (source) shader, so that is the initial source layout for the chain.
			ExecuteChain(material, translatorChain, material.shader);

			// Apply final shader swap unless caller wants to handle it
			if (!deferFinalShaderSwap && finalShader != null)
			{
				ScriptedShaderTranslator.ApplyDeferredShaderSwap(material, finalShader);
				ThryLogger.Log($"Material <b>{material.name}</b> upgraded to version {PoiyomiVersionDetector.LatestVersion}");
			}

			return true;
		}

		public static bool UpgradeToLatest(Material material) => UpgradeToLatest(material, false, out _);

		// <summary>
		/// Upgrades a material whose shader has <b>already</b> been swapped to a newer Poiyomi version (e.g. via the
		/// inspector's shader dropdown). The old property values are still serialized on the material, so the source
		/// layout is read from <paramref name="oldShader"/> and no further shader swap is performed.
		/// </summary>
		/// <param name="material">The material that was swapped. Expected to already reference <paramref name="newShader"/>.</param>
		/// <param name="oldShader">The Poiyomi shader the material used before the swap.</param>
		/// <param name="newShader">The Poiyomi shader the material now uses.</param>
		/// <returns>True if a property upgrade was run.</returns>
		public static bool UpgradeAcrossShaderSwap(Material material, Shader oldShader, Shader newShader)
		{
			if (material == null || oldShader == null || newShader == null || oldShader == newShader)
				return false;

			// Never touch a locked/optimized material here. Its real values are baked into the generated shader and
      		// stripped from the material, so translating against the live layout scrambles them - properties land on the
      		// wrong slots and feature toggles flip on at random. The menu-driven UpgradeToLatest path unlocks first;
      		// the inspector-swap path must simply leave locked materials alone.
			if (Thry.ThryEditor.ShaderOptimizer.IsMaterialLocked(material))
			{
				ThryLogger.LogWarn($"Skipping auto-upgrade for <b>{material.name}</b>: the material is locked. Unlock it before changing shaders to upgrade it.");
				return false;
			}

			// Stay within the same shader variant, with one exception: a Toon <-> Pro edition swap of the *same* sub-variant
			// (e.g. "Poiyomi Toon" -> "Poiyomi Pro")  with shared properties should have their values translate cleanly as
			// a legitimate user choice. Any other variant change maps values onto the wrong properties and enables features
			// at random, so leave those untouched and let the user set them up.
			string oldVariant = PoiyomiVersionDetector.GetShaderVariant(oldShader);
			string newVariant = PoiyomiVersionDetector.GetShaderVariant(newShader);
			bool sameVariant = string.Equals(oldVariant, newVariant, StringComparison.OrdinalIgnoreCase);
			bool editionSwap = !sameVariant && PoiyomiVersionDetector.IsEditionSwap(oldShader, newShader);
			if (!sameVariant && !editionSwap)
			{
				ThryLogger.LogWarn($"Skipping auto-upgrade for <b>{material.name}</b>: shader variant changed (<b>{oldVariant}</b> -> <b>{newVariant}</b>). This is a variant change, not a version upgrade, so settings were left untouched.");
				return false;
			}

			if (!PoiyomiVersionDetector.TryGetVersionFromShader(oldShader, out Version oldVersion))
				return false;

			if (!PoiyomiVersionDetector.TryGetVersionFromShader(newShader, out Version newVersion))
				return false;

			// Only run for forward upgrades. Downgrades and same-version swaps are left untouched.
			if (oldVersion >= newVersion)
				return false;

			List<ScriptedShaderTranslator> translatorChain = BuildChain(oldVersion, newVersion);
			if (translatorChain.Count == 0)
				return false;

			ThryLogger.Log($"Auto-upgrading material <b>{material.name}</b> from version {oldVersion} to {newVersion} after shader swap{(editionSwap ? $" (edition change: {oldVariant} -> {newVariant})" : "")}.");

			// The material already references newShader, so read the source layout from oldShader and don't swap again.
			ExecuteChain(material, translatorChain, oldShader);

			// Material already points at the target shader - this just preserves the render queue and fixes keywords.
			ScriptedShaderTranslator.ApplyDeferredShaderSwap(material, newShader);

			return true;
		}

		/// <summary>
		/// Builds the ordered list of translators needed to walk from <paramref name="startVersion"/> up to (and no
		/// further than) <paramref name="targetCap"/>. Guards against non-advancing/looping translators.
		/// </summary>
		static List<ScriptedShaderTranslator> BuildChain(Version startVersion, Version targetCap)
		{
			var chain = new List<ScriptedShaderTranslator>();
			Version simVersion = startVersion;
			int guard = 0;

			while (simVersion < targetCap)
			{
				// Worst case the chain visits every translator exactly once; anything beyond that is a loop.
				if (++guard > Translators.Count + 1)
				{
					ThryLogger.LogWarn($"Aborting upgrade chain for start version {startVersion} - exceeded maximum steps. Check translator source/target versions for a loop.");
					break;
				}

				ScriptedShaderTranslator translator = FindTranslatorForVersion(simVersion);
				if (translator == null)
					break;

				Version next = ((IPoiyomiVersionUpgrade)translator).GetTargetVersion();

				// A translator that doesn't advance the version would loop forever.
				if (next <= simVersion)
				{
					ThryLogger.LogWarn($"Translator <b>{translator.GetType().Name}</b> does not advance the version ({simVersion} -> {next}); stopping chain.");
					break;
				}

				// Don't overshoot the requested target version.
				if (next > targetCap)
					break;

				chain.Add(translator);
				simVersion = next;
			}

			return chain;
		}

		/// <summary>
		/// Runs each translator in the chain with a deferred shader swap, threading the resolved target of each step in
		/// as the source layout of the next so multi-step chains read the correct properties.
		/// </summary>
		static void ExecuteChain(Material material, List<ScriptedShaderTranslator> chain, Shader initialSourceShader)
		{
			Shader currentSource = initialSourceShader;
			foreach (var translator in chain)
			{
				Shader stepTarget = translator.ResolveTargetShader(material, string.Empty);
				translator.Translate(material, string.Empty, true, currentSource);
				if (stepTarget != null)
					currentSource = stepTarget;
			}
		}

		static void UnlockIfNeeded(Material material)
		{
			if (material != null && Thry.ThryEditor.ShaderOptimizer.IsMaterialLocked(material)) Thry.ThryEditor.ShaderOptimizer.UnlockMaterials(new[] { material });
		}

		static ScriptedShaderTranslator FindTranslatorForVersion(Version version)
		{
			foreach (var translator in Translators)
			{
				if (translator is IPoiyomiVersionUpgrade upgrade)
				{
					var src = upgrade.GetSourceVersion();
					if (version.Major == src.Major && version.Minor == src.Minor)
						return translator;
				}
			}
			return null;
		}

		public static void UpgradeMaterials(IEnumerable<Material> materials)
		{
			var materialList = new List<Material>(materials);
			var pendingSwaps = new List<(Material mat, Shader shader)>();

			try
			{
				// Unlock any locked materials up front in a single pass rather than one at a time.
				var lockedMaterials = materialList.Where(m => m != null && Thry.ThryEditor.ShaderOptimizer.IsMaterialLocked(m)).ToArray();
				if (lockedMaterials.Length > 0) Thry.ThryEditor.ShaderOptimizer.UnlockMaterials(lockedMaterials);

				for (int i = 0; i < materialList.Count; i++)
				{
					var material = materialList[i];
					if (material == null)
						continue;

					EditorUtility.DisplayProgressBar("Upgrading Materials",
						$"Processing {material.name} ({i + 1}/{materialList.Count})",
						(float)i / materialList.Count);

					if (UpgradeToLatest(material, true, out Shader finalShader) && finalShader != null)
						pendingSwaps.Add((material, finalShader));
				}

				EditorUtility.DisplayProgressBar("Upgrading Materials",
					$"Applying shader swaps ({pendingSwaps.Count} materials)",
					0.95f);

				// Apply all shader swaps at the end to avoid repeated compilation
				foreach (var (mat, shader) in pendingSwaps)
				{
					ScriptedShaderTranslator.ApplyDeferredShaderSwap(mat, shader);
					ThryLogger.Log($"Applied deferred shader swap for {mat.name}");
				}
			}
			finally
			{
				EditorUtility.ClearProgressBar();
			}

			ThryLogger.Log($"Upgraded {pendingSwaps.Count} materials");
		}
	}
}
