using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomTrigger : MonoBehaviour
{
    public GameObject[] triggers;
    public int[] values;
    public bool oneuse;

    private bool inuse = false, finished = false;
    private int maxValue = 0;

    private void Awake()
    {
        for (int i = 0; i < values.Length; i++)
        {
            maxValue += values[i];
            values[i] = maxValue;
        }

        gameObject.transform.GetChild(0).gameObject.SetActive(false);
    }
    public IEnumerator Choose()
    {
        finished = false;
        int random_value = Random.Range(0, maxValue+1);

        SpawnTrigger spawntrigger = gameObject.AddComponent<SpawnTrigger>();
        spawntrigger.enabled = false;
        spawntrigger.TriggerOrb = true;
        spawntrigger.exe = SpawnTrigger.ExecutionType.Parallel;
        spawntrigger.delay = new float[1];
        spawntrigger.triggers = new GameObject[1];

        for (int j = 0; j < values.Length; j++)
        {
            if(random_value <= values[j])
            {
                spawntrigger.triggers[0] = triggers[j];
                break;
            }
        }

        yield return null;

        //if(spawntrigger == null) { Debug.Log("NULL"); }
        StartCoroutine(spawntrigger.Begin());

        if (spawntrigger.getExeType() == SpawnTrigger.ExecutionType.Parallel)
        {
            while (!spawntrigger.getFinished())
            {
                yield return null;
            }
        }

        finished = true;

        if (oneuse)
        {
            Destroy(gameObject);
        }

        Destroy(spawntrigger);
        inuse = false;
    }

    public bool getFinished()
    {
        return finished;
    }

    public void SpawnActivate()
    {
        if(!inuse)
        {
            StartCoroutine(Choose());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && !inuse)
        {
            inuse = true;
            StartCoroutine(Choose());
            //Activate();
        }
    }
}
