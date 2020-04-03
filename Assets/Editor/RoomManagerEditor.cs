using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoomManager))]
public class RoomManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var tileManager = (RoomManager)target;
        if (GUILayout.Button("Spawn Room"))
        {
            tileManager.SpawnRooms();
        }
    }
}