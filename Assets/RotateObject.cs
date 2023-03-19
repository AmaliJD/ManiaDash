using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RotateObject : MonoBehaviour
{
    [Header("Target Objects")]
    public List<int> groupIDs;
    public List<Transform> targets;
    public Transform centerObject;
    public bool useCenterObject = false;
    public bool updateCenterPosition = false;
    public bool lockRotation = false;

    [Header("Pre-Rotate Conditions")]
    [Min(0)]
    public float delay;
    [Min(0)]
    public float requiredManaCount;

    [Header("Rotate Properites")]
    public float rotateAmount;
    //public float rotateScale = 1;
    public float duration;

    [Header("Stay Rotate Properites")]
    public bool stayToRotate = false;
    public bool clockwise = false;
    [Min(0)]
    public float rotateSpeed;

    [Header("Mods")]
    public float addRotateAmount = 0;
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
    public TriggerOffScreenDisable offScreenDisable;
    public bool resetOnDeathPerCheckpoint = false;
    public bool playOnAwake;
    public bool paused = false;
    private bool offScreenPaused;
    public bool stopped = false;
    public bool hideIcon;
    public SpeedGizmo speedGizmo = SpeedGizmo.x1;

    private GameObject texture;

    private List<Rigidbody2D> rb;
    private bool hasRigidBody;
    private int triggerCount = 0;
    private List<Quaternion> startRotation;
    private List<Vector3> startPosition;
    private bool inUse = false;

    private float initialAddRotateAmount;

    private PlayerControllerV2 player;
    private GroupIDManager groupIDManager;
    private GameManager gamemanager;
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
        if(targetsWithRenderers.Count() == 0) { offScreenDisable = TriggerOffScreenDisable.None; }
        if(centerObject != null && centerObject.GetComponent<Renderer>() != null) { targetsWithRenderers.Add(centerObject.GetComponent<Renderer>()); }

        bool nullRB = false;
        rb = new List<Rigidbody2D>();
        startRotation = new List<Quaternion>();
        startPosition = new List<Vector3>();
        foreach (Transform tr in targets)
        {
            rb.Add(tr.GetComponent<Rigidbody2D>());
            if (tr.GetComponent<Rigidbody2D>() == null) { nullRB = true; }

            startRotation.Add(tr.localRotation);
            startPosition.Add(tr.position);
        }

        hasRigidBody = useRigidbody && !nullRB;

        if (useCenterObject && centerObject == null)
        {
            rotateAmount = 0;
            useCenterObject = false;
        }

        initialAddRotateAmount = addRotateAmount;

        if (playOnAwake && !stayToRotate)
        {
            Rotate();
        }
    }

    public void Rotate()
    {
        if ((waitToFinish && inUse) || requiredManaCount > gamemanager.getManaCount()) { return; }

        if (triggerLimit == -1 || triggerCount < triggerLimit)
        {
            stopped = false;
            triggerCount++;

            /*if (useCenterObject)
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    if (hasRigidBody)
                    {
                        rb[i].angularVelocity = 0;
                    }
                }
                StopAllCoroutines();
            }*/

            StartCoroutine(RotateCoroutine());

            if(offScreenDisable != TriggerOffScreenDisable.None && disableOffScreenCoroutine == null)
            {
                disableOffScreenCoroutine = StartCoroutine(OffScreenCheck());
            }
        }
    }

    public void ResetTrigger()
    {
        for (int i = 0; i < targets.Count; i++)
        {
            targets[i].localRotation = startRotation[i];
            if (hasRigidBody)
            {
                rb[i].angularVelocity = 0;
            }

            if (useCenterObject)
            {
                targets[i].transform.position = startPosition[i];
            }
        }

        disableOffScreenCoroutine = null;
        StopAllCoroutines();
        triggerCount = 0;

        addRotateAmount = initialAddRotateAmount;

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
        //Debug.Log("Waiting to be visible");
        yield return new WaitUntil(() => isVisible());

        //Debug.Log("Checking If Visible");
        while (true)
        {
            switch (offScreenDisable)
            {
                case TriggerOffScreenDisable.Pause:
                    if (!isVisible() && !offScreenPaused && !paused)
                    {
                        paused = true;
                        offScreenPaused = true;
                        //Debug.Log("Paused");
                    }
                    else if (isVisible() && offScreenPaused)
                    {
                        paused = false;
                        offScreenPaused = false;
                        //Debug.Log("Unpaused");
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
        foreach(Renderer r in targetsWithRenderers)
        {
            if (r.isVisible) { visible = true; break; }
        }

        return visible;
    }

    private IEnumerator RotateCoroutine()
    {
        //float duration = duration;
        //bool useCenterObject = useCenterObject;
        //Transform centerObject = centerObject;

        inUse = true;
        if (delay > 0) { yield return new WaitForSeconds(delay); }

        float elapsedTime = 0;
        float[] angularVelocity0 = new float[targets.Count];
        float[] angularVelocity1 = new float[targets.Count];
        float[] angularVelocityDeltaTotal = new float[targets.Count];
        float[] totalDisplacement = new float[targets.Count];
        Vector2[] lastCenterPosition = centerObject != null ? Enumerable.Repeat((Vector2)centerObject.position, targets.Count).ToArray() : new Vector2[0];
        float[] thisRotateAmount = Enumerable.Repeat(rotateAmount, targets.Count).ToArray();
        float[] totalRotateAmount = new float[targets.Count];

        Vector2[] radius = new Vector2[targets.Count];

        Vector2[] velocity0 = new Vector2[targets.Count];
        Vector2[] velocity1 = new Vector2[targets.Count];
        Vector2[] velocityDeltaTotal = new Vector2[targets.Count];

        if (applyModPerTarget && targets.Count > 1)
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
        }

        if(centerObject == null) { centerObject = transform; }
        Vector3 centerPosition = centerObject.position;

        while (elapsedTime < duration)
        {
            if (stopped)
            {
                if (hasRigidBody)
                {
                    for (int i = 0; i < targets.Count; i++)
                    {
                        rb[i].angularVelocity -= angularVelocityDeltaTotal[i];
                    }
                }

                addRotateAmount = initialAddRotateAmount;

                inUse = false;
                yield break;
            }

            if (paused)
            {
                if (hasRigidBody)
                {
                    for (int i = 0; i < targets.Count; i++)
                    {
                        rb[i].angularVelocity -= angularVelocityDeltaTotal[i];
                    }
                }
                yield return new WaitUntil(() => !paused);
                if (hasRigidBody)
                {
                    for (int i = 0; i < targets.Count; i++)
                    {
                        rb[i].angularVelocity += angularVelocityDeltaTotal[i];
                    }
                }
            }

            inUse = true;

            float t0 = Mathf.Clamp01(elapsedTime / duration);

            if (hasRigidBody) { yield return new WaitForFixedUpdate(); }
            else { yield return null; }

            elapsedTime += (hasRigidBody) ? Time.fixedDeltaTime : Time.deltaTime;
            float t1 = Mathf.Clamp01(elapsedTime / duration);
            float easeT0, easeT1;

            easeT0 = GetEaseValue(t0);
            easeT1 = GetEaseValue(t1);

            if (updateCenterPosition) { centerPosition = centerObject.position; }

            for (int i = 0; i < targets.Count; i++)
            {
                totalRotateAmount[i] = thisRotateAmount[i];

                float delta = totalRotateAmount[i] * (easeT1 - easeT0);
                if (hasRigidBody)
                {
                    angularVelocity0[i] = angularVelocity1[i];
                    angularVelocity1[i] = delta / Time.fixedDeltaTime;
                    float angularVelocityDelta = angularVelocity1[i] - angularVelocity0[i];
                    totalDisplacement[i] += angularVelocity1[i] * Time.fixedDeltaTime;

                    if (!useCenterObject || (useCenterObject && !lockRotation))
                    {
                        angularVelocityDeltaTotal[i] += angularVelocityDelta;
                        rb[i].angularVelocity += angularVelocityDelta;
                    }

                    if(useCenterObject)
                    {
                        //Quaternion q = Quaternion.AngleAxis(delta, Vector3.forward);
                        //rb[i].MovePosition(q * (targets[i].position - centerPosition) + centerPosition);
                        //rb[i].MoveRotation(targets[i].rotation * q);

                        Vector2 vector1 = rb[i].position - (Vector2)centerPosition;
                        Vector2 vector2 = vector1.Rotate(delta);
                        Vector2 point1 = vector1 + (Vector2)centerPosition;
                        Vector2 point2 = vector2 + (Vector2)centerPosition;
                        Vector2 direction = point2 - point1;

                        velocity0[i] = velocity1[i];
                        velocity1[i] = direction / Time.fixedDeltaTime;
                        Vector2 velocityDelta = velocity1[i] - velocity0[i];
                        velocityDeltaTotal[i] += velocityDelta;

                        rb[i].velocity += velocityDelta;
                    }
                }
                else
                {
                    totalDisplacement[i] += delta;

                    if(useCenterObject)
                    {
                        targets[i].RotateAround(centerPosition, Vector3.forward, delta);
                        if (lockRotation) { targets[i].Rotate(0, 0, -delta); }
                    }
                    else
                    {
                        targets[i].Rotate(0, 0, delta);
                    }
                }
            }
        }

        //if (hasRigidBody) { yield return new WaitForFixedUpdate(); }
        //else { yield return null; }

        for (int i = 0; i < targets.Count; i++)
        {
            totalRotateAmount[i] = thisRotateAmount[i];
            float difference = totalRotateAmount[i] - totalDisplacement[i];
            
            if (useCenterObject)
            {
                if (updateCenterPosition) { centerPosition = centerObject.position; }
                targets[i].RotateAround(centerPosition, Vector3.forward, difference);
                if (lockRotation) { targets[i].Rotate(0, 0, -difference); }
            }
            else
            {
                targets[i].Rotate(0, 0, difference);
            }

            if (hasRigidBody)
            {
                if(!(useCenterObject && lockRotation))
                {
                    rb[i].angularVelocity -= angularVelocityDeltaTotal[i];
                }
                
                if(useCenterObject)
                {
                    //Quaternion q = Quaternion.AngleAxis(difference, Vector3.forward);
                    //rb[i].MovePosition(q * (targets[i].position - centerPosition) + centerPosition);

                    rb[i].velocity -= velocityDeltaTotal[i];
                }
            }
        }

        if (loop && loopDelay > 0)
        {
            yield return new WaitForSeconds(loopDelay);
        }

        inUse = false;

        if (loop) { Rotate(); }
    }

    public bool IsFinished()
    {
        return !inUse;
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
        if (other.tag == "Player" && !stayToRotate)
        {
            player.AddRotateTriggers(this);
            Rotate();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "Player" && stayToRotate)
        {
            float rev = clockwise ? -1 : 1;
            for (int i = 0; i < targets.Count; i++)
            {
                if (hasRigidBody)
                {
                    if (!useCenterObject)
                    {
                        if ((rb[i].angularVelocity < rev * rotateSpeed && Mathf.Sign(rev * rotateSpeed) == 1) || (rb[i].angularVelocity > rev * rotateSpeed && Mathf.Sign(rev * rotateSpeed) == -1))
                        {
                            float difference = rev * rotateSpeed - rb[i].angularVelocity;
                            rb[i].angularVelocity += difference;
                        }
                    }
                    else
                    {
                        Quaternion q = Quaternion.AngleAxis(rev * rotateSpeed * Time.fixedDeltaTime, Vector3.forward);
                        rb[i].MovePosition(q * (targets[i].position - centerObject.position) + centerObject.position);
                        if (!lockRotation) { rb[i].MoveRotation(targets[i].rotation * q); }
                    }
                }
                else
                {
                    if(useCenterObject)
                    {
                        targets[i].RotateAround(centerObject.position, Vector3.forward, rev * rotateSpeed * Time.deltaTime);
                        if (lockRotation) { targets[i].Rotate(0, 0, -rev * rotateSpeed * Time.deltaTime); }
                    }
                    else
                    {
                        targets[i].Rotate(0, 0, rev * rotateSpeed * Time.deltaTime);
                    }
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player" && stayToRotate)
        {
            float rev = clockwise ? -1 : 1;
            for (int i = 0; i < targets.Count; i++)
            {
                if (hasRigidBody)
                {
                    if (!useCenterObject)
                    {
                        if (Mathf.Sign(rb[i].angularVelocity) == 1)
                        {
                            rb[i].angularVelocity -= Mathf.Min(Mathf.Abs(rotateSpeed), rb[i].angularVelocity);
                        }
                        else if (Mathf.Sign(rb[i].angularVelocity) == -1)
                        {
                            rb[i].angularVelocity -= Mathf.Max(-Mathf.Abs(rotateSpeed), rb[i].angularVelocity);
                        }
                    }
                }
            }
        }
    }

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

            if (centerObject != null && useCenterObject)
            {
                Gizmos.color = new Color(1f, 1f, .6f);
                Gizmos.DrawLine(tf.position, centerObject.position);
            }
        }
    }
#endif
}