using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToPositions : MonoBehaviour
{
    public float delay;
    public Vector4[] movement; //x, y, duration, waitbefore
    public GameObject eyes, scale;
    public MoveTrigger.Ease easing = MoveTrigger.Ease.EaseInOut;
    public MoveTrigger.Ease[] easings;
    public bool userigidbody = true;
    public bool spaceSelf = false;
    public bool invertmotionx, invertmotiony;
    public float Clamp = 1;
    public bool loop = true;
    private bool whileTrue = true;

    private Rigidbody2D body;

    //public bool useReferenceStartTime;
    //public MoveToPositions referenceStartScript;
    //[HideInInspector] public float startTime;
    /*private void OnEnable()
    {
        startTime = Time.time;

        if(useReferenceStartTime && referenceStartScript != null && referenceStartScript.startTime != 0)
        {
            startTime = referenceStartScript.startTime;
        }
    }*/

    private void Start()
    {
        if(GetComponent<Rigidbody2D>() != null) body = gameObject.GetComponent<Rigidbody2D>();

        /*startTime = Time.time;
        if (useReferenceStartTime && referenceStartScript != null && referenceStartScript.startTime != 0)
        {
            startTime = referenceStartScript.startTime;
        }*/

        StartCoroutine(Move());
    }
    
    public IEnumerator Move()
    {
        //float errortime_0 = startTime;
        if (delay > 0) yield return new WaitForSeconds(delay);
        //float errortime_1 = Time.time;
        //float errortime = ((errortime_1 - errortime_0) - delay);

        //Debug.Log("ERROR TIME: " + errortime);

        int i = 0;
        int revx = invertmotionx ? -1 : 1;
        int revy = invertmotiony ? -1 : 1;

        //*
        GameObject moveTriggerHolder = new GameObject();
        moveTriggerHolder.name = "Move Trigger Holder " + moveTriggerHolder.GetHashCode();
        moveTriggerHolder.transform.parent = transform;
        MoveTrigger trigger = moveTriggerHolder.AddComponent<MoveTrigger>();
        MoveTrigger trigger2 = moveTriggerHolder.AddComponent<MoveTrigger>();
        //MoveTrigger trigger = moveTriggerHolder.GetComponent<MoveTrigger>();
        //MoveTrigger trigger2 = moveTriggerHolder.GetComponent<MoveTrigger>();

        bool T1 = true;
        //*/

        while (whileTrue)
        {
            float time = 0;

            /*float adjustTime = 0;
            if (Mathf.Abs(errortime) >= .1f)
            {
                adjustTime = errortime;
                errortime = 0;
            }*/

            //errortime_0 = Time.time;
            while (time < movement[i].w)// - adjustTime)
            {
                if (eyes != null) eyes.transform.localPosition = new Vector3(Mathf.Clamp((revx * body.velocity.x) / 50, -Clamp, Clamp), (revy*scale.transform.localScale.y - 1) / 2, 0);
                time += Time.deltaTime;
                yield return null;
            }
            //errortime_1 = Time.time;
            //errortime += ((errortime_1 - errortime_0) - movement[i].w);

            /*GameObject moveTriggerHolder = new GameObject();
            moveTriggerHolder.name = "Move Trigger Holder " + moveTriggerHolder.GetHashCode();
            moveTriggerHolder.transform.parent = transform;
            moveTriggerHolder.AddComponent<MoveTrigger>();*/

            //MoveTrigger trigger = moveTriggerHolder.GetComponent<MoveTrigger>();
            //MoveTrigger trigger = new MoveTrigger();
            if (T1)
            {
                trigger.disableOnTrigger = true;
                trigger.group = gameObject;
                trigger.x = movement[i].x;
                trigger.y = movement[i].y;
                trigger.duration = movement[i].z;
                trigger.easing = easings.Length == 0 ? easing : easings[i];
                trigger.userigidbody = userigidbody;
                trigger.spaceSelf = spaceSelf;

                trigger.Initialize();

                StartCoroutine(trigger.Move());
                trigger2.ResetTrigger();
            }
            else
            {
                trigger2.disableOnTrigger = true;
                trigger2.group = gameObject;
                trigger2.x = movement[i].x;
                trigger2.y = movement[i].y;
                trigger2.duration = movement[i].z;
                trigger2.easing = easings.Length == 0 ? easing : easings[i];
                trigger2.userigidbody = userigidbody;
                trigger2.spaceSelf = spaceSelf;

                trigger2.Initialize();

                StartCoroutine(trigger2.Move());
                trigger.ResetTrigger();
            }

            T1 = !T1;

            time = 0;
            //errortime_0 = Time.time;
            while (time < movement[i].z)
            {
                if (eyes != null && body != null) eyes.transform.localPosition = new Vector3(Mathf.Clamp((revx * body.velocity.x) / 50, -Clamp, Clamp), revy*(scale.transform.localScale.y - 1)/2, 0);
                time += Time.deltaTime;
                yield return null;
            }
            //errortime_1 = Time.time;
            //errortime += ((errortime_1 - errortime_0) - movement[i].z);

            //Debug.Log("ERROR TIME: " + errortime);
            //trigger.ResetTrigger();

            //trigger.StopAllCoroutines();
            //Debug.Log(trigger == null);

            if (eyes != null && body != null) eyes.transform.localPosition = new Vector3(Mathf.Clamp((revx * body.velocity.x) / 50, -Clamp, Clamp), revy*(scale.transform.localScale.y - 1) / 2, 0);
            i++;
            if(i >= movement.Length) { i = 0; whileTrue = loop; }
            //yield return null;

            //trigger.ResetTrigger();
            //Destroy(moveTriggerHolder);
            //Destroy(moveTriggerHolder.GetComponent<MoveTrigger>());
            //moveTriggerHolder.AddComponent<MoveTrigger>();
        }
    }
}
