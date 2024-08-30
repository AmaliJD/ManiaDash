using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Cinemachine;
using TMPro;

public class CameraTrigger : MonoBehaviour
{
    [Header("Camera")]
    //[SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private CameraControl cameraControl;

    [Header("Zoom")]
    public bool useZoom;
    public bool getCurrentZoom;
    [Min(0)] public float zoomValue;
    [Min(0)] public float zoomDuration;
    public EasingFunction.Ease zoomEase;

    [Header("Rotation")]
    public bool useRotation;
    public bool getCurrentRotation;
    [Range(-360, 360)] public float rotationValue;
    [Min(0)] public float rotationDuration;
    public EasingFunction.Ease rotationEase;

    [Header("Offset")]
    public bool useOffset;
    public bool getCurrentOffset;
    public Vector2 offsetValue;
    [Min(0)] public float offsetDuration;
    public EasingFunction.Ease offsetEase;

    [Header("Look Ahead")]
    public bool useLookAhead;
    public bool getCurrentLookAhead;
    public bool resetLookAhead;
    public bool ignoreY;
    [Min(0)] public float lookaheadTimeValue;
    [Min(0)] public float lookaheadSmoothingValue;
    [Min(0)] public float lookaheadDuration;
    public EasingFunction.Ease lookaheadEase;

    [Header("Dead Zone")]
    public bool useDeadZone;
    public bool getCurrentDeadZone;
    public Vector2 deadzoneValue;
    [Min(0)] public float deadzoneDuration;
    public EasingFunction.Ease deadzoneEase;

    [Header("Damping")]
    public bool useDamping;
    public bool getCurrentDamping;
    public Vector2 dampingValue;
    [Min(0)] public float dampingDuration;
    public EasingFunction.Ease dampingEase;

    [Header("Noise")]
    public bool useNoise;
    public bool getCurrentNoise;
    [Min(0)] public float amplitudeValue;
    [Min(0)] public float frequencyValue;
    [Min(0)] public float noiseDuration;
    public EasingFunction.Ease noiseEase;

    [Header("Locks")]
    public bool useLockCenter;
    public bool getCurrentLockCenter;
    public bool startFromCurrent;
    public Transform lockCenterTarget;
    public Vector2 lockCenterOffset;
    public LockCameraXY.LockType lockCenterType;
    [Range(0, 1)] public float lockCenterLerp;
    [Min(0)] public float lockCenterDuration;
    public EasingFunction.Ease lockCenterEase;

    [Header("")]
    public bool useLockLeft;
    public bool getCurrentLockLeft;
    public Transform lockLeftTarget;
    [Range(0, 1)] public float lockLeftLerp;
    [Min(0)] public float lockLeftDuration;
    public EasingFunction.Ease lockLeftEase;

    [Header("")]
    public bool useLockRight;
    public bool getCurrentLockRight;
    public Transform lockRightTarget;
    [Range(0, 1)] public float lockRightLerp;
    [Min(0)] public float lockRightDuration;
    public EasingFunction.Ease lockRightEase;

    [Header("")]
    public bool useLockTop;
    public bool getCurrentLockTop;
    public Transform lockTopTarget;
    [Range(0, 1)] public float lockTopLerp;
    [Min(0)] public float lockTopDuration;
    public EasingFunction.Ease lockTopEase;

    [Header("")]
    public bool useLockBottom;
    public bool getCurrentLockBottom;
    public Transform lockBottomTarget;
    [Range(0, 1)] public float lockBottomLerp;
    [Min(0)] public float lockBottomDuration;
    public EasingFunction.Ease lockBottomEase;

    [Header("Settings")]
    [SerializeField] private bool hideIcon;
    [SerializeField] private bool getCurrentValues;
    [SerializeField] private bool getCurrentOnEnter;
    [SerializeField] private bool stopOnDeath;
    [SerializeField] [Min(0)] private float duration;
    [SerializeField] [Min(0)] private int preUses;
    [SerializeField] [Min(0)] private int maxUses;
    private int uses;

    public delegate void ActivateAction(bool stop);
    public static event ActivateAction OnActivate;

