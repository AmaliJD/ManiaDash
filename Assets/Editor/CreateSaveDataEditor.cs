using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CreateSaveFile)), CanEditMultipleObjects]
public class CreateSaveDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        CreateSaveFile createsave = (CreateSaveFile)target;

        if (GUILayout.Button("Generate"))
        {
            createsave.GenerateSaveFile();
        }
    }
}