using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    public Animator anim;
    public bool reviveOnDeath;
    private bool collected = false;
    private GameManager gamemanager;
    private PlayerControllerV2 player;

    private void Awake()
    {
        //anim.Play("CollectableIdle");
        gamemanager = FindObjectOfType<GameManager>();
    }

    private void Update()
    {
        player = gamemanager.getController();
        //Debug.Log(player.getAble());
        if (!player.getAble() && collected)
        {
            if(reviveOnDeath)
            {
                collected = false;
                anim.SetTrigger("Idle");
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && !collected)
        {
            collected = true;
            anim.SetTrigger("Got");
            //anim.Play("CollectableGot");
        }
    }
}
