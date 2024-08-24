using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ProximityEffector : MonoBehaviour
{
    [Header("Target")]
    [SerializeField]
    private Transform[] targets;

    [Header("Options")]
    [Min(0)]
    public float range;
    [Min(0)]
    public float buffer;
    [Min(0)]
    public float fadein;
    [Min(0)]
    public float fadeout;
    public bool local;

    public bool scaledRange;
    public bool invert;

    [Header("Light")]
    public bool affectintensity;
    [Range(0.0f, 2.0f)]
    public float intensityValue;

    [Header("LightColor")]
    public bool affectlightcolor;
    public Color lightColorValue;

    [Header("Alpha")]
    public bool affectalpha;
    [Range(0.0f, 1.0f)]
    public float alphaValue;

    [Header("Color")]
    public bool affectcolor;
    public Color colorValue;

    [Header("Rotation")]
    public bool affectrotation;
    public float rotateValue;
    public Vector2 rotationValueRange;
    public bool randomRot;

    [Header("Position")]
    public bool affectposition;
    public Vector2 positionValue;
    public Vector4 positionValueRange;
    private Vector2 randomPositionValue;
    public bool randomPos;

    [Header("Distance")]
    public bool affectdistance;
    public float distanceValue;
    //public bool distanceX, distanceY;

    [Header("Scale")]
    public bool affectscale;
    public Vector2 scaleValue;
    public bool setScale;

    [Header("Direction")]
    public distanceDirection directionType;
    public enum distanceDirection { radial, x, y };    
    public bool exclusive = false, keepstate, xPos = true, xNeg = true, yPos = true, yNeg = true;
    public bool enableWhileNotVisible;

    private Transform player;
    private Light2D light;
    private SpriteRenderer sprite;

    private float initIntensity;
    private float initAlpha;
    private Color initColor;
    private Color initLightColor;
    private Quaternion initRotation;
    private Vector2 initPosition;
    private Vector2 initWorldPosition;
    private Vector2 initScale;

    private float lerp, lastLerp;
    private float timer;
    private float fade;

    private DrawColliders dc;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        light = gameObject.GetComponent<Light2D>();
        sprite = gameObject.GetComponent<SpriteRenderer>();

        if (light != null)
        {
            initIntensity = light.intensity;
            initLightColor = light.color;
        }
        if (sprite != null)
        {
            initAlpha = sprite.color.a;
            initColor = sprite.color;
        }

        initRotation = transform.localRotation; 
        initPosition = transform.localPosition;
        initWorldPosition = transform.position;
        initScale = transform.localScale;

        //if (randomPos) { randomPositionValue = new Vector2(Random.Range(-positionValue.x, positionValue.x), Random.Range(-positionValue.y, positionValue.y)); }
        if (randomPos) { randomPositionValue = new Vector2(Random.Range(positionValueRange.x, positionValueRange.y), Random.Range(positionValueRange.z, positionValueRange.w)); }
        if (randomRot) { rotateValue = Random.Range(rotationValueRange.x, rotationValueRange.y); }

        if (scaledRange)
        {
            range *= transform.lossyScale.x;
            buffer *= transform.lossyScale.x;
        }

        fade = fadein;

        dc = Camera.main.GetComponent<DrawColliders>();
    }

 
#if UNITY_EDITOR
    private void OnValidate()
    {
        //initWorldPosition = transform.position;
    }
