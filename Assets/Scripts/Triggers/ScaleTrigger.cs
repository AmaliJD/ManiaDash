using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ScaleTrigger : MonoBehaviour
{
    public int activate;
    public bool oneuse;
    GameManager gamemanager;

    public GameObject group;
    public enum scaleState
    {
        multiply, add, set
    }

    public scaleState state = scaleState.multiply;
    public bool ignoreParticles, disableParticles, dontScaleParticles, zeroCollider;
    private List<ParticleSystem> particles;
    private List<bool> particlesState;
    public bool zero;
    public float x, y, full;
    private float finalX, finalY, deltaX, deltaY;
    public float duration;

    private ScaleTracker tracker;

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
    public bool disable;
    public AnimationCurve curve;

    private bool inuse;
    private Vector3 original_scale;

    public enum Speed
    {
        x0, x1, x2, x3, x4
    }
    public Speed speed = Speed.x1;

    private void Awake()
    {
        tracker = transform.parent.gameObject.GetComponent<ScaleTracker>();
        if(tracker == null)
        {
            tracker = GameObject.FindGameObjectWithTag("Master").GetComponent<ScaleTracker>();
        }

        particles = new List<ParticleSystem>();
        particlesState = new List<bool>();
        original_scale = group.transform.localScale;

        x += full;
        y += full;

        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        gamemanager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        if (ignoreParticles)
            return;

        if (group.transform.GetComponent<ParticleSystem>())
        {
            particles.Add(GetComponent<ParticleSystem>());
            particlesState.Add(GetComponent<ParticleSystem>().enableEmission);
        }
            

        foreach(Transform t in group.transform)
        {
            if (t.gameObject.GetComponent<ParticleSystem>())
            {
                particles.Add(t.gameObject.GetComponent<ParticleSystem>());
                particlesState.Add(t.gameObject.GetComponent<ParticleSystem>().enableEmission);
            }

            foreach (Transform c in t)
            {
                if (c.gameObject.GetComponent<ParticleSystem>())
                {
                    particles.Add(c.gameObject.GetComponent<ParticleSystem>());
                    particlesState.Add(c.gameObject.GetComponent<ParticleSystem>().enableEmission);
                }
            }
        }
    }

    public IEnumerator Move()
    {
        if (group == null) yield break;
        if (delay > 0) yield return new WaitForSeconds(delay);
        finished = false;

        Vector3 baseScale = tracker.getBaseScale(group.GetHashCode(), group.transform.localScale);
        //Debug.Log("Base: " + baseScale);

        /*finalX = multiply ? group.transform.localScale.x * x : group.transform.localScale.x + x;
        finalY = multiply ? group.transform.localScale.y * y : group.transform.localScale.y + y;
        deltaX = finalX - group.transform.localScale.x;
        deltaY = finalY - group.transform.localScale.y;*/
        if (!disable)
        {
            group.SetActive(true);
        }

        switch (state)
        {
            case scaleState.multiply:
                finalX = baseScale.x * x;
                finalY = baseScale.y * y;
                break;

            case scaleState.add:
                finalX = baseScale.x + x;
                finalY = baseScale.y + y;
                break;

            case scaleState.set:
                finalX = x;
                finalY = y;
                break;
        }
        deltaX = finalX - baseScale.x;
        deltaY = finalY - baseScale.y;

        if (x == 0 && !zero) deltaX = 0;
        if (y == 0 && !zero) deltaY = 0;

        if(zeroCollider && group.GetComponent<Collider2D>() != null) { group.GetComponent<Collider2D>().enabled = !(x == 0 || y == 0); }

        Vector3 newScale = new Vector3(baseScale.x + deltaX, baseScale.y + deltaY, group.transform.localScale.z);
        tracker.setNewScale(group.GetHashCode(), newScale);
        //Debug.Log("Scale to: " + new Vector3(baseScale.x + deltaX, baseScale.y + deltaY, group.transform.localScale.z));d

        bool xOppositeSign = (baseScale.x >= 0 && newScale.x < 0) || (baseScale.x < 0 && newScale.x >= 0);
        bool yOppositeSign = (baseScale.y >= 0 && newScale.y < 0) || (baseScale.y < 0 && newScale.y >= 0);

        float time = 0, step = (Time.deltaTime / duration) / 10, t = 0, t0 = 0, d = duration;
        float x1 = 0, y1 = 0, x0 = 0, y0 = 0, xPos = 0, yPos = 0;

        while (time <= duration)
        {
            if (duration == 0) { break; }

            float p, s;
            float u0, u1, u2, u3;
            t = time;

            switch (easing)
            {
                case Ease.Linear:
                    x1 = deltaX * (t / d);
                    y1 = deltaY * (t / d);

                    x0 = deltaX * (t0 / d);
                    y0 = deltaY * (t0 / d);
                    break;

                case Ease.EaseInOut:
                    t /= d / 2;

                    if (t < 1)
                    {
                        x1 = deltaX / 2 * t * t * t;
                        y1 = deltaY / 2 * t * t * t;
                    }
                    else
                    {
                        t -= 2;
                        x1 = deltaX / 2 * (t * t * t + 2);
                        y1 = deltaY / 2 * (t * t * t + 2);
                    }

                    t0 /= d / 2;
                    if (t0 < 1)
                    {
                        x0 = deltaX / 2 * t0 * t0 * t0;
                        y0 = deltaY / 2 * t0 * t0 * t0;
                    }
                    else
                    {
                        t0 -= 2;
                        x0 = deltaX / 2 * (t0 * t0 * t0 + 2);
                        y0 = deltaY / 2 * (t0 * t0 * t0 + 2);
                    }

                    break;

                case Ease.EaseIn:
                    t /= d;
                    x1 = deltaX * t * t * t;
                    y1 = deltaY * t * t * t;

                    t0 /= d;
                    x0 = deltaX * t0 * t0 * t0;
                    y0 = deltaY * t0 * t0 * t0;
                    break;

                case Ease.EaseOut:
                    t /= d;
                    t--;
                    x1 = deltaX * (t * t * t + 1);
                    y1 = deltaY * (t * t * t + 1);

                    t0 /= d;
                    t0--;
                    x0 = deltaX * (t0 * t0 * t0 + 1);
                    y0 = deltaY * (t0 * t0 * t0 + 1);
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
                            x1 = -(deltaX * Mathf.Pow(2, 20 * t - 10) * Mathf.Sin((2 * t * d - (s + d)) * (2 * Mathf.PI) / p)) / 2;
                            y1 = -(deltaY * Mathf.Pow(2, 20 * t - 10) * Mathf.Sin((2 * t * d - (s + d)) * (2 * Mathf.PI) / p)) / 2;
                        }
                        else
                        {
                            x1 = deltaX * (Mathf.Pow(2, -20 * t + 10) * Mathf.Sin((2 * t * d - (s + d)) * (2 * Mathf.PI) / p) / 2 + 1);
                            y1 = deltaY * (Mathf.Pow(2, -20 * t + 10) * Mathf.Sin((2 * t * d - (s + d)) * (2 * Mathf.PI) / p) / 2 + 1);
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
                            x0 = -(deltaX * Mathf.Pow(2, 20 * t0 - 10) * Mathf.Sin((2 * t0 * d - (s + d)) * (2 * Mathf.PI) / p)) / 2;
                            y0 = -(deltaY * Mathf.Pow(2, 20 * t0 - 10) * Mathf.Sin((2 * t0 * d - (s + d)) * (2 * Mathf.PI) / p)) / 2;
                        }
                        else
                        {
                            x0 = deltaX * (Mathf.Pow(2, -20 * t0 + 10) * Mathf.Sin((2 * t0 * d - (s + d)) * (2 * Mathf.PI) / p) / 2 + 1);
                            y0 = deltaY * (Mathf.Pow(2, -20 * t0 + 10) * Mathf.Sin((2 * t0 * d - (s + d)) * (2 * Mathf.PI) / p) / 2 + 1);
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

                        x1 = -(deltaX * Mathf.Pow(2, 10 * t - 10) * Mathf.Sin((t * d - (s + d)) * (2 * Mathf.PI) / p));
                        y1 = -(deltaY * Mathf.Pow(2, 10 * t - 10) * Mathf.Sin((t * d - (s + d)) * (2 * Mathf.PI) / p));
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

                        x0 = -(deltaX * Mathf.Pow(2, 10 * t0 - 10) * Mathf.Sin((t0 * d - (s + d)) * (2 * Mathf.PI) / p));
                        y0 = -(deltaY * Mathf.Pow(2, 10 * t0 - 10) * Mathf.Sin((t0 * d - (s + d)) * (2 * Mathf.PI) / p));
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

                        x1 = deltaX * Mathf.Pow(2, -10 * t) * Mathf.Sin((t * d - s) * (2 * Mathf.PI) / p) + deltaX;
                        y1 = deltaY * Mathf.Pow(2, -10 * t) * Mathf.Sin((t * d - s) * (2 * Mathf.PI) / p) + deltaY;
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

                        x0 = deltaX * Mathf.Pow(2, -10 * t0) * Mathf.Sin((t0 * d - s) * (2 * Mathf.PI) / p) + deltaX;
                        y0 = deltaY * Mathf.Pow(2, -10 * t0) * Mathf.Sin((t0 * d - s) * (2 * Mathf.PI) / p) + deltaY;
                    }

                    break;

                case Ease.ExpoInOut:
                    u0 = 1f; u1 = 0f; u2 = 0f; u3 = 1f;

                    x1 = deltaX * cubic_bezier(t / d, u0, u1, u2, u3);
                    x0 = deltaX * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    y1 = deltaY * cubic_bezier(t / d, u0, u1, u2, u3);
                    y0 = deltaY * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    break;

                case Ease.ExpoIn:
                    u0 = 1f; u1 = 0f; u2 = 1f; u3 = 0f;

                    x1 = deltaX * cubic_bezier(t / d, u0, u1, u2, u3);
                    x0 = deltaX * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    y1 = deltaY * cubic_bezier(t / d, u0, u1, u2, u3);
                    y0 = deltaY * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    break;

                case Ease.ExpoOut:
                    u0 = 0f; u1 = 1f; u2 = 0f; u3 = 1f;

                    x1 = deltaX * cubic_bezier(t / d, u0, u1, u2, u3);
                    x0 = deltaX * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    y1 = deltaY * cubic_bezier(t / d, u0, u1, u2, u3);
                    y0 = deltaY * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    break;

                case Ease.SinInOut:
                    x1 = (-x / 2) * (Mathf.Cos(Mathf.PI * (t / d)) - 1);
                    y1 = (-y / 2) * (Mathf.Cos(Mathf.PI * (t / d)) - 1);

                    x0 = (-x / 2) * (Mathf.Cos(Mathf.PI * (t0 / d)) - 1);
                    y0 = (-y / 2) * (Mathf.Cos(Mathf.PI * (t0 / d)) - 1);
                    break;

                case Ease.SinIn:
                    x1 = -x * Mathf.Cos(t / d * (Mathf.PI / 2)) + x;
                    y1 = -y * Mathf.Cos(t / d * (Mathf.PI / 2)) + y;

                    x0 = -x * Mathf.Cos(t0 / d * (Mathf.PI / 2)) + x;
                    y0 = -y * Mathf.Cos(t0 / d * (Mathf.PI / 2)) + y;
                    break;

                case Ease.SinOut:
                    x1 = deltaX * Mathf.Sin(t / d * (Mathf.PI / 2));
                    y1 = deltaY * Mathf.Sin(t / d * (Mathf.PI / 2));

                    x0 = deltaX * Mathf.Sin(t0 / d * (Mathf.PI / 2));
                    y0 = deltaY * Mathf.Sin(t0 / d * (Mathf.PI / 2));
                    break;

                case Ease.BackInOut:
                    u0 = .43f; u1 = -.5f; u2 = .57f; u3 = 1.5f;

                    x1 = deltaX * cubic_bezier(t / d, u0, u1, u2, u3);
                    x0 = deltaX * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    y1 = deltaY * cubic_bezier(t / d, u0, u1, u2, u3);
                    y0 = deltaY * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    break;

                case Ease.BackIn:
                    u0 = .43f; u1 = -.5f; u2 = 1f; u3 = 1f;

                    x1 = deltaX * cubic_bezier(t / d, u0, u1, u2, u3);
                    x0 = deltaX * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    y1 = deltaY * cubic_bezier(t / d, u0, u1, u2, u3);
                    y0 = deltaY * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    break;

                case Ease.BackOut:
                    u0 = 0f; u1 = 0f; u2 = .57f; u3 = 1.5f;

                    x1 = deltaX * cubic_bezier(t / d, u0, u1, u2, u3);
                    x0 = deltaX * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    y1 = deltaY * cubic_bezier(t / d, u0, u1, u2, u3);
                    y0 = deltaY * cubic_bezier(t0 / d, u0, u1, u2, u3);

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

                    x1 = deltaX * C;
                    y1 = deltaY * C;

                    x0 = deltaX * C0;
                    y0 = deltaY * C0;

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

                    x1 = deltaX * C;
                    y1 = deltaY * C;

                    x0 = deltaX * C0;
                    y0 = deltaY * C0;

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

                    x1 = deltaX * C;
                    y1 = deltaY * C;

                    x0 = deltaX * C0;
                    y0 = deltaY * C0;

                    break;

                case Ease.Custom:
                    u0 = U0; u1 = U1; u2 = U2; u3 = U3;

                    x1 = deltaX * cubic_bezier(t / d, u0, u1, u2, u3);
                    x0 = deltaX * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    y1 = deltaY * cubic_bezier(t / d, u0, u1, u2, u3);
                    y0 = deltaY * cubic_bezier(t0 / d, u0, u1, u2, u3);

                    break;

                case Ease.Curve:
                    x1 = deltaX * curve.Evaluate(t / d);
                    x0 = deltaX * curve.Evaluate(t0 / d);

                    y1 = deltaY * curve.Evaluate(t / d);
                    y0 = deltaY * curve.Evaluate(t0 / d);

                    break;

                default:
                    break;
            }

            xPos = x1 - x0;
            yPos = y1 - y0;

            group.transform.localScale = new Vector3(group.transform.localScale.x + xPos, group.transform.localScale.y + yPos, group.transform.localScale.z);

            if (!dontScaleParticles)
            {
                foreach (ParticleSystem ps in particles)
                {
                    ps.startSize = ps.startSize + (((!xOppositeSign ? xPos : 0) + (!yOppositeSign ? yPos : 0)) / 2);
                    var shape = ps.shape;
                    shape.scale = new Vector3(ps.shape.scale.x + xPos, ps.shape.scale.y + yPos, ps.shape.scale.z);
                }
            }

            t0 = time;
            time += Time.deltaTime;

            yield return null;
        }

        xPos = deltaX - x1;
        yPos = deltaY - y1;

        group.transform.localScale = new Vector3(group.transform.localScale.x + xPos, group.transform.localScale.y + yPos, group.transform.localScale.z);

        foreach (ParticleSystem ps in particles)
        {
            if (!dontScaleParticles)
            {
                ps.startSize = ps.startSize + (((!xOppositeSign ? xPos : 0) + (!yOppositeSign ? yPos : 0)) / 2);
                var shape = ps.shape;
                shape.scale = new Vector3(ps.shape.scale.x + xPos, ps.shape.scale.y + yPos, ps.shape.scale.z);
            }

            ps.enableEmission = !disableParticles;
        }

        if (disable)
        {
            group.SetActive(false);
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

        /*
        t = p.x;

        p = (p0 * Mathf.Pow((1 - t), 3)) +
            (3 * p1 * Mathf.Pow((1 - t), 2) * t) +
            (3 * p2 * Mathf.Pow(t, 2) * (1 - t)) +
            (p3 * Mathf.Pow(t, 3));*/

        return p.y;
    }

    public void Reset()
    {
        if (group == null) return;
        if (disable)
        {
            group.SetActive(true);
        }
        group.transform.localScale = original_scale;
        tracker.setOriginalScale(group.GetHashCode());

        int i = 0;
        foreach (ParticleSystem ps in particles)
        {
            ps.enableEmission = particlesState[i];
            i++;
        }

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
