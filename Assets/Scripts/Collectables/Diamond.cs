using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class Diamond : MonoBehaviour
{
    private bool collected = false;
    private float init = .3f;
    //private CubeController cube;
    private PlayerControllerV2 player;
    private Vector3 jumpPos;

    public float x, y;

    public Light2D blueLight;

    public AudioSource pickup, sfx;

    private GameManager gamemanager;

    private void Awake()
    {
        gamemanager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        jumpPos = transform.position + new Vector3(x, y, 0);
        //cube = GameObject.FindObjectOfType<CubeController>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControllerV2>();
    }
    
    private IEnumerator Collect()
    {
        transform.parent = null;
        if (pickup != null && pickup.name != "None") { pickup.PlayOneShot(pickup.clip, gamemanager.sfx_volume); }
        jumpPos = transform.position + new Vector3(x, y, 0);
        //pickup.Play();

        float time = .4f;
        float BLI = blueLight.intensity;

        while (true)
        {
            if (init > 0)
            {
                transform.position = Vector3.Lerp(transform.position, jumpPos, 1 - Mathf.Pow((init / .4f), 1.2f)); //.2f
                //init--;
                init -= Time.deltaTime;
                //transform.position = Vector2.MoveTowards(transform.position, jumpPos, 6 * Time.deltaTime);
            }
            else
            {
                /*if (Mathf.Abs(transform.position.x - cube.transform.position.x) >= .3 && Mathf.Abs(transform.position.y - cube.transform.position.y) >= .3)
                {
                    transform.position = Vector3.Lerp(transform.position, new Vector3(cube.transform.position.x, cube.transform.position.y, 0f), .5f);
                }
                else
                {
                    transform.position = Vector3.Lerp(transform.position, new Vector3(cube.transform.position.x, cube.transform.position.y, 0f), .66f);
                }

                if (Mathf.Abs(transform.position.x - cube.transform.position.x) <= .2 && Mathf.Abs(transform.position.y - cube.transform.position.y) <= .2)
                {
                    break;
                }

                blueLight.intensity *= .95f;*/

                transform.position = Vector3.Lerp(transform.position, player.transform.position, 1 - (time / .4f));
                time -= Time.deltaTime;
                //transform.position = Vector2.MoveTowards(transform.position, player.transform.position, 25 * Time.deltaTime);

                blueLight.intensity = Mathf.Lerp(BLI, 0, 1 - (time / .4f));

                if (Vector2.Distance(transform.position, player.transform.position) <= .2)
                {
                    break;
                }
            }

            yield return null;
        }

        gamemanager.incrementDiamondCount(1);

        if (sfx != null && sfx.name != "None") { sfx.PlayOneShot(sfx.clip, gamemanager.sfx_volume); }
        //sfx.Play();
        Destroy(gameObject);
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
