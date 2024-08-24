using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSettingsTrigger : MonoBehaviour
{
    [Range(-1, 3)]
    public int gravityDirection = -1;

    [Range(-1, 3)]
    public int reverseDirection = 0;

    public bool stopDash;
    public bool disableTrail;
    public bool cancelJump;
    public bool cancelY;
    public bool zeroMovingPlatformVelocity;
    public bool disableSelfDestruct;
    public bool waveScreenSpace;

    public Transform setPosition;

    [Min(0)]
    public int uses;

    public bool useUntilExit;

    private void Awake()
    {
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!useUntilExit && collision.tag == "Player" && uses > 0)
        {
            uses--;

            if(uses == 0)
            {
                GetComponent<Collider2D>().enabled = false;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (useUntilExit && collision.tag == "Player" && uses > 0)
        {
            uses--;

            if (uses == 0)
            {
                GetComponent<Collider2D>().enabled = false;
            }
        }
    }
}
