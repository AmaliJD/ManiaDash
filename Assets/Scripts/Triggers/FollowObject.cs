using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TransformData
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale, localScale;

    public TransformData(Vector3 pos, Quaternion rot, Vector3 scl)
    {
        position = pos;
        rotation = rot;
        scale = scl;
    }

    public TransformData(Transform tr, bool local = false)
    {
        //if(tr.parent == null) { local = false; }

        position = local ? tr.localPosition : tr.position;
        rotation = local ? tr.localRotation : tr.rotation;
        scale = local || (tr.parent == null) ? tr.localScale : tr.lossyScale;
        localScale = tr.localScale;
    }

    public void SetData(Transform tr, bool local = false)
    {
        //if (tr.parent == null) { local = false; }

        position = local ? tr.localPosition : tr.position;
        rotation = local ? tr.localRotation : tr.rotation;
        scale = local || (tr.parent == null) ? tr.localScale : tr.lossyScale;
        localScale = tr.localScale;
    }

    public override string ToString()
    {
        return "Position: " + position + "    Rotation: " + rotation.eulerAngles + "    Scale: " + scale;
    }
}

public class FollowObject : MonoBehaviour
{
    [Header("Target Objects")]
    public List<int> groupIDs;
    public List<Transform> targets;
    public Transform followObject;

    [Header("Conditions")]
    [Min(0)]
    public float requiredManaCount;
    [Min(0)]
    public float delay;
    private float delayTimer = 0;

    [Min(0)]
    public float duration;
    private float time = 0;

    [Min(0)]
    public float offsetTime;
    private float offsetTimer = 0;

    private LinkedList<TransformData> pastTransforms;
    private TransformData transformData;

    [Header("Follow Type")]
    public FollowType followType = FollowType.Copy;
    public enum FollowType { Copy, MoveTowards, Match }
    private Vector3 prevPosition;
    private Quaternion prevRotation;
    private Vector3 prevScale;

    [Header("Follow Transform")]
    public bool followMove;
    public bool followRotate;
    public bool followScale;
    public bool relativeScale;

    [Header("Multiplier")]
    public float multiplier = 1;

    [Header("Move Towards Speed")]
    [Min(0)]
    public float speed;

    [Header("Ignore Axis")]
    public AxisType ignorePositionAxis;
    public AxisType ignoreScaleAxis;
    private bool ignoreX;
    private bool ignoreY;
    private bool ignoreSX;
    private bool ignoreSY;
    public enum AxisType { None, X, Y }
    private float[] multSX, multSY;

    [Header("Min Max")]
    public bool localMinMax;

    public bool useMinMaxPosX;
    public Vector2 min_max_posX;
    public bool useMinMaxPosY;
    public Vector2 min_max_posY;
    public bool useMinMaxRotZ;
    public Vector2 min_max_rotZ;
    public bool useMinMaxSclX;
    public Vector2 min_max_sclX;
    public bool useMinMaxSclY;
    public Vector2 min_max_sclY;


    [Header("Properties")]
    public bool stayToFollow;
    public bool local;
    //public bool useRigidbody = false;
    [Min(-1)]
    public int triggerLimit = -1;

    [Header("Settings")]
    //public TriggerOffScreenDisable offScreenDisable;
    public bool resetOnDeathPerCheckpoint = false;
    public bool playOnAwake;
    //public bool paused = false;
    //private bool offScreenPaused;
    public bool stopped = false;
    public bool hideIcon;
    public SpeedGizmo speedGizmo = SpeedGizmo.x1;

    private GameObject texture;

    //private bool hasRigidBody = false;
    //private List<Rigidbody2D> rb;
    private int triggerCount = 0;
    private List<Vector3> startPosition;
    private List<Vector3> startScale;
    private List<Quaternion> startRotation;
    //private bool inUse = false;

    private PlayerControllerV2 player;
    private GroupIDManager groupIDManager;
    private GameManager gamemanager;
    //private Coroutine disableOffScreenCoroutine;
    //private List<Renderer> targetsWithRenderers;

    private bool start = false;

    private void Awake()
    {
        gamemanager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControllerV2>();

        if (transform.childCount > 0)
        {
            texture = transform.GetChild(0).gameObject;
            texture.SetActive(!hideIcon);
        }

        startPosition = new List<Vector3>();
        startRotation = new List<Quaternion>();
        startScale = new List<Vector3>();

        pastTransforms = new LinkedList<TransformData>();

        enabled = false;

        if (playOnAwake && !stayToFollow)
        {
            Activate();
        }
    }

