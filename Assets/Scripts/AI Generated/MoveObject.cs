using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum EasingOption
{
    AnimationCurve,
    EasingFunction
}

public enum SpeedGizmo
{
    x0, x1, x2, x3, x4
}

public class MoveObject : MonoBehaviour
{
    [Header("Target Objects")]
    public List<int> groupIDs;
    public List<Transform> targets;
    public Transform destinationObject;
    public bool useDestinationObject = false;
    public bool updateDestinationDistance = false;

    [Header("Target Objects")]
    [Min(0)]
    public float delay;
    [Min(0)]
    public float requiredManaCount;

    [Header("Move Properites")]
    public Vector2 moveAmount;
    public float moveScale = 1;
    public float duration;

    [Header("Stay Move Properites")]
    public bool stayToMove = false;
    public float moveAngle;
    public float moveSpeed;

    [Header("Mods")]
    [Range(-180, 180)]
    public float rotateMoveDirection;
    public float multiplyMoveAmount = 1;
    public bool applyModPerTarget;
    public bool reverseOrderPerTarget;
    public bool invertModRotate = false;
    public bool invertModMultiply = false;
    public bool reverseModMultiply = false;

    [Header("Easing")]
    public AnimationCurve easing;
    public EasingFunction.Ease easeType;
    public EasingOption easeOption;

    [Header("Properties")]
    public bool waitToFinish = true;
    public bool useRigidbody = false;
    public bool local = false;
    
    public bool loop = false;

    [Min(0)]
    public float loopDelay;

    [Min(-1)]
    public int triggerLimit = -1;

    [Header("Settings")]
    public bool playOnAwake;
    public bool paused = false;
    public bool stopped = false;
    public bool hideIcon;
    public SpeedGizmo speedGizmo = SpeedGizmo.x1;

    private GameObject texture;

    private List<Rigidbody2D> rb;
    private bool hasRigidBody = false;
    private List<bool> hasParent;
    private int triggerCount = 0;
    private List<Vector3> startPosition;
    private bool inUse = false;

    private float initialRotateMoveDirection;
    private float initialMultiplyMoveAmount;

    private GroupIDManager groupIDManager;
    private GameManager gamemanager;

    private void Awake()
    {
        gamemanager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        texture = transform.GetChild(0).gameObject;
        texture.SetActive(!hideIcon);
    }

    public void Start()
    {
        if (groupIDs.Count > 0)
        {
            groupIDManager = GameObject.FindGameObjectWithTag("Master").GetComponent<GroupIDManager>();
            foreach(int i in groupIDs)
            {
                targets.AddRange(groupIDManager.groupIDList[i].ConvertAll(x => x.transform));
            }
            targets = targets.Distinct().ToList();
        }

        if(targets.Count == 0) { enabled = false; return; }

        bool nullRB = false;
        rb = new List<Rigidbody2D>();
        hasParent = new List<bool>();
        startPosition = new List<Vector3>();
        foreach(Transform tr in targets)
        {
            rb.Add(tr.GetComponent<Rigidbody2D>());
            if(tr.GetComponent<Rigidbody2D>() == null) { nullRB = true; }
            startPosition.Add(tr.position);
            hasParent.Add(local && tr.parent != null);
        }

        hasRigidBody = useRigidbody && !nullRB;

        if (useDestinationObject && destinationObject == null)
        {
            moveAmount = Vector2.zero;
            useDestinationObject = false;
        }

        initialRotateMoveDirection = rotateMoveDirection;
        initialMultiplyMoveAmount = multiplyMoveAmount;

        if(playOnAwake && !stayToMove)
        {
            Move();
        }
    }

    public void Move()
    {
        if ((waitToFinish && inUse) || requiredManaCount > gamemanager.getManaCount()) { return; }
        
        if (triggerLimit == -1 || triggerCount < triggerLimit)
        {
            stopped = false;
            triggerCount++;

            if (useDestinationObject)
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    if (hasRigidBody)
                    {
                        rb[i].velocity = Vector2.zero;
                    }
                }
                StopAllCoroutines();
            }

