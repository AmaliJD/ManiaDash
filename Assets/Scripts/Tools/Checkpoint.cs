using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Checkpoint : MonoBehaviour
{
    public GameObject gray;
    public GameObject green;
    public GameObject active;

    [Range(0, 4)]
    public int gravityDirection = 0;
    [Range(-1, 1)]
    public int reverseDirection = 1;
    public bool reversed = false, mini = false, respawnCentered = false, auto = false;

    public bool ignoreSpeed;
    public enum Speed { x0, x1, x2, x3, x4 }
    public Speed speed;

    //public enum Mode { X, cube, auto, ship, auto_ship, ufo, auto_ufo, wave, auto_wave, ball, auto_ball, spider, auto_spider, copter, auto_copter }
    //public Mode mode;

    private PlayerControllerV2 player;
    private GameManager gamemanager;
    private Checkpoint_Controller check;

    private int state = 0, index = -1;

    // Start is called before the first frame update
    void Awake()
    {
        gamemanager = FindObjectOfType<GameManager>();
        player = gamemanager.getController();
        check = FindObjectOfType<Checkpoint_Controller>();

        gray.SetActive(true);
        green.SetActive(false);
        active.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            gray.SetActive(false);
            green.SetActive(false);
            active.SetActive(true);
            state = 2;

            check.updateStates(index);
            player = gamemanager.getController();


            //float cos = -Mathf.Cos(transform.rotation.eulerAngles.z);
            //float sin = -Mathf.Sin(transform.rotation.eulerAngles.z);
            //Vector3 pos = new Vector3(transform.position.x + sin * .7f, transform.position.y + cos * .7f, transform.position.z);
            //Vector3 pos = transform.position;
            Vector3 offset = Vector3.zero;
            switch(gravityDirection)
            {
                case 0:
                    offset.y = -.7f * (mini ? .44f : 1); break;
                case 1:
                    offset.x = -.7f * (mini ? .44f : 1); break;
                case 2:
                    offset.y = .7f * (mini ? .44f : 1); break;
                case 3:
                    offset.x = .7f * (mini ? .44f : 1); break;
            }
            Vector3 pos = transform.position + offset;
            //Vector3 add = new Vector3(auto ? -.5f : 0f, 0, 0);
            Vector3 add = -player.getForwardOrientation() * (auto ? -.5f : 0f);

            player.setRespawn(respawnCentered ? transform.position + add : pos + add, gravityDirection, Convert.ToInt32(speed.ToString().Substring(1)), mini, reverseDirection, ignoreSpeed);
            //player.setRepawnSpeed(Convert.ToInt32(speed.ToString().Substring(1)));
            //Debug.Log(Convert.ToInt32(speed.ToString().Substring(1)));
        }
    }

    public void Green()
    {
        gray.SetActive(false);
        active.SetActive(false);
        green.SetActive(true);
        state = 1;
    }

    public int getState()
    {
        return state;
    }

    public void setIndex(int i)
    {
        index = i;
    }
}
