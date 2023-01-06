using UnityEngine;

[ExecuteInEditMode]
public abstract class ColorAssigner : MonoBehaviour
{
    public bool disabled;
    [SerializeField] private ColorReference color;
    [SerializeField] [Range(-360f, 360f)] public float hue;
    [SerializeField] [Range(-1f, 1f)] public float sat;
    [SerializeField] [Range(-1f, 1f)] public float val;
    [SerializeField] [Range(-1f, 1f)] public float alpha;

    //public bool sat_add, val_add;
    public bool alwaysbright;
    public bool brightOverride, filter;
    public Vector3 filterRGB;
    public bool advancedFilter;
    public Color Lsafe, Lblack, Rsafe, Rblack;

    //public Gradient[] gradientMap;

    private Color savedColor;
    private int start = 0;

    [System.Serializable]
    public struct PaletteSwap
    {
        public bool startInclusive, endInclusive;
        public bool clockwiseA, clockwiseB;

        public Gradient gradientA, gradientB;

        [Range(0, 1)]
        public float minSaturation, maxSaturation;
        public bool remapSaturation;
        [Range(0, 1)]
        public float minValue, maxValue;
        public bool remapValue;

        [Range(0, 1)]
        public float lerpWhite, lerpBlack;
    }
    public PaletteSwap[] paletteMap;
    public Color whiteLerp = Color.white, blackLerp = Color.black;

    public ColorReference ColorReference
    {
        get => color;
        set
        {
            if (color == value)
                return;

            ColorReference oldValue = color;

            if (oldValue != null && value == null)
                UnsubscribeColorChange();

            color = value;

            if (oldValue == null && value != null)
                SubscribeColorChange();
        }
    }

    protected abstract void AssignColor(Color color, float hue, float sat, float val, float alpha);

    private void SubscribeColorChange() => color.Changed += OnColorChanged;

    private void UnsubscribeColorChange() => color.Changed -= OnColorChanged;

    private void OnColorChanged() => AssignColor(color, hue, sat, val, alpha);

    public void AssignSelf() => AssignColor(color, hue, sat, val, alpha);


    private void Start()
    {
        if ((!brightOverride && transform.root.tag != "Player") && (color.name == "Player Color 1" || color.name == "Player Color 2"
            || (color.refer != null ? (color.refer.name == "Player Color 1" || color.refer.name == "Player Color 2") : false)))
        {
            alwaysbright = true;
        }

        start = 1;
        if (color != null)
        {
            UnsubscribeColorChange();
            SubscribeColorChange();
            AssignColor(color.channelcolor, hue, sat, val, alpha);
        }
        if(color.flag)
        {
            //Debug.Log("FLAG");
            color.flag = false;
            color.channelcolor = color.refer.channelcolor;
            AssignColor(color.refer.channelcolor, hue, sat, val, alpha);
            SubscribeColorChange();
        }

        savedColor.r = color.r;
        savedColor.g = color.g;
        savedColor.b = color.b;
        savedColor.a = color.a;
    }

    public void Restart()
    {
        if ((!brightOverride && transform.root.tag != "Player") && (color.name == "Player Color 1" || color.name == "Player Color 2"
            || (color.refer != null ? (color.refer.name == "Player Color 1" || color.refer.name == "Player Color 2") : false)))
        {
            alwaysbright = true;
        }

        start = 1;
        if (color != null)
        {
            UnsubscribeColorChange();
            SubscribeColorChange();
            AssignColor(color.channelcolor, hue, sat, val, alpha);
        }
        if (color.flag)
        {
            //Debug.Log("FLAG");
            color.flag = false;
            color.channelcolor = color.refer.channelcolor;
            AssignColor(color.refer.channelcolor, hue, sat, val, alpha);
            SubscribeColorChange();
        }

        savedColor.r = color.r;
        savedColor.g = color.g;
        savedColor.b = color.b;
        savedColor.a = color.a;
    }

