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
    private bool updateCenterPosition = false;

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
    public Vector2 scaleSpeed;

    [Header("Mods")]
    public ModInvertMode invertScaleMode = ModInvertMode.none;
    public ModScaleMode modScaleMode = ModScaleMode.add;
    public Vector2 modScaleAmount = Vector2.zero;
    public bool applyModPerTarget;
    public bool reverseOrderPerTarget;
    public enum ModScaleMode
    {
        none, multiply, add
    }
    public enum ModInvertMode
    {
        none, reverse, invert
    }

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
    public TriggerOffScreenDisable offScreenDisable;
    public bool resetOnDeathPerCheckpoint = false;
    public bool playOnAwake;
    public bool paused = false;
    private bool offScreenPaused;
    public bool stopped = false;
    public bool hideIcon;
    public SpeedGizmo speedGizmo = SpeedGizmo.x1;

    private GameObject texture;

    private bool hasRigidBody = false;
    private List<Rigidbody2D> rb;
    private int triggerCount = 0;
    private List<Vector3> startScale;
    private List<Vector3> startPosition;
    private bool inUse = false;

    private Vector2 initialModScaleAmount;

    private PlayerControllerV2 player;
    private GroupIDManager groupIDManager;
    private GameManager gamemanager;
    private ScaleTracker tracker;
    private Coroutine disableOffScreenCoroutine;
    private List<Renderer> targetsWithRenderers;

    private void Awake()
    {
        gamemanager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControllerV2>();

        if (transform.childCount > 0)
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

        targetsWithRenderers = targets.Where(t => t.GetComponent<Renderer>() != null).ToList().ConvertAll(r => r.GetComponent<Renderer>());
        if (targetsWithRenderers.Count() == 0) { offScreenDisable = TriggerOffScreenDisable.None; }
        if (centerObject != null && centerObject.GetComponent<Renderer>() != null) { targetsWithRenderers.Add(centerObject.GetComponent<Renderer>()); }

        tracker = GameObject.FindGameObjectWithTag("Master").GetComponent<ScaleTracker>();

        bool nullRB = false;
        rb = new List<Rigidbody2D>();
        startScale = new List<Vector3>();
        startPosition = new List<Vector3>();
        foreach (Transform tr in targets)
        {
            rb.Add(tr.GetComponent<Rigidbody2D>());
            if (tr.GetComponent<Rigidbody2D>() == null) { nullRB = true; }
            startScale.Add(tr.localScale);
            startPosition.Add(tr.position);
        }

        hasRigidBody = useRigidbody && !nullRB;


        if (useCenterObject && centerObject == null)
        {
            useCenterObject = false;
        }

        initialModScaleAmount = modScaleAmount;

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

            if (offScreenDisable != TriggerOffScreenDisable.None && disableOffScreenCoroutine == null)
            {
                disableOffScreenCoroutine = StartCoroutine(OffScreenCheck());
            }
        }
    }

    public void ResetTrigger()
    {
        for (int i = 0; i < targets.Count; i++)
        {
            targets[i].localScale = startScale[i];
            tracker.setOriginalScale(targets[i].GetHashCode());
            if (hasRigidBody)
            {
                rb[i].velocity = Vector2.zero;
            }

            if(useCenterObject)
            {
                targets[i].transform.position = startPosition[i];
            }
        }

        disableOffScreenCoroutine = null;
        StopAllCoroutines();
        triggerCount = 0;

        modScaleAmount = initialModScaleAmount;

        offScreenPaused = false;
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

    public void TogglePauseTrigger()
    {
        paused = !paused;
    }

    private IEnumerator OffScreenCheck()
    {
        yield return new WaitUntil(() => isVisible());

        while (true)
        {
            switch (offScreenDisable)
            {
                case TriggerOffScreenDisable.Pause:
                    if (!isVisible() && !offScreenPaused && !paused)
                    {
                        paused = true;
                        offScreenPaused = true;
                    }
                    else if (isVisible() && offScreenPaused)
                    {
                        paused = false;
                        offScreenPaused = false;
                    }
                    break;

                case TriggerOffScreenDisable.Disable:
                    if (!isVisible())
                    {
                        paused = false;
                        offScreenPaused = false;
                        StopAllCoroutines();
                        yield break;
                    }
                    break;
            }

            yield return null;
        }
    }

    private bool isVisible()
    {
        bool visible = false;
        foreach (Renderer r in targetsWithRenderers)
        {
            if (r.isVisible) { visible = true; break; }
        }

        return visible;
    }

    private IEnumerator ScaleCoroutine()
    {
        inUse = true;
        if (delay > 0) { yield return new WaitForSeconds(delay); }

        float elapsedTime = 0;
        Vector2[] totalDisplacement = new Vector2[targets.Count];
        Vector2[] targetBase = new Vector2[targets.Count];
        Vector2 lastCenterPosition = centerObject != null ? (Vector2)centerObject.position : Vector2.zero;
        Vector2[] thisScaleAmount = Enumerable.Repeat(scaleValue, targets.Count).ToArray();
        Vector2[] deltaMultiplier = Enumerable.Repeat(Vector2.one, targets.Count).ToArray();

        Vector2[] velocity0 = new Vector2[targets.Count];
        Vector2[] velocity1 = new Vector2[targets.Count];
        Vector2[] velocityDeltaTotal = new Vector2[targets.Count];

        if (applyModPerTarget && targets.Count > 1)
        {
            if (!reverseOrderPerTarget)
            {
                Vector2 tempModScaleAmount = modScaleAmount;
                for (int i = 1; i < targets.Count; i++)
                {
                    switch (invertScaleMode)
                    {
                        case ModInvertMode.invert:
                            thisScaleAmount[i] = thisScaleAmount[i - 1].Invert();
                            break;

                        case ModInvertMode.reverse:
                            thisScaleAmount[i] = -thisScaleAmount[i - 1];
                            break;
                    }

                    float rev1 = Mathf.Sign(modScaleAmount.x) * Mathf.Sign(thisScaleAmount[i - 1].x);
                    float rev2 = Mathf.Sign(modScaleAmount.y) * Mathf.Sign(thisScaleAmount[i - 1].y);

                    switch (modScaleMode)
                    {
                        case ModScaleMode.multiply:
                            thisScaleAmount[i] = Vector2.Scale(thisScaleAmount[i - 1], modScaleAmount);
                            break;

                        case ModScaleMode.add:
                            //thisScaleAmount[i] = thisScaleAmount[i - 1] + modScaleAmount;
                            thisScaleAmount[i] = thisScaleAmount[i - 1] + new Vector2(rev1 * Mathf.Abs(modScaleAmount.x), rev2 * Mathf.Abs(modScaleAmount.y));
                            break;
                    }
                }
                modScaleAmount = tempModScaleAmount;
            }
            else
            {
                Vector2 tempModScaleAmount = modScaleAmount;
                for (int i = targets.Count - 1; i > 0; i--)
                {
                    switch (invertScaleMode)
                    {
                        case ModInvertMode.invert:
                            thisScaleAmount[i] = thisScaleAmount[(i + 1) % targets.Count].Invert();
                            break;

                        case ModInvertMode.reverse:
                            thisScaleAmount[i] = -thisScaleAmount[(i + 1) % targets.Count];
                            break;
                    }

                    float rev1 = Mathf.Sign(modScaleAmount.x) * Mathf.Sign(thisScaleAmount[(i + 1) % targets.Count].x);
                    float rev2 = Mathf.Sign(modScaleAmount.y) * Mathf.Sign(thisScaleAmount[(i + 1) % targets.Count].y);

                    switch (modScaleMode)
                    {
                        case ModScaleMode.multiply:
                            thisScaleAmount[i] = Vector2.Scale(thisScaleAmount[(i + 1) % targets.Count], modScaleAmount);
                            break;

                        case ModScaleMode.add:
                            //thisScaleAmount[i] = thisScaleAmount[(i + 1) % targets.Count] + modScaleAmount;
                            thisScaleAmount[i] = thisScaleAmount[(i + 1) % targets.Count] + new Vector2(rev1 * Mathf.Abs(modScaleAmount.x), rev2 * Mathf.Abs(modScaleAmount.y));
                            break;
                    }
                }
                modScaleAmount = tempModScaleAmount;
            }
        }

        Vector2[] distanceToCenter = targets.ConvertAll(x => (Vector2)x.localPosition - lastCenterPosition).ToArray();
        Vector2[] thisMoveAmount = Enumerable.Repeat(Vector2.zero, targets.Count).ToArray();
        Vector2[] totalMoveDisplacement = new Vector2[targets.Count];

        Vector2 centerBaseScale = centerObject != null ? centerObject.localScale : Vector3.zero;
        if (useCenterObject)
        {
            centerBaseScale = tracker.getBaseScale(centerObject.GetHashCode(), centerObject.localScale);
        }
        
        for (int i = 0; i < targets.Count; i++)
        {
            Vector2 baseScale = tracker.getBaseScale(targets[i].GetHashCode(), targets[i].localScale);
            Vector2 addAmount = thisScaleAmount[i];
            switch (scaleMode)
            {
                case ScaleMode.add:
                    addAmount = thisScaleAmount[i];
                    //thisMoveAmount[i] = Vector2.Scale(distanceToCenter[i], scaleValue.SignedAdd(1));
                    break;

                case ScaleMode.multiply:
                    Vector2 newScale = new Vector2(thisScaleAmount[i].x * baseScale.x, thisScaleAmount[i].y * baseScale.y);
                    addAmount = newScale - baseScale;
                    break;

                case ScaleMode.set:
                    addAmount = thisScaleAmount[i] - baseScale;
                    break;
            }

            if (useCenterObject)
            {
                Vector2 centerObjectLossyScale = Vector2.Scale(centerBaseScale, centerObject.transform.parent != null ? (Vector2)centerObject.transform.parent.lossyScale : Vector2.one);
                switch (scaleMode)
                {
                    case ScaleMode.add:
                        Vector2 scaleFactor = VectorExtension.Divide(scaleValue + centerObjectLossyScale, centerObjectLossyScale);
                        thisMoveAmount[i] = Vector2.Scale(distanceToCenter[i], scaleFactor) - distanceToCenter[i];
                        break;

                    case ScaleMode.multiply:
                        thisMoveAmount[i] = Vector2.Scale(distanceToCenter[i], scaleValue) - distanceToCenter[i];
                        break;

                    case ScaleMode.set:
                        scaleFactor = VectorExtension.Divide(scaleValue, centerObjectLossyScale);
                        thisMoveAmount[i] = Vector2.Scale(distanceToCenter[i], scaleFactor) - distanceToCenter[i];
                        break;
                }
            }

            //Debug.Log("From: " + baseScale + "   To: " + (baseScale + addAmount) + "   Add: " + addAmount);
            thisScaleAmount[i] = addAmount;
            //Debug.Log("Move Amount: " + thisMoveAmount[i]);
            tracker.setNewScale(targets[i].GetHashCode(), new Vector3(baseScale.x + addAmount.x, baseScale.y + addAmount.y, targets[i].localScale.z));
            targetBase[i] = baseScale + addAmount;
        }

        //Vector2[] radius = new Vector2[targets.Count];

        switch (invertScaleMode)
        {
            case ModInvertMode.invert:
                scaleValue = scaleValue.Invert();
                break;

            case ModInvertMode.reverse:
                scaleValue = -scaleValue;
                break;
        }

        float revX = Mathf.Sign(scaleValue.x) * Mathf.Sign(modScaleAmount.x);
        float revY = Mathf.Sign(scaleValue.y) * Mathf.Sign(modScaleAmount.y);

        switch (modScaleMode)
        {
            case ModScaleMode.multiply:
                scaleValue = Vector2.Scale(scaleValue, modScaleAmount);
                break;

            case ModScaleMode.add:
                scaleValue += new Vector2(revX * Mathf.Abs(modScaleAmount.x), revY * Mathf.Abs(modScaleAmount.y));
                break;
        }


        if (centerObject == null) { centerObject = transform; }
        Vector3 centerPosition = centerObject.position;

        while (elapsedTime < duration)
        {
            if (stopped)
            {
                modScaleAmount = initialModScaleAmount;
                for (int i = 0; i < targets.Count; i++)
                {
                    Vector3 amountLeft = (Vector3)(thisScaleAmount[i] - totalDisplacement[i]);
                    tracker.setNewScale(targets[i].GetHashCode(), tracker.getBaseScale(targets[i].GetHashCode(), targets[i].localScale) - amountLeft);
                }

                if (hasRigidBody)
                {
                    for (int i = 0; i < targets.Count; i++)
                    {
                        rb[i].velocity -= velocityDeltaTotal[i];
                    }
                }

                inUse = false;
                yield break;
            }

            if (paused)
            {
                Vector2[] setBase = new Vector2[targets.Count];
                Vector2[] lastScale = new Vector2[targets.Count];
                Vector2[] amountLeft = new Vector2[targets.Count];
                Vector2[] totalLinearDisplacement = new Vector2[targets.Count];
                Vector2[] adjustedScaleAmount = new Vector2[targets.Count];
                //float displacementLeftPercentage = 1 - (elapsedTime / duration);
                float displacementLeftPercentage = 1 - GetEaseValue(Mathf.Clamp01(elapsedTime / duration));

                for (int i = 0; i < targets.Count; i++)
                {
                    adjustedScaleAmount[i] = thisScaleAmount[i];
                    totalLinearDisplacement[i] = (1 - displacementLeftPercentage) * thisScaleAmount[i];
                    lastScale[i] = targets[i].localScale;
                    setBase[i] = tracker.getBaseScale(targets[i].GetHashCode(), targets[i].localScale);
                    amountLeft[i] = thisScaleAmount[i] - totalDisplacement[i];
                    thisScaleAmount[i] -= amountLeft[i];
                    tracker.setNewScale(targets[i].GetHashCode(), tracker.getBaseScale(targets[i].GetHashCode(), targets[i].localScale) - (Vector3)amountLeft[i]);

                    //Debug.Log("Set Base: " + setBase[0]);
                    //Debug.Log("Total Disp: " + totalDisplacement[i] + "   Total Linear Disp: " + totalLinearDisplacement[i]);
                }

                if (hasRigidBody)
                {
                    for (int i = 0; i < targets.Count; i++)
                    {
                        rb[i].velocity -= velocityDeltaTotal[i];
                    }
                }

                yield return new WaitUntil(() => !paused);

                Vector2[] newBase = new Vector2[targets.Count];
                for (int i = 0; i < targets.Count; i++)
                {
                    newBase[i] = tracker.getBaseScale(targets[i].GetHashCode(), targets[i].localScale);

                    if (newBase[i] != setBase[i] - amountLeft[i] && scaleMode != ScaleMode.add)
                    {
                        Vector2 multDifference = new Vector2(newBase[i].x / lastScale[i].x, newBase[i].y / lastScale[i].y);
                        amountLeft[i] = Vector2.Scale(amountLeft[i], multDifference);

                        
                        Vector2 newScale = thisScaleAmount[i] + amountLeft[i];

                        //Vector2 totalDisplacementScaled = VectorExtension.Divide(totalLinearDisplacement[i], VectorExtension.Divide(totalDisplacement[i], totalLinearDisplacement[i]));
                        deltaMultiplier[i] = VectorExtension.Divide((newScale - totalDisplacement[i]) / displacementLeftPercentage, newScale).SetNaNToOne();
                    }

                    thisScaleAmount[i] += amountLeft[i];
                    tracker.setNewScale(targets[i].GetHashCode(), tracker.getBaseScale(targets[i].GetHashCode(), targets[i].localScale) + (Vector3)amountLeft[i]);

                    //Debug.Log("New Base: " + newBase[0]);
                    //Debug.Log("From: " + newBase[0] + "   To: " + (newBase[0] + amountLeft[i]) + "   Add: " + amountLeft[i] + "   This Scale Amount: " + thisScaleAmount[i] + "   Scaled Scale Amount: " + adjustedScaleAmount[i] + "   Delta Adder: " + deltaAdder[i]);
                    //Debug.Log("From: " + newBase[0] + "   To: " + (newBase[0] + amountLeft[i]) + "   Add: " + amountLeft[i] + "   This Scale Amount: " + thisScaleAmount[i] + "   Delta Multiplier: " + deltaMultiplier[i]);
                }

                if (hasRigidBody)
                {
                    for (int i = 0; i < targets.Count; i++)
                    {
                        rb[i].velocity += velocityDeltaTotal[i];
                    }
                }
            }

            inUse = true;

            float t0 = Mathf.Clamp01(elapsedTime / duration);

            //yield return null;
            if (hasRigidBody) { yield return new WaitForFixedUpdate(); }
            else { yield return null; }

            //elapsedTime += Time.deltaTime;
            elapsedTime += (hasRigidBody) ? Time.fixedDeltaTime : Time.deltaTime;
            float t1 = Mathf.Clamp01(elapsedTime / duration);
            float easeT0, easeT1;

            easeT0 = GetEaseValue(t0);
            easeT1 = GetEaseValue(t1);

            if (updateCenterPosition) { centerPosition = centerObject.position; }

            for (int i = 0; i < targets.Count; i++)
            {
                Vector2 delta = Vector2.Scale(thisScaleAmount[i] * (easeT1 - easeT0) , deltaMultiplier[i]);

                totalDisplacement[i] += delta;

                targets[i].localScale += (Vector3)delta;

                if (useCenterObject)
                {
                    Vector2 moveDelta = thisMoveAmount[i] * (easeT1 - easeT0);
                    if (hasRigidBody)
                    {
                        velocity0[i] = velocity1[i];
                        velocity1[i] = moveDelta / Time.fixedDeltaTime;
                        Vector2 velocityDelta = velocity1[i] - velocity0[i];
                        totalMoveDisplacement[i] += velocity1[i] * Time.fixedDeltaTime;

                        velocityDeltaTotal[i] += velocityDelta;
                        rb[i].velocity += velocityDelta;
                    }
                    else
                    {
                        targets[i].transform.position += (Vector3)moveDelta;
                        totalMoveDisplacement[i] += moveDelta;
                        //targets[i].transform.position += (Vector3)(moveDelta.magnitude * distanceToCenter[i].normalized);
                    }
                }
            }
        }

        for (int i = 0; i < targets.Count; i++)
        {
            //Debug.Log("Scale Amt: " + thisScaleAmount[i] + "   Displacement: " + totalDisplacement[i]);
            Vector2 difference = thisScaleAmount[i] - totalDisplacement[i];
            targets[i].localScale += (Vector3)difference;

            if (useCenterObject)
            {
                Vector2 moveDifference = thisMoveAmount[i] - totalMoveDisplacement[i];
                targets[i].transform.position += (Vector3)moveDifference;
            }

            if (hasRigidBody)
            {
                rb[i].velocity -= velocityDeltaTotal[i];
            }
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
                switch(scaleMode)
                {
                    case ScaleMode.add:
                        tracker.setNewScale(targets[i].GetHashCode(), tracker.getBaseScale(targets[i].GetHashCode(), targets[i].localScale) + (Vector3)scaleSpeed * Time.deltaTime);
                        targets[i].localScale += (Vector3)scaleSpeed * Time.deltaTime;
                        break;

                    case ScaleMode.multiply:
                        Vector2 newScale = new Vector2(targets[i].localScale.x * Mathf.Pow(scaleSpeed.x, Time.deltaTime), targets[i].localScale.y * Mathf.Pow(scaleSpeed.y, Time.deltaTime));
                        Vector2 diff = newScale - (Vector2)targets[i].localScale;
                        tracker.setNewScale(targets[i].GetHashCode(), tracker.getBaseScale(targets[i].GetHashCode(), targets[i].localScale) + (Vector3)diff);
                        targets[i].localScale += (Vector3)diff;
                        break;

                    case ScaleMode.set:
                        Vector2 amountToValue = scaleValue - (Vector2)targets[i].localScale;
                        //Vector2 speed = new Vector2(amountToValue.normalized.x * scaleSpeed.x, amountToValue.normalized.y * scaleSpeed.y);
                        Vector2 speed = Vector2.Scale(amountToValue.normalized, scaleSpeed);
                        tracker.setNewScale(targets[i].GetHashCode(), tracker.getBaseScale(targets[i].GetHashCode(), targets[i].localScale) + (Vector3)speed * Time.deltaTime);
                        targets[i].localScale += (Vector3)speed * Time.deltaTime;
                        break;
                }
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