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
}
