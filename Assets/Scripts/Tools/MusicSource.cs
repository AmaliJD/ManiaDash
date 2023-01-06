using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicSource : MonoBehaviour
{
    public AudioSource audio;
    public float realVolume;

    public int startSong = 0;
    public int endSong;
    public int endLoop;

    public bool playonawake;
    public float currTimesample;

    void Awake()
    {
        audio = GetComponent<AudioSource>();
        realVolume = audio.volume;
        audio.timeSamples = startSong;
        audio.loop = endLoop == 0;
    }

    private void Start()
    {
        if(playonawake)
        {
            Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if (audio.isPlaying) { Debug.Log(audio.timeSamples); }
        if(endSong == 0) { return; }

        if(audio.timeSamples > endSong)
        {
            audio.timeSamples = endLoop;
        }
    }

    // AudioSource Methods
    public void PlayORStop()
    {
        if(audio.isPlaying)
        {
            GetCurrentTime();
            Stop();
        }
        else
        {
            audio.timeSamples = startSong;
            Play();
        }
    }
    public void Play()
    {
        audio.Play();
    }

    public void Stop()
    {
        audio.Stop();
    }

    public void Pause()
    {
        audio.Pause();
    }

    private void GetCurrentTime()
    {
        currTimesample = audio.timeSamples;
    }
}
