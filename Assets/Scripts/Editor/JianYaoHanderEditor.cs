using UnityEngine;
using UnityEditor;
using Herbs;

[CustomEditor(typeof(JianYaoHandler))] // 绑定到 JianYaoHandler
public class JianYaoHandlerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 显示原来的 public 变量和 SerializedField 变量
        DrawDefaultInspector();

        // 获取目标脚本实例
        JianYaoHandler handler = (JianYaoHandler)target;

        // 添加一个按钮
        if (GUILayout.Button("重新分配火力 Rearrange FireBar"))
        {
            // 确保参数正确传递
            handler.SplitRect(handler.fireBar, handler.noFireRange, handler.smallFireRange, handler.middleFireRange,
                handler.noFireColor, handler.smallFireColor, handler.middleFireColor, handler.largeFireColor);
            
            // 标记对象已修改，确保在 Scene 视图中更新
            EditorUtility.SetDirty(handler);
        }
    }
}