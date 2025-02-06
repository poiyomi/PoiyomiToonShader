using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class ThryDecalPositioningDecorator : MaterialPropertyDrawer
    {
        string _texturePropertyName;
        string _uvIndexPropertyName;
        string _positionPropertyName;
        string _rotationPropertyName;
        string _scalePropertyName;
        string _offsetPropertyName;
        DecalSceneTool _sceneTool;
        DecalTool _tool;

        public ThryDecalPositioningDecorator(string textureProp, string uvIndexPropertyName, string positionProp, string rotationProp, string scaleProp, string offsetProp)
        {
            _texturePropertyName = textureProp;
            _uvIndexPropertyName = uvIndexPropertyName;
            _positionPropertyName = positionProp;
            _rotationPropertyName = rotationProp;
            _offsetPropertyName = offsetProp;
            _scalePropertyName = scaleProp;
        }

        void CreateSceneTool()
        {
            DiscardSceneTool();
            _sceneTool = DecalSceneTool.Create(
                Selection.activeTransform.GetComponent<Renderer>(),
                ShaderEditor.Active.Materials[0],
                (int)ShaderEditor.Active.PropertyDictionary[_uvIndexPropertyName].MaterialProperty.GetNumber(),
                ShaderEditor.Active.PropertyDictionary[_positionPropertyName].MaterialProperty,
                ShaderEditor.Active.PropertyDictionary[_rotationPropertyName].MaterialProperty,
                ShaderEditor.Active.PropertyDictionary[_scalePropertyName].MaterialProperty,
                ShaderEditor.Active.PropertyDictionary[_offsetPropertyName].MaterialProperty);
        }

        void DiscardSceneTool(bool discardChanges = false)
        {
            if (_sceneTool != null)
            {
                _sceneTool.Deactivate(discardChanges);
                _sceneTool = null;
            }
        }

        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            position = new RectOffset(0, 0, 0, 3).Remove(EditorGUI.IndentedRect(position));
            bool isInScene = Selection.activeTransform != null && Selection.activeTransform.GetComponent<Renderer>() != null;
            if (isInScene)
            {
                position.width /= 3;
                ButtonGUI(position);
                position.x += position.width;
                ButtonRaycast(position);
                position.x += position.width;
                ButtonSceneTools(position);
                if (_sceneTool != null)
                {
                    _sceneTool.SetMaterialProperties(
                        ShaderEditor.Active.PropertyDictionary[_positionPropertyName].MaterialProperty,
                        ShaderEditor.Active.PropertyDictionary[_rotationPropertyName].MaterialProperty,
                        ShaderEditor.Active.PropertyDictionary[_scalePropertyName].MaterialProperty,
                        ShaderEditor.Active.PropertyDictionary[_offsetPropertyName].MaterialProperty);
                    if(Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Escape)
                        DiscardSceneTool(true);
                }
            }
            else
            {
                ButtonGUI(position);
            }
        }

        void ButtonGUI(Rect r)
        {
            if (GUI.Button(r, "Open Positioning Tool"))
            {
                _tool = DecalTool.OpenDecalTool(ShaderEditor.Active.Materials[0]);
            }
            // This is done because the tool didnt want to update if the data was changed from the outside
            if (_tool != null)
            {
                _tool.SetMaterialProperties(
                    ShaderEditor.Active.PropertyDictionary[_texturePropertyName].MaterialProperty,
                    ShaderEditor.Active.PropertyDictionary[_uvIndexPropertyName].MaterialProperty,
                    ShaderEditor.Active.PropertyDictionary[_positionPropertyName].MaterialProperty,
                    ShaderEditor.Active.PropertyDictionary[_rotationPropertyName].MaterialProperty,
                    ShaderEditor.Active.PropertyDictionary[_scalePropertyName].MaterialProperty,
                    ShaderEditor.Active.PropertyDictionary[_offsetPropertyName].MaterialProperty);
            }
        }

        void ButtonRaycast(Rect r)
        {
            if (GUI.Button(r, "Raycast"))
            {
                if (_sceneTool != null && _sceneTool.GetMode() == DecalSceneTool.Mode.Raycast)
                {
                    DiscardSceneTool(true);
                }
                else
                {
                    CreateSceneTool();
                    _sceneTool.StartRaycastMode();
                }
            }
            if (_sceneTool != null && _sceneTool.GetMode() == DecalSceneTool.Mode.Raycast)
                GUI.DrawTexture(r, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, new Color(0.5f, 0.5f, 0.5f, 0.5f), 0, 3);
        }

        void ButtonSceneTools(Rect r)
        {
            if (GUI.Button(r, "Scene Tools"))
            {
                if (_sceneTool != null && _sceneTool.GetMode() == DecalSceneTool.Mode.Handles)
                {
                    DiscardSceneTool();
                }
                else
                {
                    CreateSceneTool();
                    _sceneTool.StartHandleMode();
                }
            }
            if (_sceneTool != null && _sceneTool.GetMode() == DecalSceneTool.Mode.Handles)
                GUI.DrawTexture(r, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, new Color(0.5f, 0.5f, 0.5f, 0.5f), 0, 3);
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            ShaderProperty.RegisterDecorator(this);
            return EditorGUIUtility.singleLineHeight + 6;
        }
    }

}