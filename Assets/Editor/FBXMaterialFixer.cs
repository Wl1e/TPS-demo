using UnityEngine;
using UnityEditor;
using System.IO;

public class FBXMaterialFixer
{
    [MenuItem("Tools/Convert ALL Model Materials (Full Pipeline)")]
    static void ConvertAllMaterials()
    {
        string matDir = Application.dataPath + "/Models";
        string[] matFiles = Directory.GetFiles(matDir, "*.mat", SearchOption.AllDirectories);

        int converted = 0, skipped = 0;

        try
        {
            AssetDatabase.StartAssetEditing();

            for (int i = 0; i < matFiles.Length; i++)
            {
                string rel = "Assets" + matFiles[i].Replace(Application.dataPath, "").Replace("\\", "/");
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(rel);
                if (mat == null) continue;

                string shaderName = mat.shader.name;
                if (shaderName != "Standard" && shaderName != "Autodesk Interactive")
                { skipped++; continue; }

                Texture mainTex   = mat.GetTexture("_MainTex");
                Texture bumpMap   = mat.GetTexture("_BumpMap");
                Texture emission  = mat.GetTexture("_EmissionMap");
                Texture occlusion = mat.GetTexture("_OcclusionMap");
                Texture specGloss = mat.HasProperty("_SpecGlossMap") ? mat.GetTexture("_SpecGlossMap") : null;

                Color color      = mat.GetColor("_Color");
                float metallic   = mat.HasProperty("_Metallic") ? mat.GetFloat("_Metallic") : 0f;
                float glossiness = mat.HasProperty("_Glossiness") ? mat.GetFloat("_Glossiness") : 0.5f;
                float mode       = mat.HasProperty("_Mode") ? mat.GetFloat("_Mode") : 0f;
                float cutoff     = mat.HasProperty("_Cutoff") ? mat.GetFloat("_Cutoff") : 0.5f;

                mat.shader = Shader.Find("Universal Render Pipeline/Lit");

                if (mainTex != null)    mat.SetTexture("_BaseMap", mainTex);
                if (bumpMap != null)    mat.SetTexture("_BumpMap", bumpMap);
                if (emission != null)   mat.SetTexture("_EmissionMap", emission);
                if (occlusion != null)  mat.SetTexture("_OcclusionMap", occlusion);
                if (specGloss != null)  mat.SetTexture("_MetallicGlossMap", specGloss);

                mat.SetColor("_BaseColor", color);
                mat.SetFloat("_Metallic", metallic);
                mat.SetFloat("_Smoothness", glossiness);

                if (mode >= 1f)
                {
                    mat.SetFloat("_Surface", mode >= 2f ? 1f : 0f);
                    mat.SetFloat("_AlphaClip", mode == 1f ? 1f : 0f);
                    mat.SetFloat("_Cutoff", cutoff);
                }

                EditorUtility.SetDirty(mat);
                converted++;

                if (i % 50 == 0)
                    EditorUtility.DisplayProgressBar("Converting to URP/Lit", $"{i + 1}/{matFiles.Length}", (float)i / matFiles.Length);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            EditorUtility.ClearProgressBar();
        }

        Debug.Log($"Converted {converted}, Skipped {skipped}. Total .mat: {matFiles.Length}");
    }
}
