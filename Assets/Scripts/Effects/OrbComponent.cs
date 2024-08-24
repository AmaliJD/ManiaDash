using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using DG.Tweening;
using Freya;
using Shapes2D;
using System.Linq;

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
    public bool centerX, centerY;
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

    [Header("Super Orb")]
    public bool superOrb;
    public Gradient strengthGradient;

    [Range(0, 360)]
    public float forceDirection;

    [HideInInspector]
    public int superGravityDirection;

    [Range(-1, 3)]
    public int G0 = -1, G1 = -1, G2 = -1, G3 = -1;
    [Min(0)]
    public float strengthChangeSpeed;

    [Range(minStrength, maxStrength)]
    public float changedMin, changedMax;
    public bool increasing = true;
    private const float minStrength = 0, maxStrength = 50;
    public Transform sprites;
    private SpriteRenderer centerSprite, bgSprite;
    private Shape[] triangleOutlines = new Shape[6];
    private Transform[] arrowsHolder = new Transform[3];
    private SpriteRenderer[] nodules = new SpriteRenderer[4];
    private SpriteRenderer[] nodes = new SpriteRenderer[4];
    private ParticleSystem[] particles = new ParticleSystem[2];
    private Transform gravityParticle;

    [Header("Audio")]
    public AudioSource sfx;
    private GameManager gamemanager;

    [Header("Spawn Trigger")]
    public SpawnTrigger spawn;

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
        else if(superOrb)
        {
            initSuperOrbSprites();
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
            if (superOrb)
            {
                if (strengthChangeSpeed != 0) { UpdateSuperOrbStrength(); }
                else { SuperOrbUpdate(); }
            }
        }
    }

    private void OnValidate()
    {
        if(superOrb)
        {
            if (ifnullSuperOrbSprites())
            {
                initSuperOrbSprites();
            }
            setSuperOrbSprite();

            //sprites.GetChild(0).GetComponent<SpriteRenderer>().color = strengthGradient.Evaluate(Mathfs.Remap(minStrength, maxStrength, 0, 1, multiplier));
            //sprites.GetChild(2).GetChild(0).GetComponent<SpriteRenderer>().color = strengthGradient.Evaluate(Mathfs.Remap(minStrength, maxStrength, 0, 1, multiplier)).SetAlpha(.4f);
        }
    }

    void UpdateSuperOrbStrength()
    {
        multiplier = Mathf.MoveTowards(multiplier, increasing ? changedMax : changedMin, strengthChangeSpeed * Time.fixedDeltaTime);

        if(increasing && multiplier >= changedMax) { increasing = false; }
        else if (!increasing && multiplier <= changedMin) { increasing = true; }

        setSuperOrbSprite();
    }

    void initSuperOrbSprites()
    {
        centerSprite = sprites.GetChild(0).GetComponent<SpriteRenderer>();
        bgSprite = sprites.GetChild(2).GetChild(0).GetComponent<SpriteRenderer>();
        triangleOutlines[0] = sprites.GetChild(6).GetChild(0).GetChild(0).GetComponent<Shape>();
        triangleOutlines[1] = sprites.GetChild(6).GetChild(1).GetChild(0).GetComponent<Shape>();
        triangleOutlines[2] = sprites.GetChild(6).GetChild(1).GetChild(1).GetComponent<Shape>();
        triangleOutlines[3] = sprites.GetChild(6).GetChild(2).GetChild(0).GetComponent<Shape>();
        triangleOutlines[4] = sprites.GetChild(6).GetChild(2).GetChild(1).GetComponent<Shape>();
        triangleOutlines[5] = sprites.GetChild(6).GetChild(2).GetChild(2).GetComponent<Shape>();
        arrowsHolder[0] = sprites.GetChild(6).GetChild(0).GetComponent<Transform>();
        arrowsHolder[1] = sprites.GetChild(6).GetChild(1).GetComponent<Transform>();
        arrowsHolder[2] = sprites.GetChild(6).GetChild(2).GetComponent<Transform>();
        nodules[0] = sprites.GetChild(4).GetChild(0).GetComponent<SpriteRenderer>();
        nodules[1] = sprites.GetChild(4).GetChild(1).GetComponent<SpriteRenderer>();
        nodules[2] = sprites.GetChild(4).GetChild(2).GetComponent<SpriteRenderer>();
        nodules[3] = sprites.GetChild(4).GetChild(3).GetComponent<SpriteRenderer>();
        nodes[0] = sprites.GetChild(5).GetChild(0).GetComponent<SpriteRenderer>();
        nodes[1] = sprites.GetChild(5).GetChild(1).GetComponent<SpriteRenderer>();
        nodes[2] = sprites.GetChild(5).GetChild(2).GetComponent<SpriteRenderer>();
        nodes[3] = sprites.GetChild(5).GetChild(3).GetComponent<SpriteRenderer>();
        pulseSprite = pulse.GetComponent<SpriteRenderer>();
        pulse_light = pulse.transform.GetChild(0).gameObject.GetComponent<Light2D>();
        gravityParticle = sprites.parent.parent.GetChild(3);
        particles[0] = sprites.parent.parent.GetChild(1).GetChild(0).GetComponent<ParticleSystem>();
        particles[1] = sprites.parent.parent.GetChild(1).GetChild(1).GetComponent<ParticleSystem>();
    }

    void setSuperOrbSprite()
    {
        Color color = strengthGradient.Evaluate(Mathfs.Remap(minStrength, maxStrength, 0, 1, multiplier));
        centerSprite.color = color;
        bgSprite.color = color.SetAlpha(.4f);
        foreach(Shape sp in triangleOutlines)
        {
            sp.settings.outlineColor = color.SetBrightness(color.Brightness() * .7f);
        }

        red = color.r;
        green = color.g;
        blue = color.b;
        pulseSprite.color = color.SetAlpha(pulseSprite.color.a).SetBrightness(1);
        pulse_light.color = color.SetAlpha(pulse_light.color.a);

        ParticleSystem.MainModule ps0main = particles[0].main;
        ParticleSystem.MainModule ps1main = particles[1].main;
        ps0main.startColor = ps1main.startColor = color.SetBrightness(1);

        arrowsHolder[0].gameObject.SetActive(multiplier >= minStrength && multiplier <= 35);
        arrowsHolder[1].gameObject.SetActive(multiplier > 35 && multiplier <= 45);
        arrowsHolder[2].gameObject.SetActive(multiplier > 45);
        if (multiplier >= minStrength && multiplier <= 35)
        {
            arrowsHolder[0].localScale = arrowsHolder[0].localScale.SetY(Mathf.Sqrt(Mathfs.Remap(minStrength, 35f, 0f, 1.44f, multiplier)));
        }
        else if (multiplier > 35 && multiplier <= 45)
        {
            arrowsHolder[1].localScale = arrowsHolder[1].localScale.SetY(Mathfs.Remap(35f, 45f, .6f, .8f, multiplier));
        }
        else if (multiplier > 45 && multiplier <= maxStrength)
        {
            arrowsHolder[2].localScale = arrowsHolder[2].localScale.SetY(Mathfs.Remap(45f, 50f, .45f, .55f, multiplier));
        }

        SuperOrbUpdate();
    }

    bool ifnullSuperOrbSprites()
    {
        return centerSprite == null
                || bgSprite == null
                || triangleOutlines.Where(x => x == null).Count() != 0
                || arrowsHolder.Where(x => x == null).Count() != 0
                || nodules.Where(x => x == null).Count() != 0
                || nodes.Where(x => x == null).Count() != 0
                || particles.Where(x => x == null).Count() != 0
                || pulseSprite == null
                || pulse_light == null
                || gravityParticle == null;
    }

    void SuperOrbUpdate()
    {
        int G = -1;
        int playerGravityDirection = player != null ? player.gravityDirection : 0;
        switch (playerGravityDirection)
        {
            case 0: G = G0; break;
            case 1: G = G1; break;
            case 2: G = G2; break;
            case 3: G = G3; break;
        }

        if(G != -1)
        {
            nodules[0].color = Color.white.SetAlpha(G == 0 ? 1 : 0);
            nodules[1].color = Color.white.SetAlpha(G == 1 ? 1 : 0);
            nodules[2].color = Color.white.SetAlpha(G == 2 ? 1 : 0);
            nodules[3].color = Color.white.SetAlpha(G == 3 ? 1 : 0);

            gravityParticle.localRotation = Quaternion.Euler(new Vector3(0, 0, -90 * G));
            superGravityDirection = G;
            nodes[G].color = Color.white;
        }

        int nextG = G;
        float nextAlpha = 1;
        int[] nextGs = new int[3] { -1, -1, -1 };
        for (int i = 0; i < nextGs.Length; i++)
        {
            switch (nextG)
            {
                case 0: nextG = G0; break;
                case 1: nextG = G1; break;
                case 2: nextG = G2; break;
                case 3: nextG = G3; break;
                default: nextG = -1; break;
            }
            
            if (nextG != -1 && nextG != G && !nextGs.Contains(nextG))
            {
                nodes[nextG].color = Color.white.SetAlpha(nextAlpha);
                //Debug.Log(nextG + " = " + nextAlpha);
                nextAlpha *= .75f;
            }
            nextGs[i] = nextG;
        }

        for (int i = 0; i < 4; i++)
        {
            if(i != G && !nextGs.Contains(i))
            {
                nodes[i].color = Color.white.SetAlpha((G0 != i && G1 != i && G2 != i && G3 != i) ? 0 : .25f);
            }
        }

        foreach (Transform tr in arrowsHolder)
        {
            tr.rotation = Quaternion.Euler(new Vector3(0, 0, forceDirection + (superGravityDirection * -90) + 180));
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

        if (sfx != null)
        {
            if(!superOrb)
            {
                sfx.PlayOneShot(sfx.clip, gamemanager.sfx_volume);
            }
            else
            {
                sfx.pitch = Mathfs.Remap(-50, 0, .5f, 1.4f, -Mathf.Clamp(multiplier, minStrength, maxStrength));
                sfx.volume = Mathfs.Remap(0, 50, .3f, .75f, Mathf.Clamp(multiplier, minStrength, maxStrength));
                sfx.PlayOneShot(sfx.clip, gamemanager.sfx_volume);
            }
        }

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

    public Vector3 GetTeleportDisplacement()
    {
        //Vector3 to = TeleportTo.position;
        //Vector3 from = transform.position;
        return TeleportTo.position - transform.position;
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