#endif

    // Update is called once per frame
    void LateUpdate()
    {
        Vector2 targetSelfPosition = !local ? initWorldPosition : (Vector2)transform.TransformPoint(initPosition);

        if (targets.Count() > 0)
            player = targets.OrderBy(t => Vector2.Distance(targetSelfPosition, t.position)).FirstOrDefault();

        Vector3 ajdustedPlayerPosition = player.position;

        if (exclusive)
        {
            if (!xPos && ajdustedPlayerPosition.x - targetSelfPosition.x > 0) { ajdustedPlayerPosition.x = int.MaxValue; if (keepstate) return; }
            if (!xNeg && ajdustedPlayerPosition.x - targetSelfPosition.x < 0) { ajdustedPlayerPosition.x = -int.MaxValue; if (keepstate) return; }
            if (!yPos && ajdustedPlayerPosition.y - targetSelfPosition.y > 0) { ajdustedPlayerPosition.y = int.MaxValue; if (keepstate) return; }
            if (!yNeg && ajdustedPlayerPosition.y - targetSelfPosition.y < 0) { ajdustedPlayerPosition.y = -int.MaxValue; if (keepstate) return; }
        }

        float distance = Vector2.Distance(ajdustedPlayerPosition, targetSelfPosition);
        switch (directionType)
        {
            case distanceDirection.radial:
                distance = Vector2.Distance(ajdustedPlayerPosition, targetSelfPosition); break;
            case distanceDirection.x:
                distance = Mathf.Abs(ajdustedPlayerPosition.x - targetSelfPosition.x); break;
            case distanceDirection.y:
                distance = Mathf.Abs(ajdustedPlayerPosition.y - targetSelfPosition.y); break;
        }

        float prevLerp = lerp;
        lerp = 1 - Mathf.Clamp((distance - buffer) / range, 0, 1);
        if (invert) { lerp = 1 - lerp; }

        if(lerp == prevLerp)
        {
            if(fade != 0)
            {
                lerp = Mathf.Lerp(lastLerp, lerp, timer / fade);
            }
            timer += Time.deltaTime;
        }
        if(lerp > prevLerp && fadein > 0)
        {
            lastLerp = prevLerp;
            fade = fadein;
            timer = Time.deltaTime;
            lerp = Mathf.Lerp(lastLerp, lerp, timer / fade);
        }
        else if(lerp < prevLerp && fadeout > 0)
        {
            lastLerp = prevLerp;
            fade = fadeout;
            timer = Time.deltaTime;
            lerp = Mathf.Lerp(lastLerp, lerp, timer / fade);
        }

        Vector2 basePosition = targetSelfPosition;
        if (affectintensity)
        {
            light.intensity = Mathf.Lerp(initIntensity, intensityValue, lerp);
        }

        if (affectlightcolor)
        {
            light.color = Color.Lerp(initLightColor, lightColorValue, lerp);
        }

        if (affectcolor)
        {
            sprite.color = Color.Lerp(initColor, colorValue, lerp);
        }

        if (affectalpha)
        {
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, Mathf.Lerp(initAlpha, alphaValue, lerp));
        }

        if (affectrotation)
        {
            if (lerp == 0 && randomRot) { rotateValue = Random.Range(rotationValueRange.x, rotationValueRange.y); }
            transform.localRotation = Quaternion.Lerp(initRotation, Quaternion.Euler(transform.localEulerAngles.x, transform.localEulerAngles.y, rotateValue), lerp);
        }

        if (affectposition)
        {
            if (lerp == 0 && randomPos) { randomPositionValue = new Vector2(Random.Range(-positionValue.x, positionValue.x), Random.Range(-positionValue.y, positionValue.y)); }
            transform.localPosition = Vector2.Lerp(initPosition, initPosition + (randomPos ? randomPositionValue : positionValue), lerp);
            basePosition = transform.position;
        }

        if (affectdistance)
        {
            Vector2 playerPos = (Vector2)player.position;
            //if (distanceX) { playerPos.y = basePosition.y; }
            //if (distanceY) { playerPos.x = basePosition.x; }
            Vector2 newPosition = basePosition + ((basePosition - playerPos).normalized * distanceValue);
            transform.position = Vector2.Lerp(basePosition, newPosition, lerp);
        }

        if (affectscale)
        {
            if(!setScale)
            {
                transform.localScale = Vector2.Lerp(initScale, new Vector2(initScale.x * scaleValue.x, initScale.y * scaleValue.y), lerp);
            }
            else
            {
                transform.localScale = Vector2.Lerp(initScale, new Vector2(scaleValue.x, scaleValue.y), lerp);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        float R = range, B = buffer;
        if (scaledRange)
        {
            R *= transform.lossyScale.x;
            B *= transform.lossyScale.x;
        }

#if UNITY_EDITOR
        if (!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            initWorldPosition = transform.position;
        }
#endif

        Gizmos.color = new Color(1, 1, 0, .9f);
        Gizmos.DrawWireSphere(initWorldPosition, R + B);

        Gizmos.color = new Color(1, 1, 1, .7f);
        Gizmos.DrawWireSphere(initWorldPosition, B);
    }

    private void OnBecameInvisible()
    {
        if (enableWhileNotVisible) { return; }
        //transform.GetChild(0).gameObject.SetActive(false);
        if (dc == null || (dc != null && !dc.hideRenderers)) { enabled = false; }
        //visible = false;
    }

    private void OnBecameVisible()
    {
        if (enableWhileNotVisible) { return; }
        //transform.GetChild(0).gameObject.SetActive(true);
        enabled = true;
        //visible = true;
    }
}