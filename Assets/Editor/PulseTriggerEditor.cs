using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PulseTrigger)), CanEditMultipleObjects]
public class PulseTriggerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

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
            EditorGUILayout.PropertyField(serializedObject.FindProperty("assignerType"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("groupID"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("objects"), true);
        }

        if (!serializedObject.FindProperty("copy").boolValue)
        {
            if(!serializedObject.FindProperty("useGradient").boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("new_color"), true);
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("newColorGradient"), true);
            }
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useGradient"), true);
        }
        else
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("copy_color"), true);
        }

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("hue"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("sat"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("val"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("alpha"), true);

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

        EditorGUILayout.PropertyField(serializedObject.FindProperty("fadein"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("hold"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("duration"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("refresh"), true);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("usecurve"), true);
        //EditorGUILayout.PropertyField(serializedObject.FindProperty("curve"), true);
        if (serializedObject.FindProperty("usecurve").boolValue)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("curve"), true);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("oneuse"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("cancelActivePulse"), true);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("durationline"), true);
        if (serializedObject.FindProperty("durationline").boolValue)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("speed"), true);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
