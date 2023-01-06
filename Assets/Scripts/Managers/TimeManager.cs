using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    private float[] TimeScales = new float[5];
    private float[] timers = new float[5];
    private float[] durations = new float[5];
    private float[] startValues = new float[5];
    private float[] endValues = new float[5];
    //private int usageCount;
    private TimeScaleTrigger activeTrigger;

    private void Awake()
    {
        for (int i = 0; i < TimeScales.Length; i++)
        {
            TimeScales[i] = 1;
            timers[i] = 0;
            durations[i] = 0;
            startValues[i] = 1;
            endValues[i] = 1;
        }
    }

    private void Start()
    {
        StartCoroutine(Fade());
    }

    // Update is called once per frame
    void LateUpdate()
    {
        float product = 1;
        for (int i = 0; i < TimeScales.Length; i++)
        {
            product *= TimeScales[i];
        }
        Time.timeScale = product;
        //Debug.Log(Time.timeScale);
    }

    public void setScale(int index, float scale)
    {
        TimeScales[index] = scale;
        startValues[index] = scale;
        endValues[index] = scale;
    }

    public void setFade(int index, float newScale, float duration)
    {
        startValues[index] = TimeScales[index];
        endValues[index] = newScale;
        timers[index] = 0;
        durations[index] = duration;
    }

    public float getScale(int index)
    {
        return TimeScales[index];
    }

    IEnumerator Fade()
    {
        while (true)
        {
            for (int i = 1; i < TimeScales.Length; i++)
            {
                if (TimeScales[i] != endValues[i])
                {
                    if (durations[i] == 0)
                    {
                        TimeScales[i] = endValues[i];
                    }
                    else
                    {
                        TimeScales[i] = Mathf.Lerp(startValues[i], endValues[i], timers[i] / durations[i]);
                        timers[i] += Time.unscaledDeltaTime;
                    }
                }
            }

            yield return null;
        }
    }
}
