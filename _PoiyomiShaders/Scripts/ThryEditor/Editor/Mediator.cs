using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Thry
{
    public class Mediator
    {

        private static Shader activeShader = null;
        private static Material activeShaderMaterial = null;

        public static void SetActiveShader(Shader shader, Material material = null)
        {
            activeShader = shader;
            activeShaderMaterial = material;
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