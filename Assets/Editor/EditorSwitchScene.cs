using UnityEditor;
using UnityEditor.SceneManagement;

public static class EditorSwitchScene
{
    /// <summary>
    /// 打开场景
    /// </summary>
    /// <param name="filename">场景路径</param>
    public static void OpenScene(string filename)
    {
        // 询问是否保存对当前场景的修改
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
            EditorSceneManager.OpenScene(filename);
        }
    }

    [MenuItem("切换场景/Boot")]
    public static void SwitchBoot()
    {
        OpenScene("Assets/Scenes/Boot.unity");
    }

    [MenuItem("切换场景/Hub")]
    public static void SwitchHub()
    {
        OpenScene("Assets/Scenes/Hub.unity");
    }
    [MenuItem("切换场景/Combat1")]
    public static void SwitchCombat1()
    {
        OpenScene("Assets/Scenes/Combat1.unity");
    }
    [MenuItem("切换场景/Win")]
    public static void SwitchWin()
    {
        OpenScene("Assets/Scenes/WinScene.unity");
    }
    [MenuItem("切换场景/Lose")]
    public static void SwitchLose()
    {
        OpenScene("Assets/Scenes/LoseScene.unity");
    }
}