    private void Awake()
    {
        // delete trigger icon
        if (hideIcon) { gameObject.transform.GetChild(0).gameObject.SetActive(false); }
        else
        {
            gameObject.transform.GetChild(0).GetChild(1).GetComponent<TextMeshPro>().color = useZoom ? Color.white : new Color(1, 1, 1, .25f);
            gameObject.transform.GetChild(0).GetChild(2).GetComponent<TextMeshPro>().color = useRotation ? Color.white : new Color(1, 1, 1, .25f);
            gameObject.transform.GetChild(0).GetChild(3).GetComponent<TextMeshPro>().color = useOffset ? Color.white : new Color(1, 1, 1, .25f);
            gameObject.transform.GetChild(0).GetChild(4).GetComponent<TextMeshPro>().color = useLookAhead ? Color.white : new Color(1, 1, 1, .25f);
            gameObject.transform.GetChild(0).GetChild(5).GetComponent<TextMeshPro>().color = useDeadZone ? Color.white : new Color(1, 1, 1, .25f);
            gameObject.transform.GetChild(0).GetChild(6).GetComponent<TextMeshPro>().color = useDamping ? Color.white : new Color(1, 1, 1, .25f);
            gameObject.transform.GetChild(0).GetChild(7).GetComponent<TextMeshPro>().color = useNoise ? Color.white : new Color(1, 1, 1, .25f);
            gameObject.transform.GetChild(0).GetChild(8).GetComponent<TextMeshPro>().color = useLockCenter ? Color.white : new Color(1, 1, 1, .25f);
            gameObject.transform.GetChild(0).GetChild(9).GetComponent<TextMeshPro>().color = useLockLeft ? Color.white : new Color(1, 1, 1, .25f);
            gameObject.transform.GetChild(0).GetChild(10).GetComponent<TextMeshPro>().color = useLockRight ? Color.white : new Color(1, 1, 1, .25f);
            gameObject.transform.GetChild(0).GetChild(11).GetComponent<TextMeshPro>().color = useLockTop ? Color.white : new Color(1, 1, 1, .25f);
            gameObject.transform.GetChild(0).GetChild(12).GetComponent<TextMeshPro>().color = useLockBottom ? Color.white : new Color(1, 1, 1, .25f);
        }

        if(getCurrentValues)
        {
            getCurrentZoom = getCurrentRotation = getCurrentOffset = getCurrentLookAhead = getCurrentDeadZone = getCurrentDamping = getCurrentNoise
                = getCurrentLockCenter = getCurrentLockLeft = getCurrentLockRight = getCurrentLockTop = getCurrentLockBottom = true;
        }

        if(duration > 0)
        {
            zoomDuration += duration;
            rotationDuration += duration;
            offsetDuration += duration;
            lookaheadDuration += duration;
            deadzoneDuration += duration;
            dampingDuration += duration;
            noiseDuration += duration;
            lockCenterDuration += duration;
            lockLeftDuration += duration;
            lockRightDuration += duration;
            lockTopDuration += duration;
            lockBottomDuration += duration;
        }

        GetCurrentValues();
    }

