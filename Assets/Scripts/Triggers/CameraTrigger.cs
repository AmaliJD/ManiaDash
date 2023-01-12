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
    [Min(0)] public float zoomValue;
    [Min(0)] public float zoomDuration;
    public EasingFunction.Ease zoomEase;

    [Header("Rotation")]
    public bool useRotation;
    [Range(-360, 360)] public float rotationValue;
    [Min(0)] public float rotationDuration;
    public EasingFunction.Ease rotationEase;

    [Header("Offset")]
    public bool useOffset;
    public Vector2 offsetValue;
    [Min(0)] public float offsetDuration;
    public EasingFunction.Ease offsetEase;

    [Header("Look Ahead")]
    public bool useLookAhead;
    public bool ignoreY;
    [Min(0)] public float lookaheadTimeValue;
    [Min(0)] public float lookaheadSmoothingValue;
    [Min(0)] public float lookaheadDuration;
    public EasingFunction.Ease lookaheadEase;

    [Header("Dead Zone")]
    public bool useDeadZone;
    public Vector2 deadzoneValue;
    [Min(0)] public float deadzoneDuration;
    public EasingFunction.Ease deadzoneEase;

    [Header("Damping")]
    public bool useDamping;
    public Vector2 dampingValue;
    [Min(0)] public float dampingDuration;
    public EasingFunction.Ease dampingEase;

    [Header("Locks")]
    public bool useLockCenter;
    public Transform lockCenterTarget;
    [Range(0, 1)] public float lockCenterLerp;
    [Min(0)] public float lockCenterDuration;
    public EasingFunction.Ease lockCenterEase;

    [Header("")]
    public bool useLockLeft;
    public Transform lockLeftTarget;
    [Range(0, 1)] public float lockLeftLerp;
    [Min(0)] public float lockLeftDuration;
    public EasingFunction.Ease lockLeftEase;

    [Header("")]
    public bool useLockRight;
    public Transform lockRightTarget;
    [Range(0, 1)] public float lockRightLerp;
    [Min(0)] public float lockRightDuration;
    public EasingFunction.Ease lockRightEase;

    [Header("")]
    public bool useLockTop;
    public Transform lockTopTarget;
    [Range(0, 1)] public float lockTopLerp;
    [Min(0)] public float lockTopDuration;
    public EasingFunction.Ease lockTopEase;

    [Header("")]
    public bool useLockBottom;
    public Transform lockBottomTarget;
    [Range(0, 1)] public float lockBottomLerp;
    [Min(0)] public float lockBottomDuration;
    public EasingFunction.Ease lockBottomEase;

    [Header("Settings")]
    [SerializeField] private bool hideIcon;
    [SerializeField] private bool getCurrentValues;
    [SerializeField] [Min(0)] private int maxUses;
    private int uses;

    private void Awake()
    {
        // delete trigger icon
        if (hideIcon) { gameObject.transform.GetChild(0).gameObject.SetActive(false); }

        if(getCurrentValues)
        {
            CinemachineVirtualCamera virtualCamera = cameraControl.GetComponent<CinemachineVirtualCamera>();            

            zoomValue = virtualCamera.m_Lens.OrthographicSize;
            rotationValue = virtualCamera.m_Lens.Dutch;

            if (virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>() != null)
            {
                CinemachineFramingTransposer transposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
                offsetValue = new Vector2(transposer.m_ScreenX, transposer.m_ScreenY);
                lookaheadTimeValue = transposer.m_LookaheadTime;
                lookaheadSmoothingValue = transposer.m_LookaheadSmoothing;
                ignoreY = transposer.m_LookaheadIgnoreY;
                deadzoneValue = new Vector2(transposer.m_DeadZoneWidth, transposer.m_DeadZoneHeight);
                dampingValue = new Vector2(transposer.m_XDamping, transposer.m_YDamping);
            }
            else
            {
                useOffset = false;
                useLookAhead = false;
                useDeadZone = false;
                useDamping = false;
            }

            if (virtualCamera.GetComponent<LockCameraXY>() != null)
            {
                LockCameraXY lockCam = virtualCamera.GetComponent<LockCameraXY>();

                lockCenterTarget = lockCam.lockTarget;
                lockCenterLerp = lockCam.lerpLock;
                lockLeftTarget = lockCam.leftWall;
                lockLeftLerp = lockCam.lerpLeft;
                lockRightTarget = lockCam.rightWall;
                lockRightLerp = lockCam.lerpRight;
                lockTopTarget = lockCam.topWall;
                lockTopLerp = lockCam.lerpTop;
                lockBottomTarget = lockCam.bottomWall;
                lockBottomLerp = lockCam.lerpBottom;
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
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (maxUses > 0 && uses >= maxUses)
        {
            GetComponent<Collider2D>().enabled = false;
            return;
        }

        if (collision.gameObject.tag == "Player")
        {
            uses++;

            if(useZoom)
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
                cameraControl.StartLookAhead(lookaheadTimeValue, lookaheadSmoothingValue, lookaheadDuration, ignoreY, lookaheadEase);
            }
            if (useDeadZone)
            {
                cameraControl.StartDeadZone(deadzoneValue.x, deadzoneValue.y, deadzoneDuration, deadzoneEase);
            }
            if (useDamping)
            {
                cameraControl.StartDamping(dampingValue.x, dampingValue.y, dampingDuration, dampingEase);
            }
            if (useLockCenter)
            {
                cameraControl.StartLockCenter(lockCenterTarget, lockCenterLerp, lockCenterDuration, lockCenterEase);
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
    }

    // -----------------------------------

    public enum Speed
    {
        x0, x1, x2, x3, x4
    }
    public Speed speed = Speed.x1;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
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

        gameObject.transform.GetChild(0).GetChild(1).GetComponent<TextMeshPro>().color = useZoom ? Color.white : new Color(1,1,1,.25f);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position + (Vector3.down * .4f), transform.position + (Vector3.down * .4f) + new Vector3((scale * Time.fixedDeltaTime * 10f) * zoomDuration, 0, 0));

        gameObject.transform.GetChild(0).GetChild(2).GetComponent<TextMeshPro>().color = useRotation ? Color.white : new Color(1, 1, 1, .25f);
        Gizmos.color = new Color(.5f, 0, 1f);
        Gizmos.DrawLine(transform.position + (Vector3.down * .6f), transform.position + (Vector3.down * .6f) + new Vector3((scale * Time.fixedDeltaTime * 10f) * rotationDuration, 0, 0));

        gameObject.transform.GetChild(0).GetChild(3).GetComponent<TextMeshPro>().color = useOffset ? Color.white : new Color(1, 1, 1, .25f);
        Gizmos.color = new Color(0, 1f, 1f);
        Gizmos.DrawLine(transform.position + (Vector3.down * .8f), transform.position + (Vector3.down * .8f) + new Vector3((scale * Time.fixedDeltaTime * 10f) * offsetDuration, 0, 0));

        gameObject.transform.GetChild(0).GetChild(4).GetComponent<TextMeshPro>().color = useLookAhead ? Color.white : new Color(1, 1, 1, .25f);
        Gizmos.color = new Color(1f, 1f, 0f);
        Gizmos.DrawLine(transform.position + (Vector3.down * 1f), transform.position + (Vector3.down * 1f) + new Vector3((scale * Time.fixedDeltaTime * 10f) * lookaheadDuration, 0, 0));

        gameObject.transform.GetChild(0).GetChild(5).GetComponent<TextMeshPro>().color = useDeadZone ? Color.white : new Color(1, 1, 1, .25f);
        Gizmos.color = new Color(1f, 0f, 0f);
        Gizmos.DrawLine(transform.position + (Vector3.down * 1.2f), transform.position + (Vector3.down * 1.2f) + new Vector3((scale * Time.fixedDeltaTime * 10f) * deadzoneDuration, 0, 0));

        gameObject.transform.GetChild(0).GetChild(6).GetComponent<TextMeshPro>().color = useDamping ? Color.white : new Color(1, 1, 1, .25f);
        Gizmos.color = new Color(1f, 0f, 1f);
        Gizmos.DrawLine(transform.position + (Vector3.down * 1.4f), transform.position + (Vector3.down * 1.4f) + new Vector3((scale * Time.fixedDeltaTime * 10f) * dampingDuration, 0, 0));

        gameObject.transform.GetChild(0).GetChild(7).GetComponent<TextMeshPro>().color = useLockCenter ? Color.white : new Color(1, 1, 1, .25f);
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position + (Vector3.down * 1.6f), transform.position + (Vector3.down * 1.6f) + new Vector3((scale * Time.fixedDeltaTime * 10f) * lockCenterDuration, 0, 0));

        gameObject.transform.GetChild(0).GetChild(8).GetComponent<TextMeshPro>().color = useLockLeft ? Color.white : new Color(1, 1, 1, .25f);
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position + (Vector3.down * 1.8f), transform.position + (Vector3.down * 1.8f) + new Vector3((scale * Time.fixedDeltaTime * 10f) * lockLeftDuration, 0, 0));

        gameObject.transform.GetChild(0).GetChild(9).GetComponent<TextMeshPro>().color = useLockRight ? Color.white : new Color(1, 1, 1, .25f);
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position + (Vector3.down * 2f), transform.position + (Vector3.down * 2f) + new Vector3((scale * Time.fixedDeltaTime * 10f) * lockRightDuration, 0, 0));

        gameObject.transform.GetChild(0).GetChild(10).GetComponent<TextMeshPro>().color = useLockTop ? Color.white : new Color(1, 1, 1, .25f);
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position + (Vector3.down * 2.2f), transform.position + (Vector3.down * 2.2f) + new Vector3((scale * Time.fixedDeltaTime * 10f) * lockTopDuration, 0, 0));

        gameObject.transform.GetChild(0).GetChild(11).GetComponent<TextMeshPro>().color = useLockBottom ? Color.white : new Color(1, 1, 1, .25f);
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position + (Vector3.down * 2.4f), transform.position + (Vector3.down * 2.4f) + new Vector3((scale * Time.fixedDeltaTime * 10f) * lockBottomDuration, 0, 0));
    }
#endif
}
