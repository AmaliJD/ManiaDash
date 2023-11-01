using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicTrigger : MonoBehaviour
{
    public enum Mode { music, volume, freqPass }
    public Mode mode;

    private GameManager gamemanager;

    public MusicSource bgmusic;
    private AudioLowPassFilter lowPassFilter;
    private AudioHighPassFilter highPassFilter;

    public float volume, fadetime, playvolume = 1;

    //[Range(10, 22000)]
    public float lowcutoff = 22000, highcutoff = 10;
    public bool play;
    public bool oneuse;
    public bool ignorePlayerIsTrigger = true;
    public bool cancelIfPlaying;
    private bool omaewamou = false, finished = false;

    [Min(0)]
    public float delay;
    // Start is called before the first frame update
    void Awake()
    {
        gamemanager = FindObjectOfType<GameManager>();
        gameObject.transform.GetChild(0).gameObject.SetActive(false);

        //lowPassFilter = bgmusic.GetComponent<AudioLowPassFilter>();
        //highPassFilter = bgmusic.GetComponent<AudioHighPassFilter>();
    }

    IEnumerator Delay()
    {
        if (delay > 0) { yield return new WaitForSeconds(delay); }

        if (mode.ToString() == "music") { setBGMusic(); }
        else if (mode.ToString() == "volume") { StartCoroutine(setMusicVolume()); }
        else if (mode.ToString() == "freqPass") { StartCoroutine(setFrequencyPass()); }

        yield break;
    }

    public void setBGMusic()
    {
        if (cancelIfPlaying && gamemanager.bgmusic.audio == bgmusic.audio && gamemanager.bgmusic.audio.isPlaying)
        {
            finished = true;
            if (oneuse)
            {
                Destroy(gameObject);
            }
            return;
        }

        gamemanager.setBGMusic(bgmusic);

        if (play)
        {
            gamemanager.playBGMusic(playvolume);

            lowPassFilter = bgmusic.GetComponent<AudioLowPassFilter>();
            highPassFilter = bgmusic.GetComponent<AudioHighPassFilter>();

            if (lowPassFilter != null)
            {
                lowPassFilter.cutoffFrequency = lowcutoff;
            }
            if (highPassFilter != null)
            {
                highPassFilter.cutoffFrequency = highcutoff;
            }
        }

        finished = true;

        if (oneuse)
        {
            Destroy(gameObject);
        }
    }

    public IEnumerator setMusicVolume()
    {
        MusicSource bgMusic = gamemanager.getBGMusic();
        gamemanager.StopMusicTrigger(this);

        if (fadetime == 0) { bgMusic.realVolume = volume; }
        else
        {
            float time = 0;
            float step = (bgMusic.realVolume - volume) / (fadetime / Time.deltaTime);

            while (time < fadetime)
            {
                bgMusic.realVolume = bgMusic.realVolume - step;

                time += Time.deltaTime;
                //time += Time.deltaTime / (fadetime * 100);
                yield return null;
            }
        }

        bgMusic.realVolume = volume;
        finished = true;

        if (oneuse)
        {
            Destroy(gameObject);
        }
    }

    public IEnumerator setFrequencyPass()
    {
        MusicSource bgMusic = gamemanager.getBGMusic();
        gamemanager.StopMusicTrigger(this);

        lowPassFilter = bgMusic.GetComponent<AudioLowPassFilter>();// != null ? bgmusic.GetComponent<AudioLowPassFilter>() : null;
        highPassFilter = bgMusic.GetComponent<AudioHighPassFilter>();// != null ? bgmusic.GetComponent<AudioHighPassFilter>() : null;

        if (lowPassFilter == null && highPassFilter == null) { yield break; }

        float startLow = 22000, startHigh = 10;

        if (lowPassFilter != null && lowcutoff != 22000)
        {
            lowPassFilter.enabled = true;
            startLow = lowPassFilter.cutoffFrequency;
        }
        if (highPassFilter != null && highcutoff != 10)
        {
            highPassFilter.enabled = true;
            startHigh = highPassFilter.cutoffFrequency;
        }

        startLow = lowPassFilter.cutoffFrequency;
        startHigh = highPassFilter.cutoffFrequency;

        if (fadetime == 0)
        {
            if (lowPassFilter != null)
            {
                lowPassFilter.cutoffFrequency = lowcutoff;
            }
            if (highPassFilter != null)
            {
                highPassFilter.cutoffFrequency = highcutoff;
            }
        }
        else
        {
            float time = 0;
            //float step = (bgMusic.realVolume - volume) / (fadetime / Time.deltaTime);

            while (time < fadetime)
            {
                if (lowPassFilter != null)
                {
                    lowPassFilter.cutoffFrequency = Mathf.Lerp(startLow, lowcutoff, time/fadetime);
                }
                if (highPassFilter != null)
                {
                    highPassFilter.cutoffFrequency = Mathf.Lerp(startHigh, highcutoff, time / fadetime);
                }

                time += Time.deltaTime;
                yield return null;
            }
        }

        if(lowPassFilter != null && lowcutoff == 22000)
        {
            lowPassFilter.enabled = false;
        }
        if (highPassFilter != null && highcutoff == 0)
        {
            highPassFilter.enabled = false;
        }
        finished = true;

        if (oneuse)
        {
            Destroy(gameObject);
        }
    }

    public float getDuration()
    {
        return fadetime;
    }

    public bool getFinished()
    {
        return finished;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && !omaewamou && (ignorePlayerIsTrigger ? !collision.isTrigger : true))
        {
            finished = false;
            if (oneuse) { omaewamou = true; }
            StartCoroutine(Delay());
            /*if(mode.ToString() == "music") { setBGMusic(); }
            else if (mode.ToString() == "volume") { StartCoroutine(setMusicVolume()); }
            else if (mode.ToString() == "freqPass") { StartCoroutine(setFrequencyPass()); }*/
        }
    }

    public void Activate()
    {
        StartCoroutine(Delay());
    }
}
