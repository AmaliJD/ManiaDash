using System.Collections;
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
    public Transform target;
    public Transform destinationObject;

    [Header("Move Properites")]
    public Vector2 moveAmount;
    public float moveScale = 1;
    public float duration;

    [Header("Mods")]
    [Range(-180, 180)]
    public float rotateMoveDirection;
    public float multiplyMoveAmount = 1;

    [Header("Easing")]
    public AnimationCurve easing;
    public EasingFunction.Ease easeType;
    public EasingOption easeOption;
    public Ease tweenEaseType;

    [Header("Properties")]
    public bool waitToFinish = true;
    public bool useRigidbody = false;
    public bool local = false;
    public bool useDestinationObject = false;
    public bool updateDestinationDistance = false;
    public bool useTweening = false;
    public bool invertModRotate = false;
    public bool invertModMultiply = false;
    public bool loop = false;

    [Min(0)]
    public float loopDelay;

    [Min(-1)]
    public int triggerLimit = -1;

    private Rigidbody2D rb;
    private bool hasRigidBody = false;
    private int triggerCount = 0;
    private Vector3 startPosition;
    private bool inUse = false;

    public void Awake()
    {
        rb = target.GetComponent<Rigidbody2D>();
        hasRigidBody = (useRigidbody && rb != null);
        startPosition = target.position;

        if (useDestinationObject && destinationObject == null)
        {
            moveAmount = Vector2.zero;
            useDestinationObject = false;
        }

        if(local && target.parent == null)
        {
            local = false;
        }
    }

    public void Move()
    {
        if (waitToFinish && inUse) { return; }

        if (triggerLimit == -1 || triggerCount < triggerLimit)
        {
            triggerCount++;

            if (useDestinationObject)
            {
                if (hasRigidBody)
                {
                    rb.velocity = Vector2.zero;
                }
                StopAllCoroutines();
            }

            if (useTweening)
            {
                MoveTween();
            }
            else
            {
                StartCoroutine(MoveCoroutine());
            }
        }
    }

    public void Reset()
    {
        target.position = startPosition;
        if (hasRigidBody)
        {
            rb.velocity = Vector2.zero;
        }
        StopAllCoroutines();
        triggerCount = 0;
    }

    private void MoveTween()
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
                /*var tween = target.DOMove(destinationObject.position + (Vector3)moveAmount * moveScale, duration).SetEase(tweenEaseType)
                .OnComplete(() =>
                {
                    inUse = false;
                });

                if (updateDestinationDistance)
                {
                    tween.OnUpdate(() =>
                    {
                        tween.ChangeEndValue(destinationObject.position + (Vector3)moveAmount * moveScale, true);
                    });
                }*/

                //DOTween.To(() => target.position, x => target.DOMove(x, duration), destinationObject.position, duration);
                Debug.Log("yo yo yo");
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
    }


    private IEnumerator MoveCoroutine()
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
}