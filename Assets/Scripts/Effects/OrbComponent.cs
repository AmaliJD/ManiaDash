using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using DG.Tweening;

public class OrbComponent : MonoBehaviour
{
    [Header("Design")]
    public float speed;
    private float ring_speed = .1f, pulse_speed = .1f;
    public GameObject pulse, ring;
    private SpriteRenderer pulseSprite, ringSprite;
    private Light2D pulse_light;
    private bool entered = false, jumped = false;
    private float red = 255, green = 255, blue = 255, scale = 1;

    [Header("Orb")]
    public float multiplier = 1;
    [Range(-1, 3)]
    public int reverse; //-1-no direction 0-same dir    1-forward dir   2-backwards dir     3-reverse dir
    public bool overrideCancelJump = false;
    public bool cancel_jump = false;
    public bool keepOrbActive = false;

    [Header("Teleport Orb")]
    public Transform TeleportTo;
    public bool smoothTeleport;

    [Header("DashOrb")]
    public bool overrideSpeed;
    public int dashSpeed = 3;
    public int gravityDirection = -1;
    public Transform parent;

    [Header("Rebound Orb")]
    public bool rebound;
    public Transform innerSection;
    private SpriteRenderer innerSolid, innerShader;
    private PlayerControllerV2 player;
    private float transitionSpeed = 2.5f;

    [Header("Audio")]
    public AudioSource sfx;
    private GameManager gamemanager;

    // Start is called before the first frame update
    void Awake()
    {
        pulseSprite = pulse.GetComponent<SpriteRenderer>();
        ringSprite = ring.GetComponent<SpriteRenderer>();

        visible = GetComponent<SpriteRenderer>() == null;
        pulse_light = pulse.transform.GetChild(0).gameObject.GetComponent<Light2D>();
        pulse.SetActive(false);
        ring.SetActive(false);

        red = pulseSprite.color.r;
        green = pulseSprite.color.g;
        blue = pulseSprite.color.b;
        scale = pulse.transform.localScale.x;

        gamemanager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControllerV2>();
        //innerSection = transform.GetChild(0).GetChild(0);
        if (rebound)
        {
            innerSolid = innerSection.GetChild(0).GetComponent<SpriteRenderer>();
            innerShader = innerSection.GetChild(1).GetComponent<SpriteRenderer>();
        }

        if (parent == null) parent = transform;

        StartCoroutine(OrbUpdate());
    }

    public IEnumerator OrbUpdate()
    {
        while(true)
        {
            if (!visible && !disableVisibleCheck)
            {
                yield return new WaitWhile(() => !visible && !disableVisibleCheck);
            }
            else
            {
                yield return new WaitForFixedUpdate();
            }

            transform.Rotate(Vector3.forward, speed * Time.deltaTime);
            if (rebound) { SetRebound(); }
        }
    }


    /*void FixedUpdate()
    {
        if (!visible && !disableVisibleCheck) return;
        transform.Rotate(Vector3.forward, speed * Time.deltaTime);

        if (rebound)
        {
            SetRebound();
        }
    }*/

