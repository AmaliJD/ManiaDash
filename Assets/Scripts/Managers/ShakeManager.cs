using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ShakeManager : MonoBehaviour
{
    ShakeTrigger activeTrigger;
    Dictionary<CinemachineBasicMultiChannelPerlin, float> initialAmplitudes;
    Dictionary<CinemachineBasicMultiChannelPerlin, float> initialFrequencies;

    private void Awake()
    {
        initialAmplitudes = new Dictionary<CinemachineBasicMultiChannelPerlin, float>();
        initialFrequencies = new Dictionary<CinemachineBasicMultiChannelPerlin, float>();
    }

    public void SetActive(ShakeTrigger st)
    {
        if(activeTrigger != null && activeTrigger != st)
        {
            activeTrigger.Stop();
        }
        activeTrigger = st;
    }

    public void addPerlin(CinemachineBasicMultiChannelPerlin perlin, float amp, float freq)
    {
        if(!initialAmplitudes.ContainsKey(perlin))
        {
            initialAmplitudes.Add(perlin, amp);
            initialFrequencies.Add(perlin, freq);
        }
    }

    public float getInitialAmplitude(CinemachineBasicMultiChannelPerlin perlin)
    {
        return initialAmplitudes.ContainsKey(perlin) ? initialAmplitudes[perlin] : 0;
    }

    public float getInitialFrequency(CinemachineBasicMultiChannelPerlin perlin)
    {
        return initialFrequencies.ContainsKey(perlin) ? initialFrequencies[perlin] : 0;
    }

    public void resetCameras()
    {
        foreach(CinemachineBasicMultiChannelPerlin perlin in initialAmplitudes.Keys)
        {
            perlin.m_AmplitudeGain = initialAmplitudes[perlin];
            perlin.m_FrequencyGain = initialFrequencies[perlin];
        }

        initialAmplitudes = new Dictionary<CinemachineBasicMultiChannelPerlin, float>();
        initialFrequencies = new Dictionary<CinemachineBasicMultiChannelPerlin, float>();
    }
}
