using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopTrigger : MonoBehaviour
{
    public GameObject[] triggers;
    public float delay;
    public bool oneuse;

    private bool inuse = false;

    private void Awake()
    {
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator Stop()
    {
        float time = 0;
        if(delay > 0)
        {
            while(time < delay)
            {
                time += Time.deltaTime;
                yield return null;
            }
        }

        foreach(GameObject trigger in triggers)
        {
            switch (trigger.gameObject.tag)
            {
                // MOVE TRIGGER
                case "MoveTrigger":

                    MoveTrigger move = trigger.GetComponent<MoveTrigger>();
                    RotateTrigger rotate = trigger.GetComponent<RotateTrigger>();
                    ScaleTrigger scale = trigger.GetComponent<ScaleTrigger>();

                    if (move != null)
                    {
                        move.StopAllCoroutines();
                    }
                    else if (rotate != null)
                    {
                        rotate.StopAllCoroutines();
                    }
                    else if (scale != null)
                    {
                        scale.StopAllCoroutines();
                    }
                    break;

                // SPAWN TRIGGER
                case "SpawnTrigger":
                    SpawnTrigger spawn = trigger.GetComponent<SpawnTrigger>();
                    spawn.StopAllCoroutines();
                    break;

                default:
                    break;
            }
        }

        inuse = false;
    }

    public void Activate()
    {
        inuse = true;
        StartCoroutine(Stop());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && !inuse)
        {
            Activate();
        }
    }
}