    void GetCurrentValues()
    {
        CinemachineVirtualCamera virtualCamera = cameraControl.GetComponent<CinemachineVirtualCamera>();

        if (getCurrentZoom) { zoomValue = virtualCamera.m_Lens.OrthographicSize; }
        if (getCurrentRotation) { rotationValue = virtualCamera.m_Lens.Dutch; }

        if (virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>() != null)
        {
            CinemachineFramingTransposer transposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            if (getCurrentOffset) { offsetValue = new Vector2((transposer.m_ScreenX - .5f) * -2, (transposer.m_ScreenY - .5f) * -2); }
            if (getCurrentLookAhead)
            {
                lookaheadTimeValue = transposer.m_LookaheadTime;
                lookaheadSmoothingValue = transposer.m_LookaheadSmoothing;
                ignoreY = transposer.m_LookaheadIgnoreY;
            }
            if (getCurrentDeadZone) { deadzoneValue = new Vector2(transposer.m_DeadZoneWidth, transposer.m_DeadZoneHeight); }
            if (getCurrentDamping) { dampingValue = new Vector2(transposer.m_XDamping, transposer.m_YDamping); }
        }
        else
        {
            useOffset = false;
            useLookAhead = false;
            useDeadZone = false;
            useDamping = false;
        }


        if (virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>() != null)
        {
            CinemachineBasicMultiChannelPerlin perlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if (getCurrentNoise)
            {
                amplitudeValue = perlin.m_AmplitudeGain;
                frequencyValue = perlin.m_FrequencyGain;
            }
        }
        else
        {
            getCurrentNoise = false;
        }
        

        if (virtualCamera.GetComponent<LockCameraXY>() != null)
        {
            LockCameraXY lockCam = virtualCamera.GetComponent<LockCameraXY>();

            if (getCurrentLockCenter)
            {
                lockCenterTarget = lockCam.lockTarget;
                lockCenterLerp = lockCam.lerpLock;
                lockCenterOffset = lockCam.offset;
                lockCenterType = lockCam.lockAxis;
            }
            if (getCurrentLockLeft)
            {
                lockLeftTarget = lockCam.leftWall;
                lockLeftLerp = lockCam.lerpLeft;
            }
            if (getCurrentLockRight)
            {
                lockRightTarget = lockCam.rightWall;
                lockRightLerp = lockCam.lerpRight;
            }
            if (getCurrentLockTop)
            {
                lockTopTarget = lockCam.topWall;
                lockTopLerp = lockCam.lerpTop;
            }
            if (getCurrentLockBottom)
            {
                lockBottomTarget = lockCam.bottomWall;
                lockBottomLerp = lockCam.lerpBottom;
            }
        }
        else
        {
            useLockCenter = false;
            useLockLeft = false;
            useLockRight = false;
            useLockTop = false;
            useLockBottom = false;
        }
    }

    /*CinemachineVirtualCamera validate_virtualCamera;
    CinemachineFramingTransposer validate_transposer;
    LockCameraXY validate_lockCam;*/
    private void OnValidate()
    {
        if (getCurrentOnEnter) { return; }
        if (getCurrentValues)
        {
            getCurrentZoom = getCurrentRotation = getCurrentOffset = getCurrentLookAhead = getCurrentDeadZone = getCurrentDamping
                = getCurrentLockCenter = getCurrentLockLeft = getCurrentLockRight = getCurrentLockTop = getCurrentLockBottom = true;

            getCurrentValues = false;
        }

        if (Application.isEditor && cameraControl != null)
        {
            CinemachineVirtualCamera validate_virtualCamera = cameraControl.GetComponent<CinemachineVirtualCamera>();

            if (getCurrentZoom) { zoomValue = validate_virtualCamera.m_Lens.OrthographicSize; }
            if (getCurrentRotation) { rotationValue = validate_virtualCamera.m_Lens.Dutch; }

            if (validate_virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>() != null)
            {
                CinemachineFramingTransposer validate_transposer = validate_virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
                if (getCurrentOffset) { offsetValue = new Vector2((validate_transposer.m_ScreenX - .5f) * -2, (validate_transposer.m_ScreenY - .5f) * -2); }
                if (getCurrentLookAhead)
                {
                    lookaheadTimeValue = validate_transposer.m_LookaheadTime;
                    lookaheadSmoothingValue = validate_transposer.m_LookaheadSmoothing;
                    ignoreY = validate_transposer.m_LookaheadIgnoreY;
                }
                if (getCurrentDeadZone) { deadzoneValue = new Vector2(validate_transposer.m_DeadZoneWidth, validate_transposer.m_DeadZoneHeight); }
                if (getCurrentDamping) { dampingValue = new Vector2(validate_transposer.m_XDamping, validate_transposer.m_YDamping); }
            }

            if (validate_virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>() != null)
            {
                CinemachineBasicMultiChannelPerlin validate_perlin = validate_virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                if (getCurrentNoise)
                {
                    amplitudeValue = validate_perlin.m_AmplitudeGain;
                    frequencyValue = validate_perlin.m_FrequencyGain;
                }
            }

            if (validate_virtualCamera.GetComponent<LockCameraXY>() != null)
            {
                LockCameraXY validate_lockCam = validate_virtualCamera.GetComponent<LockCameraXY>();

                if (getCurrentLockCenter)
                {
                    lockCenterTarget = validate_lockCam.lockTarget;
                    lockCenterLerp = validate_lockCam.lerpLock;
                    lockCenterOffset = validate_lockCam.offset;
                    lockCenterType = validate_lockCam.lockAxis;
                }
                if (getCurrentLockLeft)
                {
                    lockLeftTarget = validate_lockCam.leftWall;
                    lockLeftLerp = validate_lockCam.lerpLeft;
                }
                if (getCurrentLockRight)
                {
                    lockRightTarget = validate_lockCam.rightWall;
                    lockRightLerp = validate_lockCam.lerpRight;
                }
                if (getCurrentLockTop)
                {
                    lockTopTarget = validate_lockCam.topWall;
                    lockTopLerp = validate_lockCam.lerpTop;
                }
                if (getCurrentLockBottom)
                {
                    lockBottomTarget = validate_lockCam.bottomWall;
                    lockBottomLerp = validate_lockCam.lerpBottom;
                }
            }
        }

        /*if (getCurrentZoom) { useZoom = true; }
        if (getCurrentRotation) { useRotation = true; }
        if (getCurrentOffset) { useOffset = true; }
        if (getCurrentLookAhead) { useLookAhead = true; }
        if (getCurrentDeadZone) { useDeadZone = true; }
        if (getCurrentDamping) { useDamping = true; }
        if (getCurrentLockCenter) { useLockCenter = true; }
        if (getCurrentLockLeft) { useLockLeft = true; }
        if (getCurrentLockRight) { useLockRight = true; }
        if (getCurrentLockTop) { useLockTop = true; }
        if (getCurrentLockBottom) { useLockBottom = true; }*/
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if(maxPreUses > 0 && preuses < maxPreUses) { preuses++; return; }
        /*if(preUses > 0) { preUses--; return; }

        if (maxUses > 0 && uses >= maxUses)
        {
            GetComponent<Collider2D>().enabled = false;
            return;
        }*/

        if (collision.gameObject.tag == "Player")
        {
            if (preUses > 0)
            {
                if (getCurrentOnEnter) { GetCurrentValues(); getCurrentOnEnter = false; }
                preUses--;
                return;
            }

            if (maxUses > 0 && uses >= maxUses)
            {
                GetComponent<Collider2D>().enabled = false;
                return;
            }

            uses++;
            if (getCurrentOnEnter) { GetCurrentValues(); getCurrentOnEnter = false; }

            Activate();
        }
    }

