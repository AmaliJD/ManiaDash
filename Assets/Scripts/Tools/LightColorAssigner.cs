using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class LightColorAssigner : ColorAssigner
{
    public float base_intensity = 1;
    protected override void AssignColor(Color color, float hue, float sat, float val, float alpha)
    {
        if (this == null) { return; }

        float h = 0, s = 0, v = 0, a = color.a;
        a *= base_intensity;
        if (filter)
        {
            color = new Color(filterRGB.x * color.r, filterRGB.y * color.g, filterRGB.z * color.b, a);
        }
        else if (advancedFilter)
        {
            float H = 0, S = 0, V = 0, A = color.a;
            Color.RGBToHSV(color, out H, out S, out V);

            Color.RGBToHSV(Lsafe, out float LeftSafe, out float s0, out float v0);
            Color.RGBToHSV(Lblack, out float LeftBlack, out float s1, out float v1);
            Color.RGBToHSV(Rsafe, out float RightSafe, out float s2, out float v2);
            Color.RGBToHSV(Rblack, out float RightBlack, out float s3, out float v3);

            float realH = H;
            float shift = 1 - RightBlack;
            LeftSafe += shift;
            LeftBlack += shift;
            RightSafe += shift;
            RightBlack += shift;
            H += shift;

            if (LeftSafe >= 1) LeftSafe -= 1;
            if (LeftBlack >= 1) LeftBlack -= 1;
            if (RightSafe >= 1) RightSafe -= 1;
            if (RightBlack >= 1) RightBlack -= 1;
            if (H >= 1) H -= 1;

            if (H > RightBlack && H < LeftBlack)
            {
                if (!(H >= RightSafe && H <= LeftSafe))
                {
                    if (H < LeftBlack && H > LeftSafe)
                    {
                        V = 1 - Mathf.InverseLerp(LeftSafe, LeftBlack, H);
                        S = Mathf.Sqrt(V * S);
                    }
                    else if (H > RightBlack && H < RightSafe)
                    {
                        V = Mathf.InverseLerp(RightBlack, RightSafe, H);
                        S = Mathf.Sqrt(V * S);
                    }
                }
            }
            else
            {
                V = 0;
                S = 0;
            }

            color = Color.HSVToRGB(realH, S, V);
        }

        Color.RGBToHSV(color, out h, out s, out v);
        
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

        a += alpha;

        if (h > 1) { h -= 1; }
        else if (h < 0) { h += 1; }

        if (s > 1) { s = 1; }
        else if (s < 0) { s = 0; }

        if (v > 1) { v = 1; }
        else if (v < 0) { v = 0; }

        //a += 1;
        //if (a > 1) { a = 1; }
        if (a < 0) { a = 0; }

        if(alwaysbright)
        {
            if (v <= .5f)
            {
                s = s * Mathf.Sqrt(v * 2);
            }
            v = v_ratio;
        }

        color = Color.HSVToRGB(h, s, v);
        //color.a = a;

        GetComponent<Light2D>().color = color;
        GetComponent<Light2D>().intensity = a;
    }
}
