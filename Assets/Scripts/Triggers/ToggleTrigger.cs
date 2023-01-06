using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ToggleTrigger : MonoBehaviour
{
    public int activeCount;
    public float delay;
    public bool childTrigger;
    public float childDelay;
    public bool toggleMode;
    public bool destroy;
    public bool oneuse = true;
    GameManager gamemanager;

    public GameObject manaburst;
    public AudioSource sfx;

    public GameObject[] on_targets;
    public GameObject[] off_targets;
    private bool omaewamou = false;
    private bool finished = true;
    private WaitForSeconds childWaitTime;
    private DrawColliders dc;

    private void Awake()
    {
        gamemanager = GameObject.FindObjectOfType<GameManager>();
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        if(childDelay != 0)
        {
            childWaitTime = new WaitForSeconds(childDelay);
        }

        dc = Camera.main.GetComponent<DrawColliders>();
    }

    public void toggle()
    {
        finished = false;
        foreach(GameObject obj in on_targets)
        {
            obj.SetActive(true);
        }
        foreach (GameObject obj in off_targets)
        {
            obj.SetActive(false);

            if (destroy)
                Destroy(obj);
        }
        finished = true;
    }

    public IEnumerator Toggle()
    {
        finished = false;

        if(delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }

        foreach (GameObject obj in on_targets)
        {
            if (obj == null) continue;
            if(!childTrigger)
            {
                obj.SetActive(true);
                yield return null;
            }
            else
            {
                List<GameObject> childlist = new List<GameObject>();
                foreach (Transform child in obj.transform)
                {
                    childlist.Add(child.gameObject);
                    //yield return null;
                }
                foreach (GameObject child in childlist)
                {
                    child.SetActive(true);

                    /*if (dc != null && dc.enabled && child.GetComponent<Collider2D>())
                    {
                        dc.AddColliders(child.GetComponent<Collider2D>());

                        foreach (Transform tr in child.transform)
                        {
                            foreach (Transform trc in tr)
                            {
                                if (trc.GetComponent<Collider2D>())
                                {
                                    dc.AddColliders(trc.GetComponent<Collider2D>());
                                }
                            }

                            if (tr.GetComponent<Collider2D>())
                            {
                                dc.AddColliders(tr.GetComponent<Collider2D>());
                            }
                        }
                    }*/

                    if (childDelay == 0)
                    {
                        yield return null;
                    }
                    else
                    {
                        yield return childWaitTime;
                    }
                }
                /*foreach (Transform child in obj.transform)
                {
                    child.gameObject.SetActive(true);
                    yield return null;
                }*/
            }

            /*if(dc != null && dc.enabled && obj.GetComponent<Collider2D>())
            {
                dc.AddColliders(obj.GetComponent<Collider2D>());

                foreach (Transform tr in obj.transform)
                {
                    foreach (Transform trc in tr)
                    {
                        if (trc.GetComponent<Collider2D>())
                        {
                            dc.AddColliders(trc.GetComponent<Collider2D>());
                        }
                    }
                    if (tr.GetComponent<Collider2D>())
                    {
                        dc.AddColliders(tr.GetComponent<Collider2D>());
                    }
                }
            }*/
        }
        foreach (GameObject obj in off_targets)
        {
            /*obj.SetActive(false);

            if (destroy)
                Destroy(obj);*/
            if (obj == null) continue;
            if (!childTrigger)
            {
                obj.SetActive(false);
                if (destroy) Destroy(obj);

                yield return null;
            }
            else
            {
                List<GameObject> childlist = new List<GameObject>();
                foreach (Transform child in obj.transform)
                {
                    childlist.Add(child.gameObject);
                    //yield return null;
                }
                foreach (GameObject child in childlist)
                {
                    child.SetActive(false);

                    /*if (dc != null && dc.enabled && child.GetComponent<Collider2D>())
                    {
                        dc.RemoveColliders(child.GetComponent<Collider2D>());
                    }*/

                    if (destroy) Destroy(child);

                    if (childDelay == 0)
                    {
                        yield return null;
                    }
                    else
                    {
                        yield return childWaitTime;
                    }
                }
            }

            /*if (dc != null && dc.enabled && obj.GetComponent<Collider2D>())
            {
                dc.RemoveColliders(obj.GetComponent<Collider2D>());

                foreach (Transform tr in obj.transform)
                {
                    foreach (Transform trc in tr)
                    {
                        if (trc.GetComponent<Collider2D>())
                        {
                            dc.RemoveColliders(trc.GetComponent<Collider2D>());
                        }
                    }

                    if (tr.GetComponent<Collider2D>())
                    {
                        dc.RemoveColliders(tr.GetComponent<Collider2D>());
                    }
                }
            }*/
        }

        if (dc != null && dc.enabled)
        {
            if (on_targets.Length > 2 || (on_targets.Length > 0 && on_targets[0].transform.childCount > 2))
            {
                dc.GetColliders();
            }
            else
            {
                foreach (GameObject obj in on_targets)
                {
                    if (obj.GetComponent<Collider2D>() != null) { dc.AddColliders(obj.GetComponent<Collider2D>()); }

                    foreach (Transform tr1 in obj.transform)
                    {
                        if (tr1.GetComponent<Collider2D>() != null) { dc.AddColliders(tr1.GetComponent<Collider2D>()); }

                        foreach (Transform tr2 in tr1)
                        {
                            if (tr2.GetComponent<Collider2D>() != null) { dc.AddColliders(tr2.GetComponent<Collider2D>()); }
                        }
                    }
                }
            }
        }

        if (toggleMode)
        {
            /*GameObject[] temp_on = off_targets;
            GameObject[] temp_off = on_targets;

            off_targets = new GameObject[temp_off.Length];
            on_targets = new GameObject[temp_on.Length];

            off_targets = temp_off;
            on_targets = temp_on;*/

            GameObject[] temp = off_targets;
            off_targets = on_targets;
            on_targets = temp;
        }

        finished = true;

        if(oneuse)
        {
            Destroy(gameObject);
        }

        omaewamou = false;
    }

    public bool getFinished()
    {
        return finished;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && omaewamou == false && gamemanager.getManaCount() >= activeCount)
        {
            omaewamou = true;
            StartCoroutine(Toggle());//toggle();
            gamemanager.incrementManaCount(-activeCount);

            if(manaburst != null)
            {
                manaburst.SetActive(true);
                Vector3 playerPos = collision.gameObject.transform.position;
                manaburst.transform.position = new Vector3(playerPos.x, playerPos.y, manaburst.transform.position.z);
            }

            if (sfx != null)
            {
                sfx.pitch += Random.Range(-.2f, .2f);
                sfx.PlayOneShot(sfx.clip, gamemanager.sfx_volume);
            }
            //Destroy(gameObject);
        }
    }

    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        foreach (GameObject obj in on_targets)
        {
            if (obj == null) continue;
            Vector3 triggerPos = transform.position;
            Vector3 objPos = obj.transform.position;
            float halfHeight = (triggerPos.y - objPos.y) / 2f;
            Vector3 offset = Vector3.up * halfHeight;

            Handles.DrawBezier
            (
                triggerPos,
                objPos,
                triggerPos - offset,
                objPos + offset,
                Color.green,
                EditorGUIUtility.whiteTexture,
                1f
            );
        }
        foreach (GameObject obj in off_targets)
        {
            if (obj == null) continue;
            Vector3 triggerPos = transform.position;
            Vector3 objPos = obj.transform.position;
            float halfHeight = (triggerPos.y - objPos.y) / 2f;
            Vector3 offset = Vector3.up * halfHeight;

            Handles.DrawBezier
            (
                triggerPos,
                objPos,
                triggerPos - offset,
                objPos + offset,
                Color.red,
                EditorGUIUtility.whiteTexture,
                1f
            );
        }
    }
    #endif
}
