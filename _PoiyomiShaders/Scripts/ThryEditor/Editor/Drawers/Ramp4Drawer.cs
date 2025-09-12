using UnityEditor;
using UnityEngine;
using Thry.ThryEditor.Helpers;

namespace Thry.ThryEditor.Drawers
{
    // Usage in shader properties:
    // [Ramp4] _MyRamp ("My Ramp (v0,v1,t0,t1)", Vector) = (0,1,0.4,0.5)
    // [Ramp4(normalized)] _MyRamp ("My Ramp (v0,v1,t0,t1)", Vector) = (-2,3,0.4,0.5) // Shows normalized graph for values outside 0-1
    // Packs: x=startValue, y=endValue, z=startTime, w=endTime
    public class Ramp4Drawer : MaterialPropertyDrawer
    {
        private Texture2D _previewTex;
        private Vector4 _lastRamp;
        private int _activeHandle = -1; // 0 = t0, 1 = t1, 2 = v0, 3 = v1
        private const float ValueHandleMargin = 12f; // left/right margin reserved for value handles
        private bool _normalized; // Show normalized visualization for values outside 0-1

        public Ramp4Drawer() { }
        public Ramp4Drawer(string mode)
        {
            _normalized = mode.ToLower() == "normalized";
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            if (prop.type != MaterialProperty.PropType.Vector)
            {
                EditorGUI.HelpBox(position, "[Ramp4] requires a Vector property (x=v0,y=v1,z=t0,w=t1)", MessageType.Warning);
                return;
            }

            Vector4 v = prop.vectorValue; // x=v0, y=v1, z=t0, w=t1
            float v0 = v.x;
            float v1 = v.y;
            float t0 = Mathf.Clamp01(v.z);
            float t1 = Mathf.Clamp01(v.w);

            // Use PrefixLabel to get exact value column rect (right side)
            Rect valueRect = EditorGUI.PrefixLabel(position, label);
            int oldIndent = EditorGUI.indentLevel; EditorGUI.indentLevel = 0;

            // Split right area: preview (same width), slider, and numeric fields
            float rowH = Mathf.Max(EditorGUIUtility.singleLineHeight, 16f);
            Rect previewRect = new Rect(valueRect.x, valueRect.y, valueRect.width, rowH * 1.2f);
            // Integrate time controls into preview to save vertical space
            Rect fieldsRect = new Rect(valueRect.x, previewRect.y + previewRect.height + 4, valueRect.width, rowH);

            // Draw preview curve and interactive handles (time and value)
            DrawRampPreview(previewRect, new Vector4(v0, v1, t0, t1));
            HandlePreviewDrag(previewRect, ref t0, ref t1, ref v0, ref v1, prop);

            EditorGUI.BeginChangeCheck();
            // Times handled by drag handles in preview

            // Numeric fields: v0, v1, t0, t1
            float spacing = 2f;
            float col = (fieldsRect.width - spacing * 3f) / 4f;
            Rect f0 = new Rect(fieldsRect.x, fieldsRect.y, col, fieldsRect.height);
            Rect f1 = new Rect(f0.xMax + spacing, fieldsRect.y, col, fieldsRect.height);
            Rect f2 = new Rect(f1.xMax + spacing, fieldsRect.y, col, fieldsRect.height);
            // Make last field consume remaining width exactly
            Rect f3 = new Rect(f2.xMax + spacing, fieldsRect.y, Mathf.Max(0, fieldsRect.xMax - (f2.xMax + spacing)), fieldsRect.height);

            // Allow unbounded values when normalized mode is enabled
            if (_normalized)
            {
                v0 = EditorGUI.FloatField(f0, v0);
                v1 = EditorGUI.FloatField(f1, v1);
            }
            else
            {
                v0 = EditorGUI.FloatField(f0, v0);
                v1 = EditorGUI.FloatField(f1, v1);
            }
            t0 = Mathf.Clamp01(EditorGUI.FloatField(f2, t0));
            t1 = Mathf.Clamp01(EditorGUI.FloatField(f3, t1));

            // Ensure order
            if (t1 < t0)
            {
                float tmp = t0; t0 = t1; t1 = tmp;
            }

            if (EditorGUI.EndChangeCheck())
            {
                prop.vectorValue = new Vector4(v0, v1, t0, t1);
            }

            EditorGUI.indentLevel = oldIndent;
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            ShaderProperty.RegisterDrawer(this);
            // Two rows tall (preview + fields) plus bottom padding so it doesn't touch the next drawer
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            return EditorGUIUtility.singleLineHeight * 2f + spacing * 6f;
        }

        private void DrawRampPreview(Rect rect, Vector4 ramp)
        {
            // Background
            EditorGUI.DrawRect(rect, new Color(0.16f, 0.16f, 0.16f, 1f));
            // Define inner plotting rect excluding left/right handle margins
            Rect inner = new Rect(rect.x + ValueHandleMargin, rect.y, Mathf.Max(1f, rect.width - ValueHandleMargin * 2f), rect.height);
            // Subtle shading for handle gutters
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, ValueHandleMargin, rect.height), new Color(0.1f, 0.1f, 0.1f, 1f));
            EditorGUI.DrawRect(new Rect(rect.xMax - ValueHandleMargin, rect.y, ValueHandleMargin, rect.height), new Color(0.1f, 0.1f, 0.1f, 1f));

