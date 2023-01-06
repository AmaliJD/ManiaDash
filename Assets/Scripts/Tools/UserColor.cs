using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserColor : MonoBehaviour
{
    public Color basecolor, finalcolor;
    public int playercolor;
    public bool alwaysbright;

    [SerializeField] [Range(-360f, 360f)] public float hue;
    [SerializeField] [Range(-1f, 1f)] public float sat;
    [SerializeField] [Range(-1f, 1f)] public float val;
    [SerializeField] [Range(0f, 1f)] public float alpha = 1;

    public Color setColor(Color base1, Color base2)
    {
        basecolor = playercolor == 1 ? base1 : base2;
        alpha = playercolor == 1 ? alpha : 1;

        float h = 0, s = 0, v = 0;

        Color.RGBToHSV(basecolor, out h, out s, out v);

        h += (hue / 360);

        float s_ratio = (1 + sat);
        if (sat > 0)
        {
            s += sat;
        }
        else
        {
            s *= s_ratio;
        }

        float v_ratio = (1 + val);
        if (val > 0)
        {
            v += val;
        }
        else
        {
            v *= v_ratio;
        }

        if (h > 1) { h -= 1; }
        else if (h < 0) { h += 1; }

        if (s > 1) { s = 1; }
        else if (s < 0) { s = 0; }

        if (v > 1) { v = 1; }
        else if (v < 0) { v = 0; }

        if (alwaysbright)
        {
            if (v <= .5f)
            {
                s = s * Mathf.Sqrt(v * 2);
            }
            v = v_ratio;
        }

        finalcolor = Color.HSVToRGB(h, s, v);
        finalcolor.a = alpha;

        return finalcolor;
    }
}
