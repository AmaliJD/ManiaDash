using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class ProximityLighting : MonoBehaviour
{
    [Range(0.0f, 10.0f)]
    public float range;
    public float max_light_value;

    [Range(0.0f, 1.0f)]
    public float max_alpha_value = 1;
    public float buffer;
    public float fadein, fadeout;
    public bool affect_sprite, inverse, scaledRange;

    public enum distanceDirection { radial, x, y};
    public distanceDirection directionType;

    public bool exclusive = false, xPos = true, xNeg = true, yPos = true, yNeg = true;

    /*public enum distanceSign { both, positive, negative };
    public distanceSign signType;*/

    Light2D light;
    GameObject player;
    SpriteRenderer sprite;
    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        light = gameObject.GetComponent<Light2D>();
        sprite = gameObject.GetComponent<SpriteRenderer>();

        if(scaledRange)
        {
            range *= transform.lossyScale.x;
            buffer *= transform.lossyScale.x;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        setMax();
    }
#endif

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 ajdustedPlayerPosition = player.transform.position;

        if (exclusive)
        {
            if (!xPos && ajdustedPlayerPosition.x - transform.position.x > 0) { ajdustedPlayerPosition.x = int.MaxValue; }
            if (!xNeg && ajdustedPlayerPosition.x - transform.position.x < 0) { ajdustedPlayerPosition.x = -int.MaxValue; }
            if (!yPos && ajdustedPlayerPosition.y - transform.position.y > 0) { ajdustedPlayerPosition.y = int.MaxValue; }
            if (!yNeg && ajdustedPlayerPosition.y - transform.position.y < 0) { ajdustedPlayerPosition.y = -int.MaxValue; }
        }

        float distance = Vector2.Distance(ajdustedPlayerPosition, transform.position);
        switch (directionType)
        {
            case distanceDirection.radial:
                distance = Vector2.Distance(ajdustedPlayerPosition, transform.position); break;
            case distanceDirection.x:
                distance = Mathf.Abs(ajdustedPlayerPosition.x - transform.position.x); break;
            case distanceDirection.y:
                distance = Mathf.Abs(ajdustedPlayerPosition.y - transform.position.y); break;
        }

        //float intensity = Mathf.Clamp((range - distance + buffer) / (range / max_light_value), 0, max_light_value);
        float intensity = (range - distance + buffer) / (range / max_light_value);
        float alpha = (range - distance + buffer) / (range / max_alpha_value);

        if(inverse)
        {
            intensity = max_light_value - intensity;
            alpha = max_alpha_value - alpha;
        }

        if (intensity < 0) { intensity = 0; alpha = 0; }
        else if (intensity > max_light_value) { intensity = max_light_value; alpha = max_alpha_value; }
        //if (alpha < 0) {  }

        if (intensity > light.intensity)
        {
            light.intensity = Mathf.Lerp(light.intensity, intensity, fadein);
            if (affect_sprite)
            {
                sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b,
                               Mathf.Lerp(sprite.color.a, alpha, fadein));
            }
            /*increasing = true;
            if (!prevIncreasing)
            {
                lastIntensity = light.intensity;
                lastAlpha = sprite.color.a;
            }
            light.intensity = Mathf.Lerp(lastIntensity, intensity, inTimer / fadein);
            if (affect_sprite)
            {
                sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b,
                               Mathf.Lerp(lastAlpha, alpha, inTimer / fadein));
            }

            inTimer += Time.deltaTime;
            inTimer = Mathf.Clamp(inTimer, 0, fadein);
            outTimer = 0;*/
        }
        else if (intensity < light.intensity)
        {
            light.intensity = Mathf.Lerp(light.intensity, intensity, fadeout);
            if (affect_sprite)
            {
                sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b,
                               Mathf.Lerp(sprite.color.a, alpha, fadeout));
            }
            /*increasing = false;
            if (prevIncreasing)
            {
                lastIntensity = light.intensity;
                lastAlpha = sprite.color.a;
            }
            light.intensity = Mathf.Lerp(lastIntensity, intensity, outTimer / fadeout);
            if (affect_sprite)
            {
                sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b,
                               Mathf.Lerp(lastAlpha, alpha, outTimer / fadeout));
            }

            outTimer += Time.deltaTime;
            outTimer = Mathf.Clamp(outTimer, 0, fadeout);
            inTimer = 0;*/
        }

        if (Mathf.Abs(intensity - light.intensity) < 0.001f)
        {
            light.intensity = intensity;
        }

        //prevIncreasing = increasing;
    }

    private void OnDrawGizmosSelected()
    {
        float R = range, B = buffer;
        if (scaledRange)
        {
            R *= transform.lossyScale.x;
            B *= transform.lossyScale.x;
        }

        Gizmos.color = new Color(1, 1, 0, .9f);
        Gizmos.DrawWireSphere(transform.position, R + B);

        Gizmos.color = new Color(1, 1, 1, .7f);
        Gizmos.DrawWireSphere(transform.position, B);
    }

    void setMax()
    {
        light = gameObject.GetComponent<Light2D>();
        sprite = gameObject.GetComponent<SpriteRenderer>();
        light.intensity = max_light_value;
        if (affect_sprite) sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, max_alpha_value);
    }

    //bool visible = false;

    private void OnBecameInvisible()
    {
        //transform.GetChild(0).gameObject.SetActive(false);
        enabled = false;
        //visible = false;
    }

    private void OnBecameVisible()
    {
        //transform.GetChild(0).gameObject.SetActive(true);
        enabled = true;
        //visible = true;
    }
}