    public void SetRebound()
    {
        float currPlayerVelocityY = player.getVelocityComponentY();
        //if (Mathf.Approximately(playerVelocityY, 0)) { transitionSpeed = 1; }
        /*if (transitionSpeed < 1)
        {
            transitionSpeed += .004f;
        }*/
        if (currPlayerVelocityY <= .00001f) { transitionSpeed = 2.5f; }
        float height = Vector2.Dot(player.transform.position, -player.getGravityOrientation()) - Vector2.Dot(transform.position, -player.getGravityOrientation());
        float playerVelocityY = height >= 0 ? Mathf.Sqrt(2*9.81f*9.81f*height) : currPlayerVelocityY;
        if (currPlayerVelocityY >= 0)
        {
            //Debug.Log(playerVelocityY);
            Color newInnerColor = innerSolid.color;
            Color newShaderColor = innerShader.color;
            Vector3 innerScale = innerSection.localScale;
            Vector3 transformScale = transform.localScale;

            //innerSection.localScale = Vector2.Lerp(0.8f * Vector2.one, 1.2f * Vector2.one, (playerVelocityY - 23.1f) / (40f - 23.1f));
            //transform.localScale = Vector2.Lerp(1 * Vector2.one, 1.2f * Vector2.one, (playerVelocityY - 23.1f) / (40f - 23.1f));
            innerScale = Vector2.Lerp(0.6f * Vector2.one, 1.2f * Vector2.one, (playerVelocityY - 0) / (60f));
            transformScale = Vector2.Lerp(1 * Vector2.one, 1.2f * Vector2.one, (playerVelocityY - 0) / (60f));
            float SP = Mathf.Lerp(-500, 0, 1 - (playerVelocityY / 60));
            if (playerVelocityY >= 40f) //30.45
            {
                newShaderColor = Color.Lerp(Color.clear, Color.red, (playerVelocityY - 40f) / 20);
                newInnerColor = Color.red;
            }
            else if (playerVelocityY >= 20f) //23.1
            {
                newInnerColor = Color.Lerp(Color.yellow, Color.red, (playerVelocityY - 20f) / 20f);
                newShaderColor = Color.clear;
                //innerShader.color = newShaderColor;
            }
            else if (playerVelocityY >= 0)//19.95)
            {
                newInnerColor = Color.Lerp(new Color(1f, .5f, .8f, 1), Color.yellow, playerVelocityY / 20f);
                newShaderColor = Color.clear;
                //innerShader.color = newShaderColor;
            }

            speed = Mathf.MoveTowards(speed, SP, transitionSpeed * Time.deltaTime);
            innerSection.localScale = Vector3.MoveTowards(innerSection.localScale, innerScale, transitionSpeed * Time.deltaTime);
            transform.localScale = Vector3.MoveTowards(transform.localScale, transformScale, transitionSpeed * Time.deltaTime);
            innerSolid.color = MoveTowardsColor(innerSolid.color, newInnerColor, transitionSpeed * Time.deltaTime);
            innerShader.color = MoveTowardsColor(innerShader.color, newShaderColor, transitionSpeed * Time.deltaTime);
            //innerSolid.color = new Color(innerSolid.color, newInnerColor, transitionSpeed);
            //innerShader.color = new Color.Lerp(innerShader.color, newShaderColor, transitionSpeed);
        }

        pulseSprite.color = new Color(innerSolid.color.r, innerSolid.color.g, innerSolid.color.b, pulseSprite.color.a);
        pulse_light.color = new Color(innerSolid.color.r, innerSolid.color.g, innerSolid.color.b, pulse_light.color.a);
    }

    public Color MoveTowardsColor(Color x, Color y, float moveAmt)
    {
        Vector4 X = x;
        Vector4 Y = y;
        Vector4 Z = Vector4.MoveTowards(x, y, moveAmt);
        Color c = new Color(Z.x, Z.y, Z.z, Z.w);
        return c;
    }

    //Sequence pulseSequence;// = DOTween.Sequence();
    Tweener pulseScaleTween;
    Tweener pulseAlphaTween;
    public void Pulse()
    {
        pulse.SetActive(true);

        pulseScaleTween.Kill();
        pulseAlphaTween.Kill();

        pulse.transform.localScale = new Vector2(scale, scale);
        pulseSprite.color = new Color(red, green, blue, 1);
        pulse_light.intensity = 1;

        pulseScaleTween = pulse.transform.DOScale(Vector3.zero, .5f).SetEase(Ease.Linear);
        pulseAlphaTween = pulseSprite.DOColor(new Color(red, green, blue, 0), .3f).SetEase(Ease.Linear);

        if (sfx != null) { sfx.PlayOneShot(sfx.clip, gamemanager.sfx_volume); }

        if (rebound)
        {
            transitionSpeed = 0;
        }
    }

    Tweener ringScaleTween;
    Tweener ringAlphaTween;
    public void RingPulse()
    {
        ring.SetActive(true);
        ringScaleTween.Kill();
        ringAlphaTween.Kill();

        ring.transform.localScale = new Vector2(.6f, .6f);
        ringSprite.color = new Color(ringSprite.color.r, ringSprite.color.g, ringSprite.color.b, 1f);

        ringScaleTween = ring.transform.DOScale(Vector3.one * 3, .5f).SetEase(Ease.Linear);
        ringAlphaTween = ringSprite.DOColor(new Color(ringSprite.color.r, ringSprite.color.g, ringSprite.color.b, 0), .4f).SetEase(Ease.OutQuad);
    }

    public Vector2 GetDirection()
    {
        return parent.right;
    }

    public float GetAngle()
    {
        return parent.eulerAngles.z;
    }

    public Transform getTeleport()
    {
        return TeleportTo;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            RingPulse();
        }
    }

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