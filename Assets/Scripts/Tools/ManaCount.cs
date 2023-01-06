using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.UI;

public class ManaCount : MonoBehaviour
{
    GameManager gamemanager;
    public Text text;
    public int count;
    public Light2D[] lights;
    private List<Collider2D> playerColliders;

    private bool enter = false, exit = true;
    public Color activateColor = Color.green;
    public float speedMultiplier = 1;

    private void Awake()
    {
        gamemanager = GameObject.FindObjectOfType<GameManager>();
        playerColliders = new List<Collider2D>();
    }

    private IEnumerator Show()
    {
        float value = text.color.a;

        while (value < 1)
        {
            text.text = gamemanager.getManaCount() + "/" + count;

            text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a + .02f * speedMultiplier);
            
            foreach (Light2D light in lights)
            {
                light.intensity += .02f * speedMultiplier;
            }
            value += .02f * speedMultiplier;

            yield return null;
        }
    }

    private IEnumerator Hide()
    {
        float value = text.color.a;

        while (value > 0)
        {
            text.text = gamemanager.getManaCount() + "/" + count;

            text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a - .01f * speedMultiplier);
            foreach (Light2D light in lights)
            {
                light.intensity -= .01f * speedMultiplier;
            }
            value -= .01f * speedMultiplier;

            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        /*if (playerColliders.Contains(collision)) { playerColliders.Add(collision); }
        if (collision.gameObject.tag == "Player" && !enter && playerColliders.Count > 0)
        {
            enter = true; exit = false;
            StopAllCoroutines();
            StartCoroutine(Show());
        }*/

        if (collision.gameObject.tag == "Player" && !enter)
        {
            enter = true; exit = false;
            StopAllCoroutines();
            StartCoroutine(Show());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        /*if (playerColliders.Contains(collision)) { playerColliders.Remove(collision); }
        if (collision.gameObject.tag == "Player" && !exit && playerColliders.Count == 0)
        {
            exit = true; enter = false;
            StopAllCoroutines();
            StartCoroutine(Hide());
        }*/

        if (collision.gameObject.tag == "Player" && !exit)
        {
            exit = true; enter = false;
            StopAllCoroutines();
            StartCoroutine(Hide());
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            text.text = gamemanager.getManaCount() + "/" + count;
            if (gamemanager.getManaCount() >= count)
            { //text.color = new Color(0, 255, 0, text.color.a);
                text.color = new Color(activateColor.r, activateColor.g, activateColor.b, text.color.a);
            }
            else { text.color = new Color(1, 1, 1, text.color.a); }
        }
    }
}
