using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnTimerHazard : MonoBehaviour
{
    public AudioSource sound;
    private Transform player;
    public ParticleSystem particles, entryparticles;
    public BoxCollider2D collider;

    [Min(0f)]
    public float inittime, timeon, timeoff, fadein, fadeout;
    WaitForSeconds waitINIT, waitON, waitOFF, waitIN, waitOUT;

    [Min(0f)]
    public float entrytime;
    WaitForSeconds waitENTRY;

    public Vector4 startBox, endBox;

    [Min(0f)]
    public float minDistance, maxDistance, nullDistance;

    public bool disablecollider, mute_if_invisivle;

    private bool start = false;
    private GameManager gamemanager;

    public bool showGizmos = true;

    private void Awake()
    {
        gamemanager = FindObjectOfType<GameManager>();
        particles.Stop();
        waitINIT = new WaitForSeconds(inittime);
        waitON = new WaitForSeconds(timeon);
        waitOFF = new WaitForSeconds(timeoff);
        waitIN = new WaitForSeconds(fadein);
        waitOUT = new WaitForSeconds(fadeout);
        waitENTRY = new WaitForSeconds(entrytime);

        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Start()
    {
        collider.size = new Vector2(endBox.x, endBox.y);
        collider.offset = new Vector2(endBox.z, endBox.w);
        collider.enabled = !disablecollider;
        StartCoroutine(Init());
    }

    void Update()
    {
        Vector3 screen_pos = Camera.main.WorldToViewportPoint(gameObject.transform.position);
        float distance = Vector2.Distance(transform.position, player.position);
        if(distance > nullDistance || (mute_if_invisivle && !(screen_pos.x >= -0.05 && screen_pos.x <= 1.05 && screen_pos.y >= -0.05 && screen_pos.y <= 1.05)))
        {
            sound.Stop(); sound.volume = 0;
        }
        else if (distance > maxDistance)
        {
            sound.volume = 0;
        }
        else if(distance < minDistance)
        {
            sound.volume = gamemanager.sfx_volume;
        }
        else
        {
            sound.volume = gamemanager.sfx_volume * (1 - (distance - minDistance) / (maxDistance - minDistance));
        }

        if(start)
        {
            StopAllCoroutines();
            StartCoroutine(Sequence());
            start = false;
        }
    }

    IEnumerator Init()
    {
        /*float time = 0;
        while (time < inittime)
        {
            time += Time.deltaTime;
            yield return null;
        }*/

        if(inittime > 0)
        {
            yield return waitINIT;
        }
        
        start = true;
    }

    IEnumerator Sequence()
    {
        sound.Stop();
        entryparticles.Stop();
        particles.Stop();
        collider.size = new Vector2(endBox.x, endBox.y);
        collider.offset = new Vector2(endBox.z, endBox.w);
        collider.enabled = !disablecollider;

        float time = 0;
        /*while(time < timeoff)
        {
            time += Time.deltaTime;
            yield return null;
        }*/
        if(timeoff > 0)
        {
            yield return waitOFF;
        }

        if(entrytime > 0)
        {
            time = 0;
            entryparticles.Play();
            /*while (time < entrytime)
            {
                time += Time.deltaTime;
                yield return null;
            }*/
            yield return waitENTRY;
        }

        if (Vector2.Distance(transform.position, player.position) < nullDistance)
        {
            sound.Play();
        }

        particles.Play();

        collider.enabled = true;
        time = 0;
        while(time/fadein < 1)
        {
            collider.size = Vector2.Lerp(new Vector2(endBox.x, endBox.y), new Vector2(startBox.x, startBox.y), time);
            collider.offset = Vector2.Lerp(new Vector2(endBox.z, endBox.w), new Vector2(startBox.z, startBox.w), time);
            time += Time.deltaTime;
            yield return null;
        }

        collider.size = new Vector2(startBox.x, startBox.y);
        collider.offset = new Vector2(startBox.z, startBox.w);

        /*time = 0;
        while (time < timeon)
        {
            time += Time.deltaTime;
            yield return null;
        }*/
        if(timeon > 0)
        {
            yield return waitON;
        }

        time = 0;
        while (time / fadeout < 1)
        {
            collider.size = Vector2.Lerp(new Vector2(startBox.x, startBox.y), new Vector2(endBox.x, endBox.y), time);
            collider.offset = Vector2.Lerp(new Vector2(startBox.z, startBox.w), new Vector2(endBox.z, endBox.w), time);
            time += Time.deltaTime;
            yield return null;
        }

        collider.size = new Vector2(endBox.x, endBox.y);
        collider.offset = new Vector2(endBox.z, endBox.w);
        collider.enabled = !disablecollider;

        start = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (showGizmos)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + new Vector3(startBox.z, startBox.w, 0), new Vector3(startBox.x, startBox.y, 0));

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position + new Vector3(endBox.z, endBox.w, 0), new Vector3(endBox.x, endBox.y, 0));

            Gizmos.color = new Color(0, 1, 1, .2f);
            Gizmos.DrawWireSphere(transform.position, minDistance);

            Gizmos.color = new Color(0, 0, 1, .2f);
            Gizmos.DrawWireSphere(transform.position, maxDistance);

            Gizmos.color = new Color(1, 0, 0, .2f);
            Gizmos.DrawWireSphere(transform.position, nullDistance);
        }
    }
}
