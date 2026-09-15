using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Poiyomi.ModularShaderSystem
{
	[Serializable]
	public class ModuleTogglePreferences
	{
		private const string PrefsFileName = "PoiModuleTogglePrefs.json";
		private static string PrefsFilePath => Path.Combine(Application.dataPath, "..", "Library", PrefsFileName);
		
		private static ModuleTogglePreferences _instance;
		public static ModuleTogglePreferences Instance
		{
			get
			{
				if (_instance == null)
					_instance = Load();
				return _instance;
			}
		}
		
		[Serializable]
		public class AssetToggles
		{
			public string assetGuid;
			public List<bool> toggles = new List<bool>();
		}
		
		public List<AssetToggles> modularShaderToggles = new List<AssetToggles>();
		public List<AssetToggles> moduleCollectionToggles = new List<AssetToggles>();
		
		public static List<bool> GetToggles(UnityEngine.Object asset, int moduleCount)
		{
			if (asset == null) return CreateAllEnabled(moduleCount);
			if (!UnityEditorInternal.InternalEditorUtility.CurrentThreadIsMainThread()) return CreateAllEnabled(moduleCount);
			
			string path = AssetDatabase.GetAssetPath(asset);
			string guid = AssetDatabase.AssetPathToGUID(path);
			if (string.IsNullOrEmpty(guid)) return CreateAllEnabled(moduleCount);
			
			List<AssetToggles> togglesList = asset is ModularShader 
				? Instance.modularShaderToggles 
				: Instance.moduleCollectionToggles;
			
			var existing = togglesList.Find(t => t.assetGuid == guid);
			if (existing != null)
			{
				while (existing.toggles.Count < moduleCount)
					existing.toggles.Add(true);
				while (existing.toggles.Count > moduleCount)
					existing.toggles.RemoveAt(existing.toggles.Count - 1);
				return existing.toggles;
			}
			
			var newToggles = new AssetToggles { assetGuid = guid, toggles = CreateAllEnabled(moduleCount) };
			togglesList.Add(newToggles);
			return newToggles.toggles;
		}
		
		public static void SetToggle(UnityEngine.Object asset, int index, bool value)
		{
			if (asset == null) return;
			
			string path = AssetDatabase.GetAssetPath(asset);
			string guid = AssetDatabase.AssetPathToGUID(path);
			if (string.IsNullOrEmpty(guid)) return;
			
			List<AssetToggles> togglesList = asset is ModularShader 
				? Instance.modularShaderToggles 
				: Instance.moduleCollectionToggles;
			
			var existing = togglesList.Find(t => t.assetGuid == guid);
			if (existing == null)
			{
				existing = new AssetToggles { assetGuid = guid, toggles = new List<bool>() };
				togglesList.Add(existing);
			}
			
			while (existing.toggles.Count <= index)
				existing.toggles.Add(true);
			
			existing.toggles[index] = value;
			Save();
		}
		
		public static void SetAllToggles(UnityEngine.Object asset, int count, bool value)
		{
			if (asset == null) return;
			
			string path = AssetDatabase.GetAssetPath(asset);
			string guid = AssetDatabase.AssetPathToGUID(path);
			if (string.IsNullOrEmpty(guid)) return;
			
			List<AssetToggles> togglesList = asset is ModularShader 
				? Instance.modularShaderToggles 
				: Instance.moduleCollectionToggles;
			
			var existing = togglesList.Find(t => t.assetGuid == guid);
			if (existing == null)
			{
				existing = new AssetToggles { assetGuid = guid, toggles = new List<bool>() };
				togglesList.Add(existing);
			}
			
			existing.toggles.Clear();
			for (int i = 0; i < count; i++)
				existing.toggles.Add(value);
			
			Save();
		}
		
		public static void MoveToggle(UnityEngine.Object asset, int fromIndex, int toIndex)
		{
			if (asset == null) return;
			
			string path = AssetDatabase.GetAssetPath(asset);
			string guid = AssetDatabase.AssetPathToGUID(path);
			if (string.IsNullOrEmpty(guid)) return;
			
			List<AssetToggles> togglesList = asset is ModularShader 
				? Instance.modularShaderToggles 
				: Instance.moduleCollectionToggles;
			
			var existing = togglesList.Find(t => t.assetGuid == guid);
			if (existing == null || fromIndex >= existing.toggles.Count) return;
			
			bool value = existing.toggles[fromIndex];
			existing.toggles.RemoveAt(fromIndex);
			if (toIndex > existing.toggles.Count)
				toIndex = existing.toggles.Count;
			existing.toggles.Insert(toIndex, value);
			Save();
		}
		
		public static void RemoveToggle(UnityEngine.Object asset, int index)
		{
			if (asset == null) return;
			
			string path = AssetDatabase.GetAssetPath(asset);
			string guid = AssetDatabase.AssetPathToGUID(path);
			if (string.IsNullOrEmpty(guid)) return;
			
			List<AssetToggles> togglesList = asset is ModularShader 
				? Instance.modularShaderToggles 
				: Instance.moduleCollectionToggles;
			
			var existing = togglesList.Find(t => t.assetGuid == guid);
			if (existing == null || index >= existing.toggles.Count) return;
			
			existing.toggles.RemoveAt(index);
			Save();
		}
		
		public static void InsertToggle(UnityEngine.Object asset, int index, bool value = true)
		{
			if (asset == null) return;
			
			string path = AssetDatabase.GetAssetPath(asset);
			string guid = AssetDatabase.AssetPathToGUID(path);
			if (string.IsNullOrEmpty(guid)) return;
			
			List<AssetToggles> togglesList = asset is ModularShader 
				? Instance.modularShaderToggles 
				: Instance.moduleCollectionToggles;
			
			var existing = togglesList.Find(t => t.assetGuid == guid);
			if (existing == null)
			{
				existing = new AssetToggles { assetGuid = guid, toggles = new List<bool>() };
				togglesList.Add(existing);
			}
			
			while (existing.toggles.Count < index)
				existing.toggles.Add(true);
			
			existing.toggles.Insert(index, value);
			Save();
		}
		
		public static void DuplicateToggle(UnityEngine.Object asset, int index)
		{
			if (asset == null) return;
			
			string path = AssetDatabase.GetAssetPath(asset);
			string guid = AssetDatabase.AssetPathToGUID(path);
			if (string.IsNullOrEmpty(guid)) return;
			
			List<AssetToggles> togglesList = asset is ModularShader 
				? Instance.modularShaderToggles 
				: Instance.moduleCollectionToggles;
			
			var existing = togglesList.Find(t => t.assetGuid == guid);
			if (existing == null || index >= existing.toggles.Count) return;
			
			existing.toggles.Insert(index + 1, existing.toggles[index]);
			Save();
		}
		
		private static List<bool> CreateAllEnabled(int count)
		{
			var list = new List<bool>();
			for (int i = 0; i < count; i++)
				list.Add(true);
			return list;
		}
		
		private static ModuleTogglePreferences Load()
		{
			try
			{
				if (File.Exists(PrefsFilePath))
				{
					string json = File.ReadAllText(PrefsFilePath);
					return JsonUtility.FromJson<ModuleTogglePreferences>(json) ?? new ModuleTogglePreferences();
				}
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogWarning($"Failed to load module toggle preferences: {e.Message}");
			}
			return new ModuleTogglePreferences();
		}
		
		private static void Save()
		{
			try
			{
				string json = JsonUtility.ToJson(_instance, true);
				File.WriteAllText(PrefsFilePath, json);
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogWarning($"Failed to save module toggle preferences: {e.Message}");
			}
		}
	}
}

