using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSyncer : MonoBehaviour
{
    private float previousAudioValue;
    private float audioValue;
    private float timer;

    public float bias, timeStep, timeToBeat, restSmoothTime;

    protected bool isBeat;

    private GameManager gamemanager;

    private void Awake()
    {
        gamemanager = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        OnUpdate();
    }

    public virtual void OnUpdate()
    {
        float adj_bias = bias * gamemanager.music_volume;
        previousAudioValue = audioValue;
        audioValue = AudioSpectrum.spectrumValue;

        if(previousAudioValue > adj_bias &&
            audioValue <= adj_bias)
        {
            if(timer > timeStep)
            {
                OnBeat();
            }
        }

        if (previousAudioValue <= adj_bias &&
            audioValue > adj_bias)
        {
            if (timer > timeStep)
            {
                OnBeat();
            }
        }

        timer += Time.deltaTime;
    }

    public virtual void OnBeat()
    {
        //Debug.Log("beat");
        timer = 0;
        isBeat = true;
    }
}
