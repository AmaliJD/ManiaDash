using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalComponent : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject pulse;
    SpriteRenderer pulseSprite;
    public float deactiveDelay;
    private float deactiveTimer;
    private bool activated;

    public bool smoothTeleport;

    public AudioSource sfx;

    private bool enter = false, start = false;
    private float red = 255, green = 255, blue = 255, scale = 1;
    private float pulse_speed = .1f;

    private GameManager gamemanager;

    void Awake()
    {
        pulse.SetActive(false);
        pulseSprite = pulse.GetComponent<SpriteRenderer>();

        red = pulseSprite.color.r;
        green = pulseSprite.color.g;
        blue = pulseSprite.color.b;
        scale = pulse.transform.localScale.x;

        gamemanager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }
    /*
    private IEnumerator Pulse()
    {
        enter = false;
        start = true;
        PulseSetup();
        pulse.SetActive(true);

        while (start)
        {
            pulse.transform.localScale = new Vector2(pulse.transform.localScale.x * .95f, pulse.transform.localScale.y * .95f);
            pulse.GetComponent<SpriteRenderer>().color = new Color(red, green, blue, pulse.GetComponent<SpriteRenderer>().color.a * .92f);

            if (pulse.GetComponent<SpriteRenderer>().color.a <= 0)
            {
                start = false;
                pulse.SetActive(false);
                pulse.transform.localScale = new Vector2(scale, scale);
                pulse.GetComponent<SpriteRenderer>().color = new Color(red, green, blue, 1f);
            }

            //yield return new WaitForSeconds(.015f);
            yield return new WaitForFixedUpdate();
        }
    }

    void PulseSetup()
    {
        pulse.SetActive(false);
        pulse.transform.localScale = new Vector2(scale, scale);
        pulse.GetComponent<SpriteRenderer>().color = new Color(pulse.GetComponent<SpriteRenderer>().color.r, pulse.GetComponent<SpriteRenderer>().color.g, 1);
        //pulse.GetComponent<SpriteRenderer>().color = Color.white;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            enter = true;
            StartCoroutine(Pulse());
        }
    }*/

    //*
    // Update is called once per frame
    void Update()
    {
        if (!visible && !disableVisibleCheck) return;
        if (enter)
        {
            PulseSetup();
            enter = false;
            start = true;
            pulse.SetActive(true);
        }

        if(activated)
        {
            if (deactiveTimer >= deactiveDelay)
            {
                activated = false;
                deactiveTimer = 0;
                GetComponent<CapsuleCollider2D>().enabled = true;
            }
            deactiveTimer += Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (!visible && !disableVisibleCheck) return;
        if (start)
        {
            pulse.transform.localScale = new Vector2(pulse.transform.localScale.x * .95f, pulse.transform.localScale.y * .95f);
            pulseSprite.color = new Color(red, green, blue, pulseSprite.color.a * .92f);

            if (pulseSprite.color.a <= 0.02f)
            {
                start = false;
                pulse.SetActive(false);
                pulse.transform.localScale = new Vector2(scale, scale);
                pulseSprite.color = new Color(red, green, blue, 1f);
            }
        }
    }

    void PulseSetup()
    {
        pulse.SetActive(false);
        pulse.transform.localScale = new Vector2(scale, scale);
        pulseSprite.color = new Color(pulseSprite.color.r, pulseSprite.color.g, 1);
        //pulse.GetComponent<SpriteRenderer>().color = Color.white;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && !activated && !collision.isTrigger)
        {
            enter = true;
            if (sfx != null) { sfx.PlayOneShot(sfx.clip, gamemanager.sfx_volume); }
            if (deactiveDelay != 0)
            {
                activated = true;                
                GetComponent<CapsuleCollider2D>().enabled = false;
            }
        }
    }//*/

    public bool disableVisibleCheck;
    bool visible = false;

    private void OnBecameInvisible()
    {
        if (disableVisibleCheck) return;
        //transform.GetChild(0).gameObject.SetActive(false);
        enabled = false;
        visible = false;
    }

    private void OnBecameVisible()
    {
        if (disableVisibleCheck) return;
        //transform.GetChild(0).gameObject.SetActive(true);
        enabled = true;
        visible = true;
    }
}