    public void Activate()
    {
        if (OnActivate != null) { OnActivate(stopOnDeath); }
        if (useZoom)
        {
            cameraControl.StartZoom(zoomValue, zoomDuration, zoomEase);
        }
        if (useRotation)
        {
            cameraControl.StartRotation(rotationValue, rotationDuration, rotationEase);
        }
        if (useOffset)
        {
            cameraControl.StartOffset(offsetValue.x, offsetValue.y, offsetDuration, offsetEase);
        }
        if (useLookAhead)
        {
            cameraControl.StartLookAhead(lookaheadTimeValue, lookaheadSmoothingValue, lookaheadDuration, ignoreY, lookaheadEase, resetLookAhead);
        }
        if (useDeadZone)
        {
            cameraControl.StartDeadZone(deadzoneValue.x, deadzoneValue.y, deadzoneDuration, deadzoneEase);
        }
        if (useDamping)
        {
            cameraControl.StartDamping(dampingValue.x, dampingValue.y, dampingDuration, dampingEase);
        }
        if (useNoise)
        {
            cameraControl.StartNoise(amplitudeValue, frequencyValue, noiseDuration, noiseEase);
        }
        if (useLockCenter)
        {
            cameraControl.StartLockCenter(lockCenterTarget, lockCenterOffset, lockCenterType, lockCenterLerp, lockCenterDuration, lockCenterEase, startFromCurrent);
        }
        if (useLockLeft)
        {
            cameraControl.StartLockLeft(lockLeftTarget, lockLeftLerp, lockLeftDuration, lockLeftEase);
        }
        if (useLockRight)
        {
            cameraControl.StartLockRight(lockRightTarget, lockRightLerp, lockRightDuration, lockRightEase);
        }
        if (useLockTop)
        {
            cameraControl.StartLockTop(lockTopTarget, lockTopLerp, lockTopDuration, lockTopEase);
        }
        if (useLockBottom)
        {
            cameraControl.StartLockBottom(lockBottomTarget, lockBottomLerp, lockBottomDuration, lockBottomEase);
        }
    }

