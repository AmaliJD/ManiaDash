using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NumberText : MonoBehaviour
{
    public bool Integer;
    public bool sign;
    public int i;
    public float f;

    [Min(0)]
    public int decimals;

    [Min(0)]
    public int minimumDigits;

    public string leadingText;
    public bool dontShowZero;

    private TextMeshProUGUI text;
    private string format;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if(Integer)
        {
            text.text = ((sign && i > 0) ? "+" : "") + i;

            if(minimumDigits > 0)
            {
                bool leadingSign = sign || i < 0;
                int digits = !leadingSign ? text.text.Length : text.text.Length - 1;
                int newDigits = minimumDigits - digits;

                if(newDigits > 0)
                {
                    string leadingZeroes = "";
                    for(int j = 0; j < newDigits; j++)
                    {
                        leadingZeroes += "0";
                    }

                    if(!leadingSign)
                    {
                        text.text = leadingZeroes + text.text;
                    }
                    else
                    {
                        string s = text.text[0] + "";
                        text.text = s + leadingZeroes + text.text.Substring(1);
                    }
                }

            }
            
            if (i == 0 && dontShowZero) text.text = "";
        }
        else
        {
            format = "0.";
            for(int a = 0; a < decimals; a++) { format += "0"; }

            float rounded = (Mathf.Round(f * Mathf.Pow(10f, decimals)) / Mathf.Pow(10f, decimals));
            text.text = rounded.ToString(format);

            if (f == 0 && dontShowZero) text.text = "";
        }

        text.text = leadingText + text.text;
    }
}
