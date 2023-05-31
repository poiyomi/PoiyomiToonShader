using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class DecalTool : EditorWindow
    {
        const string GUID_GIZMO_SHADER = "94a63a84353e517488db284dc8fab0ca";

        private MaterialProperty _propPosition;
        private MaterialProperty _propRotation;
        private MaterialProperty _propScale;
        private MaterialProperty _propOffset;
        private MaterialProperty _propUVChannel;
        private Material _material;
        private Material _gizmoMaterial;

        public static DecalTool OpenDecalTool(Material m)
        {
            var window = EditorWindow.GetWindow<DecalTool>("Decal Tool");
            window._material = m;
            window._gizmoMaterial = new Material(AssetDatabase.LoadAssetAtPath<Shader>(AssetDatabase.GUIDToAssetPath(GUID_GIZMO_SHADER)));
            window._gizmoMaterial.color = Color.white;
            return window;
        }

        public void SetMaterialProperties(MaterialProperty decalProp, MaterialProperty uvProp, MaterialProperty positionProp, MaterialProperty rotationProp, MaterialProperty scaleProp, MaterialProperty offsetProp)
        {
            _propPosition = positionProp;
            _propRotation = rotationProp;
            _propScale = scaleProp;
            _propOffset = offsetProp;
            _propUVChannel = uvProp;
            _gizmoMaterial.SetTexture("_DecalTex", decalProp.textureValue);
            this.Repaint();
        }

        private void OnGUI()
        {
            if(_propPosition == null)
            {
                return;
            }
            HandleInput();
            // EditorGUI.DrawPreviewTexture(new Rect(0, 0, position.width, position.height), _material.mainTexture, _material);
            EditorGUI.DrawTextureTransparent(new Rect(0, 0, position.width, position.height), _material.mainTexture, ScaleMode.StretchToFill);
            _gizmoMaterial.SetVector("_Position", _propPosition.vectorValue);
            _gizmoMaterial.SetVector("_Scale", _propScale.vectorValue);
            _gizmoMaterial.SetFloat("_Rotation", _propRotation.floatValue);
            _gizmoMaterial.SetVector("_Offset", _propOffset.vectorValue);
            _gizmoMaterial.SetFloat("_UVChannel", _propUVChannel.floatValue);
            EditorGUI.DrawPreviewTexture(new Rect(0, 0, position.width, position.height), Texture2D.whiteTexture, _gizmoMaterial);
        }

        private Vector2 _lastMousePosition;
        private Vector2 _initialMousePosition;
        private Vector4 _initalScale;
        private float _initialRotation;
        private bool _isInsideAction;
        private bool _isOutsideAction;
        private Vector2 _grabbedSnappoint;
        private Vector2 _lastDecalMouseUV;
        private Vector2 _initalDecalMouseUV;
        private Vector2 _initalMouseUV;
        private void HandleInput()
        {
            Vector2 pivot = _propPosition.vectorValue * position.size;
            Vector2 pivotUV = pivot / position.size;

            Vector2 mouseUV = Event.current.mousePosition / position.size;
            mouseUV.y = 1 - mouseUV.y;
            Vector2 decalMouseUV = DecalUV(mouseUV, _propPosition.vectorValue, _propRotation.floatValue, _propScale.vectorValue, _propOffset.vectorValue);

            Event e = Event.current;
            bool isMouseDrag = e.type == EventType.MouseDrag;

            Vector2 delta = e.mousePosition - _lastMousePosition;
            Vector2 deltaDecalUV = decalMouseUV - _lastDecalMouseUV;
            if(isMouseDrag)
            {
                _lastMousePosition = e.mousePosition;
            }
                

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                _initialMousePosition = e.mousePosition;
                _lastMousePosition = e.mousePosition;

                _initalMouseUV = mouseUV;

                _initalDecalMouseUV = decalMouseUV;
                _lastDecalMouseUV = decalMouseUV;

                _initalScale = _propScale.vectorValue;
                _initialRotation = _propRotation.floatValue;
                _isInsideAction = decalMouseUV.x >= 0.1f && decalMouseUV.x <= 0.9f && decalMouseUV.y >= 0.1f && decalMouseUV.y <= 0.9f;
                _isOutsideAction = decalMouseUV.x < -0.1f || decalMouseUV.x > 1.1f || decalMouseUV.y < -0.1f || decalMouseUV.y > 1.1f;
                _grabbedSnappoint = new Vector2(Mathf.Round(decalMouseUV.x * 2), Mathf.Round(decalMouseUV.y * 2)) / 2;
            }
            // Translate
            if (_isInsideAction && isMouseDrag && !e.alt)
            {
                delta = delta / position.size;
                Vector2 pos = _propPosition.vectorValue;
                pos.x += delta.x;
                pos.y -= delta.y;
                _propPosition.vectorValue = pos;
                this.Repaint();
            }
            // Rotate
            else if(_isOutsideAction && isMouseDrag && !e.alt)
            {
                Vector2 vecInital = _initalMouseUV - pivotUV;
                Vector2 vecLast = mouseUV - pivotUV;
                float angle = Vector2.SignedAngle(vecInital, vecLast);
                SetClampedRotation(_propRotation, _initialRotation - angle);
                this.Repaint();
            }
            // Scale
            else if(isMouseDrag && e.alt)
            {
                Vector2 vecInital = _initalMouseUV - pivotUV;
                Vector2 vecLast = mouseUV - pivotUV;
                float vecLastIntoInitalDir = Vector2.Dot(vecLast, vecInital.normalized);
                float uniform = vecInital.magnitude / vecLastIntoInitalDir;
                _propScale.vectorValue = _initalScale / uniform;
                this.Repaint();
            }
            // Offset
            else if(isMouseDrag && !_isInsideAction && !_isOutsideAction)
            {
                Vector4 offset = _propOffset.vectorValue;
                if(_grabbedSnappoint.x == 0)
                    offset.x = Mathf.Max(-_propScale.vectorValue.x / 2, offset.x - deltaDecalUV.x * _propScale.vectorValue.y);
                if(_grabbedSnappoint.x == 1)
                    offset.y = Mathf.Max(-_propScale.vectorValue.x / 2, offset.y + deltaDecalUV.x * _propScale.vectorValue.y);
                if(_grabbedSnappoint.y == 0)
                    offset.z = Mathf.Max(-_propScale.vectorValue.y / 2, offset.z - deltaDecalUV.y * _propScale.vectorValue.y);
                if(_grabbedSnappoint.y == 1)
                    offset.w = Mathf.Max(-_propScale.vectorValue.y / 2, offset.w + deltaDecalUV.y * _propScale.vectorValue.y);
                _propOffset.vectorValue = offset;
                this.Repaint();
            }

            if(isMouseDrag)
            {
                _lastDecalMouseUV = DecalUV(mouseUV, _propPosition.vectorValue, _propRotation.floatValue, _propScale.vectorValue, _propOffset.vectorValue);
            }
        }

        static Vector2 Remap(Vector2 x, Vector2 minOld, Vector2 maxOld, Vector2 minNew, Vector2 maxNew)
        {
            return minNew + (x - minOld) * (maxNew - minNew) / (maxOld - minOld);
        }

        static Vector2 DecalUV(Vector2 uv, Vector2 position, float rotation, Vector2 scale, Vector4 scaleOffset)
        {
            scaleOffset = new Vector4(-scaleOffset.x, scaleOffset.y, -scaleOffset.z, scaleOffset.w);
            Vector2 centerOffset = new Vector2((scaleOffset.x + scaleOffset.y)/2, (scaleOffset.z + scaleOffset.w)/2);
            Vector2 decalCenter = position + centerOffset;
            float theta = Mathf.Deg2Rad * rotation;
            float cs = Mathf.Cos(theta);
            float sn = Mathf.Sin(theta);
            uv = new Vector2((uv.x - decalCenter.x) * cs - (uv.y - decalCenter.y) * sn + decalCenter.x, (uv.x - decalCenter.x) * sn + (uv.y - decalCenter.y) * cs + decalCenter.y);
            uv = Remap(uv, new Vector2(0, 0) - scale / 2 + position + new Vector2(scaleOffset.x, scaleOffset.z), scale / 2 + position + new Vector2(scaleOffset.y,scaleOffset.w), Vector2.zero, Vector2.one);
            return uv;
        }

        public static void SetClampedRotation(MaterialProperty property, float value)
        {
            Vector2 limits = property.rangeLimits;
            value = Helper.Mod(value - limits.x, limits.y - limits.x) + limits.x;
            property.floatValue = value;
        }
    }
}