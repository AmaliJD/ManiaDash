using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ColorReference))]
public class ColorReferenceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();

        base.OnInspectorGUI();

        if (EditorGUI.EndChangeCheck())
        {
            (target as ColorReference).Set((target as ColorReference));
        }

        serializedObject.ApplyModifiedProperties();
    }
}