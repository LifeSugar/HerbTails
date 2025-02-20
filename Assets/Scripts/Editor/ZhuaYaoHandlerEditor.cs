using UnityEditor;
using UnityEngine;

namespace HT
{
    public class ZhuaYaoHandlerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            ZhuaYaoHandler zhuaYaoHandler = (ZhuaYaoHandler)target;

            if (GUILayout.Button("返回当前砝码代表重量"))
            {
                
            }
        }
    }
}