            // Calculate value range for normalized display
            float minVal = 0f, maxVal = 1f;
            if (_normalized)
            {
                minVal = Mathf.Min(ramp.x, ramp.y, 0f);
                maxVal = Mathf.Max(ramp.x, ramp.y, 1f);
                // Add padding for better visualization
                float range = maxVal - minVal;
                if (range > 0.001f)
                {
                    minVal -= range * 0.1f;
                    maxVal += range * 0.1f;
                }
            }

            // Draw reference lines for normalized mode
            if (_normalized && (minVal < 0f || maxVal > 1f))
            {
                // Draw 0 and 1 reference lines if they're in range
                Handles.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
                if (minVal <= 0f && maxVal >= 0f)
                {
                    float zeroY = Mathf.Lerp(inner.yMax, inner.y, Mathf.InverseLerp(minVal, maxVal, 0f));
                    Handles.DrawAAPolyLine(1f, new Vector3(inner.x, zeroY), new Vector3(inner.xMax, zeroY));
                }
                if (minVal <= 1f && maxVal >= 1f)
                {
                    float oneY = Mathf.Lerp(inner.yMax, inner.y, Mathf.InverseLerp(minVal, maxVal, 1f));
                    Handles.DrawAAPolyLine(1f, new Vector3(inner.x, oneY), new Vector3(inner.xMax, oneY));
                }
            }

            // Build antialiased polyline points across the inner rect width
            int samples = Mathf.Clamp(Mathf.RoundToInt(inner.width), 64, 512);
            var pts = new Vector3[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / (samples - 1);        // 0..1 across X (inner)
                float u = Mathf.InverseLerp(ramp.z, ramp.w, t); // 0..1 within [t0,t1]
                float s = Mathf.SmoothStep(0f, 1f, u);
                float val = Mathf.Lerp(ramp.x, ramp.y, s);

                float x = Mathf.Lerp(inner.x, inner.xMax, t);
                float normalizedVal = _normalized ? Mathf.InverseLerp(minVal, maxVal, val) : Mathf.Clamp01(val);
                float y = Mathf.Lerp(inner.yMax, inner.y, normalizedVal); // higher value -> higher on screen
                pts[i] = new Vector3(x, y, 0);
            }

            Handles.color = Color.cyan;
            Handles.DrawAAPolyLine(2.5f, pts);

            // Draw draggable time handles at t0 and t1 so users can see and grab them (inside inner rect)
            float x0 = Mathf.Lerp(inner.x, inner.xMax, ramp.z);
            float x1 = Mathf.Lerp(inner.x, inner.xMax, ramp.w);
            Color handleCol0 = new Color(1f, 0.6f, 0.2f, 1f);
            Color handleCol1 = new Color(0.4f, 0.8f, 1f, 1f);
            Handles.color = handleCol0;
            Handles.DrawAAPolyLine(2f, new Vector3(x0, inner.y), new Vector3(x0, inner.yMax));
            Handles.color = handleCol1;
            Handles.DrawAAPolyLine(2f, new Vector3(x1, inner.y), new Vector3(x1, inner.yMax));

            // Draw value handles on left/right gutters
            float norm0 = _normalized ? Mathf.InverseLerp(minVal, maxVal, ramp.x) : Mathf.Clamp01(ramp.x);
            float norm1 = _normalized ? Mathf.InverseLerp(minVal, maxVal, ramp.y) : Mathf.Clamp01(ramp.y);
            float y0 = Mathf.Lerp(inner.yMax, inner.y, norm0);
            float y1 = Mathf.Lerp(inner.yMax, inner.y, norm1);
            Handles.color = handleCol0;
            float leftX = Mathf.Clamp(rect.x + ValueHandleMargin * 0.5f, rect.x, rect.xMax);
            float y0c = Mathf.Clamp(y0, rect.y + 1f, rect.yMax - 1f);
            Handles.DrawAAPolyLine(2f, new Vector3(leftX, Mathf.Max(rect.y, y0c - 6f)), new Vector3(leftX, Mathf.Min(rect.yMax, y0c + 6f)));
            Handles.DrawAAPolyLine(2f, new Vector3(Mathf.Max(rect.x, leftX - 6f), y0c), new Vector3(Mathf.Min(rect.xMax, leftX + 6f), y0c));
            Handles.color = handleCol1;
            float rightX = Mathf.Clamp(rect.xMax - ValueHandleMargin * 0.5f, rect.x, rect.xMax);
            float y1c = Mathf.Clamp(y1, rect.y + 1f, rect.yMax - 1f);
            Handles.DrawAAPolyLine(2f, new Vector3(rightX, Mathf.Max(rect.y, y1c - 6f)), new Vector3(rightX, Mathf.Min(rect.yMax, y1c + 6f)));
            Handles.DrawAAPolyLine(2f, new Vector3(Mathf.Max(rect.x, rightX - 6f), y1c), new Vector3(Mathf.Min(rect.xMax, rightX + 6f), y1c));

