using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SfxTrigger)), CanEditMultipleObjects]
public class SfxTriggerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        SfxTrigger trigger = (SfxTrigger)target;

        if (GUILayout.Button("Test Play"))
        {
            trigger.PlayTest();
        }
    }
}
