using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MoveSequencer : MonoBehaviour
{
    [Header("Start Delay")]
    [Min(0)]
    public float startDelay;
    private WaitForSeconds waitStartDelay;

    public enum TriggerType { Move, Rotate, Scale}

    [System.Serializable]
    public struct MoveData
    {
        public TriggerType triggerType;
        public ScaleObject.ScaleMode scaleType;
        public float startTime;
        public bool addPrevDurationToStartTime;
        public Vector2 value;
        public float duration;
        public EasingFunction.Ease easing;
    }

    [Header("Move Data")]
    public List<MoveData> moves;

    [Header("End Delay")]
    [Min(0)]
    public float endDelay;
    private WaitForSeconds waitEndDelay;

    [Header("Properties")]
    public bool reverseValue;
    public bool useRigidBody;
    public bool loop;

    private MoveObject moveObjectInstance;
    private RotateObject rotateObjectInstance;
    private ScaleObject scaleObjectInstance;

    GameObject moveObjectHolder;
    GameObject rotateObjectHolder;
    GameObject scaleObjectHolder;
    bool started = false;

    private void Start()
    {
        waitStartDelay = new WaitForSeconds(startDelay);
        waitEndDelay = new WaitForSeconds(endDelay);
        //moves = moves.OrderBy(x => x.startTime).ToList();

        StartSequence();
    }

    public void StartSequence()
    {
        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        if(startDelay > 0 && !started)
        {
            started = true;
            yield return waitStartDelay;
        }

        
        float beginTime = Time.time;
        for (int i = 0; i < moves.Count(); i++)
        {
            yield return new WaitWhile(() => Time.time - beginTime < moves[i].startTime + (i > 0 ? moves[i].addPrevDurationToStartTime ? moves[i-1].duration : 0 : 0));

            switch(moves[i].triggerType)
            {
                case TriggerType.Move:
                    if(moveObjectHolder == null)
                    {
                        moveObjectHolder = new GameObject();
                        moveObjectHolder.name = "Move Trigger Holder " + moveObjectHolder.GetHashCode();
                        moveObjectHolder.transform.parent = transform;
                        moveObjectInstance = moveObjectHolder.AddComponent<MoveObject>();
                        moveObjectInstance.targets = new List<Transform>();
                        moveObjectInstance.groupIDs = new List<int>();
                        moveObjectInstance.targets.Add(transform);
                        moveObjectInstance.useRigidbody = useRigidBody;
                        moveObjectInstance.easeOption = EasingOption.EasingFunction;
                        moveObjectInstance.waitToFinish = false;
                        moveObjectInstance.Init();
                    }

                    moveObjectInstance.moveAmount = moves[i].value;
                    moveObjectInstance.duration = moves[i].duration;
                    moveObjectInstance.functionEasing = moves[i].easing;
                    moveObjectInstance.Move();
                    break;

                case TriggerType.Rotate:
                    if (rotateObjectHolder == null)
                    {
                        rotateObjectHolder = new GameObject();
                        rotateObjectHolder.name = "Rotate Trigger Holder " + rotateObjectHolder.GetHashCode();
                        rotateObjectHolder.transform.parent = transform;
                        rotateObjectInstance = rotateObjectHolder.AddComponent<RotateObject>();
                        rotateObjectInstance.targets = new List<Transform>();
                        rotateObjectInstance.groupIDs = new List<int>();
                        rotateObjectInstance.targets.Add(transform);
                        rotateObjectInstance.useRigidbody = useRigidBody;
                        rotateObjectInstance.easeOption = EasingOption.EasingFunction;
                        rotateObjectInstance.waitToFinish = false;
                        rotateObjectInstance.Init();
                    }

                    rotateObjectInstance.rotateAmount = moves[i].value.x;
                    rotateObjectInstance.duration = moves[i].duration;
                    rotateObjectInstance.functionEasing = moves[i].easing;
                    rotateObjectInstance.Rotate();
                    break;

                case TriggerType.Scale:
                    if (scaleObjectHolder == null)
                    {
                        scaleObjectHolder = new GameObject();
                        scaleObjectHolder.name = "Scale Trigger Holder " + scaleObjectHolder.GetHashCode();
                        scaleObjectHolder.transform.parent = transform;
                        scaleObjectInstance = scaleObjectHolder.AddComponent<ScaleObject>();
                        scaleObjectInstance.targets = new List<Transform>();
                        scaleObjectInstance.groupIDs = new List<int>();
                        scaleObjectInstance.targets.Add(transform);
                        scaleObjectInstance.useRigidbody = useRigidBody;
                        scaleObjectInstance.easeOption = EasingOption.EasingFunction;
                        scaleObjectInstance.waitToFinish = false;
                        scaleObjectInstance.Init();
                    }

                    scaleObjectInstance.scaleMode = moves[i].scaleType;
                    scaleObjectInstance.scaleValue = moves[i].value;
                    scaleObjectInstance.duration = moves[i].duration;
                    scaleObjectInstance.functionEasing = moves[i].easing;
                    scaleObjectInstance.Scale();
                    break;
            }
        }


        if (endDelay > 0)
        {
            yield return waitEndDelay;
        }

        if(loop)
        {
            StartSequence();
        }
    }
}
