using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FollowObject : MonoBehaviour
{
    [Header("Target Objects")]
    public List<int> groupIDs;
    public List<Transform> targets;
    public Transform followObject;

    [Header("Conditions")]
    [Min(0)]
    public float delay;
    [Min(0)]
    public float requiredManaCount;
    [Min(0)]
    public float duration;
    private float time = 0;

    [Header("Follow Type")]
    public FollowType followType = FollowType.Copy;
    public enum FollowType { Copy, MoveTowards, Match }
    private Vector3 prevPosition;
    private Quaternion prevRotation;
    private Vector3 prevScale;

    public bool followMove, followRotate, followScale, relativeScale;

    [Header("Match Constrainsts")]
    public bool ignoreX;
    public bool ignoreY;
    public bool ignoreSX;
    public bool ignoreSY;
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
    public bool useRigidbody = false;
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

        startPosition = new List<Vector3>();
        startRotation = new List<Quaternion>();
        startScale = new List<Vector3>();

        multSX = new float[targets.Count];
        multSY = new float[targets.Count];

        enabled = false;

        if (playOnAwake && !stayToFollow)
        {
            Activate();
        }
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

    // Update is called once per frame
    void Update()
    {
        if (time >= duration && duration != 0)
        {
            enabled = false;
        }

        Follow();
        MinMax();
        After();

        time += Time.deltaTime;
    }

    void Activate()
    {
        prevPosition = local ? followObject.localPosition : followObject.position;
        prevRotation = local ? followObject.localRotation : followObject.rotation;
        prevScale = local ? followObject.localScale : followObject.lossyScale;

        for (int i = 0; i < targets.Count; i++)
        {
            if (triggerCount == 0)
            {
                startPosition.Add(targets[i].position);
                startRotation.Add(targets[i].rotation);
                startScale.Add(targets[i].localScale);
            }

            multSX[i] = targets[i].localScale.x / followObject.localScale.x;
            multSY[i] = targets[i].localScale.y / followObject.localScale.y;
        }
        /*if (tr.parent == null)
            {
                min_max_posX += Vector2.one * tr.position.x;
                min_max_posY += Vector2.one * tr.position.y;
                min_max_rotZ += Vector2.one * tr.rotation.eulerAngles.z;
                min_max_sclX += Vector2.one * tr.localScale.x;
                min_max_sclY += Vector2.one * tr.localScale.y;
            }*/

        triggerCount++;

        time = 0;
        enabled = true;
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
        prevPosition = local ? followObject.localPosition : followObject.position;
        prevRotation = local ? followObject.localRotation : followObject.rotation;
        prevScale = local ? followObject.localScale : followObject.lossyScale;
    }

    void Copy()
    {
        Vector3 currPosition = local ? followObject.localPosition : followObject.position;
        Vector3 diffPosition = currPosition - prevPosition;

        Quaternion currRotation = local ? followObject.localRotation : followObject.rotation;
        Vector3 diffRotation = currRotation.eulerAngles - prevRotation.eulerAngles;

        Vector3 currScale = local ? followObject.localScale : followObject.lossyScale;
        Vector3 diffScale = relativeScale ? VectorExtension.Divide(currScale, prevScale).SetNaNToZero() : currScale - prevScale;

        foreach (Transform tr in targets)
        {
            if(followMove && currPosition != prevPosition)
            {
                tr.Translate(diffPosition);
                
            }
            if (followRotate && currRotation != prevRotation)
            {
                tr.Rotate(diffRotation);
            }
            if (followScale && currScale != prevScale)
            {
                if(relativeScale)
                {
                    tr.localScale =  Vector3.Scale(tr.localScale, diffScale);
                }
                else
                {
                    tr.localScale += diffScale;
                }
            }
        }
    }

    void MoveTowards()
    {

    }

    void Match()
    {
        int i = 0;
        foreach (Transform tr in targets)
        {
            if (followMove)
            {
                if(ignoreY && !ignoreX) { tr.position = tr.position.SetX(followObject.position.x); }
                else if (!ignoreY && ignoreX) { tr.position = tr.position.SetY(followObject.position.y); }
                else if (!ignoreX && !ignoreY) { tr.position = tr.position.SetXY(new Vector2(followObject.position.x, followObject.position.y)); }
            }
            if (followRotate)
            {
                tr.rotation = followObject.rotation;
            }
            if (followScale)
            {
                float multX = relativeScale ? multSX[i] : 1;
                float multY = relativeScale ? multSY[i] : 1;

                if (ignoreSY && !ignoreSX) { tr.localScale = tr.localScale.SetX(followObject.localScale.x * multX); }
                else if (!ignoreSY && ignoreSX) { tr.localScale = tr.localScale.SetY(followObject.localScale.y * multY); }
                else if (!ignoreSX && !ignoreSY) { tr.localScale = tr.localScale.SetXY(new Vector2(followObject.localScale.x * multX, followObject.localScale.y * multY)); }
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
                    tr.position = tr.position.SetX(Mathf.Clamp(tr.position.x, min_max_posX.x, min_max_posX.y));
                }
                else
                {
                    float addition = hasParent ? 0 : startPosition[i].x;
                    tr.localPosition = tr.localPosition.SetX(Mathf.Clamp(tr.localPosition.x, min_max_posX.x + addition, min_max_posX.y  + addition));
                }
            }
            if (useMinMaxPosY)
            {
                if (!localMinMax)
                {
                    tr.position = tr.position.SetY(Mathf.Clamp(tr.position.y, min_max_posY.x, min_max_posY.y));
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
                    tr.rotation = Quaternion.Euler(tr.rotation.eulerAngles.SetZ(Mathf.Clamp(tr.rotation.eulerAngles.z, min_max_rotZ.x, min_max_rotZ.y)));
                }
                else
                {
                    float addition = hasParent ? 0 : startRotation[i].eulerAngles.z;
                    tr.localRotation = Quaternion.Euler(tr.localRotation.eulerAngles.SetZ(Mathf.Clamp(tr.localRotation.eulerAngles.z, min_max_rotZ.x  + addition, min_max_rotZ.y + addition)));
                }
            }

            if (useMinMaxSclX)
            {
                if (!localMinMax)
                {
                    //tr.localScale = Vec tr.lossyScale.SetX(Mathf.Clamp(tr.lossyScale.x, min_max_sclX.x, min_max_sclX.y));
                    tr.localScale = Vector3.Scale(tr.localScale.SetX(Mathf.Clamp(tr.localScale.x, min_max_sclX.x, min_max_sclX.y)),
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
                    //tr.localScale = tr.lossyScale.SetY(Mathf.Clamp(tr.lossyScale.y, min_max_sclY.x, min_max_sclY.y));
                    tr.localScale = Vector3.Scale(tr.localScale.SetY(Mathf.Clamp(tr.localScale.y, min_max_sclY.x, min_max_sclY.y)),
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

            if (followObject != null)
            {
                Gizmos.color = new Color(1f, .8f, .6f);
                Gizmos.DrawLine(tf.position, followObject.position);
            }
        }
    }
#endif
}
