using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class PadComponent : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject pulse;
    private SpriteRenderer pulseSprite;

    private bool enter = false, start = false;
    private float red = 255, green = 255, blue = 255, scale = 1;
    private float pulse_speed = .1f;

    public AudioSource sfx;
    private GameManager gamemanager;

    [Header("Rebound Orb")]
    public bool rebound;
    public Transform spriteSection;
    private SpriteRenderer inner, shader;
    private Light2D pulse_light;
    private PlayerControllerV2 player;
    private float transitionSpeed = 1f;

    [Header("Reverse")]
    [Range(-1, 3)]
    public int reverse;

    void Awake()
    {
        pulse.SetActive(false);
        pulseSprite = pulse.GetComponent<SpriteRenderer>();

        red = pulseSprite.color.r;
        green = pulseSprite.color.g;
        blue = pulseSprite.color.b;
        scale = pulse.transform.localScale.x;

        gamemanager = FindObjectOfType<GameManager>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControllerV2>();

        if (rebound)
        {
            inner = spriteSection.GetChild(0).GetComponent<SpriteRenderer>();
            shader = spriteSection.GetChild(2).GetComponent<SpriteRenderer>();
            pulse_light = spriteSection.GetChild(3).GetComponent<Light2D>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!visible) return;
        if (enter)
        {
            PulseSetup();
            enter = false;
            start = true;
            pulse.SetActive(true);
            if (sfx != null) { sfx.Stop();  sfx.PlayOneShot(sfx.clip, gamemanager.sfx_volume); }
        }
    }

    private void FixedUpdate()
    {
        if (!visible) return;
        if (start)
        {
            //pulse.transform.localScale = new Vector2(pulse.transform.localScale.x * 1.2f, pulse.transform.localScale.y * 1.2f);
            pulse_speed = 1f / pulse.transform.localScale.x;
            //pulse.transform.localScale = new Vector2(pulse.transform.localScale.x + pulse_speed, pulse.transform.localScale.y + pulse_speed);
            pulse.transform.localScale = Vector3.Lerp(pulse.transform.localScale, Vector3.one * 3, .08f);
            pulseSprite.color = new Color(red, green, blue, pulseSprite.color.a * Mathf.Clamp(pulse_speed * 1.1f, .8f, 1f));
            if (pulseSprite.color.a <= 0.02f)
            {
                start = false;
                pulse.SetActive(false);
                pulse.transform.localScale = new Vector2(scale, scale);
                pulseSprite.color = new Color(red, green, blue, 1f);
            }
        }

        if (rebound)
        {
            SetRebound();
        }
    }

    public void SetRebound()
    {
        float currPlayerVelocityY = player.getVelocityComponentY();

        if (currPlayerVelocityY <= .00001f) { transitionSpeed = 1f; }
        float height = Vector2.Dot(player.transform.position, -player.getGravityOrientation()) - Vector2.Dot(transform.position, -player.getGravityOrientation());
        float playerVelocityY = height >= 0 ? Mathf.Sqrt(2 * 9.81f * 9.81f * height) : currPlayerVelocityY;
        if (currPlayerVelocityY >= 0)
        {
            Color newInnerColor = inner.color;
            Color newShaderColor = shader.color;
            Vector3 spriteScale = spriteSection.localScale;
            Vector3 spritePos = spriteSection.localPosition;

            spriteScale = Vector2.Lerp(new Vector2(.6f, .8f), new Vector2(1f, 2f), playerVelocityY / (55f));
            spritePos = Vector2.Lerp(new Vector2(0, -0.015f), new Vector2(0, 0.08f), playerVelocityY / (55f));
            if (playerVelocityY >= 40f) //30.45
            {
                newShaderColor = Color.Lerp(Color.clear, Color.red, (playerVelocityY - 40) / 20);
                newInnerColor = Color.red;
            }
            else if (playerVelocityY >= 20f) //23.1
            {
                newInnerColor = Color.Lerp(Color.yellow, Color.red, (playerVelocityY - 20f) / 20f);
                newShaderColor = Color.clear;
            }
            else if (playerVelocityY >= 0)//19.95)
            {
                newInnerColor = Color.Lerp(new Color(1f, .5f, .8f, 1), Color.yellow, playerVelocityY / 20f);
                newShaderColor = Color.clear;
            }

            spriteSection.localScale = Vector3.MoveTowards(spriteSection.localScale, spriteScale, transitionSpeed * Time.fixedDeltaTime);
            spriteSection.localPosition = Vector3.MoveTowards(spriteSection.localPosition, spritePos, transitionSpeed * Time.fixedDeltaTime);
            inner.color = MoveTowardsColor(inner.color, newInnerColor, transitionSpeed * Time.fixedDeltaTime);
            shader.color = MoveTowardsColor(shader.color, newShaderColor, transitionSpeed * Time.fixedDeltaTime);
        }

        pulseSprite.color = new Color(inner.color.r, inner.color.g, inner.color.b, pulseSprite.color.a);
        pulse_light.color = new Color(inner.color.r, inner.color.g, inner.color.b, pulse_light.color.a);
    }

    public Color MoveTowardsColor(Color x, Color y, float moveAmt)
    {
        Vector4 X = x;
        Vector4 Y = y;
        Vector4 Z = Vector4.MoveTowards(x, y, moveAmt);
        Color c = new Color(Z.x, Z.y, Z.z, Z.w);
        return c;
    }

    void PulseSetup()
    {
        pulse.SetActive(false);
        pulse.transform.localScale = new Vector2(scale, scale);
        pulseSprite.color = new Color(pulseSprite.color.r, pulseSprite.color.g, 1);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            enter = true;
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
