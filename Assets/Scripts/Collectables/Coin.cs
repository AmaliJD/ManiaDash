using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class Coin : MonoBehaviour
{
    private bool collected = false;
    private Animator animator;
    private SpriteRenderer sprite;
    private CubeController cube;
    private Vector3 jumpPos;

    public AudioSource pickup;
    public Light2D coinLight;
    public ParticleSystem particles;
    public int coinNum;
    public bool checkpoint_collect;
    public bool ghost;

    private GameManager gamemanager;
    private Vector3 startingPos;
    private float startingIntens;

    private void Awake()
    {
        gamemanager = FindObjectOfType<GameManager>();
        cube = FindObjectOfType<CubeController>();
        animator = gameObject.GetComponent<Animator>();
        sprite = gameObject.GetComponent<SpriteRenderer>();

        startingPos = transform.position;
        startingIntens = coinLight.intensity;
    }

    private IEnumerator Collect()
    {
        //transform.parent = null;
        if (particles.isPlaying) { particles.Stop(); }
        if (pickup != null) { pickup.PlayOneShot(pickup.clip, gamemanager.sfx_volume); }
        gamemanager.incrementCoinCount(coinNum, checkpoint_collect, ghost);
        animator.speed = 3;
        sprite.color = new Color(1, 1, 1, 1);

        while (sprite.color.a >= .1)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + .03f, transform.position.z);
            sprite.color = new Color(1,1,1, sprite.color.a *.90f);
            coinLight.intensity *= .90f;

            yield return null;
        }

        sprite.color = new Color(1, 1, 1, 0);
        coinLight.intensity = 0;

        if (!checkpoint_collect) { Destroy(gameObject); }
    }

    public void resetCoin()
    {
        StopAllCoroutines();
        collected = false;
        if (!particles.isPlaying) { particles.Play(); }
        animator.speed = 0.8f;
        sprite.color = new Color(1, 1, 1, 1);
        coinLight.intensity = startingIntens;
        transform.position = startingPos;
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
