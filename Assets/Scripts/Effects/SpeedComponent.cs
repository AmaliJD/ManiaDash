using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedComponent : MonoBehaviour
{
    public GameObject pulse, ring;

    private bool enter = false, start = false;
    private float red = 255, green = 255, blue = 255, scale = 1;
    private float ring_speed;


    void Awake()
    {
        pulse.SetActive(false);
        ring.SetActive(false);

        red = ring.GetComponent<SpriteRenderer>().color.r;
        green = ring.GetComponent<SpriteRenderer>().color.g;
        blue = ring.GetComponent<SpriteRenderer>().color.b;
        scale = ring.transform.localScale.x;
    }

    private void Update()
    {
        if (!visible) return;
        if (enter)
        {
            Setup();
            enter = false;
            start = true;
            ring.SetActive(true);
            pulse.SetActive(true);
        }
    }

    void FixedUpdate()
    {
        if (!visible) return;
        if (start)
        {
            ring_speed = .09f / ring.transform.localScale.x;
            ring.transform.localScale = new Vector2(ring.transform.localScale.x + ring_speed, ring.transform.localScale.y + ring_speed);
            ring.GetComponent<SpriteRenderer>().color = new Color(red, green, blue, ring.GetComponent<SpriteRenderer>().color.a * .8f);
            pulse.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, pulse.GetComponent<SpriteRenderer>().color.a * .92f);

            if (ring.GetComponent<SpriteRenderer>().color.a <= 0 && pulse.GetComponent<SpriteRenderer>().color.a <= 0.02f)
            {
                start = false;
                ring.SetActive(false);
                pulse.SetActive(false);
            }
        }
    }

    void Setup()
    {
        start = false;
        ring.SetActive(false);
        pulse.SetActive(false);
        ring.transform.localScale = new Vector2(.6f, .6f);
        pulse.transform.localScale = new Vector2(1f, 1f);
        ring.GetComponent<SpriteRenderer>().color = new Color(red, green, blue, 1);
        pulse.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, 1);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            enter = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            enter = false;
        }
    }

    bool visible = false;

    private void OnBecameInvisible()
    {
        //transform.GetChild(0).gameObject.SetActive(false);
        enabled = false;
        visible = false;
    }

    private void OnBecameVisible()
    {
        //transform.GetChild(0).gameObject.SetActive(true);
        enabled = true;
        visible = true;
    }
}
