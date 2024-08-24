using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraControl : MonoBehaviour
{
    private CinemachineVirtualCamera virtualCamera;

    //Coroutines
    [HideInInspector] public Coroutine zoomCoroutine;
    [HideInInspector] public Coroutine rotationCoroutine;
    [HideInInspector] public Coroutine offsetCoroutine;
    [HideInInspector] public Coroutine lookaheadCoroutine;
    [HideInInspector] public Coroutine deadzoneCoroutine;
    [HideInInspector] public Coroutine dampingCoroutine;
    [HideInInspector] public Coroutine lockCenterCoroutine;
    [HideInInspector] public Coroutine lockLeftCoroutine;
    [HideInInspector] public Coroutine lockRightCoroutine;
    [HideInInspector] public Coroutine lockTopCoroutine;
    [HideInInspector] public Coroutine lockBottomCoroutine;

    Transform emptyLockCenter, emptyLockLeft, emptyLockRight, emptyLockTop, emptyLockBottom;

    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    public void StartZoom(float endValue, float duration, EasingFunction.Ease ease)
    {
        if (zoomCoroutine != null) { StopCoroutine(zoomCoroutine); }
        zoomCoroutine = StartCoroutine(SetZoom(endValue, duration, ease));
    }

    public IEnumerator SetZoom(float endValue, float duration, EasingFunction.Ease ease)
    {
        // break if already at value
        if(virtualCamera.m_Lens.OrthographicSize == endValue) { yield break; }

        // set parameters
        float startValue = virtualCamera.m_Lens.OrthographicSize;
        float time = duration != 0 ? 0 : 1;
        duration = duration != 0 ? duration : 1;

        // ease value
        while (time < duration)
        {
            virtualCamera.m_Lens.OrthographicSize = EasingFunction.GetEasingFunction(ease)(startValue, endValue, Mathf.Clamp01(time / duration));
            time += Time.deltaTime;
            yield return null;
        }

        // set end value
        time = duration;
        virtualCamera.m_Lens.OrthographicSize = EasingFunction.GetEasingFunction(ease)(startValue, endValue, Mathf.Clamp01(time / duration));
    }

    public void StartRotation(float endValue, float duration, EasingFunction.Ease ease)
    {
        if (rotationCoroutine != null) { StopCoroutine(rotationCoroutine); }
        rotationCoroutine = StartCoroutine(SetRotation(endValue, duration, ease));
    }

    public IEnumerator SetRotation(float endValue, float duration, EasingFunction.Ease ease)
    {
        // break if already at value
        if (virtualCamera.m_Lens.Dutch == endValue) { yield break; }

        // set parameters
        float startValue = virtualCamera.m_Lens.Dutch;
        float time = duration != 0 ? 0 : 1;
        duration = duration != 0 ? duration : 1;

        // ease value
        while (time < duration)
        {
            virtualCamera.m_Lens.Dutch = EasingFunction.GetEasingFunction(ease)(startValue, endValue, Mathf.Clamp01(time / duration));
            time += Time.deltaTime;
            yield return null;
        }

        // set end value
        time = duration;
        virtualCamera.m_Lens.Dutch = EasingFunction.GetEasingFunction(ease)(startValue, endValue, Mathf.Clamp01(time / duration));

        // clamp value between 0 and 360
        virtualCamera.m_Lens.Dutch = endValue % 360;
    }

    public void StartOffset(float endXValue, float endYValue, float duration, EasingFunction.Ease ease)
    {
        if (offsetCoroutine != null) { StopCoroutine(offsetCoroutine); }
        offsetCoroutine = StartCoroutine(SetOffset(endXValue, endYValue, duration, ease));
    }

    public IEnumerator SetOffset(float endXValue, float endYValue, float duration, EasingFunction.Ease ease)
    {
        CinemachineFramingTransposer transposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        endXValue /= -2f; endYValue /= -2f;
        endXValue += 0.5f; endYValue += 0.5f;

        // break if already at value
        if (transposer.m_ScreenX == endXValue && transposer.m_ScreenY == endYValue) { yield break; }

        // set parameters
        float startXValue = transposer.m_ScreenX;
        float startYValue = transposer.m_ScreenY;
        float time = duration != 0 ? 0 : 1;
        duration = duration != 0 ? duration : 1;

        // ease value
        while (time < duration)
        {
            transposer.m_ScreenX = EasingFunction.GetEasingFunction(ease)(startXValue, endXValue, Mathf.Clamp01(time / duration));
            transposer.m_ScreenY = EasingFunction.GetEasingFunction(ease)(startYValue, endYValue, Mathf.Clamp01(time / duration));
            time += Time.deltaTime;
            yield return null;
        }

        // set end value
        time = duration;
        transposer.m_ScreenX = EasingFunction.GetEasingFunction(ease)(startXValue, endXValue, Mathf.Clamp01(time / duration));
        transposer.m_ScreenY = EasingFunction.GetEasingFunction(ease)(startYValue, endYValue, Mathf.Clamp01(time / duration));
    }

    public void StartLookAhead(float endXValue, float endYValue, float duration, bool ignoreY, EasingFunction.Ease ease, bool resetLookAhead = false)
    {
        if (lookaheadCoroutine != null) { StopCoroutine(lookaheadCoroutine); }
        lookaheadCoroutine = StartCoroutine(SetLookAhead(endXValue, endYValue, duration, ignoreY, ease, resetLookAhead));
    }

    public IEnumerator SetLookAhead(float endXValue, float endYValue, float duration, bool ignoreY, EasingFunction.Ease ease, bool resetLookAhead = false)
    {
        CinemachineFramingTransposer transposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        transposer.m_LookaheadIgnoreY = ignoreY;

        // reset lookahead
        if (resetLookAhead)
        {
            transposer.m_LookaheadTime = 0;
            virtualCamera.enabled = false;
            virtualCamera.Follow = virtualCamera.Follow;
            virtualCamera.enabled = true;
        }

        // break if already at value
        if (transposer.m_LookaheadTime == endXValue && transposer.m_LookaheadSmoothing == endYValue) { yield break; }

        // set parameters
        float startXValue = transposer.m_LookaheadTime;
        float startYValue = transposer.m_LookaheadSmoothing;
        float time = duration != 0 ? 0 : 1;
        duration = duration != 0 ? duration : 1;

        // ease value
        while (time < duration)
        {
            transposer.m_LookaheadTime = EasingFunction.GetEasingFunction(ease)(startXValue, endXValue, Mathf.Clamp01(time / duration));
            transposer.m_LookaheadSmoothing = EasingFunction.GetEasingFunction(ease)(startYValue, endYValue, Mathf.Clamp01(time / duration));
            time += Time.deltaTime;
            yield return null;
        }

        // set end value
        time = duration;
        transposer.m_LookaheadTime = EasingFunction.GetEasingFunction(ease)(startXValue, endXValue, Mathf.Clamp01(time / duration));
        transposer.m_LookaheadSmoothing = EasingFunction.GetEasingFunction(ease)(startYValue, endYValue, Mathf.Clamp01(time / duration));
    }

    public void StartDeadZone(float endXValue, float endYValue, float duration, EasingFunction.Ease ease)
    {
        if (deadzoneCoroutine != null) { StopCoroutine(deadzoneCoroutine); }
        deadzoneCoroutine = StartCoroutine(SetDeadZone(endXValue, endYValue, duration, ease));
    }

    public IEnumerator SetDeadZone(float endXValue, float endYValue, float duration, EasingFunction.Ease ease)
    {
        CinemachineFramingTransposer transposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();

        // break if already at value
        if (transposer.m_DeadZoneWidth == endXValue && transposer.m_DeadZoneHeight == endYValue) { yield break; }

        // set parameters
        float startXValue = transposer.m_DeadZoneWidth;
        float startYValue = transposer.m_DeadZoneHeight;
        float time = duration != 0 ? 0 : 1;
        duration = duration != 0 ? duration : 1;

        // ease value
        while (time < duration)
        {
            transposer.m_DeadZoneWidth = EasingFunction.GetEasingFunction(ease)(startXValue, endXValue, Mathf.Clamp01(time / duration));
            transposer.m_DeadZoneHeight = EasingFunction.GetEasingFunction(ease)(startYValue, endYValue, Mathf.Clamp01(time / duration));
            time += Time.deltaTime;
            yield return null;
        }

        // set end value
        time = duration;
        transposer.m_DeadZoneWidth = EasingFunction.GetEasingFunction(ease)(startXValue, endXValue, Mathf.Clamp01(time / duration));
        transposer.m_DeadZoneHeight = EasingFunction.GetEasingFunction(ease)(startYValue, endYValue, Mathf.Clamp01(time / duration));
    }

    public void StartDamping(float endXValue, float endYValue, float duration, EasingFunction.Ease ease)
    {
        if (dampingCoroutine != null) { StopCoroutine(dampingCoroutine); }
        dampingCoroutine = StartCoroutine(SetDamping(endXValue, endYValue, duration, ease));
    }

    public IEnumerator SetDamping(float endXValue, float endYValue, float duration, EasingFunction.Ease ease)
    {
        CinemachineFramingTransposer transposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();

        // break if already at value
        if (transposer.m_XDamping == endXValue && transposer.m_YDamping == endYValue) { yield break; }

        // set parameters
        float startXValue = transposer.m_XDamping;
        float startYValue = transposer.m_YDamping;
        float time = duration != 0 ? 0 : 1;
        duration = duration != 0 ? duration : 1;

        // ease value
        while (time < duration)
        {
            transposer.m_XDamping = EasingFunction.GetEasingFunction(ease)(startXValue, endXValue, Mathf.Clamp01(time / duration));
            transposer.m_YDamping = EasingFunction.GetEasingFunction(ease)(startYValue, endYValue, Mathf.Clamp01(time / duration));
            time += Time.deltaTime;
            yield return null;
        }

        // set end value
        time = duration;
        transposer.m_XDamping = EasingFunction.GetEasingFunction(ease)(startXValue, endXValue, Mathf.Clamp01(time / duration));
        transposer.m_YDamping = EasingFunction.GetEasingFunction(ease)(startYValue, endYValue, Mathf.Clamp01(time / duration));
    }


    // -------------------------------------------- LOCKS


    public void StartLockCenter(Transform target, Vector2 offset, LockCameraXY.LockType type, float endLerpValue, float duration, EasingFunction.Ease ease, bool startFromCenter = false)
    {
        if (lockCenterCoroutine != null) { StopCoroutine(lockCenterCoroutine); }
        lockCenterCoroutine = StartCoroutine(SetLockCenter(target, offset, type, endLerpValue, duration, ease, startFromCenter));
    }

    public IEnumerator SetLockCenter(Transform target, Vector2 offset, LockCameraXY.LockType type, float endLerpValue, float duration, EasingFunction.Ease ease, bool startFromCenter = false)
    {
        LockCameraXY lockCam = virtualCamera.GetComponent<LockCameraXY>();
        
        // break if already at value
        if ((lockCam.lockTarget == target && lockCam.lerpLock == endLerpValue && lockCam.offset == offset && lockCam.lockAxis == type) || (lockCam.lockTarget == null && target == null)) { yield break; }

        if(emptyLockCenter == null)
        {
            emptyLockCenter = new GameObject().transform;
            emptyLockCenter.gameObject.name = "Empty Lock Center: " + this.GetHashCode();
        }

        // set parameters
        float startLerpValue = lockCam.lerpLock;
        if(lockCam.lockTarget == null && target != null)
        {
            lockCam.lerpLock = 0;
            startLerpValue = 0;

            //emptyLockCenter.position = target.position;
            emptyLockCenter.position = lockCam.GetCamPosition("center");
            lockCam.lockTarget = emptyLockCenter;
        }
        else if (lockCam.lockTarget != null && target == null)
        {
            lockCam.lerpLock = 1;
            startLerpValue = 1;
            endLerpValue = 0;
        }

        //switch((lockCam.lockAxis, type))
        //{
        //    case (LockCameraXY.LockType.lock_Y, LockCameraXY.LockType.lock_XY):
        //        target.position = target.position.SetX(lockCam.GetCamPosition("center").x);
        //        break;
        //    case (LockCameraXY.LockType.lock_X, LockCameraXY.LockType.lock_XY):
        //        target.position = target.position.SetY(lockCam.GetCamPosition("center").y);
        //        break;
        //}

        if (lockCam.lockTarget != null && lockCam.lockTarget.parent != null && target != null && lockCam.lockAxis == LockCameraXY.LockType.lock_Y && type == LockCameraXY.LockType.lock_XY)
        {
            lockCam.lockTarget.parent.position = lockCam.lockTarget.parent.position.SetX(target.position.x);
            lockCam.lockTarget.localPosition = lockCam.lockTarget.localPosition.SetX(0);
        }
        else if (lockCam.lockTarget != null && lockCam.lockTarget.parent && target != null && lockCam.lockAxis == LockCameraXY.LockType.lock_X && type == LockCameraXY.LockType.lock_XY)
        {
            lockCam.lockTarget.parent.position = lockCam.lockTarget.parent.position.SetY(target.position.y);
            lockCam.lockTarget.localPosition = lockCam.lockTarget.localPosition.SetY(0);
        }

        lockCam.lockTarget.parent = target;

        Vector2 startTargetPos = lockCam.lockTarget != null ? (startFromCenter ? lockCam.GetCamPosition("center") : lockCam.lockTarget.position) : target.position;
        Vector2 endTargetPos = target != null ? target.position : lockCam.lockTarget.position;

        Vector2 startOffset = lockCam.offset;
        float time = duration != 0 ? 0 : 1;
        duration = duration != 0 ? duration : 1;

        float startAxisXWeight = lockCam.lerpAxisX;
        float startAxisYWeight = lockCam.lerpAxisY;
        float startAxisXYWeight = lockCam.lerpAxisXY;
        float endAxisXWeight = type == LockCameraXY.LockType.lock_X ? 1 : 0;
        float endAxisYWeight = type == LockCameraXY.LockType.lock_Y ? 1 : 0;
        float endAxisXYWeight = type == LockCameraXY.LockType.lock_XY ? 1 : 0;

        lockCam.lockAxis = type;

        // ease value
        while (time < duration)
        {
            lockCam.lerpLock = EasingFunction.GetEasingFunction(ease)(startLerpValue, endLerpValue, Mathf.Clamp01(time / duration));

            lockCam.lockTarget.position = new Vector3(EasingFunction.GetEasingFunction(ease)(startTargetPos.x, endTargetPos.x, Mathf.Clamp01(time / duration)),
                                                        EasingFunction.GetEasingFunction(ease)(startTargetPos.y, endTargetPos.y, Mathf.Clamp01(time / duration)),
                                                        lockCam.lockTarget.position.z);

            lockCam.offset = new Vector2(EasingFunction.GetEasingFunction(ease)(startOffset.x, offset.x, Mathf.Clamp01(time / duration)),
                                        EasingFunction.GetEasingFunction(ease)(startOffset.y, offset.y, Mathf.Clamp01(time / duration)));

            lockCam.lerpAxisX = EasingFunction.GetEasingFunction(ease)(startAxisXWeight, endAxisXWeight, Mathf.Clamp01(time / duration));
            lockCam.lerpAxisY = EasingFunction.GetEasingFunction(ease)(startAxisYWeight, endAxisYWeight, Mathf.Clamp01(time / duration));
            lockCam.lerpAxisXY = EasingFunction.GetEasingFunction(ease)(startAxisXYWeight, endAxisXYWeight, Mathf.Clamp01(time / duration));

            time += Time.deltaTime;
            yield return null;
        }

        // set end value
        time = duration;
        lockCam.lerpLock = EasingFunction.GetEasingFunction(ease)(startLerpValue, endLerpValue, Mathf.Clamp01(time / duration));

        lockCam.lockTarget.position = new Vector3(EasingFunction.GetEasingFunction(ease)(startTargetPos.x, endTargetPos.x, Mathf.Clamp01(time / duration)),
                                                    EasingFunction.GetEasingFunction(ease)(startTargetPos.y, endTargetPos.y, Mathf.Clamp01(time / duration)),
                                                    lockCam.lockTarget.position.z);

        lockCam.offset = new Vector2(EasingFunction.GetEasingFunction(ease)(startOffset.x, offset.x, Mathf.Clamp01(time / duration)),
                                        EasingFunction.GetEasingFunction(ease)(startOffset.y, offset.y, Mathf.Clamp01(time / duration)));

        lockCam.lerpAxisX = EasingFunction.GetEasingFunction(ease)(startAxisXWeight, endAxisXWeight, Mathf.Clamp01(time / duration));
        lockCam.lerpAxisY = EasingFunction.GetEasingFunction(ease)(startAxisYWeight, endAxisYWeight, Mathf.Clamp01(time / duration));
        lockCam.lerpAxisXY = EasingFunction.GetEasingFunction(ease)(startAxisXYWeight, endAxisXYWeight, Mathf.Clamp01(time / duration));

        if (target == null || endLerpValue == 0) { lockCam.lockTarget = null; }
        //lockCam.lockAxis = type;
    }

    public void StartLockLeft(Transform target, float endLerpValue, float duration, EasingFunction.Ease ease)
    {
        if (lockLeftCoroutine != null) { StopCoroutine(lockLeftCoroutine); }
        lockLeftCoroutine = StartCoroutine(SetLockLeft(target, endLerpValue, duration, ease));
    }

    public IEnumerator SetLockLeft(Transform target, float endLerpValue, float duration, EasingFunction.Ease ease)
    {
        LockCameraXY lockCam = virtualCamera.GetComponent<LockCameraXY>();

        // break if already at value
        if ((lockCam.leftWall == target && lockCam.lerpLeft == endLerpValue) || (lockCam.leftWall == null && target == null)) { yield break; }

        if (emptyLockLeft == null)
        {
            emptyLockLeft = new GameObject().transform;
            emptyLockLeft.gameObject.name = "Empty Lock Left: " + this.GetHashCode();
        }

        // set parameters
        float startLerpValue = lockCam.lerpLeft;
        if (lockCam.leftWall == null && target != null)
        {
            lockCam.lerpLeft = 0;
            startLerpValue = 0;

            emptyLockLeft.position = target.position;
            lockCam.leftWall = emptyLockLeft;
        }
        else if (lockCam.leftWall != null && target == null)
        {
            lockCam.lerpLeft = 1;
            startLerpValue = 1;
            endLerpValue = 0;
        }

        lockCam.leftWall.parent = target;

        Vector2 startTargetPos = lockCam.leftWall != null ? lockCam.GetCamPosition("left")/* lockCam.leftWall.position*/ : target.position;
        Vector2 endTargetPos = target != null ? target.position : lockCam.leftWall.position;

        float time = duration != 0 ? 0 : 1;
        duration = duration != 0 ? duration : 1;

        // ease value
        while (time < duration)
        {
            lockCam.lerpLeft = EasingFunction.GetEasingFunction(ease)(startLerpValue, endLerpValue, Mathf.Clamp01(time / duration));

            lockCam.leftWall.position = new Vector3(EasingFunction.GetEasingFunction(ease)(startTargetPos.x, endTargetPos.x, Mathf.Clamp01(time / duration)),
                                                    EasingFunction.GetEasingFunction(ease)(startTargetPos.y, endTargetPos.y, Mathf.Clamp01(time / duration)),
                                                    lockCam.leftWall.position.z);
            time += Time.deltaTime;
            yield return null;
        }

        // set end value
        time = duration;
        lockCam.lerpLeft = EasingFunction.GetEasingFunction(ease)(startLerpValue, endLerpValue, Mathf.Clamp01(time / duration));

        lockCam.leftWall.position = new Vector3(EasingFunction.GetEasingFunction(ease)(startTargetPos.x, endTargetPos.x, Mathf.Clamp01(time / duration)),
                                                EasingFunction.GetEasingFunction(ease)(startTargetPos.y, endTargetPos.y, Mathf.Clamp01(time / duration)),
                                                lockCam.leftWall.position.z);

        if (target == null || endLerpValue == 0) { lockCam.leftWall = null; }
    }

    public void StartLockRight(Transform target, float endLerpValue, float duration, EasingFunction.Ease ease)
    {
        if (lockRightCoroutine != null) { StopCoroutine(lockRightCoroutine); }
        lockRightCoroutine = StartCoroutine(SetLockRight(target, endLerpValue, duration, ease));
    }

    public IEnumerator SetLockRight(Transform target, float endLerpValue, float duration, EasingFunction.Ease ease)
    {
        LockCameraXY lockCam = virtualCamera.GetComponent<LockCameraXY>();

        // break if already at value
        if ((lockCam.rightWall == target && lockCam.lerpRight == endLerpValue) || (lockCam.rightWall == null && target == null)) { yield break; }

        if (emptyLockRight == null)
        {
            emptyLockRight = new GameObject().transform;
            emptyLockRight.gameObject.name = "Empty Lock Right: " + this.GetHashCode();
        }

        // set parameters
        float startLerpValue = lockCam.lerpRight;
        if (lockCam.rightWall == null && target != null)
        {
            lockCam.lerpRight = 0;
            startLerpValue = 0;

            emptyLockRight.position = target.position;
            lockCam.rightWall = emptyLockRight;
        }
        else if (lockCam.rightWall != null && target == null)
        {
            lockCam.lerpRight = 1;
            startLerpValue = 1;
            endLerpValue = 0;
        }

        lockCam.rightWall.parent = target;

        Vector2 startTargetPos = lockCam.rightWall != null ? lockCam.GetCamPosition("right")/*lockCam.rightWall.position*/ : target.position;
        Vector2 endTargetPos = target != null ? target.position : lockCam.rightWall.position;

        float time = duration != 0 ? 0 : 1;
        duration = duration != 0 ? duration : 1;

        // ease value
        while (time < duration)
        {
            lockCam.lerpRight = EasingFunction.GetEasingFunction(ease)(startLerpValue, endLerpValue, Mathf.Clamp01(time / duration));

            lockCam.rightWall.position = new Vector3(EasingFunction.GetEasingFunction(ease)(startTargetPos.x, endTargetPos.x, Mathf.Clamp01(time / duration)),
                                                    EasingFunction.GetEasingFunction(ease)(startTargetPos.y, endTargetPos.y, Mathf.Clamp01(time / duration)),
                                                    lockCam.rightWall.position.z);
            time += Time.deltaTime;
            yield return null;
        }

        // set end value
        time = duration;
        lockCam.lerpRight = EasingFunction.GetEasingFunction(ease)(startLerpValue, endLerpValue, Mathf.Clamp01(time / duration));

        lockCam.rightWall.position = new Vector3(EasingFunction.GetEasingFunction(ease)(startTargetPos.x, endTargetPos.x, Mathf.Clamp01(time / duration)),
                                                EasingFunction.GetEasingFunction(ease)(startTargetPos.y, endTargetPos.y, Mathf.Clamp01(time / duration)),
                                                lockCam.rightWall.position.z);

        if (target == null || endLerpValue == 0) { lockCam.rightWall = null; }
    }

    public void StartLockTop(Transform target, float endLerpValue, float duration, EasingFunction.Ease ease)
    {
        if (lockTopCoroutine != null) { StopCoroutine(lockTopCoroutine); }
        lockTopCoroutine = StartCoroutine(SetLockTop(target, endLerpValue, duration, ease));
    }

    public IEnumerator SetLockTop(Transform target, float endLerpValue, float duration, EasingFunction.Ease ease)
    {
        LockCameraXY lockCam = virtualCamera.GetComponent<LockCameraXY>();

        // break if already at value
        if ((lockCam.topWall == target && lockCam.lerpTop == endLerpValue) || (lockCam.topWall == null && target == null)) { yield break; }

        if (emptyLockTop == null)
        {
            emptyLockTop = new GameObject().transform;
            emptyLockTop.gameObject.name = "Empty Lock Top: " + this.GetHashCode();
        }

        // set parameters
        float startLerpValue = lockCam.lerpTop;
        if (lockCam.topWall == null && target != null)
        {
            lockCam.lerpTop = 0;
            startLerpValue = 0;

            emptyLockTop.position = target.position;
            lockCam.topWall = emptyLockTop;
        }
        else if (lockCam.topWall != null && target == null)
        {
            lockCam.lerpTop = 1;
            startLerpValue = 1;
            endLerpValue = 0;
        }

        lockCam.topWall.parent = target;

        Vector2 startTargetPos = lockCam.topWall != null ? lockCam.GetCamPosition("top")/*lockCam.topWall.position*/ : target.position;
        Vector2 endTargetPos = target != null ? target.position : lockCam.topWall.position;

        float time = duration != 0 ? 0 : 1;
        duration = duration != 0 ? duration : 1;

        // ease value
        while (time < duration)
        {
            lockCam.lerpTop = EasingFunction.GetEasingFunction(ease)(startLerpValue, endLerpValue, Mathf.Clamp01(time / duration));

            lockCam.topWall.position = new Vector3(EasingFunction.GetEasingFunction(ease)(startTargetPos.x, endTargetPos.x, Mathf.Clamp01(time / duration)),
                                                    EasingFunction.GetEasingFunction(ease)(startTargetPos.y, endTargetPos.y, Mathf.Clamp01(time / duration)),
                                                    lockCam.topWall.position.z);
            time += Time.deltaTime;
            yield return null;
        }

        // set end value
        time = duration;
        lockCam.lerpTop = EasingFunction.GetEasingFunction(ease)(startLerpValue, endLerpValue, Mathf.Clamp01(time / duration));

        lockCam.topWall.position = new Vector3(EasingFunction.GetEasingFunction(ease)(startTargetPos.x, endTargetPos.x, Mathf.Clamp01(time / duration)),
                                                EasingFunction.GetEasingFunction(ease)(startTargetPos.y, endTargetPos.y, Mathf.Clamp01(time / duration)),
                                                lockCam.topWall.position.z);

        if (target == null || endLerpValue == 0) { lockCam.topWall = null; }
    }

    public void StartLockBottom(Transform target, float endLerpValue, float duration, EasingFunction.Ease ease)
    {
        if (lockBottomCoroutine != null) { StopCoroutine(lockBottomCoroutine); }
        lockBottomCoroutine = StartCoroutine(SetLockBottom(target, endLerpValue, duration, ease));
    }

    public IEnumerator SetLockBottom(Transform target, float endLerpValue, float duration, EasingFunction.Ease ease)
    {
        LockCameraXY lockCam = virtualCamera.GetComponent<LockCameraXY>();

        // break if already at value
        if ((lockCam.bottomWall == target && lockCam.lerpBottom == endLerpValue) || (lockCam.bottomWall == null && target == null)) { yield break; }

        if (emptyLockBottom == null)
        {
            emptyLockBottom = new GameObject().transform;
            emptyLockBottom.gameObject.name = "Empty Lock Bottom: " + this.GetHashCode();
        }

        // set parameters
        float startLerpValue = lockCam.lerpBottom;
        if (lockCam.bottomWall == null && target != null)
        {
            lockCam.lerpBottom = 0;
            startLerpValue = 0;

            emptyLockBottom.position = target.position;
            lockCam.bottomWall = emptyLockBottom;
        }
        else if (lockCam.bottomWall != null && target == null)
        {
            lockCam.lerpBottom = 1;
            startLerpValue = 1;
            endLerpValue = 0;
        }

        lockCam.bottomWall.parent = target;

        Vector2 startTargetPos = lockCam.bottomWall != null ? lockCam.GetCamPosition("bottom")/*lockCam.bottomWall.position*/ : target.position;
        Vector2 endTargetPos = target != null ? target.position : lockCam.bottomWall.position;

        float time = duration != 0 ? 0 : 1;
        duration = duration != 0 ? duration : 1;

        // ease value
        while (time < duration)
        {
            lockCam.lerpBottom = EasingFunction.GetEasingFunction(ease)(startLerpValue, endLerpValue, Mathf.Clamp01(time / duration));

            lockCam.bottomWall.position = new Vector3(EasingFunction.GetEasingFunction(ease)(startTargetPos.x, endTargetPos.x, Mathf.Clamp01(time / duration)),
                                                    EasingFunction.GetEasingFunction(ease)(startTargetPos.y, endTargetPos.y, Mathf.Clamp01(time / duration)),
                                                    lockCam.bottomWall.position.z);
            time += Time.deltaTime;
            yield return null;
        }

        // set end value
        time = duration;
        lockCam.lerpBottom = EasingFunction.GetEasingFunction(ease)(startLerpValue, endLerpValue, Mathf.Clamp01(time / duration));

        lockCam.bottomWall.position = new Vector3(EasingFunction.GetEasingFunction(ease)(startTargetPos.x, endTargetPos.x, Mathf.Clamp01(time / duration)),
                                                EasingFunction.GetEasingFunction(ease)(startTargetPos.y, endTargetPos.y, Mathf.Clamp01(time / duration)),
                                                lockCam.bottomWall.position.z);

        if (target == null || endLerpValue == 0) { lockCam.bottomWall = null; }
    }

    private bool stopOnDeath;
    void ActivateOnDeath()
    {
        if (!stopOnDeath)
            return;

        stopOnDeath = false;
        StopAllCoroutines();
        //StartLockCenter(Camera.main.transform, Vector2.zero, LockCameraXY.LockType.lock_Y, 0, 1, EasingFunction.Ease.EaseInQuad);
    }

    void SetStopOnDeath(bool stop)
    {
        stopOnDeath = stop;
    }

    void OnEnable()
    {
        PlayerControllerV2.OnDeath += ActivateOnDeath;
        CameraTrigger.OnActivate += SetStopOnDeath;
    }


    void OnDisable()
    {
        PlayerControllerV2.OnDeath -= ActivateOnDeath;
        CameraTrigger.OnActivate -= SetStopOnDeath;
    }
}
