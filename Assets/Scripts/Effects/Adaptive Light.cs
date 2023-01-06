using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using System;

[CreateAssetMenu]
public class AdaptiveLight : ScriptableObject
{
    [SerializeField] private Light2D global;
    void Awake()
    {
        global = GameObject.FindGameObjectWithTag("Global Light").GetComponent<Light2D>();
    }

    // Update is called once per frame
    void Update()
    {
        light.intensity = 1 - global.intensity;
    }

    public Light2D light
    {
        get => light;
        set => light = value;
    }

    public int count
    {
        get => count;
        set => count = value;
    }

    public void Set(float i)
    {
        this.light.intensity = i;
        Changed?.Invoke();
    }

    public event Action Changed;
}