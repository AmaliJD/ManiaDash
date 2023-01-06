using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SfxTrigger : MonoBehaviour
{
    private GameManager gamemanager;

    private AudioSource audio;
    public AudioClip clip;

    [Range(0, 2)]
    public float pitch;

    [Range(0, 1)]
    public float volume;

    public bool oneuse;
    bool used;

    void Awake()
    {
        gamemanager = FindObjectOfType<GameManager>();
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        audio = GetComponent<AudioSource>();
    }

    public void Play()
    {
        used = true;
        audio.pitch = pitch;
        audio.PlayOneShot(clip, gamemanager.sfx_volume * volume);
    }

    public void PlayTest()
    {
        audio.pitch = pitch;
        audio.PlayOneShot(clip, volume);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && !used && !collision.isTrigger)
        {
            Play();
        }
    }
}
