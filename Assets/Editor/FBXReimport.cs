using UnityEditor;
using System.IO;
using UnityEngine;

public class FBXReimport
{
    [MenuItem("Tools/Reimport All Model FBX Files")]
    static void ReimportAllFBX()
    {
        string fbxDir = Application.dataPath + "/Models";
        string[] fbxFiles = Directory.GetFiles(fbxDir, "*.fbx", SearchOption.AllDirectories);

        for (int i = 0; i < fbxFiles.Length; i++)
        {
            string path = "Assets" + fbxFiles[i].Replace(Application.dataPath, "").Replace("\\", "/");
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            if (i % 10 == 0)
                EditorUtility.DisplayProgressBar("Reimporting FBX", $"{i + 1}/{fbxFiles.Length}", (float)i / fbxFiles.Length);
        }

        AssetDatabase.SaveAssets();
        EditorUtility.ClearProgressBar();
        Debug.Log($"Reimported {fbxFiles.Length} FBX files.");
    }
}