    /*void ActivateOnDeath()
    {
        if (stopOnDeath)
            cameraControl.StopAllCoroutines();
    }

    void OnEnable()
    {
        PlayerControllerV2.OnDeath += ActivateOnDeath;
    }


    void OnDisable()
    {
        PlayerControllerV2.OnDeath -= ActivateOnDeath;
    }*/

    // -----------------------------------

    public enum Speed
    {
        x0, x1, x2, x3, x4
    }
    public Speed speed = Speed.x1;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        float scale = 0;
        switch (speed)
        {
            case Speed.x0:
                scale = 40f; break;
            case Speed.x1:
                scale = 55f; break;
            case Speed.x2:
                scale = 75f; break;
            case Speed.x3:
                scale = 90f; break;
            case Speed.x4:
                scale = 110f; break;
        }

        bool[] uses = { useZoom, useRotation, useOffset, useLookAhead, useDeadZone, useDamping, useNoise, useLockCenter, useLockLeft, useLockRight, useLockTop, useLockBottom };
        float[] durations = { zoomDuration, rotationDuration, offsetDuration, lookaheadDuration, deadzoneDuration, dampingDuration, noiseDuration, lockCenterDuration, lockLeftDuration, lockRightDuration, lockTopDuration, lockBottomDuration };
        Color[] colors = { Color.green, new Color(.5f, 0, 1f), new Color(0, 1f, 1f), new Color(1f, 1f, 0f), new Color(1f, 0f, 0f), new Color(1f, 0f, 1f), new Color(.5f, .8f, 1f), Color.white };

        for(int i = 0; i < uses.Length; i++)
        {
            gameObject.transform.GetChild(0).GetChild(i + 1).GetComponent<TextMeshPro>().color = uses[i] ? Color.white : new Color(1, 1, 1, .25f);
            Gizmos.color = colors[Mathf.Clamp(i, 0, colors.Length - 1)];
            if(uses[i])
                Gizmos.DrawLine(transform.position + (Vector3.down * (.4f + (.2f * i))), transform.position + (Vector3.down * (.4f + (.2f * i))) + new Vector3((scale * Time.fixedDeltaTime * 10f) * (durations[i] + duration), 0, 0));
        }

        //gameObject.transform.GetChild(0).GetChild(1).GetComponent<TextMeshPro>().color = useZoom ? Color.white : new Color(1,1,1,.25f);
        //Gizmos.color = Color.green;
        //if(useZoom)
        //    Gizmos.DrawLine(transform.position + (Vector3.down * .4f), transform.position + (Vector3.down * .4f) + new Vector3((scale * Time.fixedDeltaTime * 10f) * (zoomDuration + duration), 0, 0));

        //gameObject.transform.GetChild(0).GetChild(2).GetComponent<TextMeshPro>().color = useRotation ? Color.white : new Color(1, 1, 1, .25f);
        //Gizmos.color = new Color(.5f, 0, 1f);
        //if(useRotation)
        //    Gizmos.DrawLine(transform.position + (Vector3.down * .6f), transform.position + (Vector3.down * .6f) + new Vector3((scale * Time.fixedDeltaTime * 10f) * (rotationDuration + duration), 0, 0));

        //gameObject.transform.GetChild(0).GetChild(3).GetComponent<TextMeshPro>().color = useOffset ? Color.white : new Color(1, 1, 1, .25f);
        //Gizmos.color = new Color(0, 1f, 1f);
        //if(useOffset)
        //    Gizmos.DrawLine(transform.position + (Vector3.down * .8f), transform.position + (Vector3.down * .8f) + new Vector3((scale * Time.fixedDeltaTime * 10f) * (offsetDuration + duration), 0, 0));

        //gameObject.transform.GetChild(0).GetChild(4).GetComponent<TextMeshPro>().color = useLookAhead ? Color.white : new Color(1, 1, 1, .25f);
        //Gizmos.color = new Color(1f, 1f, 0f);
        //if(useLookAhead)
        //    Gizmos.DrawLine(transform.position + (Vector3.down * 1f), transform.position + (Vector3.down * 1f) + new Vector3((scale * Time.fixedDeltaTime * 10f) * (lookaheadDuration + duration), 0, 0));

