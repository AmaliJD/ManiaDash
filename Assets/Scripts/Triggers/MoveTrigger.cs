using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MoveTrigger : MonoBehaviour
{
    public int activate;
    public bool oneuse;
    GameManager gamemanager;

    public GameObject group;
    public GameObject target;
    public GameObject follow;
    public bool spaceSelf;
    public bool userigidbody;
    public bool xOnly, yOnly;
    public bool updateTargetAngle;
    public float towards;
    public float followMultiplier;
    public float x, y;
    private float initX, initY;
    public float duration;
    public bool disableOnTrigger;

    private Rigidbody2D group_body;

    private bool finished = true;
    public enum Ease
    {
        Linear, 
        EaseInOut, EaseIn, EaseOut,
        ElasInOut, ElasIn, ElasOut,
        ExpoInOut, ExpoIn, ExpoOut,
        SinInOut, SinIn, SinOut,
        BackInOut, BackIn, BackOut,
        BounceInOut, BounceIn, BounceOut,
        Custom, Curve
    };
    public Ease easing;

    public float U0, U1, U2, U3;

    private bool inuse;
    private Vector3 original_position;

    public float delay;
    public AnimationCurve curve;

    public enum Speed
    {
        x0, x1, x2, x3, x4
    }
    public Speed speed = Speed.x1;

    public MoveTrigger(GameObject g)
    {
        group = g;
    }

    private void Awake()
    {
        if (group != null)
        {
            //original_position = group.transform.position;
            if (!(spaceSelf && !follow))
            {
                original_position = group.transform.position;
            }
            else
            {
                original_position = group.transform.localPosition;
            }
            gameObject.transform.GetChild(0).gameObject.SetActive(false);
        }
        else
        {
            original_position = gameObject.transform.position;
        }

        /*if (userigidbody)
        {
            x -= .1f;
            y -= .1f;
        }*/
        x /= 10; y /= 10;
        initX = x; initY = y;
        if (target != null)
        {
            towards /= 10;
        }

        gamemanager = GameObject.FindObjectOfType<GameManager>();
    }

    public void Initialize()
    {
        original_position = group.transform.position;
        x /= 10; y /= 10;
    }

    public IEnumerator Follow()
    {
        float time = 0;
        Vector3 followPos = follow.transform.position;
        yield return null;

        while (time <= duration)
        {
            Vector3 deltaPos = follow.transform.position - followPos;
            deltaPos = new Vector3(yOnly ? 0 : deltaPos.x, xOnly ? 0 : deltaPos.y, deltaPos.z) * followMultiplier;
            group.transform.Translate(deltaPos, !spaceSelf ? Space.World : Space.Self);

            followPos = follow.transform.position;
            time += Time.deltaTime;
            yield return null;
        }
    }

    public IEnumerator Move()
    {
        /*float delaytimer = 0;
        while(delaytimer < delay)
        {
            delaytimer += Time.deltaTime;
            yield return null;
        }*/
        group_body = userigidbody ? group.GetComponent<Rigidbody2D>() : null;
        if (delay > 0) yield return new WaitForSeconds(delay);

        finished = false;

        if (target != null)
        {
            float initDistanceToTarget = Vector3.Distance(target.transform.position, group.transform.position);
            towards = towards == 0 ? initDistanceToTarget : towards;
        }

        if (follow != null)
        {
            StartCoroutine(Follow());
            yield break;
        }
        if (target != null && !updateTargetAngle)
        {
            Vector3 direction = Vector3.Normalize(target.transform.position - group.transform.position) * towards;
            x = initX + direction.x * (yOnly ? 0 : 1);
            y = initY + direction.y * (xOnly ? 0 : 1);
        }

        float time = 0, step = (Time.deltaTime / duration) / 10, t = 0, t0 = 0, d = duration;
        float x1 = 0, y1 = 0, x0 = 0, y0 = 0, xPos = 0, yPos = 0, prevXPos = 0, prevYPos = 0;

        while (time <= duration)
        {
            if(duration == 0) { break; }
            if (target != null && updateTargetAngle)
            {
                Vector3 direction = Vector3.Normalize(target.transform.position - group.transform.position) * towards;
                x = initX + direction.x * (yOnly ? 0 : 1);
                y = initY + direction.y * (xOnly ? 0 : 1);
            }

            float p, s;
            float u0, u1, u2, u3;
            t = time;

            switch (easing)
            {
                case Ease.Linear:
                    x1 = x * (t / d);
                    y1 = y * (t / d);
                    x0 = x * (t0 / d);
                    y0 = y * (t0 / d);
                    break;

                case Ease.EaseInOut:
                    t /= d / 2;

                    if (t < 1)
                    {
                        x1 = x / 2 * t * t * t;
                        y1 = y / 2 * t * t * t;
                    }
                    else
                    {
                        t -= 2;
                        x1 = x / 2 * (t * t * t + 2);
                        y1 = y / 2 * (t * t * t + 2);
                    }

                    t0 /= d / 2;
                    if (t0 < 1)
                    {
                        x0 = x / 2 * t0 * t0 * t0;
                        y0 = y / 2 * t0 * t0 * t0;
                    }
                    else
                    {
                        t0 -= 2;
                        x0 = x / 2 * (t0 * t0 * t0 + 2);
                        y0 = y / 2 * (t0 * t0 * t0 + 2);
                    }

                    break;

                case Ease.EaseIn:
                    t /= d;
                    x1 = x * t * t * t;
                    y1 = y * t * t * t;

                    t0 /= d;
                    x0 = x * t0 * t0 * t0;
                    y0 = y * t0 * t0 * t0;
                    break;

                case Ease.EaseOut:
                    t /= d;
                    t--;
                    x1 = x * (t * t * t + 1);
                    y1 = y * (t * t * t + 1);

                    t0 /= d;
                    t0--;
                    x0 = x * (t0 * t0 * t0 + 1);
                    y0 = y * (t0 * t0 * t0 + 1);
                    break;
                // ELAS DOESN'T WORK
                case Ease.ElasInOut:
                    t = t / d;
                    t0 = t0 / d;
                    if (t == 1)
                    {
                        x1 = x;
                        y1 = y;
                    }
                    else
                    {
                        p = d * .3f;
                        s = p / 4f;

                        if (t < .5f)
                        {
                            x1 = -(x * Mathf.Pow(2, 20 * t - 10) * Mathf.Sin((2 * t * d - (s + d)) * (2 * Mathf.PI) / p)) / 2;
                            y1 = -(y * Mathf.Pow(2, 20 * t - 10) * Mathf.Sin((2 * t * d - (s + d)) * (2 * Mathf.PI) / p)) / 2;
                        }
                        else
                        {
                            x1 = x * (Mathf.Pow(2, -20 * t + 10) * Mathf.Sin((2 * t * d - (s + d)) * (2 * Mathf.PI) / p) / 2 + 1);
                            y1 = y * (Mathf.Pow(2, -20 * t + 10) * Mathf.Sin((2 * t * d - (s + d)) * (2 * Mathf.PI) / p) / 2 + 1);
                        }
                    }

                    if (t0 == 1)
                    {
                        x0 = x;
                        y0 = y;
                    }
                    else
                    {
                        p = d * .3f;
                        s = p / 4f;

                        if (t0 < .5f)
                        {
                            x0 = -(x * Mathf.Pow(2, 20 * t0 - 10) * Mathf.Sin((2 * t0 * d - (s + d)) * (2 * Mathf.PI) / p)) / 2;
                            y0 = -(y * Mathf.Pow(2, 20 * t0 - 10) * Mathf.Sin((2 * t0 * d - (s + d)) * (2 * Mathf.PI) / p)) / 2;
                        }
                        else
                        {
                            x0 = x * (Mathf.Pow(2, -20 * t0 + 10) * Mathf.Sin((2 * t0 * d - (s + d)) * (2 * Mathf.PI) / p) / 2 + 1);
                            y0 = y * (Mathf.Pow(2, -20 * t0 + 10) * Mathf.Sin((2 * t0 * d - (s + d)) * (2 * Mathf.PI) / p) / 2 + 1);
                        }
                    }

                    break;

                case Ease.ElasIn:
                    if ((t /= d) == 1)
                    {
                        x1 = x;
                        y1 = y;
                    }
                    else
                    {
                        p = d * .3f;
                        s = p / 4f;

                        x1 = -(x * Mathf.Pow(2, 10 * t - 10) * Mathf.Sin((t * d - (s + d)) * (2 * Mathf.PI) / p));
                        y1 = -(y * Mathf.Pow(2, 10 * t - 10) * Mathf.Sin((t * d - (s + d)) * (2 * Mathf.PI) / p));
                    }

                    if ((t0 /= d) == 1)
                    {
                        x0 = x;
                        y0 = y;
                    }
                    else
                    {
                        p = d * .3f;
                        s = p / 4f;

                        x0 = -(x * Mathf.Pow(2, 10 * t0 - 10) * Mathf.Sin((t0 * d - (s + d)) * (2 * Mathf.PI) / p));
                        y0 = -(y * Mathf.Pow(2, 10 * t0 - 10) * Mathf.Sin((t0 * d - (s + d)) * (2 * Mathf.PI) / p));
                    }
                    break;

                case Ease.ElasOut:
                    if ((t /= d) == 1)
                    {
                        x1 = x;
                        y1 = y;
                    }
                    else
                    {
                        p = d * .3f;
                        s = p / 4f;

                        x1 = x * Mathf.Pow(2, -10 * t) * Mathf.Sin((t * d - s) * (2 * Mathf.PI) / p) + x;
                        y1 = y * Mathf.Pow(2, -10 * t) * Mathf.Sin((t * d - s) * (2 * Mathf.PI) / p) + y;
                    }

                    if ((t0 /= d) == 1)
                    {
                        x0 = x;
                        y0 = y;
                    }
                    else
                    {
                        p = d * .3f;
                        s = p / 4f;

                        x0 = x * Mathf.Pow(2, -10 * t0) * Mathf.Sin((t0 * d - s) * (2 * Mathf.PI) / p) + x;
                        y0 = y * Mathf.Pow(2, -10 * t0) * Mathf.Sin((t0 * d - s) * (2 * Mathf.PI) / p) + y;
                    }

                    break;
                
                case Ease.ExpoInOut:
                    u0 = 1f; u1 = 0f; u2 = 0f; u3 = 1f;

                    x1 = x * cubic_bezier(t / d, u0, u1, u2, u3);
                    x0 = x * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    y1 = y * cubic_bezier(t / d, u0, u1, u2, u3);
                    y0 = y * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    break;

                case Ease.ExpoIn:
                    u0 = 1f; u1 = 0f; u2 = 1f; u3 = 0f;

                    x1 = x * cubic_bezier(t / d, u0, u1, u2, u3);
                    x0 = x * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    y1 = y * cubic_bezier(t / d, u0, u1, u2, u3);
                    y0 = y * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    break;

                case Ease.ExpoOut:
                    u0 = 0f; u1 = 1f; u2 = 0f; u3 = 1f;

                    x1 = x * cubic_bezier(t / d, u0, u1, u2, u3);
                    x0 = x * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    y1 = y * cubic_bezier(t / d, u0, u1, u2, u3);
                    y0 = y * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    break;

                case Ease.SinInOut:
                    x1 = (-x / 2) * (Mathf.Cos(Mathf.PI * (t/d)) -1);
                    y1 = (-y / 2) * (Mathf.Cos(Mathf.PI * (t / d)) - 1);
                    x0 = (-x / 2) * (Mathf.Cos(Mathf.PI * (t0 / d)) - 1);
                    y0 = (-y / 2) * (Mathf.Cos(Mathf.PI * (t0 / d)) - 1);
                    break;

                case Ease.SinIn:
                    x1 = -x * Mathf.Cos(t/d * (Mathf.PI/2)) + x;
                    y1 = -y * Mathf.Cos(t / d * (Mathf.PI / 2)) + y;
                    x0 = -x * Mathf.Cos(t0 / d * (Mathf.PI / 2)) + x;
                    y0 = -y * Mathf.Cos(t0 / d * (Mathf.PI / 2)) + y;
                    break;

                case Ease.SinOut:
                    x1 = x * Mathf.Sin(t / d * (Mathf.PI / 2));
                    y1 = y * Mathf.Sin(t / d * (Mathf.PI / 2));
                    x0 = x * Mathf.Sin(t0 / d * (Mathf.PI / 2));
                    y0 = y * Mathf.Sin(t0 / d * (Mathf.PI / 2));
                    break;

                case Ease.BackInOut:
                    u0 = .43f; u1 = -.5f; u2 = .57f; u3 = 1.5f;

                    x1 = x * cubic_bezier(t / d, u0, u1, u2, u3);
                    x0 = x * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    y1 = y * cubic_bezier(t / d, u0, u1, u2, u3);
                    y0 = y * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    break;

                case Ease.BackIn:
                    u0 = .43f; u1 = -.5f; u2 = 1f; u3 = 1f;

                    x1 = x * cubic_bezier(t / d, u0, u1, u2, u3);
                    x0 = x * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    y1 = y * cubic_bezier(t / d, u0, u1, u2, u3);
                    y0 = y * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    break;

                case Ease.BackOut:
                    u0 = 0f; u1 = 0f; u2 = .57f; u3 = 1.5f;

                    x1 = x * cubic_bezier(t / d, u0, u1, u2, u3);
                    x0 = x * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    y1 = y * cubic_bezier(t / d, u0, u1, u2, u3);
                    y0 = y * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    break;

                case Ease.BounceInOut:
                    float c1 = 2.75f, c2 = 7.5625f;
                    float C = 0, C0 = 0;

                    if (t/d < .5f)
                    {
                        t = 1 - 2 * (t/d);

                        if (t < 1f / c1)
                        {
                            C = (1 - (c2 * t * t)) / 2;
                        }
                        else if (t < 2f / c1)
                        {
                            C = (1 - (c2 * (t - 1.5f / c1) * (t - 1.5f / c1) + .75f)) / 2;
                        }
                        else if (t < 2.5f / c1)
                        {
                            C = (1 - (c2 * (t - 2.25f / c1) * (t - 2.25f / c1) + .9375f)) / 2;
                        }
                        else
                        {
                            C = (1 - (c2 * (t - 2.625f / c1) * (t - 2.625f / c1) + .984375f)) / 2;
                        }
                    }
                    else
                    {
                        t = 2 * (t/d) - 1;

                        if (t < 1f / c1)
                        {
                            C = (1 + (c2 * t * t)) / 2;
                        }
                        else if (t < 2f / c1)
                        {
                            C = (1 + (c2 * (t - 1.5f / c1) * (t - 1.5f / c1) + .75f)) / 2;
                        }
                        else if (t < 2.5f / c1)
                        {
                            C = (1 + (c2 * (t - 2.25f / c1) * (t - 2.25f / c1) + .9375f)) / 2;
                        }
                        else
                        {
                            C = (1 + (c2 * (t - 2.625f / c1) * (t - 2.625f / c1) + .984375f)) / 2;
                        }
                    }


                    if (t0/d < .5f)
                    {
                        t0 = 1 - 2 * (t0/d);

                        if (t0 < 1f / c1)
                        {
                            C0 = (1 - (c2 * t0 * t0)) / 2;
                        }
                        else if (t0 < 2f / c1)
                        {
                            C0 = (1 - (c2 * (t0 - 1.5f / c1) * (t0 - 1.5f / c1) + .75f)) / 2;
                        }
                        else if (t0 < 2.5f / c1)
                        {
                            C0 = (1 - (c2 * (t0 - 2.25f / c1) * (t0 - 2.25f / c1) + .9375f)) / 2;
                        }
                        else
                        {
                            C0 = (1 - (c2 * (t0 - 2.625f / c1) * (t0 - 2.625f / c1) + .984375f)) / 2;
                        }
                    }
                    else
                    {
                        t0 = 2 * (t0/d) - 1;

                        if (t0 < 1f / c1)
                        {
                            C0 = (1 + (c2 * t0 * t0)) / 2;
                        }
                        else if (t0 < 2f / c1)
                        {
                            C0 = (1 + (c2 * (t0 - 1.5f / c1) * (t0 - 1.5f / c1) + .75f)) / 2;
                        }
                        else if (t0 < 2.5f / c1)
                        {
                            C0 = (1 + (c2 * (t0 - 2.25f / c1) * (t0 - 2.25f / c1) + .9375f)) / 2;
                        }
                        else
                        {
                            C0 = (1 + (c2 * (t0 - 2.625f / c1) * (t0 - 2.625f / c1) + .984375f)) / 2;
                        }
                    }

                    x1 = x * C;
                    y1 = y * C;

                    x0 = x * C0;
                    y0 = y * C0;

                    break;

                case Ease.BounceIn:
                    c1 = 2.75f; c2 = 7.5625f;
                    t = 1 - t/d;
                    t0 = 1 - t0/d;
                    C = 0; C0 = 0;
                    if (t < 1f / c1)
                    {
                        C = (1 - (c2 * t * t));
                    }
                    else if (t < 2f / c1)
                    {
                        C = (1 - (c2 * (t - 1.5f / c1) * (t - 1.5f / c1) + .75f));
                    }
                    else if (t < 2.5f / c1)
                    {
                        C = (1 - (c2 * (t - 2.25f / c1) * (t - 2.25f / c1) + .9375f));
                    }
                    else
                    {
                        C = (1 - (c2 * (t - 2.625f / c1) * (t - 2.625f / c1) + .984375f));
                    }

                    if (t0 < 1f / c1)
                    {
                        C0 = (1 - (c2 * t0 * t0));
                    }
                    else if (t0 < 2f / c1)
                    {
                        C0 = (1 - (c2 * (t0 - 1.5f / c1) * (t0 - 1.5f / c1) + .75f));
                    }
                    else if (t0 < 2.5f / c1)
                    {
                        C0 = (1 - (c2 * (t0 - 2.25f / c1) * (t0 - 2.25f / c1) + .9375f));
                    }
                    else
                    {
                        C0 = (1 - (c2 * (t0 - 2.625f / c1) * (t0 - 2.625f / c1) + .984375f));
                    }

                    x1 = x * C;
                    y1 = y * C;

                    x0 = x * C0;
                    y0 = y * C0;

                    break;

                case Ease.BounceOut:
                    c1 = 2.75f; c2 = 7.5625f;
                    t = t / d;
                    t0 = t0 / d;
                    C = 0; C0 = 0;
                    if (t < 1f / c1)
                    {
                        C = (c2 * t * t);
                    }
                    else if (t < 2f / c1)
                    {
                        C = (c2 * (t - 1.5f / c1) * (t - 1.5f / c1) + .75f);
                    }
                    else if (t < 2.5f / c1)
                    {
                        C = (c2 * (t - 2.25f / c1) * (t - 2.25f / c1) + .9375f);
                    }
                    else
                    {
                        C = (c2 * (t - 2.625f / c1) * (t - 2.625f / c1) + .984375f);
                    }

                    if (t0 < 1f / c1)
                    {
                        C0 = (c2 * t0 * t0);
                    }
                    else if (t0 < 2f / c1)
                    {
                        C0 = (c2 * (t0 - 1.5f / c1) * (t0 - 1.5f / c1) + .75f);
                    }
                    else if (t0 < 2.5f / c1)
                    {
                        C0 = (c2 * (t0 - 2.25f / c1) * (t0 - 2.25f / c1) + .9375f);
                    }
                    else
                    {
                        C0 = (c2 * (t0 - 2.625f / c1) * (t0 - 2.625f / c1) + .984375f);
                    }

                    x1 = x * C;
                    y1 = y * C;

                    x0 = x * C0;
                    y0 = y * C0;

                    break;

                case Ease.Custom:
                    u0 = U0; u1 = U1; u2 = U2; u3 = U3;

                    x1 = x * cubic_bezier(t / d, u0, u1, u2, u3);
                    x0 = x * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    y1 = y * cubic_bezier(t / d, u0, u1, u2, u3);
                    y0 = y * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    break;

                case Ease.Curve:
                    x1 = x * curve.Evaluate(t / d);
                    x0 = x * curve.Evaluate(t0 / d);

                    y1 = y * curve.Evaluate(t / d);
                    y0 = y * curve.Evaluate(t0 / d);

                    break;

                default:
                    break; 
            }

            prevXPos = xPos;
            prevYPos = yPos;
            xPos = x1 - x0;
            yPos = y1 - y0;

            if (!userigidbody)
            {
                if(!spaceSelf)
                {
                    group.transform.position = new Vector3(group.transform.position.x + xPos, group.transform.position.y + yPos, group.transform.position.z);
                }
                else
                {
                    group.transform.localPosition = new Vector3(group.transform.localPosition.x + xPos, group.transform.localPosition.y + yPos, group.transform.localPosition.z);
                }
            }
            else {
                //group.GetComponent<Rigidbody2D>().transform.Translate(new Vector3(xPos, yPos, 0));// = new Vector2(group.GetComponent<Rigidbody2D>().position.x + , group.GetComponent<Rigidbody2D>().position.y + yPos);
                Vector2 deltaVelocity = new Vector2(xPos / Time.fixedDeltaTime, yPos / Time.fixedDeltaTime) - new Vector2(prevXPos / Time.fixedDeltaTime, prevYPos / Time.fixedDeltaTime);
                group_body.velocity += deltaVelocity;
                //group.GetComponent<Rigidbody2D>().velocity = new Vector2(xPos / Time.fixedDeltaTime, yPos / Time.fixedDeltaTime);
                //Debug.Log(this.GetHashCode() + "     " + group_body.velocity);
            }
            t0 = time;
            time += Time.deltaTime;

            if (!userigidbody) { yield return null; }
            else { yield return new WaitForFixedUpdate(); }
        }

        if (target != null && updateTargetAngle)
        {
            Vector3 direction = Vector3.Normalize(target.transform.position - group.transform.position) * towards;
            x = initX + direction.x * (yOnly ? 0 : 1);
            y = initY + direction.y * (xOnly ? 0 : 1);

            Vector3 xy1 = Vector3.Normalize(target.transform.position - group.transform.position) * Vector3.Magnitude(new Vector2(x1, y1));
            int rev = (towards > 0) ? 1 : -1;
            x1 = xy1.x * rev;
            y1 = xy1.y * rev;

            //Debug.Log(x + "  " + x1);
        }

        prevXPos = xPos;
        prevYPos = yPos;
        xPos = x - x1;
        yPos = y - y1;

        if (!userigidbody)
        {
            if (!spaceSelf)
            {
                group.transform.position = new Vector3(group.transform.position.x + xPos, group.transform.position.y + yPos, group.transform.position.z);
            }
            else
            {
                group.transform.localPosition = new Vector3(group.transform.localPosition.x + xPos, group.transform.localPosition.y + yPos, group.transform.localPosition.z);
            }
        }
        else {
            //group.GetComponent<Rigidbody2D>().position = new Vector2(group.GetComponent<Rigidbody2D>().position.x + xPos, group.GetComponent<Rigidbody2D>().position.y + yPos);
            //group.GetComponent<Rigidbody2D>().velocity = new Vector2(xPos / Time.fixedDeltaTime, yPos / Time.fixedDeltaTime);
            Vector2 deltaVelocity = new Vector2(xPos / Time.fixedDeltaTime, yPos / Time.fixedDeltaTime) - new Vector2(prevXPos / Time.fixedDeltaTime, prevYPos / Time.fixedDeltaTime); ;
            group_body.velocity += deltaVelocity;
            yield return new WaitForFixedUpdate();
            deltaVelocity = Vector2.zero - new Vector2(xPos / Time.fixedDeltaTime, yPos / Time.fixedDeltaTime);
            group_body.velocity += deltaVelocity;
            //group.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }

        finished = true;
        inuse = oneuse;
        //if (oneuse) { inuse = true; }
    }

    float cubic_bezier(float t, float u0, float u1, float u2, float u3)
    {
        Vector2 p0 = Vector2.zero;
        Vector2 p3 = Vector2.one;

        Vector2 p1 = new Vector2(u0, u1);
        Vector2 p2 = new Vector2(u2, u3);

        Vector2 p;

        p = (p0 * Mathf.Pow((1 - t), 3)) +
            (3 * p1 * Mathf.Pow((1 - t), 2) * t) +
            (3 * p2 * Mathf.Pow(t, 2) * (1 - t)) +
            (p3 * Mathf.Pow(t, 3));

        return p.y;
    }

    public void Reset()
    {
        if(!(spaceSelf && !follow))
        {
            group.transform.position = original_position;
        }
        else
        {
            group.transform.localPosition = original_position;
        }
        
        finished = true;
        inuse = false;
    }

    public void ResetTrigger()
    {
        StopAllCoroutines();
        finished = true;
        inuse = false;
    }

    public float getDuration()
    {
        return duration;
    }

    public bool getFinished()
    {
        return finished;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (disableOnTrigger) { return; }
        if (collision.gameObject.tag == "Player" && !inuse && gamemanager.getManaCount() >= activate && !collision.isTrigger)
        {
            inuse = true;
            StartCoroutine(Move());
        }
    }

    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        float scale = 0;
        switch (speed)
        {
            case Speed.x0:
                scale = 40f; break;
            case Speed.x1:
                scale = 55f; break;
            case Speed.x2:
                scale = 75f; break;
            case Speed.x3:
                scale = 90f; break;
            case Speed.x4:
                scale = 110f; break;
        }

        Gizmos.DrawLine(transform.position, transform.position + new Vector3((scale * Time.fixedDeltaTime * 10f) * duration, 0, 0));

        if (group == null) { return; }

        Vector3 triggerPos = transform.position;
        Vector3 objPos = group.transform.position;
        float halfHeight = (triggerPos.y - objPos.y) / 2f;
        Vector3 offset = Vector3.up * halfHeight;

        Handles.DrawBezier
        (
            triggerPos,
            objPos,
            triggerPos - offset,
            objPos + offset,
            Color.white,
            EditorGUIUtility.whiteTexture,
            1f
        );
    }
#endif
}
