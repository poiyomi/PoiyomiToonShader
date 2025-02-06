using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class VectorLabelDrawer : MaterialPropertyDrawer
    {
        string[] _labelStrings = new string[4] { "X", "Y", "Z", "W" };
        int vectorChannels = 0;

        public VectorLabelDrawer(string labelX, string labelY)
        {
            _labelStrings[0] = labelX;
            _labelStrings[1] = labelY;
            vectorChannels = 2;
        }

        public VectorLabelDrawer(string labelX, string labelY, string labelZ)
        {
            _labelStrings[0] = labelX;
            _labelStrings[1] = labelY;
            _labelStrings[2] = labelZ;
            vectorChannels = 3;
        }

        public VectorLabelDrawer(string labelX, string labelY, string labelZ, string labelW)
        {
            _labelStrings[0] = labelX;
            _labelStrings[1] = labelY;
            _labelStrings[2] = labelZ;
            _labelStrings[3] = labelW;
            vectorChannels = 4;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMixedValue;

            Rect labelR = new Rect(position.x, position.y, position.width * 0.41f, position.height);
            Rect contentR = new Rect(position.x + labelR.width, position.y, position.width - labelR.width, position.height);

            float[] values = new float[vectorChannels];
            GUIContent[] labels = new GUIContent[vectorChannels];

            for (int i = 0; i < vectorChannels; i++)
            {
                values[i] = prop.vectorValue[i];
                labels[i] = new GUIContent(_labelStrings[i]);
            }

            EditorGUI.LabelField(labelR, label);
            EditorGUI.MultiFloatField(contentR, labels, values);

            if (EditorGUI.EndChangeCheck())
            {
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