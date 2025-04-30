using UnityEngine;
using UnityEditor;
using HT;  // 你的 ResourceManager 命名空间

[InitializeOnLoad]
static class ResourceManagerEditorInitializer
{
    static ResourceManagerEditorInitializer()
    {
        // 脚本编译完后，自动检查 instance
        if (ResourceManager.instance == null)
        {
            var go = new GameObject("ResourceManager_Editor");
            go.hideFlags = HideFlags.HideAndDontSave;
            go.AddComponent<ResourceManager>();
        }
    }
}