using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ColorReference))]
public class ColorReferencePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        DrawLabel(position, label);

        position.xMin += EditorGUIUtility.labelWidth;

        DrawScriptableObjectField(position, property);
        if (property.objectReferenceValue)
            DrawColorField(position, property);
    }

    private static void DrawLabel(Rect position, GUIContent label)
    {
        GUI.Label(position, label);
    }

    private void DrawScriptableObjectField(Rect position, SerializedProperty property)
    {
        if (property.objectReferenceValue != null)
            position.xMin = position.xMax - 20;

        EditorGUI.ObjectField(position, property, typeof(ColorReference), GUIContent.none);
    }

    private void DrawColorField(Rect position, SerializedProperty property)
    {
        SerializedObject colorReference = new SerializedObject(property.objectReferenceValue);
        SerializedProperty colorProperty = colorReference.FindProperty("color");

        EditorGUI.BeginChangeCheck();

        position.xMax -= 20;
        Color color = EditorGUI.ColorField(position, GUIContent.none, colorProperty.colorValue);

        if (EditorGUI.EndChangeCheck())
        {
            (colorReference.targetObject as ColorReference).Set(color);
            property.serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(property.serializedObject.targetObject);
        }
    }
}