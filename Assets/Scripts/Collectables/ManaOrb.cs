using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class ManaOrb : MonoBehaviour
{
    private bool collected = false;
    private float init = .3f;//.3f; //20
    private PlayerControllerV2 player;
    private Vector3 jumpPos;

    public float x, y;

    public Light2D blueLight;
    public Light2D whiteLight;

    public AudioSource pickup, sfx;

    private GameManager gamemanager;

    private void Awake()
    {
        //gamemanager = FindObjectOfType<GameManager>();
        gamemanager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        jumpPos = transform.position + new Vector3(x, y, 0);
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControllerV2>();
    }

    public GameManager getGameManager()
    {
        return gamemanager;
    }
    /*
    private void Update()
    {
        if(Input.GetKeyDown("x"))
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }
    }*/

    // Update is called once per frame
    /*
    void FixedUpdate()
    {
        if(collected)
        {
            if(init > 0)
            {
                transform.position = Vector3.Lerp(transform.position, jumpPos, .2f);
                init--;
            }
            else
            {
                if (Mathf.Abs(transform.position.x - cube.transform.position.x) >= .3 && Mathf.Abs(transform.position.y - cube.transform.position.y) >= .3)
                {
                    transform.position = Vector3.Lerp(transform.position, new Vector3(cube.transform.position.x, cube.transform.position.y, 0f), .15f);
                }
                else
                {
                    transform.position = Vector3.Lerp(transform.position, new Vector3(cube.transform.position.x, cube.transform.position.y, 0f), .6f);
                }

                if (Mathf.Abs(transform.position.x - cube.transform.position.x) <= .2 && Mathf.Abs(transform.position.y - cube.transform.position.y) <= .2)
                {
                    gamemanager.incrementManaCount(1);
                    Destroy(gameObject);
                }

                blueLight.intensity *= .95f;
                whiteLight.intensity *= .95f;
            }
        }
    }*/

    private IEnumerator Collect()
    {
        transform.parent = null;
        if (pickup != null && pickup.name != "None") { pickup.PlayOneShot(pickup.clip, gamemanager.sfx_volume); }
        jumpPos = transform.position + new Vector3(x, y, 0);

        float time = .2f;
        float BLI = blueLight.intensity;
        float WLI = whiteLight.intensity;
        
        //Vector3 initialPosition = transform.position;
        //Vector3 oldTarget = player.transform.position;
        //float moveSpeed = 15f;

        while (true)
        {
            if (init > 0)
            {
                transform.position = Vector3.Lerp(transform.position, jumpPos, 1 - Mathf.Pow((init/.3f), 1.3f)); //.2f
                //init--;
                init -= Time.deltaTime;
                //transform.position = Vector2.MoveTowards(transform.position, jumpPos, 10 * Time.deltaTime);
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, player.transform.position, Mathf.Pow(1 - (time / .2f), 1.2f));
                time -= Time.deltaTime;

                blueLight.intensity = Mathf.Lerp(BLI, 0, 1 - (time / .2f));
                whiteLight.intensity = Mathf.Lerp(WLI, 0, 1 - (time / .2f));

                if (Vector2.Distance(transform.position, player.transform.position) <= .2)
                {
                    break;
                }
            }

            yield return null;
        }

        if (sfx != null && sfx.name != "None") { sfx.PlayOneShot(sfx.clip, gamemanager.sfx_volume); }
        //sfx.Play();
        gamemanager.incrementManaCount(1);
        Destroy(gameObject);
    }

    Vector3 LinearMovingLerp(Vector3 follower_curr_pos, Vector3 target_old_pos, Vector3 target_curr_pos, float t, float k)
    {
        Vector3 f = follower_curr_pos - target_old_pos + (target_curr_pos - target_old_pos) / (k * t);
        return target_curr_pos - (target_curr_pos - target_old_pos) / (k * t) + f * Mathf.Exp(-k * t);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && !collected)
        {
            collected = true;
            StartCoroutine(Collect());
        }
    }
}
