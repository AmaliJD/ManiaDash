using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MusicTrigger)), CanEditMultipleObjects]
public class MusicTriggerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        serializedObject.Update();
        MusicTrigger trigger = (MusicTrigger)target;

        EditorGUILayout.PropertyField(serializedObject.FindProperty("mode"), true);

        if (serializedObject.FindProperty("mode").enumValueIndex == 1)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("volume"), true);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("fadetime"), true);
        }
        else if (serializedObject.FindProperty("mode").enumValueIndex == 0)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bgmusic"), true);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("play"), true);

            if(serializedObject.FindProperty("play").boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("playvolume"), true);
                //EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("highcutoff"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("lowcutoff"), true);
                //EditorGUILayout.EndHorizontal();
                EditorGUILayout.MinMaxSlider(ref trigger.highcutoff, ref trigger.lowcutoff, 10, 22000);
            }
        }
        else if (serializedObject.FindProperty("mode").enumValueIndex == 2)
        {
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("pass"), true);
            //EditorGUILayout.LabelField("Low Cutoff:", serializedObject.FindProperty("lowcutoff").floatValue.ToString());
            //EditorGUILayout.LabelField("High Cutoff:", serializedObject.FindProperty("highcutoff").floatValue.ToString());
            //float l = serializedObject.FindProperty("lowcutoff").floatValue;
            //float h = serializedObject.FindProperty("highcutoff").floatValue;
            //EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("highcutoff"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("lowcutoff"), true);
            //EditorGUILayout.EndHorizontal();
            EditorGUILayout.MinMaxSlider(ref trigger.highcutoff, ref trigger.lowcutoff, 10, 22000);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("fadetime"), true);
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("oneuse"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ignorePlayerIsTrigger"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("cancelIfPlaying"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("delay"), true);

        //EditorUtility.SetDirty(target);
        serializedObject.ApplyModifiedProperties();
    }
}
