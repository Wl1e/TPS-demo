using UnityEditor;
using UnityEngine;

public class AnimBakerWindow : EditorWindow
{
    private AnimationClip m_Clip;
    //private Avatar m_Avatar;
    private float m_SampleRate = 60f;
    private string m_OutputPath = "Assets/AnimBake/";

    // 注册菜单
    [MenuItem("Tools/Bake Animation Root Motion")]
    static void Open()
    {
        // 获取窗口，如果不存在则创建
        var window = GetWindow<AnimBakerWindow>("Anim Bake");
        window.minSize = new Vector2(300, 200);
    }

    // 绘制窗口内容
    void OnGUI()
    {
        // 标题
        GUILayout.Label("Bake Animation Root Motion", EditorStyles.boldLabel);

        // ObjectField — 拖拽或选择资源
        m_Clip = (AnimationClip)EditorGUILayout.ObjectField(
            "Source Clip", m_Clip, typeof(AnimationClip), false);

        //m_Avatar = (Avatar)EditorGUILayout.ObjectField(
        //    "Avatar", m_Avatar, typeof(Avatar), false);

        // 数值字段
        m_SampleRate = EditorGUILayout.FloatField("Sample Rate", m_SampleRate);

        // 输出路径
        GUILayout.Label("Output Path:");
        GUILayout.BeginHorizontal();
        m_OutputPath = GUILayout.TextField(m_OutputPath);
        if (GUILayout.Button("...", GUILayout.Width(30))) {
            string selected = EditorUtility.SaveFolderPanel("Choose Folder", m_OutputPath, "");
            if (!string.IsNullOrEmpty(selected)) {
                // 将绝对路径转为 Assets/ 相对路径
                m_OutputPath = "Assets" + selected.Substring(Application.dataPath.Length);
            }
        }
        GUILayout.EndHorizontal();

        if(GUILayout.Button("show curve")) {
            foreach (var b in AnimationUtility.GetCurveBindings(m_Clip)) {
                var curve = AnimationUtility.GetEditorCurve(m_Clip, b);
                Debug.Log($"path='{b.path}' type={b.type.Name} prop={b.propertyName} keys={curve.keys.Length}");
            }
        }

        // 烘培按钮
        //GUI.enabled = m_Clip != null && m_Avatar != null; // 条件启用
        GUI.enabled = m_Clip != null;
        if (GUILayout.Button("Bake", GUILayout.Height(30))) {
            Bake();
        }
        GUI.enabled = true;
    }

    void Bake()
    {
        if (!System.IO.Directory.Exists(m_OutputPath)) {
            System.IO.Directory.CreateDirectory(m_OutputPath);
        }
        // 烘培逻辑...
        EditorUtility.DisplayProgressBar("Baking", "Sampling...", 0f);
        // ...
        EditorUtility.ClearProgressBar();
        BakedAnimData data = HumanoidBaker.Bake(m_Clip, m_SampleRate);

        string outputPath = $"{m_OutputPath}/{m_Clip.name}_Baked.asset";
        AssetDatabase.CreateAsset(data, outputPath);
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = data;

        EditorUtility.DisplayDialog("Bake Complete",
            $"Clip: {data.ClipName}\n" +
            //$"Frames: {frameCount}\n" +
            $"Duration: {data.ClipLength:F2}s\n" +
            $"Saved to: {outputPath}",
            "OK");
        // 完成提示
    }
}