            StartCoroutine(MoveCoroutine());
        }
    }

    public void ResetTrigger()
    {
        for (int i = 0; i < targets.Count; i++)
        {
            targets[i].position = startPosition[i];
            if (hasRigidBody)
            {
                rb[i].velocity = Vector2.zero;
            }
        }
        StopAllCoroutines();
        triggerCount = 0;

        rotateMoveDirection = initialRotateMoveDirection;
        multiplyMoveAmount = initialMultiplyMoveAmount;

        paused = false;
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

    private IEnumerator MoveCoroutine()
    {
        inUse = true;
        if(delay > 0) { yield return new WaitForSeconds(delay); }

        float elapsedTime = 0;
        Vector2[] velocity0 = new Vector2[targets.Count];
        Vector2[] velocity1 = new Vector2[targets.Count];
        Vector2[] velocityDeltaTotal = new Vector2[targets.Count];
        Vector2[] totalDisplacement = new Vector2[targets.Count];
        Vector2[] lastDestinationPosition = destinationObject != null ? Enumerable.Repeat((Vector2)destinationObject.position, targets.Count).ToArray() : new Vector2[0];
        Vector2[] thisMoveAmount = Enumerable.Repeat(moveAmount, targets.Count).ToArray();
        Vector2[] totalMoveAmount = new Vector2[targets.Count];

        if (applyModPerTarget && targets.Count > 1)
        {
            if (!reverseOrderPerTarget)
            {
                float tempRotateMoveDirection = rotateMoveDirection, tempMultiplyMoveAmount = multiplyMoveAmount;
                for (int i = 1; i < targets.Count; i++)
                {
                    thisMoveAmount[i] = thisMoveAmount[i - 1].Rotate(-rotateMoveDirection);
                    thisMoveAmount[i] *= multiplyMoveAmount;
                    if (invertModRotate)
                    {
                        rotateMoveDirection = -rotateMoveDirection;
                    }
                    if (invertModMultiply)
                    {
                        multiplyMoveAmount = 1 / multiplyMoveAmount;
                    }
                    if (reverseModMultiply)
                    {
                        multiplyMoveAmount *= -1;
                    }
                }
                rotateMoveDirection = tempRotateMoveDirection;
                multiplyMoveAmount = tempMultiplyMoveAmount;
            }
            else
            {
                float tempRotateMoveDirection = rotateMoveDirection, tempMultiplyMoveAmount = multiplyMoveAmount;
                for (int i = targets.Count-1; i > 0; i--)
                {
                    thisMoveAmount[i] = thisMoveAmount[(i + 1) % targets.Count].Rotate(-rotateMoveDirection);
                    thisMoveAmount[i] *= multiplyMoveAmount;
                    if (invertModRotate)
                    {
                        rotateMoveDirection = -rotateMoveDirection;
                    }
                    if (invertModMultiply)
                    {
                        multiplyMoveAmount = 1 / multiplyMoveAmount;
                    }
                    if (reverseModMultiply)
                    {
                        multiplyMoveAmount *= -1;
                    }
                }
                rotateMoveDirection = tempRotateMoveDirection;
                multiplyMoveAmount = tempMultiplyMoveAmount;
            }
        }

        Vector2[] moveAmountToDestination = new Vector2[targets.Count];
        Vector2[] localStartPosition = targets.ConvertAll(x => (Vector2)x.localPosition).ToArray();
        Vector2[] localEndPosition = targets.ConvertAll(x => (Vector2)x.localPosition + thisMoveAmount[targets.IndexOf(x)] * moveScale).ToArray();
        Vector2[] localToWorldDirection = targets.ConvertAll(x => (Vector2)x.TransformPoint((Vector2)x.localPosition + thisMoveAmount[targets.IndexOf(x)] * moveScale) - (Vector2)x.localPosition).ToArray();

        moveAmount = moveAmount.Rotate(-rotateMoveDirection);
        moveAmount *= multiplyMoveAmount;
        if (invertModRotate)
        {
            rotateMoveDirection = -rotateMoveDirection;
        }
        if (invertModMultiply)
        {
            multiplyMoveAmount = 1 / multiplyMoveAmount;
        }
        if (reverseModMultiply)
        {
            multiplyMoveAmount *= -1;
        }

        if (useDestinationObject)
        {
            moveAmountToDestination = targets.ConvertAll(x => (Vector2)(destinationObject.position - x.position)).ToArray();
        }

        while (elapsedTime < duration)
        {
            if(stopped)
            {
                if (hasRigidBody)
                {
                    for (int i = 0; i < targets.Count; i++)
                    {
                        rb[i].velocity -= velocityDeltaTotal[i];
                    }
                }

                rotateMoveDirection = initialRotateMoveDirection;
                multiplyMoveAmount = initialMultiplyMoveAmount;

                inUse = false;
                yield break;
            }

            if(paused)
            {
                if (hasRigidBody)
                {
                    for (int i = 0; i < targets.Count; i++)
                    {
                        rb[i].velocity -= velocityDeltaTotal[i];
                    }
                }
                yield return new WaitUntil(() => !paused);
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
            elapsedTime += (hasRigidBody) ? Time.fixedDeltaTime : Time.deltaTime;
            float t1 = Mathf.Clamp01(elapsedTime / duration);
            float easeT0, easeT1;

            easeT0 = GetEaseValue(t0);
            easeT1 = GetEaseValue(t1);

            for (int i = 0; i < targets.Count; i++)
            {
                if (useDestinationObject && updateDestinationDistance)
                {
                    Vector2 destinationDelta = (Vector2)destinationObject.position - lastDestinationPosition[i];
                    lastDestinationPosition[i] = destinationObject.position;
                    moveAmountToDestination[i] += destinationDelta;
                }

                totalMoveAmount[i] = thisMoveAmount[i] + moveAmountToDestination[i] * moveScale;

                Vector2 delta = totalMoveAmount[i] * (easeT1 - easeT0);
                if (hasRigidBody)
                {
                    if (hasParent[i])
                    {
                        delta = new Vector2(delta.x * targets[i].parent.lossyScale.x, delta.y * targets[i].parent.lossyScale.y);
                    }

                    velocity0[i] = velocity1[i];
                    velocity1[i] = delta / Time.fixedDeltaTime;
                    Vector2 velocityDelta = velocity1[i] - velocity0[i];
                    totalDisplacement[i] += velocity1[i] * Time.fixedDeltaTime;

                    if (useDestinationObject && updateDestinationDistance)
                    {
                        Vector2 expectedDelta = totalMoveAmount[i] * (easeT1 - GetEaseValue(0));
                        Vector2 errorDelta = expectedDelta - totalDisplacement[i];
                        totalDisplacement[i] += errorDelta;

                        velocity1[i] = (delta + errorDelta) / Time.fixedDeltaTime;
                        velocityDelta = velocity1[i] - velocity0[i];
                    }

                    if (hasParent[i])
                    {
                        rb[i].velocity -= velocityDeltaTotal[i];
                        velocityDelta = velocityDelta.Rotate(-Vector2.SignedAngle(targets[i].parent.right, Vector2.right));
                        velocityDeltaTotal[i] = velocityDeltaTotal[i].Rotate(-Vector2.SignedAngle(targets[i].TransformPoint(localEndPosition[i]) - targets[i].TransformPoint(localStartPosition[i]), localToWorldDirection[i]));
                        rb[i].velocity += velocityDeltaTotal[i];

                        localToWorldDirection[i] = targets[i].TransformPoint(localEndPosition[i]) - targets[i].TransformPoint(localStartPosition[i]);
                    }

                    velocityDeltaTotal[i] += velocityDelta;
                    rb[i].velocity += velocityDelta;
                }
                else
                {
                    totalDisplacement[i] += delta;

                    if (useDestinationObject && updateDestinationDistance)
                    {
                        Vector2 expectedDelta = totalMoveAmount[i] * (easeT1 - GetEaseValue(0));
                        Vector2 errorDelta = expectedDelta - totalDisplacement[i];
                        totalDisplacement[i] += errorDelta;
                        delta += errorDelta;
                    }

                    if (hasParent[i])
                    {
                        targets[i].localPosition += (Vector3)delta;
                    }
                    else
                    {
                        targets[i].position += (Vector3)delta;
                    }
                }
            }

            if (hasRigidBody) { yield return new WaitForFixedUpdate(); }
            else { yield return null; }
        }

        for (int i = 0; i < targets.Count; i++)
        {
            if (useDestinationObject && updateDestinationDistance)
            {
                Vector2 destinationDelta = (Vector2)destinationObject.position - lastDestinationPosition[i];
                lastDestinationPosition[i] = destinationObject.position;
                moveAmountToDestination[i] += destinationDelta;
            }

            totalMoveAmount[i] = thisMoveAmount[i] + moveAmountToDestination[i] * moveScale;
            Vector2 difference = totalMoveAmount[i] - totalDisplacement[i];
            if (hasParent[i])
            {
                if (hasRigidBody)
                {
                    totalMoveAmount[i] = new Vector2(thisMoveAmount[i].x * targets[i].parent.lossyScale.x, thisMoveAmount[i].y * targets[i].parent.lossyScale.y) + moveAmountToDestination[i] * moveScale;
                    difference = totalMoveAmount[i] - totalDisplacement[i];
                }
                targets[i].localPosition += (Vector3)difference;
            }
            else
            {
                targets[i].position += (Vector3)difference;
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

        if (loop) { Move(); }
    }

    private float GetEaseValue(float t)
    {
        float ease;
        switch (easeOption)
        {
            case EasingOption.AnimationCurve:
                ease = easing.Evaluate(t);
                break;
            case EasingOption.EasingFunction:
                ease = EasingFunction.GetEasingFunction(easeType)(0, 1, t);
                break;
            default:
                ease = 0;
                break;
        }

        return ease;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            Move();
        }
    }

    // NEED TO ADD LOCAL
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "Player" && stayToMove)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                if(hasRigidBody)
                {
                    if(rb[i].velocity.magnitude < moveSpeed)
                    {
                        Vector2 difference = Vector2.right.Rotate(-moveAngle) * moveSpeed - rb[i].velocity;
                        rb[i].velocity += difference;
                    }
                }
                else
                {
                    targets[i].position += (Vector3)Vector2.right.Rotate(-moveAngle) * moveSpeed * Time.deltaTime;
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player" && stayToMove)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                if (hasRigidBody)
                {
                    rb[i].velocity -= Vector2.right.Rotate(-moveAngle) * Mathf.Min(moveSpeed, rb[i].velocity.magnitude);
                }
            }
        }
    }

    private void OnValidate()
    {
        if(texture != null && Application.isPlaying)
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

        Gizmos.color = new Color(1,1,1,.25f);
        Gizmos.DrawLine(transform.position, transform.position + delayPos);

        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position + delayPos, transform.position + durationPos);

        foreach(Transform tf in targets)
        {
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