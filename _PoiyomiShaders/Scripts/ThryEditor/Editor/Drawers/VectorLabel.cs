using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Thry.ThryEditor.Drawers
{
    public class VectorLabelDrawer : MaterialPropertyDrawer
    {
        string[] _labelStrings = new string[4] { "X", "Y", "Z", "W" };
        int vectorChannels = 0;
		bool _useLink = false;

		enum LinkMode { Off, Ratio, Delta }
		static readonly Dictionary<string, LinkMode> s_LinkStates = new Dictionary<string, LinkMode>();
		static readonly Dictionary<string, Vector4> s_RatioBaselines = new Dictionary<string, Vector4>();

		static Texture2D s_IconOff;
		static Texture2D s_IconRatio;
		static Texture2D s_IconDelta;
		static bool s_IconsLoaded = false;

		static void EnsureIconsLoaded()
		{
			if (s_IconsLoaded) return;
			s_IconOff = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(Thry.ThryEditor.RESOURCE_GUID.ICON_LINK_OFF));
			s_IconRatio = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(Thry.ThryEditor.RESOURCE_GUID.ICON_LINK_RATIO));
			s_IconDelta = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(Thry.ThryEditor.RESOURCE_GUID.ICON_LINK_DELTA));
			s_IconsLoaded = true;
		}

		void ProcessArgsAndLabels(params string[] args)
		{
			int labelIndex = 0;
			_useLink = false;
			for (int i = 0; i < args.Length && labelIndex < 4; i++)
			{
				string arg = args[i];
				if (!string.IsNullOrEmpty(arg) && arg.Trim().ToLowerInvariant() == "link")
				{
					_useLink = true;
					continue;
				}
				_labelStrings[labelIndex++] = arg;
			}
			vectorChannels = Mathf.Clamp(labelIndex, 0, 4);
		}

        public VectorLabelDrawer(string labelX, string labelY)
        {
            ProcessArgsAndLabels(labelX, labelY);
        }

        public VectorLabelDrawer(string labelX, string labelY, string labelZ)
        {
            ProcessArgsAndLabels(labelX, labelY, labelZ);
        }

        public VectorLabelDrawer(string labelX, string labelY, string labelZ, string labelW)
        {
            ProcessArgsAndLabels(labelX, labelY, labelZ, labelW);
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            EnsureIconsLoaded();

            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMixedValue;
            if (_useLink && !s_RatioBaselines.ContainsKey(prop.name)) s_RatioBaselines[prop.name] = prop.vectorValue;

            Rect fieldR = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
			Rect linkR = Rect.zero;
			if (_useLink)
			{
				float linkW = 20f;
				linkR = new Rect(fieldR.x + 2f, position.y + 2f, 18f, position.height - 4f);
				fieldR.xMin += linkW + 2f;
			}

            float[] values = new float[vectorChannels];
            GUIContent[] labels = new GUIContent[vectorChannels];

            for (int i = 0; i < vectorChannels; i++)
            {
                values[i] = prop.vectorValue[i];
                labels[i] = new GUIContent(_labelStrings[i]);
            }

			// Link toggle UI (placed at the start of content area)
			if (_useLink)
			{
				if (!s_LinkStates.ContainsKey(prop.name)) s_LinkStates[prop.name] = LinkMode.Off;
				LinkMode mode = s_LinkStates[prop.name];
				Texture2D icon = s_IconOff;
				string tooltip = "Mode: Off\nOnly the value you edit will change.\nClick to switch: Off → Ratio → Delta";
				Color prevColor = GUI.color;
				switch (mode)
				{
					case LinkMode.Ratio:
						icon = s_IconRatio;
						tooltip = "Mode: Ratio (like Transform Scale)\nChanging one scales all values by the same factor.\nClick to switch: Off → Ratio → Delta";
						GUI.color = new Color(102f/255f, 217f/255f, 239f/255f);
						break;
					case LinkMode.Delta:
						icon = s_IconDelta;
						tooltip = "Mode: Delta\nChanging one adds the same amount to all values.\nExample: +0.10 adds +0.10 to X/Y/Z/W.\nClick to switch: Off → Ratio → Delta";
						GUI.color = new Color(166f/255f, 226f/255f, 46f/255f);
						break;
					default:
						icon = s_IconOff;
						tooltip = "Mode: Off\nOnly the value you edit will change.\nClick to switch: Off → Ratio → Delta";
						break;
				}
				if (GUI.Button(linkR, new GUIContent(icon, tooltip), GUIStyle.none))
				{
					mode = (LinkMode)(((int)mode + 1) % 3);
					s_LinkStates[prop.name] = mode;
				}
				GUI.color = prevColor;
			}
            Vector4 oldV = prop.vectorValue;
            Color prevContentColor = GUI.contentColor;
            if (_useLink && s_LinkStates.TryGetValue(prop.name, out var modeForColor) && modeForColor != LinkMode.Off)
            {
                if (modeForColor == LinkMode.Ratio)
                    GUI.contentColor = new Color(102f/255f, 217f/255f, 239f/255f);
                else if (modeForColor == LinkMode.Delta)
                    GUI.contentColor = new Color(166f/255f, 226f/255f, 46f/255f);
            }
            EditorGUI.MultiFloatField(fieldR, labels, values);
            GUI.contentColor = prevContentColor;

            if (EditorGUI.EndChangeCheck())
            {
				// Apply link behavior if enabled
				if (_useLink && s_LinkStates.TryGetValue(prop.name, out var mode) && mode != LinkMode.Off)
				{
					int changedIndex = -1;
					float maxDelta = 0f;
					for (int i = 0; i < vectorChannels; i++)
					{
						float d = values[i] - oldV[i];
						float ad = Mathf.Abs(d);
						if (ad > maxDelta && ad > 0.000001f)
						{
							maxDelta = ad;
							changedIndex = i;
						}
					}
					if (changedIndex != -1)
					{
						if (mode == LinkMode.Delta)
						{
							float delta = values[changedIndex] - oldV[changedIndex];
							for (int j = 0; j < vectorChannels; j++)
								values[j] = oldV[j] + delta;
						}
						else if (mode == LinkMode.Ratio)
						{
							float denom = Mathf.Abs(oldV[changedIndex]);
							float newAbs = Mathf.Abs(values[changedIndex]);
							if (newAbs <= 0.000001f)
							{
								// Edited component at ~0: keep others unchanged; don't update baseline
								for (int j = 0; j < vectorChannels; j++) if (j != changedIndex) values[j] = oldV[j];
							}
							else if (denom > 0.000001f)
							{
								// Normal proportional scaling from current value
								float factor = values[changedIndex] / oldV[changedIndex];
								for (int j = 0; j < vectorChannels; j++) values[j] = oldV[j] * factor;
								// Update baseline to new scaled shape
								s_RatioBaselines[prop.name] = new Vector4(values[0], vectorChannels > 1 ? values[1] : 0f, vectorChannels > 2 ? values[2] : 0f, vectorChannels > 3 ? values[3] : 0f);

							}
							else
							{
								// Leaving ~0: use stored baseline to restore proportional scaling
								Vector4 baseline;
								if (!s_RatioBaselines.TryGetValue(prop.name, out baseline)) baseline = oldV;
								float baseDenom = Mathf.Abs(baseline[changedIndex]);
								if (baseDenom > 0.000001f)
								{
									float factor = values[changedIndex] / baseline[changedIndex];
									for (int j = 0; j < vectorChannels; j++) values[j] = baseline[j] * factor;
									// Update baseline to new scaled shape
									s_RatioBaselines[prop.name] = new Vector4(values[0], vectorChannels > 1 ? values[1] : 0f, vectorChannels > 2 ? values[2] : 0f, vectorChannels > 3 ? values[3] : 0f);
								}
								else
								{
									// No meaningful baseline for this component; keep others unchanged
									for (int j = 0; j < vectorChannels; j++) if (j != changedIndex) values[j] = oldV[j];
								}
							}
						}
					}
				}

                switch (vectorChannels)
                {
                    case 2:
                        prop.vectorValue = new Vector4(values[0], values[1], prop.vectorValue.z, prop.vectorValue.w);
                        break;
                    case 3:
                        prop.vectorValue = new Vector4(values[0], values[1], values[2], prop.vectorValue.w);
                        break;
                    case 4:
                        prop.vectorValue = new Vector4(values[0], values[1], values[2], values[3]);
                        break;
                    default:
                        break;
                }
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            ShaderProperty.RegisterDrawer(this);
            return base.GetPropertyHeight(prop, label, editor);
        }
    }

}