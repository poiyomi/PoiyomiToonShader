using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Thry
{
    public class Mediator
    {

        private static Shader activeShader = null;
        private static Material activeShaderMaterial = null;
        private static PresetHandler activeShaderPresetHandler = null;

        public static void SetActiveShader(Shader shader, Material material = null, PresetHandler presetHandler = null)
        {
            activeShader = shader;
            activeShaderMaterial = material;
            activeShaderPresetHandler = presetHandler;
        }

        public static Shader active_shader
        {
            get
            {
                return activeShader;
            }
        }
        public static Material active_shader_material
        {
            get
            {
                if (activeShaderMaterial == null)
                    activeShaderMaterial = new Material(activeShader);
                return activeShaderMaterial;
            }
        }
        public static PresetHandler active_shader_preset_handler
        {
            get
            {
                if (activeShaderPresetHandler == null)
                    activeShaderPresetHandler = new PresetHandler(activeShader);
                return activeShaderPresetHandler;
            }
        }

        private static Material m_copy;
        public static Material copy_material
        {
            set
            {
                m_copy = value;
            }
            get
            {
                return m_copy;
            }
        }
    }
}