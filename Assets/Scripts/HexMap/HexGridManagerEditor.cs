using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HexGridManager))]
public class HexGridManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        HexGridManager gridManager = (HexGridManager)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Regenerate Grid"))
        {
            gridManager.ClearGrid();
            gridManager.GenerateGrid();
        }
    }
}