        //gameObject.transform.GetChild(0).GetChild(5).GetComponent<TextMeshPro>().color = useDeadZone ? Color.white : new Color(1, 1, 1, .25f);
        //Gizmos.color = new Color(1f, 0f, 0f);
        //if(useDeadZone)
        //    Gizmos.DrawLine(transform.position + (Vector3.down * 1.2f), transform.position + (Vector3.down * 1.2f) + new Vector3((scale * Time.fixedDeltaTime * 10f) * (deadzoneDuration + duration), 0, 0));

        //gameObject.transform.GetChild(0).GetChild(6).GetComponent<TextMeshPro>().color = useDamping ? Color.white : new Color(1, 1, 1, .25f);
        //Gizmos.color = new Color(1f, 0f, 1f);
        //if(useDamping)
        //    Gizmos.DrawLine(transform.position + (Vector3.down * 1.4f), transform.position + (Vector3.down * 1.4f) + new Vector3((scale * Time.fixedDeltaTime * 10f) * (dampingDuration + duration), 0, 0));

        //gameObject.transform.GetChild(0).GetChild(7).GetComponent<TextMeshPro>().color = useNoise ? Color.white : new Color(1, 1, 1, .25f);
        //Gizmos.color = new Color(.5f, .8f, 1f);
        //if (useNoise)
        //    Gizmos.DrawLine(transform.position + (Vector3.down * 1.6f), transform.position + (Vector3.down * 1.6f) + new Vector3((scale * Time.fixedDeltaTime * 10f) * (noiseDuration + duration), 0, 0));

        //gameObject.transform.GetChild(0).GetChild(8).GetComponent<TextMeshPro>().color = useLockCenter ? Color.white : new Color(1, 1, 1, .25f);
        //Gizmos.color = Color.white;
        //if(useLockCenter)
        //    Gizmos.DrawLine(transform.position + (Vector3.down * 1.8f), transform.position + (Vector3.down * 1.8f) + new Vector3((scale * Time.fixedDeltaTime * 10f) * (lockCenterDuration + duration), 0, 0));

        //gameObject.transform.GetChild(0).GetChild(9).GetComponent<TextMeshPro>().color = useLockLeft ? Color.white : new Color(1, 1, 1, .25f);
        //Gizmos.color = Color.white;
        //if(useLockLeft)
        //    Gizmos.DrawLine(transform.position + (Vector3.down * 2f), transform.position + (Vector3.down * 2f) + new Vector3((scale * Time.fixedDeltaTime * 10f) * (lockLeftDuration + duration), 0, 0));

        //gameObject.transform.GetChild(0).GetChild(10).GetComponent<TextMeshPro>().color = useLockRight ? Color.white : new Color(1, 1, 1, .25f);
        //Gizmos.color = Color.white;
        //if(useLockRight)
        //    Gizmos.DrawLine(transform.position + (Vector3.down * 2.2f), transform.position + (Vector3.down * 2.2f) + new Vector3((scale * Time.fixedDeltaTime * 10f) * (lockRightDuration + duration), 0, 0));

        //gameObject.transform.GetChild(0).GetChild(11).GetComponent<TextMeshPro>().color = useLockTop ? Color.white : new Color(1, 1, 1, .25f);
        //Gizmos.color = Color.white;
        //if(useLockTop)
        //    Gizmos.DrawLine(transform.position + (Vector3.down * 2.4f), transform.position + (Vector3.down * 2.4f) + new Vector3((scale * Time.fixedDeltaTime * 10f) * (lockTopDuration + duration), 0, 0));

        //gameObject.transform.GetChild(0).GetChild(12).GetComponent<TextMeshPro>().color = useLockBottom ? Color.white : new Color(1, 1, 1, .25f);
        //Gizmos.color = Color.white;
        //if(useLockBottom)
        //    Gizmos.DrawLine(transform.position + (Vector3.down * 2.6f), transform.position + (Vector3.down * 2.6f) + new Vector3((scale * Time.fixedDeltaTime * 10f) * (lockBottomDuration + duration), 0, 0));
    }
#endif
}
