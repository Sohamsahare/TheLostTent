using UnityEngine;
using UnityEditor;
using TheLostTent;

[CustomEditor(typeof(TileManager))]
public class TileManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var tileManager = (TileManager)target;
        if (GUILayout.Button("Build Level"))
        {
            tileManager.BuildLevel();
        }
    }
}