using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelDevTools : MonoBehaviour
{
    PlayerControllerV2 player;
    ClicksPerSecond cpsManager;
    TimeManager timeManager;
    GameManager gameManager;

    public int decimals = 2;

    public Text XSpeedText, YSpeedText, ForceText;
    public Slider[] XSliders, YSliders;
    public Transform forceArrow, xArrow, yArrow;

    public Image up, down, left, right;
    public Text CPSText;

    public Text maxXText, maxYText, maxCPSText, jumpsText, deathsText;
    public Slider timeScaleSlider;
    public Text timeScaleText;

    public GameObject stepButton;

    public InputActions input;

    private float maxCPS, maxX, maxY;
    private int changeTimeScale;
    private float prevtimeScaleValue;

    private void Awake()
    {
        if (input == null)
        {
            input = new InputActions();
        }
        input.Player.Enable();

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControllerV2>();
        cpsManager = GameObject.FindGameObjectWithTag("Master").GetComponent<ClicksPerSecond>();
        timeManager = GameObject.FindGameObjectWithTag("Master").GetComponent<TimeManager>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        prevtimeScaleValue = timeScaleSlider.value;
    }

    void Update()
    {
        float xVelocity = player.getXVelocity();
        float yVelocity = player.getYVelocity();
        XSpeedText.text = "X: " + roundFloatToDecimals(xVelocity, 0);
        YSpeedText.text = "Y: " + roundFloatToDecimals(yVelocity, 0);

        Vector2 force = player.getForces();
        ForceText.text = "Force: " + new Vector2(roundFloatToDecimals(force.x, 2), roundFloatToDecimals(force.y, 2));
        forceArrow.right = force.normalized;
        xArrow.right = player.getForwardOrientation().normalized;
        yArrow.right = -player.getGravityOrientation().normalized;

        float absXVelocity = Mathf.Abs(xVelocity);
        float absYVelocity = Mathf.Abs(yVelocity);
        maxX = Mathf.Max(maxX, absXVelocity);
        maxY = Mathf.Max(maxY, absYVelocity);

        CPSText.text = "CPS: " + cpsManager.getCPS();
        maxCPS = cpsManager.getMaxCPS();

        bool jump = input.Player.Jump.ReadValue<float>() > 0;
        bool crouch = input.Player.Crouch.ReadValue<float>() >= 0.7f;
        bool move_left = input.Player.MovementHorizontal.ReadValue<float>() < 0;
        bool move_right = input.Player.MovementHorizontal.ReadValue<float>() > 0;
        up.color = jump ? Color.red : Color.white;
        down.color = crouch ? Color.red : Color.white;
        left.color = move_left ? Color.red : Color.white;
        right.color = move_right ? Color.red : Color.white;

        maxXText.text = "Max X: " + roundFloatToDecimals(maxX, 2);
        maxYText.text = "Max Y: " + roundFloatToDecimals(maxY, 2);
        maxCPSText.text = "Max CPS: " + maxCPS;

        if (xVelocity > 0)
        {
            foreach (Slider sl in XSliders)
            {
                sl.fillRect.GetComponent<Image>().color = Color.green;
            }
        }
        else
        {
            foreach (Slider sl in XSliders)
            {
                sl.fillRect.GetComponent<Image>().color = Color.red;
            }
        }

        if (yVelocity > 0)
        {
            foreach (Slider sl in YSliders)
            {
                sl.fillRect.GetComponent<Image>().color = Color.green;
            }
        }
        else
        {
            foreach (Slider sl in YSliders)
            {
                sl.fillRect.GetComponent<Image>().color = Color.red;
            }
        }

        
        float i = 10;
        float max = 10;
        foreach (Slider sl in XSliders)
        {
            sl.value = absXVelocity > 0 ? Mathf.Clamp(absXVelocity, 0, max) : 0;
            absXVelocity -= 10;
            i += 10;
            if (i >= 50) { max = 60; }
        }

        
        i = 10;
        max = 10;
        foreach (Slider sl in YSliders)
        {
            sl.value = absYVelocity > 0 ? Mathf.Clamp(absYVelocity, 0, max) : 0;
            absYVelocity -= 10;
            i += 10;
            if (i >= 50) { max = 60; }
        }

        timeManager.setScale(4, (float)timeScaleSlider.value / 4);
        timeScaleText.text = "Time Scale x" + roundFloatToDecimals((float)timeScaleSlider.value / 4, 2);

        jumpsText.text = "Jumps: " + player.getJumpCount();
        deathsText.text = "Deaths: " + player.getDeathCount();

        stepButton.SetActive(timeScaleSlider.value == 0);

        if(prevtimeScaleValue != timeScaleSlider.value)
        {
            addSeconds(1, int.MaxValue);
        }
        if (!stepping) { prevtimeScaleValue = timeScaleSlider.value; }

        //Debug.Log(gameManager.getTime());
    }

    public void setTimeSlider(float i)
    {
        timeScaleSlider.value = 4 * i;
    }

    float roundFloatToDecimals(float f, int d)
    {
        float mult = Mathf.Pow(10, d);
        float value = ((float)Mathf.RoundToInt(f * mult)) / mult;
        return value;
    }

    public void FrameStep()
    {
        if (!stepping) { StartCoroutine(Step()); }
    }

    private bool stepping;
    IEnumerator Step()
    {
        stepping = true;
        timeManager.setScale(4, 1);
        timeScaleSlider.value = 4;

        //gameManager.setTime(int.MaxValue);
        addSeconds(300, 300, true);
        yield return new WaitForEndOfFrame();

        timeManager.setScale(4, 0);
        timeScaleSlider.value = 0;
        stepping = false;
    }

    void addSeconds(float s, float ceiling = 0, bool set = false)
    {
        if(ceiling == 0) { ceiling = float.MaxValue; }
        if(gameManager.getTime() < ceiling)
        {
            if(set)
            {
                gameManager.setTime(s);
            }
            else
            {
                gameManager.setTime(gameManager.getTime() + s);
            }
        }
    }
}
