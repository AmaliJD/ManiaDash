using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public enum EasingOption
{
    AnimationCurve,
    EasingFunction
}

public class MoveObject : MonoBehaviour
{
    [Header("Target Objects")]
    //public Transform target;
    public List<int> groupIDs;
    public List<Transform> targets;
    public Transform destinationObject;

    [Header("Move Properites")]
    public Vector2 moveAmount;
    public float moveScale = 1;
    public float duration;

    [Header("Mods")]
    [Range(-180, 180)]
    public float rotateMoveDirection;
    public float multiplyMoveAmount = 1;
    public bool applyModPerTarget;
    public bool reverseOrderPerTarget;

    [Header("Easing")]
    public AnimationCurve easing;
    public EasingFunction.Ease easeType;
    public EasingOption easeOption;
    //public Ease tweenEaseType;

    [Header("Properties")]
    public bool waitToFinish = true;
    public bool useRigidbody = false;
    public bool local = false;
    public bool useDestinationObject = false;
    public bool updateDestinationDistance = false;
    //public bool useTweening = false;
    public bool invertModRotate = false;
    public bool invertModMultiply = false;
    public bool reverseModMultiply = false;
    public bool loop = false;

    [Min(0)]
    public float loopDelay;

    [Min(-1)]
    public int triggerLimit = -1;

    //private Rigidbody2D rb;
    private List<Rigidbody2D> rb;
    private bool hasRigidBody = false;
    //private List<bool> hasRigidBody;
    private List<bool> hasParent;
    private int triggerCount = 0;
    //private Vector3 startPosition;
    private List<Vector3> startPosition;
    private bool inUse = false;

    private float initialRotateMoveDirection;
    private float initialMultiplyMoveAmount;

    private GroupIDManager groupIDManager;

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

        //rb = target.GetComponent<Rigidbody2D>();
        //hasRigidBody = (useRigidbody && rb != null);
        //startPosition = target.position;
        bool nullRB = false;
        rb = new List<Rigidbody2D>();
        //hasRigidBody = new List<bool>();
        hasParent = new List<bool>();
        startPosition = new List<Vector3>();
        foreach(Transform tr in targets)
        {
            rb.Add(tr.GetComponent<Rigidbody2D>());
            if(tr.GetComponent<Rigidbody2D>() == null) { nullRB = true; }
            //hasRigidBody.Add(useRigidbody && tr.GetComponent<Rigidbody2D>() != null);
            startPosition.Add(tr.position);
            hasParent.Add(local && tr.parent != null);
        }

        hasRigidBody = useRigidbody && !nullRB;

        if (useDestinationObject && destinationObject == null)
        {
            moveAmount = Vector2.zero;
            useDestinationObject = false;
        }

        /*if(local && target.parent == null)
        {
            local = false;
        }*/