            // Border
            Handles.color = new Color(0, 0, 0, 0.6f);
            Handles.DrawAAPolyLine(1f, new Vector3(rect.x, rect.y), new Vector3(rect.xMax, rect.y));
            Handles.DrawAAPolyLine(1f, new Vector3(rect.x, rect.yMax), new Vector3(rect.xMax, rect.yMax));
            Handles.DrawAAPolyLine(1f, new Vector3(rect.x, rect.y), new Vector3(rect.x, rect.yMax));
            Handles.DrawAAPolyLine(1f, new Vector3(rect.xMax, rect.y), new Vector3(rect.xMax, rect.yMax));
        }

        private void HandlePreviewDrag(Rect rect, ref float t0, ref float t1, ref float v0, ref float v1, MaterialProperty prop)
        {
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.SlideArrow);
            // Wider hit zones around handles for easier grabbing
            Rect inner = new Rect(rect.x + ValueHandleMargin, rect.y, Mathf.Max(1f, rect.width - ValueHandleMargin * 2f), rect.height);
            float px0 = Mathf.Lerp(inner.x, inner.xMax, t0);
            float px1 = Mathf.Lerp(inner.x, inner.xMax, t1);
            Rect h0 = new Rect(px0 - 6f, rect.y, 12f, rect.height);
            Rect h1 = new Rect(px1 - 6f, rect.y, 12f, rect.height);
            EditorGUIUtility.AddCursorRect(h0, MouseCursor.SlideArrow);
            EditorGUIUtility.AddCursorRect(h1, MouseCursor.SlideArrow);

            // Value handles on left (v0) and right (v1) gutters
            Rect hv0 = new Rect(rect.x, rect.y, ValueHandleMargin, rect.height);
            Rect hv1 = new Rect(rect.xMax - ValueHandleMargin, rect.y, ValueHandleMargin, rect.height);
            EditorGUIUtility.AddCursorRect(hv0, MouseCursor.SlideArrow);
            EditorGUIUtility.AddCursorRect(hv1, MouseCursor.SlideArrow);

            int id = GUIUtility.GetControlID(FocusType.Passive);
            Event e = Event.current;
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (rect.Contains(e.mousePosition) && e.button == 0)
                    {
                        float x = e.mousePosition.x;
                        // Prefer explicit handle rects, fallback to nearest
                        if (hv0.Contains(e.mousePosition)) _activeHandle = 2; // v0
                        else if (hv1.Contains(e.mousePosition)) _activeHandle = 3; // v1
                        else if (h0.Contains(e.mousePosition)) _activeHandle = 0; // t0
                        else if (h1.Contains(e.mousePosition)) _activeHandle = 1; // t1
                        else _activeHandle = (Mathf.Abs(x - px0) < Mathf.Abs(x - px1)) ? 0 : 1;

                        GUIUtility.hotControl = id;
                        e.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == id && _activeHandle >= 0)
                    {
                        const float eps = 0.0005f;
                        if (_activeHandle == 0 || _activeHandle == 1)
                        {
                            float t = Mathf.InverseLerp(inner.x, inner.xMax, Mathf.Clamp(e.mousePosition.x, inner.x, inner.xMax));
                            if (_activeHandle == 0) t0 = Mathf.Min(t, t1 - eps);
                            else t1 = Mathf.Max(t, t0 + eps);
                        }
                        else if (_activeHandle == 2 || _activeHandle == 3)
                        {
                            if (_normalized)
                            {
                                // Calculate current range for normalized dragging
                                float minVal = Mathf.Min(v0, v1, 0f);
                                float maxVal = Mathf.Max(v0, v1, 1f);
                                float range = maxVal - minVal;
                                if (range > 0.001f)
                                {
                                    minVal -= range * 0.1f;
                                    maxVal += range * 0.1f;
                                }
                                
                                // Map mouse Y to actual value range
                                float normalizedPos = Mathf.InverseLerp(rect.yMax, rect.y, Mathf.Clamp(e.mousePosition.y, rect.y, rect.yMax));
                                float val = Mathf.Lerp(minVal, maxVal, normalizedPos);
                                if (_activeHandle == 2) v0 = val;
                                else v1 = val;
                            }
                            else
                            {
                                // Map mouse Y to value 0..1
                                float val = Mathf.InverseLerp(rect.yMax, rect.y, Mathf.Clamp(e.mousePosition.y, rect.y, rect.yMax));
                                if (_activeHandle == 2) v0 = Mathf.Clamp01(val);
                                else v1 = Mathf.Clamp01(val);
                            }
                        }

                        // Immediately update the material property so changes persist during drag
                        prop.vectorValue = new Vector4(v0, v1, t0, t1);
                        GUI.changed = true;
                        e.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id)
                    {
                        GUIUtility.hotControl = 0;
                        _activeHandle = -1;
                        e.Use();
                    }
                    break;
            }
        }
    }
}


