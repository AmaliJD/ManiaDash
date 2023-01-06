using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RotateTrigger : MonoBehaviour
{
    public int activate;
    public bool oneuse;
    GameManager gamemanager;

    public GameObject group;
    public Transform target;
    public bool userigidbody;
    public float x, y, z;
    public float duration;

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

    public float delay;
    public AnimationCurve curve;

    private bool inuse;
    private Vector3 original_rotation;

    public enum Speed
    {
        x0, x1, x2, x3, x4
    }
    public Speed speed = Speed.x1;

    private void Awake()
    {
        original_rotation = group.transform.rotation.eulerAngles;
        //x /= 10; y /= 10; z /= 10;

        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        gamemanager = GameObject.FindObjectOfType<GameManager>();
    }

    public IEnumerator Move()
    {
        if (delay > 0) yield return new WaitForSeconds(delay);
        finished = false;

        float time = 0, step = (Time.deltaTime / duration) / 10, t = 0, t0 = 0, d = duration;
        float x1 = 0, y1 = 0, z1 = 0, x0 = 0, y0 = 0, z0 = 0, xPos = 0, yPos = 0, zPos = 0;

        while (time <= duration)
        {
            if (duration == 0) { break; }

            float p, s;
            float u0, u1, u2, u3;
            t = time;

            switch (easing)
            {
                case Ease.Linear:
                    x1 = x * (t / d);
                    y1 = y * (t / d);
                    z1 = z * (t / d);

                    x0 = x * (t0 / d);
                    y0 = y * (t0 / d);
                    z0 = z * (t0 / d);
                    break;

                case Ease.EaseInOut:
                    t /= d / 2;

                    if (t < 1)
                    {
                        x1 = x / 2 * t * t * t;
                        y1 = y / 2 * t * t * t;
                        z1 = z / 2 * t * t * t;
                    }
                    else
                    {
                        t -= 2;
                        x1 = x / 2 * (t * t * t + 2);
                        y1 = y / 2 * (t * t * t + 2);
                        z1 = z / 2 * (t * t * t + 2);
                    }

                    t0 /= d / 2;
                    if (t0 < 1)
                    {
                        x0 = x / 2 * t0 * t0 * t0;
                        y0 = y / 2 * t0 * t0 * t0;
                        z0 = z / 2 * t0 * t0 * t0;
                    }
                    else
                    {
                        t0 -= 2;
                        x0 = x / 2 * (t0 * t0 * t0 + 2);
                        y0 = y / 2 * (t0 * t0 * t0 + 2);
                        z0 = z / 2 * (t0 * t0 * t0 + 2);
                    }

                    break;

                case Ease.EaseIn:
                    t /= d;
                    x1 = x * t * t * t;
                    y1 = y * t * t * t;
                    z1 = z * t * t * t;

                    t0 /= d;
                    x0 = x * t0 * t0 * t0;
                    y0 = y * t0 * t0 * t0;
                    z0 = z * t0 * t0 * t0;
                    break;

                case Ease.EaseOut:
                    t /= d;
                    t--;
                    x1 = x * (t * t * t + 1);
                    y1 = y * (t * t * t + 1);
                    z1 = z * (t * t * t + 1);

                    t0 /= d;
                    t0--;
                    x0 = x * (t0 * t0 * t0 + 1);
                    y0 = y * (t0 * t0 * t0 + 1);
                    z0 = z * (t0 * t0 * t0 + 1);
                    break;

                // ELAS DOESN'T WORK
                case Ease.ElasInOut:
                    t = t / d;
                    t0 = t0 / d;
                    if (t == 1)
                    {
                        x1 = x;
                        y1 = y;
                        z1 = z;
                    }
                    else
                    {
                        p = d * .3f;
                        s = p / 4f;

                        if (t < .5f)
                        {
                            x1 = -(x * Mathf.Pow(2, 20 * t - 10) * Mathf.Sin((2 * t * d - (s + d)) * (2 * Mathf.PI) / p)) / 2;
                            y1 = -(y * Mathf.Pow(2, 20 * t - 10) * Mathf.Sin((2 * t * d - (s + d)) * (2 * Mathf.PI) / p)) / 2;
                            z1 = -(z * Mathf.Pow(2, 20 * t - 10) * Mathf.Sin((2 * t * d - (s + d)) * (2 * Mathf.PI) / p)) / 2;
                        }
                        else
                        {
                            x1 = x * (Mathf.Pow(2, -20 * t + 10) * Mathf.Sin((2 * t * d - (s + d)) * (2 * Mathf.PI) / p) / 2 + 1);
                            y1 = y * (Mathf.Pow(2, -20 * t + 10) * Mathf.Sin((2 * t * d - (s + d)) * (2 * Mathf.PI) / p) / 2 + 1);
                            z1 = z * (Mathf.Pow(2, -20 * t + 10) * Mathf.Sin((2 * t * d - (s + d)) * (2 * Mathf.PI) / p) / 2 + 1);
                        }
                    }

                    if (t0 == 1)
                    {
                        x0 = x;
                        y0 = y;
                        z0 = z;
                    }
                    else
                    {
                        p = d * .3f;
                        s = p / 4f;

                        if (t0 < .5f)
                        {
                            x0 = -(x * Mathf.Pow(2, 20 * t0 - 10) * Mathf.Sin((2 * t0 * d - (s + d)) * (2 * Mathf.PI) / p)) / 2;
                            y0 = -(y * Mathf.Pow(2, 20 * t0 - 10) * Mathf.Sin((2 * t0 * d - (s + d)) * (2 * Mathf.PI) / p)) / 2;
                            z0 = -(z * Mathf.Pow(2, 20 * t0 - 10) * Mathf.Sin((2 * t0 * d - (s + d)) * (2 * Mathf.PI) / p)) / 2;
                        }
                        else
                        {
                            x0 = x * (Mathf.Pow(2, -20 * t0 + 10) * Mathf.Sin((2 * t0 * d - (s + d)) * (2 * Mathf.PI) / p) / 2 + 1);
                            y0 = y * (Mathf.Pow(2, -20 * t0 + 10) * Mathf.Sin((2 * t0 * d - (s + d)) * (2 * Mathf.PI) / p) / 2 + 1);
                            z0 = z * (Mathf.Pow(2, -20 * t0 + 10) * Mathf.Sin((2 * t0 * d - (s + d)) * (2 * Mathf.PI) / p) / 2 + 1);
                        }
                    }

                    break;

                case Ease.ElasIn:
                    if ((t /= d) == 1)
                    {
                        x1 = x;
                        y1 = y;
                        z1 = z;
                    }
                    else
                    {
                        p = d * .3f;
                        s = p / 4f;

                        x1 = -(x * Mathf.Pow(2, 10 * t - 10) * Mathf.Sin((t * d - (s + d)) * (2 * Mathf.PI) / p));
                        y1 = -(y * Mathf.Pow(2, 10 * t - 10) * Mathf.Sin((t * d - (s + d)) * (2 * Mathf.PI) / p));
                        z1 = -(z * Mathf.Pow(2, 10 * t - 10) * Mathf.Sin((t * d - (s + d)) * (2 * Mathf.PI) / p));
                    }

                    if ((t0 /= d) == 1)
                    {
                        x0 = x;
                        y0 = y;
                        z0 = z;
                    }
                    else
                    {
                        p = d * .3f;
                        s = p / 4f;

                        x0 = -(x * Mathf.Pow(2, 10 * t0 - 10) * Mathf.Sin((t0 * d - (s + d)) * (2 * Mathf.PI) / p));
                        y0 = -(y * Mathf.Pow(2, 10 * t0 - 10) * Mathf.Sin((t0 * d - (s + d)) * (2 * Mathf.PI) / p));
                        z0 = -(z * Mathf.Pow(2, 10 * t0 - 10) * Mathf.Sin((t0 * d - (s + d)) * (2 * Mathf.PI) / p));
                    }
                    break;

                case Ease.ElasOut:
                    if ((t /= d) == 1)
                    {
                        x1 = x;
                        y1 = y;
                        z1 = z;
                    }
                    else
                    {
                        p = d * .3f;
                        s = p / 4f;

                        x1 = x * Mathf.Pow(2, -10 * t) * Mathf.Sin((t * d - s) * (2 * Mathf.PI) / p) + x;
                        y1 = y * Mathf.Pow(2, -10 * t) * Mathf.Sin((t * d - s) * (2 * Mathf.PI) / p) + y;
                        z1 = z * Mathf.Pow(2, -10 * t) * Mathf.Sin((t * d - s) * (2 * Mathf.PI) / p) + z;
                    }

                    if ((t0 /= d) == 1)
                    {
                        x0 = x;
                        y0 = y;
                        z0 = z;
                    }
                    else
                    {
                        p = d * .3f;
                        s = p / 4f;

                        x0 = x * Mathf.Pow(2, -10 * t0) * Mathf.Sin((t0 * d - s) * (2 * Mathf.PI) / p) + x;
                        y0 = y * Mathf.Pow(2, -10 * t0) * Mathf.Sin((t0 * d - s) * (2 * Mathf.PI) / p) + y;
                        z0 = z * Mathf.Pow(2, -10 * t0) * Mathf.Sin((t0 * d - s) * (2 * Mathf.PI) / p) + z;
                    }

                    break;

                case Ease.ExpoInOut:
                    u0 = 1f; u1 = 0f; u2 = 0f; u3 = 1f;

                    x1 = x * cubic_bezier(t / d, u0, u1, u2, u3);
                    x0 = x * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    y1 = y * cubic_bezier(t / d, u0, u1, u2, u3);
                    y0 = y * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    z1 = z * cubic_bezier(t / d, u0, u1, u2, u3);
                    z0 = z * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    break;

                case Ease.ExpoIn:
                    u0 = 1f; u1 = 0f; u2 = 1f; u3 = 0f;

                    x1 = x * cubic_bezier(t / d, u0, u1, u2, u3);
                    x0 = x * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    y1 = y * cubic_bezier(t / d, u0, u1, u2, u3);
                    y0 = y * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    z1 = z * cubic_bezier(t / d, u0, u1, u2, u3);
                    z0 = z * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    break;

                case Ease.ExpoOut:
                    u0 = 0f; u1 = 1f; u2 = 0f; u3 = 1f;

                    x1 = x * cubic_bezier(t / d, u0, u1, u2, u3);
                    x0 = x * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    y1 = y * cubic_bezier(t / d, u0, u1, u2, u3);
                    y0 = y * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    z1 = z * cubic_bezier(t / d, u0, u1, u2, u3);
                    z0 = z * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    break;

                case Ease.SinInOut:
                    x1 = (-x / 2) * (Mathf.Cos(Mathf.PI * (t / d)) - 1);
                    y1 = (-y / 2) * (Mathf.Cos(Mathf.PI * (t / d)) - 1);
                    z1 = (-z / 2) * (Mathf.Cos(Mathf.PI * (t / d)) - 1);
                    x0 = (-x / 2) * (Mathf.Cos(Mathf.PI * (t0 / d)) - 1);
                    y0 = (-y / 2) * (Mathf.Cos(Mathf.PI * (t0 / d)) - 1);
                    z0 = (-z / 2) * (Mathf.Cos(Mathf.PI * (t0 / d)) - 1);
                    break;

                case Ease.SinIn:
                    x1 = -x * Mathf.Cos(t / d * (Mathf.PI / 2)) + x;
                    y1 = -y * Mathf.Cos(t / d * (Mathf.PI / 2)) + y;
                    z1 = -z * Mathf.Cos(t / d * (Mathf.PI / 2)) + z;
                    x0 = -x * Mathf.Cos(t0 / d * (Mathf.PI / 2)) + x;
                    y0 = -y * Mathf.Cos(t0 / d * (Mathf.PI / 2)) + y;
                    z0 = -z * Mathf.Cos(t0 / d * (Mathf.PI / 2)) + z;
                    break;

                case Ease.SinOut:
                    x1 = x * Mathf.Sin(t / d * (Mathf.PI / 2));
                    y1 = y * Mathf.Sin(t / d * (Mathf.PI / 2));
                    z1 = z * Mathf.Sin(t / d * (Mathf.PI / 2));
                    x0 = x * Mathf.Sin(t0 / d * (Mathf.PI / 2));
                    y0 = y * Mathf.Sin(t0 / d * (Mathf.PI / 2));
                    z0 = z * Mathf.Sin(t0 / d * (Mathf.PI / 2));
                    break;

                case Ease.BackInOut:
                    u0 = .43f; u1 = -.5f; u2 = .57f; u3 = 1.5f;

                    x1 = x * cubic_bezier(t / d, u0, u1, u2, u3);
                    x0 = x * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    y1 = y * cubic_bezier(t / d, u0, u1, u2, u3);
                    y0 = y * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    z1 = z * cubic_bezier(t / d, u0, u1, u2, u3);
                    z0 = z * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    break;

                case Ease.BackIn:
                    u0 = .43f; u1 = -.5f; u2 = 1f; u3 = 1f;

                    x1 = x * cubic_bezier(t / d, u0, u1, u2, u3);
                    x0 = x * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    y1 = y * cubic_bezier(t / d, u0, u1, u2, u3);
                    y0 = y * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    z1 = z * cubic_bezier(t / d, u0, u1, u2, u3);
                    z0 = z * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    break;

                case Ease.BackOut:
                    u0 = 0f; u1 = 0f; u2 = .57f; u3 = 1.5f;

                    x1 = x * cubic_bezier(t / d, u0, u1, u2, u3);
                    x0 = x * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    y1 = y * cubic_bezier(t / d, u0, u1, u2, u3);
                    y0 = y * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    z1 = z * cubic_bezier(t / d, u0, u1, u2, u3);
                    z0 = z * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    break;

                case Ease.BounceInOut:
                    float c1 = 2.75f, c2 = 7.5625f;
                    float C = 0, C0 = 0;

                    if (t / d < .5f)
                    {
                        t = 1 - 2 * (t / d);

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
                        t = 2 * (t / d) - 1;

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


                    if (t0 / d < .5f)
                    {
                        t0 = 1 - 2 * (t0 / d);

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
                        t0 = 2 * (t0 / d) - 1;

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
                    z1 = z * C;

                    x0 = x * C0;
                    y0 = y * C0;
                    z0 = z * C0;

                    break;

                case Ease.BounceIn:
                    c1 = 2.75f; c2 = 7.5625f;
                    t = 1 - t / d;
                    t0 = 1 - t0 / d;
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
                    z1 = z * C;

                    x0 = x * C0;
                    y0 = y * C0;
                    z0 = z * C0;

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
                    z1 = z * C;

                    x0 = x * C0;
                    y0 = y * C0;
                    z0 = z * C0;

                    break;

                case Ease.Custom:
                    u0 = U0; u1 = U1; u2 = U2; u3 = U3;

                    x1 = x * cubic_bezier(t / d, u0, u1, u2, u3);
                    x0 = x * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    y1 = y * cubic_bezier(t / d, u0, u1, u2, u3);
                    y0 = y * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    z1 = z * cubic_bezier(t / d, u0, u1, u2, u3);
                    z0 = z * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    break;

                case Ease.Curve:
                    x1 = x * curve.Evaluate(t / d);
                    x0 = x * curve.Evaluate(t0 / d);

                    y1 = y * curve.Evaluate(t / d);
                    y0 = y * curve.Evaluate(t0 / d);

                    z1 = z * curve.Evaluate(t / d);
                    z0 = z * curve.Evaluate(t0 / d);

                    break;

                default:
                    break;
            }

            xPos = x1 - x0;
            yPos = y1 - y0;
            zPos = z1 - z0;

            if (!userigidbody)
            {
                //group.transform.rotation = Quaternion.Euler(new Vector3(group.transform.rotation.x + xPos, group.transform.rotation.y + yPos, group.transform.rotation.z + zPos));
                if (target == null)
                {
                    group.transform.Rotate(xPos, yPos, zPos);
                }
                else
                {
                    group.transform.RotateAround(target.position, Vector3.right, xPos);
                    group.transform.RotateAround(target.position, Vector3.up, yPos);
                    group.transform.RotateAround(target.position, Vector3.forward, zPos);
                }
            }
            else
            {
                //group.GetComponent<Rigidbody2D>().angularVelocity = Vector3.Magnitude(new Vector3(xPos / Time.fixedDeltaTime, yPos / Time.fixedDeltaTime, zPos / Time.fixedDeltaTime));
                //group.GetComponent<Rigidbody2D>().angularVelocity = zPos / Time.fixedDeltaTime;
                if (target == null)
                {
                    group.GetComponent<Rigidbody2D>().angularVelocity = zPos / Time.fixedDeltaTime;
                }
                else
                {
                    group.GetComponent<Rigidbody2D>().angularVelocity = zPos / Time.fixedDeltaTime;

                    Quaternion q = Quaternion.AngleAxis(zPos, Vector3.forward);
                    group.GetComponent<Rigidbody2D>().MovePosition(q * (group.GetComponent<Rigidbody2D>().transform.position - target.position) + target.position);
                    group.GetComponent<Rigidbody2D>().MoveRotation(group.transform.rotation * q);

                    //group.GetComponent<Rigidbody2D>().velocity = (zPos / Time.fixedDeltaTime * Vector2.Distance(group.transform.position, target.position)) * Vector2.Perpendicular(group.GetComponent<Rigidbody2D>().position - (Vector2)target.position).normalized;
                }
            }
            t0 = time;
            time += Time.deltaTime;

            if (!userigidbody) { yield return null; }
            else { yield return new WaitForFixedUpdate(); }
        }

        xPos = x - x1;
        yPos = y - y1;
        zPos = z - z1;

        if (!userigidbody)
        {
            //group.transform.Rotate(xPos, yPos, zPos);
            if (target == null)
            {
                group.transform.Rotate(xPos, yPos, zPos);
            }
            else
            {
                group.transform.RotateAround(target.position, Vector3.right, xPos);
                group.transform.RotateAround(target.position, Vector3.up, yPos);
                group.transform.RotateAround(target.position, Vector3.forward, zPos);
            }
        }
        else
        {
            //group.GetComponent<Rigidbody2D>().angularVelocity = Vector3.Magnitude(new Vector3(xPos / Time.fixedDeltaTime, yPos / Time.fixedDeltaTime, zPos / Time.fixedDeltaTime));
            //group.GetComponent<Rigidbody2D>().MoveRotation(zPos);


            //group.GetComponent<Rigidbody2D>().angularVelocity = zPos / Time.fixedDeltaTime;
            if (target == null)
            {
                group.GetComponent<Rigidbody2D>().angularVelocity = zPos / Time.fixedDeltaTime;
            }
            else
            {
                group.GetComponent<Rigidbody2D>().angularVelocity = zPos / Time.fixedDeltaTime;
                Quaternion q = Quaternion.AngleAxis(zPos, Vector3.forward);
                group.GetComponent<Rigidbody2D>().MovePosition(q * (group.GetComponent<Rigidbody2D>().transform.position - target.position) + target.position);
                group.GetComponent<Rigidbody2D>().MoveRotation(group.transform.rotation * q);
                //group.GetComponent<Rigidbody2D>().velocity = (zPos / Time.fixedDeltaTime * Vector2.Distance(group.transform.position, target.position)) * Vector2.Perpendicular(group.GetComponent<Rigidbody2D>().position - (Vector2)target.position).normalized;
            }
            yield return new WaitForFixedUpdate();
            //group.GetComponent<Rigidbody2D>().angularVelocity = 0;
            if (target == null)
            {
                group.GetComponent<Rigidbody2D>().angularVelocity = 0;
            }
            else
            {
                group.GetComponent<Rigidbody2D>().angularVelocity = 0;
                group.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            }
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

        
        /*t = p.x;

        p = (p0 * Mathf.Pow((1 - t), 3)) +
            (3 * p1 * Mathf.Pow((1 - t), 2) * t) +
            (3 * p2 * Mathf.Pow(t, 2) * (1 - t)) +
            (p3 * Mathf.Pow(t, 3));*/

        return p.y;
    }

    public void Reset()
    {
        group.transform.rotation = Quaternion.Euler(original_rotation);
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
        if (collision.gameObject.tag == "Player" && !inuse && !collision.isTrigger && gamemanager.getManaCount() >= activate)
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
