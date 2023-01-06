using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class ClicksPerSecond : MonoBehaviour
{
    private InputActions input;
    private List<Click> clicks;

    private GameManager gamemanager;
    private bool paused, end;
    private float time;
    private bool clicked;

    private float cps, maxCps, aveCps;

    //public TextMeshProUGUI cpsText, maxCpsText, aveCpsText;

    private void Awake()
    {
        if (input == null)
        {
            input = new InputActions();
        }
        input.Player.Enable();

        clicks = new List<Click>();
        gamemanager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    private void Update()
    {
        paused = gamemanager.isPaused();
        end = gamemanager.getGameEnd();
        if (!paused && !end)
        {
            if(gameObject.scene.IsValid() && (input.Player.Crouch.ReadValue<float>() >= .7f
                || input.Player.MovementVertical.ReadValue<float>() <= -.7f || Input.GetAxisRaw("Vertical") <= -.7f
                || Input.GetKey(KeyCode.LeftShift) || Input.GetMouseButton(1)))
            {
                if(!clicked)
                {
                    clicks.Add(new Click(time));
                    aveCps = (aveCps + cps) / 2;
                }
                clicked = true;
            }
            else
            {
                clicked = false;
            }

            if(clicks.Count > 0)
            {
                clicks.RemoveAll(c => c.time < time - 1);
            }

            cps = clicks.Count;
            maxCps = Mathf.Max(maxCps, cps);

            time += Time.unscaledDeltaTime;
        }

        /*cpsText.text = "CPS:		" + cps;
        maxCpsText.text = "Max CPS:	" + maxCps;
        aveCpsText.text = "Ave CPS:	" + (int)aveCps;*/
    }

    private class Click
    {
        public float time;

        public Click(float t)
        {
            time = t;
        }
    }

    public int getCPS()
    {
        return (int)cps;
    }

    public int getMaxCPS()
    {
        return (int)maxCps;
    }

    public int getAveCPS()
    {
        return (int)aveCps;
    }
}
