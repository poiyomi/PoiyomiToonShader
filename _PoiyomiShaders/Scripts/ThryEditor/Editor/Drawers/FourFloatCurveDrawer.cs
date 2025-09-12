using UnityEditor;
using UnityEngine;
using Thry.ThryEditor.Helpers;

namespace Thry.ThryEditor.Drawers
{
    // Usage in shader:
    // [Curve4] _MyFourFloatCurve ("My Curve (4 samples)", Vector) = (1,1,1,1)
    // This drawer shows a CurveField and bakes 4 evenly spaced samples (0, 1/3, 2/3, 1)
    // into the Vector4 material property. Runtime code can evaluate with piecewise-linear.
    public class Curve4Drawer : MaterialPropertyDrawer
    {
        private AnimationCurve _curve = new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(1f, 1f)
        );

        // Guard to re-sync the curve from the underlying vector when first drawn
        private bool _initializedFromProperty = false;

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            if (prop.type != MaterialProperty.PropType.Vector)
            {
                EditorGUI.HelpBox(position, "[Curve4] requires a Vector property (stores 4 samples)", MessageType.Warning);
                return;
            }

            // Initialize curve from the current Vector4 values once
            if (!_initializedFromProperty)
            {
                Vector4 v = prop.vectorValue;
                _curve = new AnimationCurve(
                    new Keyframe(0f, v.x),
                    new Keyframe(1f / 3f, v.y),
                    new Keyframe(2f / 3f, v.z),
                    new Keyframe(1f, v.w)
                );
                _initializedFromProperty = true;
            }

            // Draw label in the label area (left column)
            Rect labelRect = new Rect(
                position.x,
                position.y,
                EditorGUIUtility.labelWidth - 15,
                position.height
            );
            EditorGUI.LabelField(labelRect, label);

            // Match Thry layout: draw within value area to avoid overlapping labels/columns
            Rect valueRect = new Rect(
                position.x + EditorGUIUtility.labelWidth - 15,
                position.y,
                position.width - EditorGUIUtility.labelWidth + 15 - GUILib.GetSmallTextureVRAMWidth(prop),
                position.height
            );

            // Let Thry render the label; we only draw the field in the valueRect
            EditorGUI.BeginChangeCheck();
            var newCurve = EditorGUI.CurveField(valueRect, _curve);
            if (EditorGUI.EndChangeCheck())
            {
                _curve = newCurve;
                BakeCurveToVector(prop);
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            ShaderProperty.RegisterDrawer(this);
            return base.GetPropertyHeight(prop, label, editor);
        }

        private void BakeCurveToVector(MaterialProperty prop)
        {
            // Sample at fixed times for compact storage
            float s0 = Mathf.Clamp01(_curve.Evaluate(0f));
            float s1 = Mathf.Clamp01(_curve.Evaluate(1f / 3f));
            float s2 = Mathf.Clamp01(_curve.Evaluate(2f / 3f));
            float s3 = Mathf.Clamp01(_curve.Evaluate(1f));

            prop.vectorValue = new Vector4(s0, s1, s2, s3);
        }
    }
}


