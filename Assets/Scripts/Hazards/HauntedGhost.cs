using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class HauntedGhost : MonoBehaviour
{
    private Transform player;
    private PlayerControllerV2 newPlayer;
    private GameManager gamemanager;
    private IconController iconcontroller;

    private float refreshRate;
    public float wait;
    private int dataCount;

    public PlayerFollowData[] data;

    // STATE
    public Transform body;
    public BoxCollider2D hitbox_activate;
    public GameObject hitbox_death;
    private int state = 0;

    // ICON
    public Color p1;
    public ColorReference playercolor1;
    public Color p2;
    public ColorReference playercolor2;

    int icon_index = 0;

    public ParticleSystem[] particles;
    public Light2D light;
    public TrailRenderer trail;
    public Transform ICONS;

    public Animator entranceAnimation;
    public AudioSource enterSfx;

    public GameObject deatheffect;
    private DrawColliders drawColliders;

    public struct PlayerFollowData
    {
        public Vector3 position;
        public Quaternion rotation;
        public PlayerControllerV2.Gamemode state;
        public bool dead;
        public int deathCount, checkCount;

        public PlayerFollowData(Vector3 pos, Quaternion rot, PlayerControllerV2 pc) // Constructor.
        {
            position = pos;
            rotation = rot;
            state = pc.getMode();
            dead = pc.getDead();
            deathCount = pc.getDeathCount();
            checkCount = pc.getCheckpointCount();
        }
    }

    private void Awake()
    {
        gamemanager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        newPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControllerV2>();
        iconcontroller = GameObject.FindGameObjectWithTag("Player").transform.GetChild(0).transform.GetChild(0).GetComponent<IconController>();
        drawColliders = Camera.main.GetComponent<DrawColliders>();

        /*refreshRate = Screen.currentResolution.refreshRate;
        dataCount = Mathf.FloorToInt(wait * refreshRate);
        data = new PlayerFollowData[dataCount];*/

        state = 0;
    }

    private void Start()
    {
        //ICON
        playercolor1.Set(p1);
        playercolor2.Set(p2);
        icon_index = iconcontroller.index;
        ICONS.GetChild(icon_index).gameObject.SetActive(true);
    }

    public void Activate()
    {
        body.position = player.position;
        body.rotation = newPlayer.getIconRotation();
        body.localScale = Vector3.one;
        trail.Clear();

        state = 1;
        hitbox_activate.enabled = false;
        body.gameObject.SetActive(true);
        entranceAnimation.Play("HauntedEntrance");

        refreshRate = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().getFPS();
        dataCount = Mathf.FloorToInt(wait * refreshRate);
        data = new PlayerFollowData[dataCount];
        //Debug.Log("Shadow Cube: refresh rate = " + refreshRate);
        //Debug.Log("Monitor: refresh rate = " + Screen.currentResolution.refreshRate);

        foreach (ParticleSystem ps in particles)
        {
            ps.Play();
        }

        light.intensity = 1;
        if (enterSfx != null) { enterSfx.PlayOneShot(enterSfx.clip, gamemanager.sfx_volume); }

        if(drawColliders != null && drawColliders.enabled)
        {
            drawColliders.AddColliders(hitbox_death.GetComponent<Collider2D>());
        }

        StartCoroutine(Follow());
    }

    public void Deactivate()
    {
        StopAllCoroutines();
        entranceAnimation.StopPlayback();
        state = 0;
        hitbox_death.SetActive(false);
        Instantiate(deatheffect, body.position, Quaternion.identity);
        StartCoroutine(Shrink());
    }

    public IEnumerator Follow()
    {        
        switch (newPlayer.getMode())
        {
            case PlayerControllerV2.Gamemode.cube:
            case PlayerControllerV2.Gamemode.auto_cube:
                Transform icon = ICONS.GetChild(icon_index);
                icon.gameObject.SetActive(true);
                icon.localScale = Vector3.one;
                icon.localPosition = Vector3.zero;
                for (int j = 30; j <= 36; j++)
                {
                    ICONS.GetChild(j).gameObject.SetActive(false);
                }
                break;
            case PlayerControllerV2.Gamemode.ball:
            case PlayerControllerV2.Gamemode.auto_ball:
                icon = ICONS.GetChild(icon_index);
                icon.gameObject.SetActive(false);
                for (int j = 30; j <= 36; j++)
                {
                    ICONS.GetChild(j).gameObject.SetActive(j == 33);
                }
                break;
        }

        PlayerControllerV2.Gamemode prevState;
        int put = 0;
        while(put != data.Length)
        {
            if(Time.timeScale != 0)
            {
                data[put] = new PlayerFollowData(player.position, newPlayer.getIconRotation(), newPlayer);
                if (data[put].dead) { Deactivate(); }
                put++;
            }
                        
            yield return null;
        }

        prevState = data[0].state;
        int i = 0;
        while(true)
        {
            if (Time.timeScale != 0)
            {
                body.position = data[i].position;
                body.rotation = data[i].rotation;

                if (data[i].state != prevState)
                {
                    switch (data[i].state)
                    {
                        case PlayerControllerV2.Gamemode.cube:
                        case PlayerControllerV2.Gamemode.auto_cube:
                            Transform icon = ICONS.GetChild(icon_index);
                            icon.gameObject.SetActive(true);
                            icon.localScale = Vector3.one;
                            icon.localPosition = Vector3.zero;
                            for (int j = 30; j <= 36; j++)
                            {
                                ICONS.GetChild(j).gameObject.SetActive(false);
                            }
                            break;
                        case PlayerControllerV2.Gamemode.ball:
                        case PlayerControllerV2.Gamemode.auto_ball:
                            icon = ICONS.GetChild(icon_index);
                            icon.gameObject.SetActive(false);
                            for (int j = 30; j <= 36; j++)
                            {
                                ICONS.GetChild(j).gameObject.SetActive(j == 33);
                            }
                            break;
                    }
                }

                prevState = data[i].state;
                int checkCount = data[i].checkCount;
                data[i] = new PlayerFollowData(player.position, newPlayer.getIconRotation(), newPlayer);
                if (data[i].dead || data[i].checkCount != checkCount) { Deactivate(); }

                i++;
                if (i == data.Length) i = 0;
            }
            yield return null;
        }
    }

    public IEnumerator Shrink()
    {
        hitbox_death.SetActive(false);
        foreach (ParticleSystem ps in particles)
        {
            ps.Stop();
        }

        float time = 0;
        while(time <= 0.9f)
        {
            body.Rotate(new Vector3(0, 0, -5f), Space.Self);
            body.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, time / 0.9f);
            light.intensity = 1 - (time / 0.9f);
            time += Time.deltaTime;
            yield return null;
        }

        body.localScale = Vector3.zero;
        light.intensity = 0;
        body.gameObject.SetActive(false);
        hitbox_activate.enabled = true;
        hitbox_death.SetActive(false);
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player" && !collision.isTrigger)
        {
            switch (state)
            {
                case 0:
                    Activate();
                    break;
            }
        }
    }
}
