using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(IntegratedMapSystem))]
public class IntegratedMapSystemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        IntegratedMapSystem mapSystem = (IntegratedMapSystem)target;
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Generate Map"))
        {
            mapSystem.GenerateMap();
        }
        
        if (GUILayout.Button("Regenerate Map"))
        {
            mapSystem.GenerateNewMap();
        }
        
        if (GUILayout.Button("Clear Maps"))
        {
            mapSystem.ClearOldMaps();
        }
        
        if (GUILayout.Button("Print Map Info"))
        {
            mapSystem.DebugPrintMapInfo();
        }
    }
}
