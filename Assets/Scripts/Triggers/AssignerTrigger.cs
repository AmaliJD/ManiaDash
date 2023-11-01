using System.Collections;
using System.Collections.Generic;
//using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AssignerTrigger : MonoBehaviour
{
    public bool disableAssigner;
    public ColorAssigner assigner;
    public ColorReference colorRef;

    [SerializeField] [Range(-360f, 360f)] private float hue;
    [SerializeField] [Range(-1f, 1f)] private float sat;
    [SerializeField] [Range(-1f, 1f)] private float val;
    [SerializeField] [Range(-1f, 1f)] private float alpha;

    public float duration;
    public bool oneuse = true;
    private bool inuse = false;
    private StopAssignerChange parent;

    // Start is called before the first frame update
    void Awake()
    {
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        parent = transform.parent.GetComponent<StopAssignerChange>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && !inuse)
        {
            inuse = true;

            if (disableAssigner != assigner.disabled)
            {
                assigner.disabled = disableAssigner;
                return;
            }

            parent.Send(assigner.GetHashCode());
            parent.setActiveTrigger(this, assigner.GetHashCode());

            StartCoroutine(Activate());
        }
    }

    public void SpawnActivate()
    {
        inuse = true;

        if (disableAssigner != assigner.disabled)
        {
            assigner.disabled = disableAssigner;
            return;
        }

        parent.Send(assigner.GetHashCode());
        parent.setActiveTrigger(this, assigner.GetHashCode());

        StartCoroutine(Activate());
    }

    private IEnumerator Activate()
    {
        float time = 0;
        Color tempColor;

        if(colorRef != null)
        {
            assigner.ColorReference = colorRef;
            assigner.Restart();
        }

        float H = assigner.hue, V = assigner.val, S = assigner.sat, A = assigner.alpha;

        if (duration > 0)
        {
            while (time <= duration)
            {
                assigner.hue = Mathf.Lerp(H, hue, time / duration);
                assigner.val = Mathf.Lerp(V, val, time / duration);
                assigner.sat = Mathf.Lerp(S, sat, time / duration);
                assigner.alpha = Mathf.Lerp(A, alpha, time / duration);

                tempColor = assigner.ColorReference.channelcolor;
                assigner.ColorReference.Set(tempColor);

                time += Time.deltaTime;
                yield return null;
            }
        }

        assigner.hue = hue;
        assigner.sat = sat;
        assigner.val = val;
        assigner.alpha = alpha;

        tempColor = assigner.ColorReference.channelcolor;
        assigner.ColorReference.Set(tempColor);

        if (oneuse)
        {
            Destroy(gameObject);
        }

        inuse = false;

        /*if (!oneuse)
        {
            inuse = false;
        }*/
    }

    /*
    private IEnumerator Activate()
    {
        float time = 0;
        Color tempColor;

        float hue_step = (hue - assigner.hue) / (duration * Time.deltaTime);
        float sat_step = (sat - assigner.sat) / (duration * Time.deltaTime);
        float val_step = (val - assigner.val) / (duration * Time.deltaTime);
        float alpha_step = (alpha - assigner.alpha) / (duration * Time.deltaTime);

        while (time < duration)
        {
            assigner.hue += hue_step;
            assigner.sat += sat_step;
            assigner.val += val_step;
            assigner.alpha += alpha_step;

            tempColor = assigner.ColorReference.channelcolor;
            assigner.ColorReference.Set(tempColor);

            time += Time.deltaTime;
            yield return null;
        }

        assigner.hue = hue;
        assigner.sat = sat;
        assigner.val = val;
        assigner.alpha = alpha;

        tempColor = assigner.ColorReference.channelcolor;
        assigner.ColorReference.Set(tempColor);

        if (oneuse)
        {
            Destroy(gameObject);
        }
        
        inuse = false;
    }*/

    public void Stop()
    {
        StopAllCoroutines();
        inuse = false;
    }
}