        initialRotateMoveDirection = rotateMoveDirection;
        initialMultiplyMoveAmount = multiplyMoveAmount;
    }

    public void Move()
    {
        if (waitToFinish && inUse) { return; }

        if (triggerLimit == -1 || triggerCount < triggerLimit)
        {
            triggerCount++;

            if (useDestinationObject)
            {
                /*if (hasRigidBody)
                {
                    rb.velocity = Vector2.zero;
                }*/
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

            /*if (useTweening)
            {
                MoveTween();
            }
            else
            {
                StartCoroutine(MoveCoroutine());
            }*/
        }
    }

    public void Reset()
    {
        /*target.position = startPosition;
        if (hasRigidBody)
        {
            rb.velocity = Vector2.zero;
        }*/
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
    }

    /*private void MoveTween()
    {
        inUse = true;

        if (!hasRigidBody)
        {
            if (!useDestinationObject)
            {
                target.DOBlendableMoveBy(target.position + (Vector3)moveAmount * moveScale, duration).SetEase(tweenEaseType)
                .OnComplete(() =>
                {
                    inUse = false;
                });
            }
            else
            {
                target.DOMove(destinationObject, (Vector3)moveAmount, moveScale, duration).SetEase(tweenEaseType)
                .OnComplete(() =>
                {
                    inUse = false;
                });
            }
        }
        else
        {
            if (!useDestinationObject)
            {
                rb.DOMove(rb.position + moveAmount * moveScale, duration).SetEase(tweenEaseType)
                .OnComplete(() =>
                {
                    inUse = false;
                });
            }
            else
            {
                var tween = rb.DOMove(destinationObject.position + (Vector3)moveAmount * moveScale, duration).SetEase(tweenEaseType)
                .OnComplete(() =>
                {
                    inUse = false;
                });

                if (updateDestinationDistance)
                {
                    tween.OnStepComplete(() =>
                    {
                        tween.ChangeEndValue(destinationObject.position + (Vector3)moveAmount * moveScale, true);
                    });
                }
            }
        }
    }*/

    private IEnumerator MoveCoroutine()
    {
        float elapsedTime = 0;
        Vector2[] velocity0 = new Vector2[targets.Count];
        Vector2[] velocity1 = new Vector2[targets.Count];
        Vector2[] velocityDeltaTotal = new Vector2[targets.Count];
        Vector2[] totalDisplacement = new Vector2[targets.Count];
        Vector2[] lastDestinationPosition = Enumerable.Repeat((Vector2)destinationObject.position, targets.Count).ToArray(); //destinationObject.position;
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
        Vector2[] localEndPosition = targets.ConvertAll(x => (Vector2)x.localPosition + thisMoveAmount[targets.IndexOf(x)] * moveScale).ToArray(); //(Vector2)target.localPosition + thisMoveAmount * moveScale;
        Vector2[] localToWorldDirection = targets.ConvertAll(x => (Vector2)x.TransformPoint((Vector2)x.localPosition + thisMoveAmount[targets.IndexOf(x)] * moveScale) - (Vector2)x.localPosition).ToArray(); //target.TransformPoint(localEndPosition) - target.TransformPoint(localStartPosition);

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
            moveAmountToDestination = targets.ConvertAll(x => (Vector2)(destinationObject.position - x.position)).ToArray();//destinationObject.position - target.position;
        }

        while (elapsedTime < duration)
        {
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
                    //yield return new WaitForFixedUpdate();
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

                    //yield return null;
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

    /*private IEnumerator MoveCoroutine()
    {
        float elapsedTime = 0;
        Vector2 velocity0 = Vector2.zero;
        Vector2 velocity1 = Vector2.zero;
        Vector2 velocityDeltaTotal = Vector2.zero;
        Vector2 totalDisplacement = Vector2.zero;
        Vector2 lastDestinationPosition = destinationObject.position;
        Vector2 thisMoveAmount = moveAmount;
        Vector2 totalMoveAmount = Vector2.zero;
        Vector2 moveAmountToDestination = Vector2.zero;
        Vector2 localStartPosition = target.localPosition;
        Vector2 localEndPosition = (Vector2)target.localPosition + thisMoveAmount * moveScale;
        Vector2 localToWorldDirection = target.TransformPoint(localEndPosition) - target.TransformPoint(localStartPosition);

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

        if (useDestinationObject)
        {
            moveAmountToDestination = destinationObject.position - target.position;
        }

        while (elapsedTime < duration)
        {
            inUse = true;

            if (useDestinationObject && updateDestinationDistance)
            {
                Vector2 destinationDelta = (Vector2)destinationObject.position - lastDestinationPosition;
                lastDestinationPosition = destinationObject.position;
                moveAmountToDestination += destinationDelta;
            }

            totalMoveAmount = thisMoveAmount + moveAmountToDestination * moveScale;

            float t0 = Mathf.Clamp01(elapsedTime / duration);
            elapsedTime += (hasRigidBody) ? Time.fixedDeltaTime : Time.deltaTime;
            float t1 = Mathf.Clamp01(elapsedTime / duration);
            float easeT0, easeT1;

            easeT0 = GetEaseValue(t0);
            easeT1 = GetEaseValue(t1);

            Vector2 delta = totalMoveAmount * (easeT1 - easeT0);
            if (hasRigidBody)
            {
                if (local)
                {
                    delta = new Vector2(delta.x * target.parent.lossyScale.x, delta.y * target.parent.lossyScale.y);
                }

                velocity0 = velocity1;
                velocity1 = delta / Time.fixedDeltaTime;
                Vector2 velocityDelta = velocity1 - velocity0;
                totalDisplacement += velocity1 * Time.fixedDeltaTime;

                if (useDestinationObject && updateDestinationDistance)
                {
                    Vector2 expectedDelta = totalMoveAmount * (easeT1 - GetEaseValue(0));
                    Vector2 errorDelta = expectedDelta - totalDisplacement;
                    totalDisplacement += errorDelta;

                    velocity1 = (delta + errorDelta) / Time.fixedDeltaTime;
                    velocityDelta = velocity1 - velocity0;
                }
                

                if (local)
                {
                    rb.velocity -= velocityDeltaTotal;
                    velocityDelta = velocityDelta.Rotate(-Vector2.SignedAngle(target.parent.right, Vector2.right));
                    velocityDeltaTotal = velocityDeltaTotal.Rotate(-Vector2.SignedAngle(target.TransformPoint(localEndPosition) - target.TransformPoint(localStartPosition), localToWorldDirection));
                    rb.velocity += velocityDeltaTotal;

                    localToWorldDirection = target.TransformPoint(localEndPosition) - target.TransformPoint(localStartPosition);
                }

                velocityDeltaTotal += velocityDelta;
                rb.velocity += velocityDelta;
                yield return new WaitForFixedUpdate();
            }
            else
            {
                totalDisplacement += delta;

                if (useDestinationObject && updateDestinationDistance)
                {
                    Vector2 expectedDelta = totalMoveAmount * (easeT1 - GetEaseValue(0));
                    Vector2 errorDelta = expectedDelta - totalDisplacement;
                    totalDisplacement += errorDelta;
                    delta += errorDelta;
                }

                if(local)
                {
                    target.localPosition += (Vector3)delta;
                }
                else
                {
                    target.position += (Vector3)delta;
                }
                
                yield return null;
            }
        }

        if (useDestinationObject && updateDestinationDistance)
        {
            Vector2 destinationDelta = (Vector2)destinationObject.position - lastDestinationPosition;
            lastDestinationPosition = destinationObject.position;
            moveAmountToDestination += destinationDelta;
        }

        totalMoveAmount = thisMoveAmount + moveAmountToDestination * moveScale;
        Vector2 difference = totalMoveAmount - totalDisplacement;
        if (local)
        {
            if(hasRigidBody)
            {
                totalMoveAmount = new Vector2(thisMoveAmount.x * target.parent.lossyScale.x, thisMoveAmount.y * target.parent.lossyScale.y) + moveAmountToDestination * moveScale;
                difference = totalMoveAmount - totalDisplacement;
            }
            target.localPosition += (Vector3)difference;
        }
        else
        {
            target.position += (Vector3)difference;
        }

        if (hasRigidBody)
        {
            rb.velocity -= velocityDeltaTotal;
        }

        if(loop && loopDelay > 0)
        {
            yield return new WaitForSeconds(loopDelay);
        }

        inUse = false;

        if (loop) { Move(); }
    }*/

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
}