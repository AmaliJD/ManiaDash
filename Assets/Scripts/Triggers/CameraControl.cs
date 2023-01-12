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
        endXValue *= -1f; endYValue *= -1f;
        endXValue /= 2f; endYValue /= 2f;
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

    public void StartLookAhead(float endXValue, float endYValue, float duration, bool ignoreY, EasingFunction.Ease ease)
    {
        if (lookaheadCoroutine != null) { StopCoroutine(lookaheadCoroutine); }
        lookaheadCoroutine = StartCoroutine(SetLookAhead(endXValue, endYValue, duration, ignoreY, ease));
    }

    public IEnumerator SetLookAhead(float endXValue, float endYValue, float duration, bool ignoreY, EasingFunction.Ease ease)
    {
        CinemachineFramingTransposer transposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        transposer.m_LookaheadIgnoreY = ignoreY;

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


    public void StartLockCenter(Transform target, float endLerpValue, float duration, EasingFunction.Ease ease)
    {
        if (lockCenterCoroutine != null) { StopCoroutine(lockCenterCoroutine); }
        lockCenterCoroutine = StartCoroutine(SetLockCenter(target, endLerpValue, duration, ease));
    }

    public IEnumerator SetLockCenter(Transform target, float endLerpValue, float duration, EasingFunction.Ease ease)
    {
        LockCameraXY lockCam = virtualCamera.GetComponent<LockCameraXY>();
        
        // break if already at value
        if ((lockCam.lockTarget == target && lockCam.lerpLock == endLerpValue) || (lockCam.lockTarget == null && target == null)) { yield break; }

        // set parameters
        float startLerpValue = lockCam.lerpLock;
        if(lockCam.lockTarget == null && target != null)
        {
            lockCam.lerpLock = 0;
            startLerpValue = 0;

            lockCam.lockTarget = target;
        }
        else if (lockCam.lockTarget != null && target == null)
        {
            lockCam.lerpLock = 1;
            startLerpValue = 1;
            endLerpValue = 0;
        }

        Vector2 startTargetPos = lockCam.lockTarget != null ? lockCam.lockTarget.position : target.position;
        Vector2 endTargetPos = target != null ? target.position : lockCam.lockTarget.position;

        float time = duration != 0 ? 0 : 1;
        duration = duration != 0 ? duration : 1;

        // ease value
        while (time < duration)
        {
            lockCam.lerpLock = EasingFunction.GetEasingFunction(ease)(startLerpValue, endLerpValue, Mathf.Clamp01(time / duration));

            lockCam.lockTarget.position = new Vector3(EasingFunction.GetEasingFunction(ease)(startTargetPos.x, endTargetPos.x, Mathf.Clamp01(time / duration)),
                                                        EasingFunction.GetEasingFunction(ease)(startTargetPos.y, endTargetPos.y, Mathf.Clamp01(time / duration)),
                                                        lockCam.lockTarget.position.z);
            time += Time.deltaTime;
            yield return null;
        }

        // set end value
        time = duration;
        lockCam.lerpLock = EasingFunction.GetEasingFunction(ease)(startLerpValue, endLerpValue, Mathf.Clamp01(time / duration));

        lockCam.lockTarget.position = new Vector3(EasingFunction.GetEasingFunction(ease)(startTargetPos.x, endTargetPos.x, Mathf.Clamp01(time / duration)),
                                                    EasingFunction.GetEasingFunction(ease)(startTargetPos.y, endTargetPos.y, Mathf.Clamp01(time / duration)),
                                                    lockCam.lockTarget.position.z);

        if(target == null || endLerpValue == 0) { lockCam.lockTarget = null; }
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

        // set parameters
        float startLerpValue = lockCam.lerpLeft;
        if (lockCam.leftWall == null && target != null)
        {
            lockCam.lerpLeft = 0;
            startLerpValue = 0;

            lockCam.leftWall = target;
        }
        else if (lockCam.leftWall != null && target == null)
        {
            lockCam.lerpLeft = 1;
            startLerpValue = 1;
            endLerpValue = 0;
        }

        Vector2 startTargetPos = lockCam.leftWall != null ? lockCam.leftWall.position : target.position;
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

        // set parameters
        float startLerpValue = lockCam.lerpRight;
        if (lockCam.rightWall == null && target != null)
        {
            lockCam.lerpRight = 0;
            startLerpValue = 0;

            lockCam.rightWall = target;
        }
        else if (lockCam.rightWall != null && target == null)
        {
            lockCam.lerpRight = 1;
            startLerpValue = 1;
            endLerpValue = 0;
        }

        Vector2 startTargetPos = lockCam.rightWall != null ? lockCam.rightWall.position : target.position;
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

        // set parameters
        float startLerpValue = lockCam.lerpTop;
        if (lockCam.topWall == null && target != null)
        {
            lockCam.lerpTop = 0;
            startLerpValue = 0;

            lockCam.topWall = target;
        }
        else if (lockCam.topWall != null && target == null)
        {
            lockCam.lerpTop = 1;
            startLerpValue = 1;
            endLerpValue = 0;
        }

        Vector2 startTargetPos = lockCam.topWall != null ? lockCam.topWall.position : target.position;
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

        // set parameters
        float startLerpValue = lockCam.lerpBottom;
        if (lockCam.bottomWall == null && target != null)
        {
            lockCam.lerpBottom = 0;
            startLerpValue = 0;

            lockCam.bottomWall = target;
        }
        else if (lockCam.bottomWall != null && target == null)
        {
            lockCam.lerpBottom = 1;
            startLerpValue = 1;
            endLerpValue = 0;
        }

        Vector2 startTargetPos = lockCam.bottomWall != null ? lockCam.bottomWall.position : target.position;
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
}
