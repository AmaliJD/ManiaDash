using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ColorPreview : MonoBehaviour
{
    public struct ColorData
    {
        float xPosition;
        int speed;
        PulseTrigger pulseTrigger;
    }

    List<ColorData> colors;
}
