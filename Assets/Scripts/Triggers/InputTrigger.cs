using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputTrigger : MonoBehaviour
{
    public GameObject[] triggers;
    //public KeyCode keycode;
    public bool hold;
    public float detectRate, duration;
    public bool oneuse;

    private bool inuse = false;
    private SpawnTrigger spawntrigger;

    private InputActions input;
    private bool detected;

    private void Awake()
    {
        // INPUT
        if (input == null)
        {
            input = new InputActions();
        }
        input.Player.Enable();

        gameObject.transform.GetChild(0).gameObject.SetActive(false);

        spawntrigger = gameObject.AddComponent<SpawnTrigger>();
        spawntrigger.enabled = false;
        spawntrigger.TriggerOrb = true;
        spawntrigger.exe = SpawnTrigger.ExecutionType.Parallel;
        spawntrigger.delay = new float[1];
        spawntrigger.triggers = triggers;
    }

    public IEnumerator DetectInput()
    {
        float time = 0;
        while(inuse)
        {
            if(!hold)
            {
                if (input.Player.Jump.ReadValue<float>() == 1 && !detected)
                {
                    detected = true;
                    Activate();
                }
            }
            else
            {
                if (input.Player.Jump.ReadValue<float>() == 1)
                {
                    detected = true;
                    Activate();
                }
            }

            if (input.Player.Jump.ReadValue<float>() != 1 && detected)
            {
                detected = false;
            }

            if (detectRate <= 0.1f)
            {
                yield return null;
                time += Time.deltaTime;
            }
            else
            {
                yield return new WaitForSeconds(detectRate);
                time += detectRate;
            }

            if(time > duration && duration != float.PositiveInfinity)
            {
                inuse = false;
            }
        }

        if(oneuse)
        {
            Destroy(this);
        }
    }

    public void Activate()
    {
        spawntrigger.ActivateIfNotInUse();
    }

    public void SpawnActivate()
    {
        if (!inuse)
        {
            inuse = true;
            StartCoroutine(DetectInput());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && !inuse)
        {
            inuse = true;
            StartCoroutine(DetectInput());
        }
    }
}