    private void OnDestroy()
    {
        if (color != null)
        {
            UnsubscribeColorChange();
        }
        if (start == 1)
        {
            color.Set(savedColor);
            SubscribeColorChange(); // added
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if((!brightOverride && transform.root.tag != "Player") && (color.name == "Player Color 1" || color.name == "Player Color 2"
            || (color.refer != null ? (color.refer.name == "Player Color 1" || color.refer.name == "Player Color 2") : false)))
        {
            alwaysbright = true;
        }

        if(color.refer != null)
        {
            color.flag = true;
        }
        if (color != null)
        {
            UnsubscribeColorChange();
            SubscribeColorChange();
            OnColorChanged();
            
            // added
            savedColor.r = color.r;
            savedColor.g = color.g;
            savedColor.b = color.b;
            savedColor.a = color.a;//*/
        }
    }
#endif

    public Color GradientRemap(Color c)
    {
        /*if (!(gradientMap != null && gradientMap.Length > 0 && gradientMap.Length % 2 == 0)) { return c; }

        Color newColor = c;
        Color adjustedColor = c;

        float H = 0, S = 0, V = 0, A = c.a;
        Color.RGBToHSV(c, out H, out S, out V);
        adjustedColor = Color.HSVToRGB(H, 1, 1);

        for (int i = 0; i < gradientMap.Length; i+=2)
        {
            Vector4 gradientI0 = gradientMap[i].Evaluate(0).ColorAsVector4();
            Vector4 gradientI1 = gradientMap[i].Evaluate(1).ColorAsVector4();
            Vector4 gradientJ0 = gradientMap[i+1].Evaluate(0).ColorAsVector4();
            Vector4 gradientJ1 = gradientMap[i+1].Evaluate(1).ColorAsVector4();
            Vector4 colorAsVector = adjustedColor.ColorAsVector4();
            float position = VectorExtension.InverseLerp(gradientI0, gradientI1, colorAsVector);
            if (position > 0 && position < 1)
            {
                newColor = VectorExtension.remap(gradientI0, gradientI1, gradientJ0, gradientJ1, colorAsVector).Vector4AsColor();

                float nH = 0, nS = 0, nV = 0, nA = c.a;
                Color.RGBToHSV(newColor, out nH, out nS, out nA);
                newColor = Color.HSVToRGB(nH, S, V);
                newColor.a = c.a;

                break;
            }
        }

        return newColor;*/
        if (paletteMap == null || paletteMap.Length == 0) { return c; }

        Color newColor = c;

        foreach (PaletteSwap ps in paletteMap)
        {
            ps.gradientA.alphaKeys = new GradientAlphaKey[0];
            ps.gradientB.alphaKeys = new GradientAlphaKey[0];

            float hueStartA, satStartA, valStartA;
            Color.RGBToHSV(ps.gradientA.Evaluate(0), out hueStartA, out satStartA, out valStartA);

            float hueEndA, satEndA, valEndA;
            Color.RGBToHSV(ps.gradientA.Evaluate(1), out hueEndA, out satEndA, out valEndA);

            float hueStartB, satStartB, valStartB;
            Color.RGBToHSV(ps.gradientB.Evaluate(0), out hueStartB, out satStartB, out valStartB);

            float hueEndB, satEndB, valEndB;
            Color.RGBToHSV(ps.gradientB.Evaluate(1), out hueEndB, out satEndB, out valEndB);

            float hueC, satC, valC;
            Color.RGBToHSV(c, out hueC, out satC, out valC);

            float hueWhite, satWhite, valWhite;
            Color.RGBToHSV(whiteLerp, out hueWhite, out satWhite, out valWhite);

            float hueBlack, satBlack, valBlack;
            Color.RGBToHSV(blackLerp, out hueBlack, out satBlack, out valBlack);

            float hueN = hueC, satN = satC, valN = valC;

            if (floatWithinRange(hueStartA, hueEndA, hueC, ps.startInclusive, ps.endInclusive, ps.clockwiseA))
            {
                // HUE
                float t = 0;
                if(!ps.clockwiseA)
                {
                    if (hueStartA <= hueEndA)
                    {
                        t = Mathf.InverseLerp(hueStartA, hueEndA, hueC);
                    }
                    else
                    {
                        t = Mathf.InverseLerp(hueStartA, hueEndA+1, hueC + ((ps.endInclusive ? hueC <= hueEndA : hueC < hueEndA) ? 1 : 0));
                    }
                }
                else
                {
                    if (hueStartA >= hueEndA)
                    {
                        t = Mathf.InverseLerp(hueStartA, hueEndA, hueC);
                    }
                    else
                    {
                        t = Mathf.InverseLerp(hueStartA, hueEndA + 1, hueC + ((ps.startInclusive ? hueC <= hueStartA : hueC < hueStartA) ? 1 : 0));
                    }
                }

                if (!ps.clockwiseB)
                {
                    if (hueStartB <= hueEndB)
                    {
                        hueN = Mathf.Lerp(hueStartB, hueEndB, t);
                    }
                    else
                    {
                        hueN = Mathf.Lerp(hueStartB, hueEndB + 1, t);
                        if (hueN >= 1) { hueN -= 1; }
                    }
                }
                else
                {
                    if (hueStartB >= hueEndB)
                    {
                        hueN = Mathf.Lerp(hueStartB, hueEndB, t);
                    }
                    else
                    {
                        hueN = Mathf.Lerp(hueStartB + 1, hueEndB, t);
                        if (hueN >= 1) { hueN -= 1; }
                    }
                }

                // SAT
                if (ps.remapSaturation)
                {
                    t = Mathf.InverseLerp(0, 1, satC);
                    satN = Mathf.Lerp(ps.minSaturation, ps.maxSaturation, t);
                }
                else
                {
                    satN = Mathf.Clamp(satC, ps.minSaturation, ps.maxSaturation);
                }

                // VAL
                if (ps.remapValue)
                {
                    t = Mathf.InverseLerp(0, 1, valC);
                    valN = Mathf.Lerp(ps.minValue, ps.maxValue, t);
                }
                else
                {
                    valN = Mathf.Clamp(valC, ps.minValue, ps.maxValue);
                }

                newColor = Color.HSVToRGB(hueN, satN, valN);

                if (satC <= ps.lerpWhite)
                {
                    newColor = Color.Lerp(whiteLerp, newColor, Mathf.InverseLerp(0, ps.lerpWhite, satC));
                }

                if (valC <= ps.lerpBlack)
                {
                    newColor = Color.Lerp(blackLerp, newColor, Mathf.InverseLerp(0, ps.lerpBlack, valC));
                }

                break;
            }
        }

        return newColor;
    }

    private bool floatWithinRange(float start, float end, float value, bool startInclusive, bool endInclusive, bool clockwise)
    {
        if(!clockwise)
        {
            if(start <= end)
            {
                return (startInclusive ? value >= start : value > start) && (endInclusive ? value <= end : value < end);
            }
            else
            {
                return (startInclusive ? value >= start : value > start) || (endInclusive ? value <= end : value < end);
            }
            
        }
        else
        {
            if (start >= end)
            {
                return (startInclusive ? value <= start : value < start) && (endInclusive ? value >= end : value > end);
            }
            else
            {
                return (startInclusive ? value <= start : value < start) || (endInclusive ? value >= end : value > end);
            }
        }
    }

    /*private float floatRemap(float startA, float endA, float startB, float endB, float value, bool startInclusive, bool endInclusive, bool clockwiseA, bool clockwiseA)
    {
        if (!clockwise)
        {
            if (start <= end)
            {
                return (startInclusive ? value >= start : value > start) && (endInclusive ? value <= end : value < end);
            }
            else
            {
                return (startInclusive ? value >= start : value > start) || (endInclusive ? value <= end : value < end);
            }

        }
        else
        {
            if (start >= end)
            {
                return (startInclusive ? value <= start : value < start) && (endInclusive ? value >= end : value > end);
            }
            else
            {
                return (startInclusive ? value <= start : value < start) || (endInclusive ? value >= end : value > end);
            }
        }
    }*/
}