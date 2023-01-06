using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class AdaptiveLighting : MonoBehaviour
{
    Light2D light;
    Light2D global;
    void Awake()
    {
        global = GameObject.FindGameObjectWithTag("Global Light").GetComponent<Light2D>();
        light = gameObject.GetComponent<Light2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float h = 0, s = 0, value = 0, a = global.color.a;
        Color.RGBToHSV(global.color, out h, out s, out value);

        float brightness = Mathf.Min(value, global.intensity);

        if (1 - brightness >= 0)
        {
            light.gameObject.SetActive(true);
            light.intensity = 1 - brightness;
        }
        else
        {
            light.intensity = 0;
            light.gameObject.SetActive(false);
        }
    }
}
