using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HT.Cangku))]
public class CangkuEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        HT.Cangku cangku = (HT.Cangku)target;

        if (GUILayout.Button("Fill Cangkus"))
        {
            cangku.FillCangkus();
        }
    }
}