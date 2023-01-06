using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpriteReplacer))]
public class SpriteReplacerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        SpriteReplacer source = (SpriteReplacer)target;

        if (GUILayout.Button("Replace"))
        {
            source.replaceSprite();
        }
    }
}
