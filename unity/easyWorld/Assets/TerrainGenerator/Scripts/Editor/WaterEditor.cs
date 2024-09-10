using UnityEngine;
using UnityEditor;
using Assets.Scripts.MapGenerator.Generators;

[CustomEditor(typeof(WaterGenerator))]
public class WaterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var gen = (WaterGenerator)target;

        if (DrawDefaultInspector())
        {
            if (gen.autoUpdate)
            {
                gen.Generate();
            }
        }

        if (GUILayout.Button("Generate Water"))
        {
            gen.Generate();
        }

        if (GUILayout.Button("Clear Water"))
        {
            gen.Clear();
        }
    }
}
