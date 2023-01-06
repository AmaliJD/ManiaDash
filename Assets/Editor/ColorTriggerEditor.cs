using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ColorTrigger)), CanEditMultipleObjects]
public class ColorTriggerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        //ColorTrigger trigger = (ColorTrigger)target;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("delay"), true);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("channelmode"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("copy"), true);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (serializedObject.FindProperty("channelmode").boolValue)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("channel"), true);
        }
        else
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("groupID"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("objects"), true);
        }

        if(!serializedObject.FindProperty("copy").boolValue)
        {
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("new_color"), true);
        }
        else
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("copy_color"), true);
        }

        // Runtime Alter
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("randomize"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("randomizeRange"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("hueshift"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("alterIterMode"), true);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("refCurrent"), true);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("filter"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("invert"), true);
        EditorGUILayout.EndHorizontal();
        if (serializedObject.FindProperty("filter").boolValue)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("filterRGB"), true);
        }

        EditorGUILayout.Space();

        /*EditorGUILayout.PropertyField(serializedObject.FindProperty("mapToPalette"), true);
        if (serializedObject.FindProperty("mapToPalette").boolValue)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gradientMap"), true);
        }*/

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("hue"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("sat"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("val"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("alpha"), true);
        

        if (!serializedObject.FindProperty("channelmode").boolValue)
        {
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("radial"), true);

            if (serializedObject.FindProperty("radial").boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("radialmode"), true);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("range"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("radialSpeed"), true);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(serializedObject.FindProperty("recalcCenter"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("center"), true);
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("duration"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("oneuse"), true);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("durationline"), true);
        if (serializedObject.FindProperty("durationline").boolValue)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("speed"), true);
        }

        //EditorUtility.SetDirty(target);
        serializedObject.ApplyModifiedProperties();
    }
}
