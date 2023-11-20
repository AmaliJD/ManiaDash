using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[CustomEditor(typeof(PostProcessTrigger)), CanEditMultipleObjects]
public class PostProcessTriggerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        PostProcessTrigger trigger = (PostProcessTrigger)target;

        EditorGUILayout.PropertyField(serializedObject.FindProperty("volumeProfile"), true);
        EditorGUILayout.Space();

        // BLOOM
        EditorGUILayout.BeginHorizontal();
        EditorGUI.indentLevel--;
        trigger.setBloom = EditorGUILayout.Toggle(trigger.setBloom, GUILayout.Width(15));
        EditorGUILayout.LabelField("Bloom", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.EndHorizontal();
        if (trigger.setBloom)
        {
            // Threshhold
            EditorGUILayout.BeginHorizontal();
            trigger.setThreshold = EditorGUILayout.Toggle(trigger.setThreshold, GUILayout.Width(15));
            EditorGUI.BeginDisabledGroup(!trigger.setThreshold);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("threshold"), true);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            // Intensity
            EditorGUILayout.BeginHorizontal();
            trigger.setIntensity = EditorGUILayout.Toggle(trigger.setIntensity, GUILayout.Width(15));
            EditorGUI.BeginDisabledGroup(!trigger.setIntensity);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("intensity"), true);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            // Scatter
            EditorGUILayout.BeginHorizontal();
            trigger.setScatter = EditorGUILayout.Toggle(trigger.setScatter, GUILayout.Width(15));
            EditorGUI.BeginDisabledGroup(!trigger.setScatter);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("scatter"), true);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            // Tint
            EditorGUILayout.BeginHorizontal();
            trigger.setTint = EditorGUILayout.Toggle(trigger.setTint, GUILayout.Width(15));
            EditorGUI.BeginDisabledGroup(!trigger.setTint);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("tint"), true);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.Space();

        // VIGNETTE
        EditorGUILayout.BeginHorizontal();
        EditorGUI.indentLevel--;
        trigger.setVignette = EditorGUILayout.Toggle(trigger.setVignette, GUILayout.Width(15));
        EditorGUILayout.LabelField("Vignette", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.EndHorizontal();
        if (trigger.setVignette)
        {
            // Color
            EditorGUILayout.BeginHorizontal();
            trigger.setVigColor = EditorGUILayout.Toggle(trigger.setVigColor, GUILayout.Width(15));
            EditorGUI.BeginDisabledGroup(!trigger.setVigColor);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("vigColor"), new GUIContent("Color"), true);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            // Center
            EditorGUILayout.BeginHorizontal();
            trigger.setVigCenter = EditorGUILayout.Toggle(trigger.setVigCenter, GUILayout.Width(15));
            EditorGUI.BeginDisabledGroup(!trigger.setVigCenter);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("vigCenter"), new GUIContent("Center"), true);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            // Intensity
            EditorGUILayout.BeginHorizontal();
            trigger.setVigIntensity = EditorGUILayout.Toggle(trigger.setVigIntensity, GUILayout.Width(15));
            EditorGUI.BeginDisabledGroup(!trigger.setVigIntensity);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("vigIntensity"), new GUIContent("Intensity"), true);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            // Smoothness
            EditorGUILayout.BeginHorizontal();
            trigger.setVigSmoothness = EditorGUILayout.Toggle(trigger.setVigSmoothness, GUILayout.Width(15));
            EditorGUI.BeginDisabledGroup(!trigger.setVigSmoothness);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("vigSmoothness"), new GUIContent("Smoothness"), true);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.Space();

        // COLOR ADJUSTMENTS
        EditorGUILayout.BeginHorizontal();
        EditorGUI.indentLevel--;
        trigger.setColorAdjustments = EditorGUILayout.Toggle(trigger.setColorAdjustments, GUILayout.Width(15));
        EditorGUILayout.LabelField("Color Adjustments", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.EndHorizontal();
        if (trigger.setColorAdjustments)
        {
            // Post Exposure
            EditorGUILayout.BeginHorizontal();
            trigger.setPostExposure = EditorGUILayout.Toggle(trigger.setPostExposure, GUILayout.Width(15));
            EditorGUI.BeginDisabledGroup(!trigger.setPostExposure);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("postExposure"), true);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            // Contrast
            EditorGUILayout.BeginHorizontal();
            trigger.setContrast = EditorGUILayout.Toggle(trigger.setContrast, GUILayout.Width(15));
            EditorGUI.BeginDisabledGroup(!trigger.setContrast);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("contrast"), true);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            // Color Filter
            EditorGUILayout.BeginHorizontal();
            trigger.setColorFilter = EditorGUILayout.Toggle(trigger.setColorFilter, GUILayout.Width(15));
            EditorGUI.BeginDisabledGroup(!trigger.setColorFilter);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("colorFilter"), true);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            // Hue Shift
            EditorGUILayout.BeginHorizontal();
            trigger.setHueShift = EditorGUILayout.Toggle(trigger.setHueShift, GUILayout.Width(15));
            EditorGUI.BeginDisabledGroup(!trigger.setHueShift);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("hueShift"), true);
            trigger.hueShiftDirection = (PostProcessTrigger.HueShiftDirection)EditorGUILayout.EnumPopup(trigger.hueShiftDirection, GUILayout.Width(20));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            // Saturation
            EditorGUILayout.BeginHorizontal();
            trigger.setSaturation = EditorGUILayout.Toggle(trigger.setSaturation, GUILayout.Width(15));
            EditorGUI.BeginDisabledGroup(!trigger.setSaturation);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("saturation"), true);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.Space();

        // SHADOWS MIDTONES HIGHLIGHTS
        EditorGUILayout.BeginHorizontal();
        EditorGUI.indentLevel--;
        trigger.setSMH = EditorGUILayout.Toggle(trigger.setSMH, GUILayout.Width(15));
        EditorGUILayout.LabelField("Shadows Midtones Highlights", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.EndHorizontal();
        if (trigger.setSMH)
        {
            // Shadows
            EditorGUILayout.BeginHorizontal();
            trigger.setShadows = EditorGUILayout.Toggle(trigger.setShadows, GUILayout.Width(15));
            EditorGUI.BeginDisabledGroup(!trigger.setShadows);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("shadows"), new GUIContent("Shadows"), true);
            trigger.shadowsSlider = EditorGUILayout.Slider(trigger.shadowsSlider, -1, 1, GUILayout.Width(110));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            // Midtones
            EditorGUILayout.BeginHorizontal();
            trigger.setMidtones = EditorGUILayout.Toggle(trigger.setMidtones, GUILayout.Width(15));
            EditorGUI.BeginDisabledGroup(!trigger.setMidtones);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("midtones"), new GUIContent("Midtones"), true);
            trigger.midtonesSlider = EditorGUILayout.Slider(trigger.midtonesSlider, -1, 1, GUILayout.Width(110));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            // Hightlights
            EditorGUILayout.BeginHorizontal();
            trigger.setHighlights = EditorGUILayout.Toggle(trigger.setHighlights, GUILayout.Width(15));
            EditorGUI.BeginDisabledGroup(!trigger.setHighlights);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("highlights"), new GUIContent("Highlights"), true);
            trigger.highlightsSlider = EditorGUILayout.Slider(trigger.highlightsSlider, -1, 1, GUILayout.Width(110));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            // Shadow Limits
            EditorGUILayout.BeginHorizontal();
            trigger.setLimits = EditorGUILayout.Toggle(trigger.setLimits, GUILayout.Width(15));
            EditorGUILayout.LabelField("Set Limits");
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            EditorGUI.BeginDisabledGroup(!trigger.setLimits);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Shadow Limits", GUILayout.Width(130));
            trigger.shadowLimitStart = EditorGUILayout.FloatField(trigger.shadowLimitStart, GUILayout.Width(50));
            EditorGUILayout.MinMaxSlider(ref trigger.shadowLimitStart, ref trigger.shadowLimitEnd, 0, 1);
            trigger.shadowLimitEnd = EditorGUILayout.FloatField(trigger.shadowLimitEnd, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();

            // Highlight Limits
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Highlight Limits", GUILayout.Width(130));
            trigger.highlightsLimitStart = EditorGUILayout.FloatField(trigger.highlightsLimitStart, GUILayout.Width(50));
            EditorGUILayout.MinMaxSlider(ref trigger.highlightsLimitStart, ref trigger.highlightsLimitEnd, 0, 1);
            trigger.highlightsLimitEnd = EditorGUILayout.FloatField(trigger.highlightsLimitEnd, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space();

        // CHROMATIC ABERRATION
        EditorGUILayout.BeginHorizontal();
        EditorGUI.indentLevel--;
        trigger.setChromaticAberration = EditorGUILayout.Toggle(trigger.setChromaticAberration, GUILayout.Width(15));
        EditorGUILayout.LabelField("Chromatic Aberration", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.EndHorizontal();
        if (trigger.setChromaticAberration)
        {
            // Intensity
            EditorGUILayout.BeginHorizontal();
            trigger.setChrAbrIntensity = EditorGUILayout.Toggle(trigger.setChrAbrIntensity, GUILayout.Width(15));
            EditorGUI.BeginDisabledGroup(!trigger.setChrAbrIntensity);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("chrAbrIntensity"), new GUIContent("Intensity"), true);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.Space();

        // FILM GRAIN
        EditorGUILayout.BeginHorizontal();
        EditorGUI.indentLevel--;
        trigger.setFilmGrain = EditorGUILayout.Toggle(trigger.setFilmGrain, GUILayout.Width(15));
        EditorGUILayout.LabelField("Film Grain", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.EndHorizontal();
        if (trigger.setFilmGrain)
        {
            // Intensity
            EditorGUILayout.BeginHorizontal();
            trigger.setFilmIntensity = EditorGUILayout.Toggle(trigger.setFilmIntensity, GUILayout.Width(15));
            EditorGUI.BeginDisabledGroup(!trigger.setFilmIntensity);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("filmIntensity"), new GUIContent("Intensity"), true);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            // Response
            EditorGUILayout.BeginHorizontal();
            trigger.setFilmResponse = EditorGUILayout.Toggle(trigger.setFilmResponse, GUILayout.Width(15));
            EditorGUI.BeginDisabledGroup(!trigger.setFilmResponse);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("filmResponse"), new GUIContent("Response"), true);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("duration"), true);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("hideTexture"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("activateOnStart"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("activateOnDeath"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("activateOnRespawn"), true);
        EditorGUILayout.Space();

        if (GUILayout.Button("Set Profile")) { trigger.SetProfile(); }
        if (GUILayout.Button("Grab Profile")) { trigger.GrabProfile(); }
        if (GUILayout.Button("Log Data")) { trigger.Log(); }

        EditorUtility.SetDirty(target);
        serializedObject.ApplyModifiedProperties();
    }

    //public override bool UseDefaultMargins() => false;
}
