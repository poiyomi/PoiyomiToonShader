using System;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

namespace Poi
{
    // Fixes bug with dynamic_branch in Unity 2022.3.0 through 2022.3.39.
    // Based on Error-mdl's Unity2022-Dynamic-Variant-Fix (MIT License).
    // If that package is already installed, this implementation becomes a no-op.
#if UNITY_2022_3_0 || UNITY_2022_3_1 || UNITY_2022_3_2 || UNITY_2022_3_3 || UNITY_2022_3_4 || UNITY_2022_3_5 || UNITY_2022_3_6 || UNITY_2022_3_7 || UNITY_2022_3_8 || UNITY_2022_3_9 || UNITY_2022_3_10 || UNITY_2022_3_11 || UNITY_2022_3_12 || UNITY_2022_3_13 || UNITY_2022_3_14 || UNITY_2022_3_15 || UNITY_2022_3_16 || UNITY_2022_3_17 || UNITY_2022_3_18 || UNITY_2022_3_19 || UNITY_2022_3_20 || UNITY_2022_3_21 || UNITY_2022_3_22 || UNITY_2022_3_23 || UNITY_2022_3_24 || UNITY_2022_3_25 || UNITY_2022_3_26 || UNITY_2022_3_27 || UNITY_2022_3_28 || UNITY_2022_3_29 || UNITY_2022_3_30 || UNITY_2022_3_31 || UNITY_2022_3_32 || UNITY_2022_3_33 || UNITY_2022_3_34 || UNITY_2022_3_35 || UNITY_2022_3_36 || UNITY_2022_3_37 || UNITY_2022_3_38 || UNITY_2022_3_39
    class PoiDynamicBranchVariantStripper : IPreprocessShaders
    {
        static readonly bool _externalFixExists;

        static PoiDynamicBranchVariantStripper()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetType("emdl.RemoveDynamicVariantDuplicates") != null)
                {
                    _externalFixExists = true;
                    return;
                }
            }
            _externalFixExists = false;
        }

        public int callbackOrder => 0;

        public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> keywordPermutations)
        {
            if (_externalFixExists) return;
            if (snippet.passType == PassType.Meta) return;

            LocalKeyword[] localKW = ShaderUtil.GetPassKeywords(shader, snippet.pass, snippet.shaderType);
            List<LocalKeyword> dynamicKWs = new List<LocalKeyword>(localKW.Length);

            for (int i = 0; i < localKW.Length; i++)
            {
                if (localKW[i].isDynamic)
                {
                    dynamicKWs.Add(localKW[i]);
                }
            }

            int numDynamic = dynamicKWs.Count;
            if (numDynamic == 0) return;

            int numPermutations = keywordPermutations.Count;
            int currentSize = 0;

            // Keep only variants that have every dynamic keyword enabled
            for (int pIdx = 0; pIdx < numPermutations; ++pIdx)
            {
                bool hasAllDynamic = true;
                for (int dIdx = 0; dIdx < numDynamic; ++dIdx)
                {
                    hasAllDynamic = hasAllDynamic && keywordPermutations[pIdx].shaderKeywordSet.IsEnabled(dynamicKWs[dIdx]);
                }

                if (hasAllDynamic)
                {
                    keywordPermutations[currentSize] = keywordPermutations[pIdx];
                    currentSize++;
                }
            }

            // Safety: don't strip if it would remove everything
            if (currentSize == 0) return;

            for (int i = numPermutations - 1; i >= currentSize; --i)
            {
                keywordPermutations.RemoveAt(i);
            }
        }
    }
#endif
}
