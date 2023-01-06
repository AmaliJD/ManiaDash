using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlternatingGravityOrb : MonoBehaviour
{
    PlayerControllerV2 player;
    int direction = 0;
    public bool down = true, right = true, up = true, left = true;
    public GameObject[] self, obj;
    public Collider2D selfCollider, objCollider;
    public AlternatingGravityOrb altOrb;

    private void Awake()
    {
        //player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControllerV2>();
        foreach(GameObject go in GameObject.FindGameObjectsWithTag("Player"))
        {
            if(go.GetComponent<PlayerControllerV2>() != null)
            {
                player = go.GetComponent<PlayerControllerV2>();
                break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        direction = player.GetGravityDirection();

        if((direction == 0 && !down) || (direction == 1 && !left) || (direction == 2 && !up) || (direction == 3 && !right))
        {
            foreach(GameObject go in self)
            {
                go.SetActive(false);
            }
            foreach (GameObject go in obj)
            {
                go.SetActive(true);
            }

            selfCollider.enabled = false;
            objCollider.enabled = true;
            altOrb.enabled = true;
            enabled = false;
        }
    }
}