    public void Start()
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

        multSX = new float[targets.Count];
        multSY = new float[targets.Count];
    }

    public void StopTrigger()
    {
        stopped = true;
        enabled = false;
    }

    public void TogglePauseTrigger()
    {
        stopped = !stopped;
        enabled = !stopped;
    }

    // Update is called once per frame
    void Update()
    {
        if(delayTimer < delay)
        {
            delayTimer += Time.deltaTime;
            return;
        }

        if (time >= duration && duration != 0)
        {
            enabled = false;
        }

        pastTransforms.AddLast(new TransformData(followObject, local));
        if (offsetTimer >= offsetTime)
        {
            if (pastTransforms.Count() == 0)
            { 
                if(transformData == null) { transformData = new TransformData(followObject, local); }
                else { transformData.SetData(followObject, local); }
            }
            else
            {
                transformData = pastTransforms.First();
                pastTransforms.RemoveFirst();
            }

            //Debug.Log(transformData + "    Count: " + pastTransforms.Count());

            if (!start) { Init(); }
            start = true;

            SetAxes();
            Follow();
            MinMax();
            After();
        }

        time += Time.deltaTime;
        offsetTimer += Time.deltaTime;
    }

    void SetAxes()
    {
        ignoreX = ignorePositionAxis == AxisType.X;
        ignoreY = ignorePositionAxis == AxisType.Y;
        ignoreSX = ignoreScaleAxis == AxisType.X;
        ignoreSY = ignoreScaleAxis == AxisType.Y;
    }

    private void Init()
    {
        prevPosition = transformData.position;
        prevRotation = transformData.rotation;
        prevScale = transformData.scale;

        for (int i = 0; i < targets.Count; i++)
        {
            if (triggerCount == 1)
            {
                startPosition.Add(targets[i].position);
                startRotation.Add(targets[i].rotation);
                startScale.Add(targets[i].localScale);
            }

            multSX[i] = targets[i].localScale.x / transformData.localScale.x;
            multSY[i] = targets[i].localScale.y / transformData.localScale.y;
        }
    }

    void Activate()
    {
        if (requiredManaCount > gamemanager.getManaCount()) { return; }
        if (triggerLimit == -1 || triggerCount < triggerLimit)
        {
            triggerCount++;

            time = 0;
            offsetTimer = 0;
            delayTimer = 0;

            pastTransforms.Clear();
            start = false;
            enabled = true;
        }
    }

    void Follow()
    {
        switch(followType)
        {
            case FollowType.Copy: Copy(); break;
            case FollowType.MoveTowards: MoveTowards(); break;
            case FollowType.Match: Match(); break;
        }
    }

    void After()
    {
        prevPosition = transformData.position;
        prevRotation = transformData.rotation;
        prevScale = transformData.scale;
    }

    void Copy()
    {
        Vector3 currPosition = transformData.position;
        Vector3 diffPosition = currPosition - prevPosition;

        Quaternion currRotation = transformData.rotation;
        Vector3 diffRotation = currRotation.eulerAngles - prevRotation.eulerAngles;

        Vector3 currScale = transformData.scale;
        Vector3 diffScale = relativeScale ? VectorExtension.Divide(currScale, prevScale).SetNaNToZero() : currScale - prevScale;

        foreach (Transform tr in targets)
        {
            if(followMove && currPosition != prevPosition)
            {
                diffPosition = new Vector3(ignoreX ? 0 : diffPosition.x, ignoreY ? 0 : diffPosition.y, diffPosition.z);
                diffPosition *= multiplier;
                tr.Translate(diffPosition, local ? Space.Self : Space.World);
            }
            if (followRotate && currRotation != prevRotation)
            {
                diffRotation *= multiplier;
                tr.Rotate(diffRotation, local ? Space.Self : Space.World);
            }
            if (followScale && currScale != prevScale)
            {
                if(relativeScale)
                {
                    diffScale = new Vector3(ignoreSX ? 1 : diffScale.x, ignoreSY ? 1 : diffScale.y, diffScale.z);
                    diffScale *= multiplier;
                    tr.localScale =  Vector3.Scale(tr.localScale, diffScale);
                }
                else
                {
                    diffScale = new Vector3(ignoreSX ? 0 : diffScale.x, ignoreSY ? 0 : diffScale.y, diffScale.z);
                    diffScale *= multiplier;
                    tr.localScale += diffScale;
                }
            }
        }
    }

    void MoveTowards()
    {
        foreach (Transform tr in targets)
        {
            Vector3 moveTo = transformData.position.ReplaceElements(ignoreX, local ? tr.localPosition.x : tr.position.x,
                                                                    ignoreY, local ? tr.localPosition.y : tr.position.y,
                                                                    false, local ? tr.localPosition.z : tr.position.z);
            Quaternion rotateTo = transformData.rotation;
            Vector3 scaleTo = transformData.scale.ReplaceElements(ignoreSX, tr.localScale.x,
                                                                   ignoreSY, tr.localScale.y,
                                                                   false, tr.localScale.z);

            if (local)
            {
                if(followMove)
                    tr.localPosition = Vector3.MoveTowards(tr.localPosition, moveTo, speed * Time.deltaTime);

                if (followRotate)
                    tr.localRotation = Quaternion.RotateTowards(tr.localRotation, rotateTo, speed * Time.deltaTime);

                if (followScale)
                    tr.localScale = Vector3.MoveTowards(tr.localScale, scaleTo, speed * Time.deltaTime);
            }
            else
            {
                if (followMove)
                    tr.position = Vector3.MoveTowards(tr.position, moveTo, speed * Time.deltaTime);

                if (followRotate)
                    tr.rotation = Quaternion.RotateTowards(tr.rotation, rotateTo, speed);

                if (followScale)
                    tr.localScale = Vector3.MoveTowards(tr.localScale, scaleTo, speed * Time.deltaTime);
            }
        }
    }

    void Match()
    {
        int i = 0;
        foreach (Transform tr in targets)
        {
            if (followMove)
            {
                if (!local)
                {
                    if (ignoreY && !ignoreX) { tr.position = tr.position.SetX(transformData.position.x); }
                    else if (!ignoreY && ignoreX) { tr.position = tr.position.SetY(transformData.position.y); }
                    else if (!ignoreX && !ignoreY) { tr.position = tr.position.SetXY(new Vector2(transformData.position.x, transformData.position.y)); }
                }
                else
                {
                    if (ignoreY && !ignoreX) { tr.localPosition = tr.localPosition.SetX(transformData.position.x); }
                    else if (!ignoreY && ignoreX) { tr.localPosition = tr.localPosition.SetY(transformData.position.y); }
                    else if (!ignoreX && !ignoreY) { tr.localPosition = tr.localPosition.SetXY(new Vector2(transformData.position.x, transformData.position.y)); }
                }
            }
            if (followRotate)
            {
                if (!local)
                {
                    tr.rotation = transformData.rotation;
                }
                else
                {
                    tr.localRotation = transformData.rotation;
                }
            }
            if (followScale)
            {
                float multX = relativeScale ? multSX[i] : 1;
                float multY = relativeScale ? multSY[i] : 1;

                if (!local)
                {
                    if (ignoreSY && !ignoreSX) { tr.localScale = tr.localScale.SetX(transformData.scale.x * multX); }
                    else if (!ignoreSY && ignoreSX) { tr.localScale = tr.localScale.SetY(transformData.scale.y * multY); }
                    else if (!ignoreSX && !ignoreSY) { tr.localScale = tr.localScale.SetXY(new Vector2(transformData.scale.x * multX, transformData.scale.y * multY)); }
                }
                else
                {
                    float lossyMultiX = tr.parent != null ? tr.parent.lossyScale.x : 1;
                    float lossyMultiY = tr.parent != null ? tr.parent.lossyScale.y : 1;
                    Vector3 lossyMulti = new Vector3(lossyMultiX, lossyMultiY, 1);

                    if (ignoreSY && !ignoreSX) { tr.localScale = Vector3.Scale(tr.lossyScale.SetX(transformData.scale.x * multX), lossyMulti); }
                    else if (!ignoreSY && ignoreSX) { tr.localScale = Vector3.Scale(tr.lossyScale.SetY(transformData.scale.y * multY), lossyMulti); }
                    else if (!ignoreSX && !ignoreSY) { tr.localScale = Vector3.Scale(tr.lossyScale.SetXY(new Vector2(transformData.scale.x * multX, transformData.scale.y * multY)), lossyMulti); }
                }
            }

            i++;
        }
    }

    void MinMax()
    {
        int i = 0;
        foreach (Transform tr in targets)
        {
            bool hasParent = tr.parent != null;

            if (useMinMaxPosX)
            {
                if (!localMinMax)
                {
                    float addition = startPosition[i].x;
                    tr.position = tr.position.SetX(Mathf.Clamp(tr.position.x, min_max_posX.x + addition, min_max_posX.y + addition));
                }
                else
                {
                    float addition = hasParent ? 0 : startPosition[i].x;
                    tr.localPosition = tr.localPosition.SetX(Mathf.Clamp(tr.localPosition.x, min_max_posX.x + addition, min_max_posX.y + addition));
                }
            }
            if (useMinMaxPosY)
            {
                if (!localMinMax)
                {
                    float addition = startPosition[i].y;
                    tr.position = tr.position.SetY(Mathf.Clamp(tr.position.y, min_max_posY.x + addition, min_max_posY.y + addition));
                }
                else
                {
                    float addition = hasParent ? 0 : startPosition[i].y;
                    tr.localPosition = tr.localPosition.SetY(Mathf.Clamp(tr.localPosition.y, min_max_posY.x + addition, min_max_posY.y + addition));
                }
            }

            if (useMinMaxRotZ)
            {
                if (!localMinMax)
                {
                    float addition = startRotation[i].eulerAngles.z;
                    tr.rotation = Quaternion.Euler(tr.rotation.eulerAngles.SetZ(Mathf.Clamp(tr.rotation.eulerAngles.z, min_max_rotZ.x + addition, min_max_rotZ.y + addition)));
                }
                else
                {
                    float addition = hasParent ? 0 : startRotation[i].eulerAngles.z;
                    tr.localRotation = Quaternion.Euler(tr.localRotation.eulerAngles.SetZ(Mathf.Clamp(tr.localRotation.eulerAngles.z, min_max_rotZ.x + addition, min_max_rotZ.y + addition)));
                }
            }

            if (useMinMaxSclX)
            {
                if (!localMinMax)
                {
                    float addition = startScale[i].x;
                    //tr.localScale = Vec tr.lossyScale.SetX(Mathf.Clamp(tr.lossyScale.x, min_max_sclX.x, min_max_sclX.y));
                    tr.localScale = Vector3.Scale(tr.localScale.SetX(Mathf.Clamp(tr.localScale.x, min_max_sclX.x + addition, min_max_sclX.y + addition)),
                                                    tr.parent != null ? tr.parent.localScale : Vector3.one);
                }
                else
                {
                    float addition = hasParent ? 0 : startScale[i].x;
                    tr.localScale = tr.localScale.SetX(Mathf.Clamp(tr.localScale.x, min_max_sclX.x + addition, min_max_sclX.y + addition));
                }
            }

            if (useMinMaxSclY)
            {
                if (!localMinMax)
                {
                    float addition = startScale[i].y;
                    //tr.localScale = tr.lossyScale.SetY(Mathf.Clamp(tr.lossyScale.y, min_max_sclY.x, min_max_sclY.y));
                    tr.localScale = Vector3.Scale(tr.localScale.SetY(Mathf.Clamp(tr.localScale.y, min_max_sclY.x + addition, min_max_sclY.y + addition)),
                                                    tr.parent != null ? tr.parent.localScale : Vector3.one);
                }
                else
                {
                    float addition = hasParent ? 0 : startScale[i].y;
                    tr.localScale = tr.localScale.SetY(Mathf.Clamp(tr.localScale.y, min_max_sclY.x + addition, min_max_sclY.y + addition));
                }
            }

            i++;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player" && !stayToFollow)
        {
            //player.AddFollowTriggers(this);
            if (!enabled)
            {
                if(delay > 0)
                {
                    Invoke("Activate", delay);
                }
                else
                {
                    Activate();
                }
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "Player" && stayToFollow)
        {
            transformData = new TransformData(followObject, local);

            triggerCount++;
            if (!start) { Init(); }
            start = true;

            SetAxes();
            Follow();
            MinMax();
            After();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player" && stayToFollow)
        {
            start = false;
        }
    }

    private void OnValidate()
    {
        if (texture != null && Application.isPlaying)
        {
            texture.SetActive(!hideIcon);
        }

        enabled = !stopped;
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

            if (followObject != null)
            {
                Gizmos.color = new Color(1f, .8f, .6f);
                Gizmos.DrawLine(tf.position, followObject.position);
            }
        }
    }
#endif
}
