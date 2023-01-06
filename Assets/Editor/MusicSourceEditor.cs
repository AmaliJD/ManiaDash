using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MusicSource)), CanEditMultipleObjects]
public class MusicSourceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        MusicSource source = (MusicSource)target;

        if(GUILayout.Button("Test Play"))
        {
            source.PlayORStop();
        }
    }
}
