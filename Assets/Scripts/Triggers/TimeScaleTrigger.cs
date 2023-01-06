using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeScaleTrigger : MonoBehaviour
{
    private TimeManager timeManager;
    public float scale, duration, cooldown;
    public bool hold, oneuse;

    private bool inuse;
    private float startScale;

    private void Awake()
    {
        timeManager = GameObject.FindGameObjectWithTag("Master").GetComponent<TimeManager>();
        if(cooldown == -1) { cooldown = duration; }

        transform.GetChild(0).gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && !inuse)
        {
            StopAllCoroutines();
            if(!hold)
            {
                inuse = true;
                timeManager.setFade(2, scale, duration);
            }
            else
            {
                inuse = true;
                startScale = Time.timeScale;
                timeManager.setFade(2, scale, duration);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && inuse)
        {
            if (hold)
            {
                timeManager.setFade(2, startScale, duration);
            }
            StartCoroutine(Cooldown(cooldown));
        }
    }

    IEnumerator Cooldown(float time)
    {
        yield return new WaitForSeconds(time);
        inuse = oneuse;
    }
}
