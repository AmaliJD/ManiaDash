using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ScaleObject : MonoBehaviour
{
    [Header("Target Objects")]
    public List<int> groupIDs;
    public List<Transform> targets;
    public Transform centerObject;
    public bool useCenterObject = false;
    public bool updateCenterPosition = false;

    [Header("Pre-Scale Conditions")]
    [Min(0)]
    public float delay;
    [Min(0)]
    public float requiredManaCount;

    [Header("Scale Properites")]
    public Vector2 scaleValue;
    public float duration;

    public enum ScaleMode
    {
        multiply, add, set
    }
    public ScaleMode scaleMode = ScaleMode.multiply;

    [Header("Stay Rotate Properites")]
    public bool stayToScale = false;
    [Min(0)]
    public float scaleSpeed;

    [Header("Mods")]
    public float addScaleAmount = 0;
    public bool reverseRotateDirection = false;
    public bool reverseModAdd = false;
    public bool applyModPerTarget;
    public bool reverseOrderPerTarget;

    [Header("Easing")]
    public EasingOption easeOption;
    public EasingFunction.Ease functionEasing;
    public AnimationCurve curveEasing;

    [Header("Properties")]
    public bool waitToFinish = true;
    public bool useRigidbody = false;

    public bool loop = false;
    [Min(0)]
    public float loopDelay;

    [Min(-1)]
    public int triggerLimit = -1;

    [Header("Settings")]
    public bool resetOnDeathPerCheckpoint = false;
    public bool playOnAwake;
    public bool paused = false;
    public bool stopped = false;
    public bool hideIcon;
    public SpeedGizmo speedGizmo = SpeedGizmo.x1;

    private GameObject texture;

    private int triggerCount = 0;
    private List<Vector3> startScale;
    private bool inUse = false;

    //private float initialAddRotateAmount;

    private PlayerControllerV2 player;
    private GroupIDManager groupIDManager;
    private GameManager gamemanager;
    private ScaleTracker tracker;

    private void Awake()
    {
        gamemanager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControllerV2>();

        if (transform.childCount > 1)
        {
            texture = transform.GetChild(0).gameObject;
            texture.SetActive(!hideIcon);
        }
    }

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        if (groupIDs.Count > 0)
        {
            groupIDManager = GameObject.FindGameObjectWithTag("Master").GetComponent<GroupIDManager>();
            foreach (int i in groupIDs)
            {
                targets.AddRange(groupIDManager.groupIDList[i].ConvertAll(x => x.transform));
            }
            targets = targets.Distinct().ToList();
        }

        if (targets.Count == 0) { enabled = false; return; }

        tracker = GameObject.FindGameObjectWithTag("Master").GetComponent<ScaleTracker>();

        startScale = new List<Vector3>();
        foreach (Transform tr in targets) { startScale.Add(tr.localScale); }

        if (useCenterObject && centerObject == null)
        {
            useCenterObject = false;
        }

        //initialAddRotateAmount = addRotateAmount;

        if (playOnAwake && !stayToScale)
        {
            Scale();
        }
    }

    public void Scale()
    {
        if ((waitToFinish && inUse) || requiredManaCount > gamemanager.getManaCount()) { return; }

        if (triggerLimit == -1 || triggerCount < triggerLimit)
        {
            stopped = false;
            triggerCount++;

            StartCoroutine(ScaleCoroutine());
        }
    }

    public void ResetTrigger()
    {
        for (int i = 0; i < targets.Count; i++)
        {
            targets[i].localScale = startScale[i];
            tracker.setOriginalScale(targets[i].GetHashCode());
        }
        StopAllCoroutines();
        triggerCount = 0;

        //addRotateAmount = initialAddRotateAmount;

        paused = false;
        inUse = false;
    }

    public void StopTrigger()
    {
        stopped = true;
    }

    public void PauseTrigger()
    {
        paused = true;
    }

    public void ResumeTrigger()
    {
        paused = false;
    }

    private IEnumerator ScaleCoroutine()
    {
        inUse = true;
        if (delay > 0) { yield return new WaitForSeconds(delay); }

        float elapsedTime = 0;
        Vector2[] totalDisplacement = new Vector2[targets.Count];
        Vector2 lastCenterPosition = centerObject != null ? (Vector2)centerObject.position : Vector2.zero;
        Vector2[] thisScaleAmount = Enumerable.Repeat(scaleValue, targets.Count).ToArray();

        for (int i = 0; i < targets.Count; i++)
        {
            Vector2 baseScale = tracker.getBaseScale(targets[i].GetHashCode(), targets[i].localScale);
            Vector2 addAmount = scaleValue;
            switch (scaleMode)
            {
                case ScaleMode.add:
                    addAmount = scaleValue;
                    break;

                case ScaleMode.multiply:
                    Vector2 newScale = new Vector2(scaleValue.x * baseScale.x, scaleValue.y * baseScale.y);
                    addAmount = newScale - baseScale;
                    break;

                case ScaleMode.set:
                    addAmount = scaleValue - baseScale;
                    break;
            }
            Debug.Log("Old: " + baseScale + "   New: " + (baseScale + addAmount) + "   Add: " + addAmount);
            thisScaleAmount[i] = addAmount;
            tracker.setNewScale(targets[i].GetHashCode(), new Vector3(baseScale.x + addAmount.x, baseScale.y + addAmount.y, targets[i].localScale.z));
        }


        //Vector2[] totalRotateAmount = new float[targets.Count];


        Vector2[] radius = new Vector2[targets.Count];

        /*if (applyModPerTarget && targets.Count > 1)
        {
            if (!reverseOrderPerTarget)
            {
                float tempAddRotateAmount = addRotateAmount;
                for (int i = 1; i < targets.Count; i++)
                {
                    float rev = Mathf.Sign(addRotateAmount) == Mathf.Sign(thisRotateAmount[i - 1]) ? 1 : -1;
                    //thisRotateAmount[i] = thisRotateAmount[i - 1] + rev * addRotateAmount;
                    if (reverseRotateDirection)
                    {
                        thisRotateAmount[i] = -thisRotateAmount[i];
                    }
                    if (reverseModAdd)
                    {
                        addRotateAmount = -addRotateAmount;
                    }
                }
                addRotateAmount = tempAddRotateAmount;
            }
            else
            {
                float tempAddRotateAmount = addRotateAmount;
                for (int i = targets.Count - 1; i > 0; i--)
                {
                    float rev = Mathf.Sign(addRotateAmount) == Mathf.Sign(thisRotateAmount[(i + 1) % targets.Count]) ? 1 : -1;
                    //thisRotateAmount[i] = thisRotateAmount[(i + 1) % targets.Count] + rev * addRotateAmount;
                    if (reverseRotateDirection)
                    {
                        thisRotateAmount[i] = -thisRotateAmount[i];
                    }
                    if (reverseModAdd)
                    {
                        addRotateAmount = -addRotateAmount;
                    }
                }
                addRotateAmount = tempAddRotateAmount;
            }
        }


        float sign = Mathf.Sign(addRotateAmount) == Mathf.Sign(rotateAmount) ? 1 : -1;
        rotateAmount = rotateAmount + sign * addRotateAmount;
        if (reverseRotateDirection)
        {
            rotateAmount = -rotateAmount;
        }
        if (reverseModAdd)
        {
            addRotateAmount = -addRotateAmount;
        }*/

        if (centerObject == null) { centerObject = transform; }
        Vector3 centerPosition = centerObject.position;

        while (elapsedTime < duration)
        {
            if (stopped)
            {
                //addRotateAmount = initialAddRotateAmount;
                for (int i = 0; i < targets.Count; i++)
                {
                    Vector3 amountLeft = (Vector3)(thisScaleAmount[i] - totalDisplacement[i]);
                    tracker.setNewScale(targets[i].GetHashCode(), tracker.getBaseScale(targets[i].GetHashCode(), targets[i].localScale) - amountLeft);
                }

                inUse = false;
                yield break;
            }

            if (paused)
            {
                Vector2[] lastBase = new Vector2[targets.Count];
                Vector2[] currentScale = new Vector2[targets.Count];
                Vector2[] amountLeft = new Vector2[targets.Count];
                for (int i = 0; i < targets.Count; i++)
                {
                    currentScale[i] = targets[i].localScale;
                    lastBase[i] = tracker.getBaseScale(targets[i].GetHashCode(), targets[i].localScale);
                    amountLeft[i] = thisScaleAmount[i] - totalDisplacement[i];
                    thisScaleAmount[i] -= amountLeft[i];
                    tracker.setNewScale(targets[i].GetHashCode(), tracker.getBaseScale(targets[i].GetHashCode(), targets[i].localScale) - (Vector3)amountLeft[i]);
                }

                yield return new WaitUntil(() => !paused);

                Vector2[] newBase = new Vector2[targets.Count];
                for (int i = 0; i < targets.Count; i++)
                {
                    newBase[i] = tracker.getBaseScale(targets[i].GetHashCode(), targets[i].localScale);
                    Vector2 multDifference = new Vector2(newBase[i].x / currentScale[i].x, newBase[i].y / currentScale[i].y);
                    amountLeft[i] = new Vector2(amountLeft[i].x * multDifference.x, amountLeft[i].y * multDifference.y);

                    thisScaleAmount[i] += amountLeft[i];
                    tracker.setNewScale(targets[i].GetHashCode(), tracker.getBaseScale(targets[i].GetHashCode(), targets[i].localScale) + (Vector3)amountLeft[i]);
                }
            }

            inUse = true;

            float t0 = Mathf.Clamp01(elapsedTime / duration);

            yield return null;

            elapsedTime += Time.deltaTime;
            float t1 = Mathf.Clamp01(elapsedTime / duration);
            float easeT0, easeT1;

            easeT0 = GetEaseValue(t0);
            easeT1 = GetEaseValue(t1);

            if (updateCenterPosition) { centerPosition = centerObject.position; }

            for (int i = 0; i < targets.Count; i++)
            {
                Vector2 delta = thisScaleAmount[i] * (easeT1 - easeT0);

                totalDisplacement[i] += delta;

                targets[i].localScale += (Vector3)delta;
            }
        }

        for (int i = 0; i < targets.Count; i++)
        {
            Vector2 difference = thisScaleAmount[i] - totalDisplacement[i];
            targets[i].localScale += (Vector3)difference;
        }

        if (loop && loopDelay > 0)
        {
            yield return new WaitForSeconds(loopDelay);
        }

        inUse = false;

        if (loop) { Scale(); }
    }

    private float GetEaseValue(float t)
    {
        float ease;
        switch (easeOption)
        {
            case EasingOption.AnimationCurve:
                ease = curveEasing.Evaluate(t);
                break;
            case EasingOption.EasingFunction:
                ease = EasingFunction.GetEasingFunction(functionEasing)(0, 1, t);
                break;
            default:
                ease = 0;
                break;
        }

        return ease;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player" && !stayToScale)
        {
            player.AddScaleTriggers(this);
            Scale();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "Player" && stayToScale)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                targets[i].localScale += (Vector3)Vector2.one * scaleSpeed * Time.deltaTime;
            }
        }
    }

    /*private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player" && stayToScale)
        {
            
        }
    }*/

    private void OnValidate()
    {
        if (texture != null && Application.isPlaying)
        {
            texture.SetActive(!hideIcon);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        float scale = 0;
        switch (speedGizmo)
        {
            case SpeedGizmo.x0:
                scale = 40f; break;
            case SpeedGizmo.x1:
                scale = 55f; break;
            case SpeedGizmo.x2:
                scale = 75f; break;
            case SpeedGizmo.x3:
                scale = 90f; break;
            case SpeedGizmo.x4:
                scale = 110f; break;
        }

        Vector3 delayPos = new Vector3((scale * Time.fixedDeltaTime * 10f) * delay, 0, 0);
        Vector3 durationPos = new Vector3((scale * Time.fixedDeltaTime * 10f) * duration, 0, 0) + delayPos;

        Gizmos.color = new Color(1, 1, 1, .25f);
        Gizmos.DrawLine(transform.position, transform.position + delayPos);

        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position + delayPos, transform.position + durationPos);

        foreach (Transform tf in targets)
        {
            if (tf == null) { continue; }
            Vector3 triggerPos = transform.position;
            Vector3 objPos = tf.position;
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
    }
#endif
}