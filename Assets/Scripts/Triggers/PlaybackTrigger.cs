using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlaybackTrigger : MonoBehaviour
{
    [Header("Pre-Activate Conditions")]
    [Min(0)]
    public float delay;
    [Min(0)]
    public float requiredManaCount;

    [Header("Target Triggers")]
    public List<GameObject> triggers;

    [Header("Properties")]
    public PlaybackState playbackState;
    public enum PlaybackState
    {
        Stop, Pause, Resume, FlipPause
    }
    public bool togglePause;

    [Min(-1)]
    public int triggerLimit = -1;
    private int triggerCount = 0;

    [Header("Settings")]
    public bool resetOnDeathPerCheckpoint = false;
    public bool hideIcon;

    private GameObject texture;
    private PlayerControllerV2 player;
    private GroupIDManager groupIDManager;
    private GameManager gamemanager;

    private void Awake()
    {
        gamemanager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        if (transform.childCount > 0)
        {
            texture = transform.GetChild(0).gameObject;
            setTexture(true, true);
        }
    }

    public void Activate()
    {
        waiting = false;
        if (!(triggerLimit == -1 || triggerCount < triggerLimit) || requiredManaCount > gamemanager.getManaCount()) { return; }

        triggerCount++;

        foreach (GameObject obj in triggers)
        {
            switch(obj.tag)
            {
                case "MoveObject":
                    MoveObject moveObject = obj.GetComponent<MoveObject>();
                    switch (playbackState)
                    {
                        case PlaybackState.Stop:
                            moveObject.StopTrigger();
                            break;

                        case PlaybackState.Pause:
                            moveObject.PauseTrigger();
                            break;

                        case PlaybackState.Resume:
                            moveObject.ResumeTrigger();
                            break;

                        case PlaybackState.FlipPause:
                            moveObject.TogglePauseTrigger();
                            break;
                    }
                    break;

                case "RotateObject":
                    RotateObject rotateObject = obj.GetComponent<RotateObject>();
                    switch(playbackState)
                    {
                        case PlaybackState.Stop:
                            rotateObject.StopTrigger();
                            break;

                        case PlaybackState.Pause:
                            rotateObject.PauseTrigger();
                            break;

                        case PlaybackState.Resume:
                            rotateObject.ResumeTrigger();
                            break;

                        case PlaybackState.FlipPause:
                            rotateObject.TogglePauseTrigger();
                            break;
                    }
                    break;

                case "ScaleObject":
                    ScaleObject scaleObject = obj.GetComponent<ScaleObject>();
                    switch (playbackState)
                    {
                        case PlaybackState.Stop:
                            scaleObject.StopTrigger();
                            break;

                        case PlaybackState.Pause:
                            scaleObject.PauseTrigger();
                            break;

                        case PlaybackState.Resume:
                            scaleObject.ResumeTrigger();
                            break;

                        case PlaybackState.FlipPause:
                            scaleObject.TogglePauseTrigger();
                            break;
                    }
                    break;
            }

            if(togglePause)
            {
                switch (playbackState)
                {
                    case PlaybackState.Pause:
                        playbackState = PlaybackState.Resume;
                        break;

                    case PlaybackState.Resume:
                        playbackState = PlaybackState.Pause;
                        break;
                }

                setTexture(false, true);
            }
        }
    }

    private bool waiting;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player" && !waiting)
        {
            waiting = true;
            Invoke("Activate", delay);
        }
    }

    void setTexture(bool doHide, bool doIcon)
    {
        if(doHide)
        {
            texture.SetActive(!hideIcon);
        }
        if (doIcon)
        {
            texture.transform.GetChild(0).gameObject.SetActive(playbackState == PlaybackState.Stop);
            texture.transform.GetChild(1).gameObject.SetActive(playbackState == PlaybackState.Pause);
            texture.transform.GetChild(2).gameObject.SetActive(playbackState == PlaybackState.Resume);
            texture.transform.GetChild(3).gameObject.SetActive(playbackState == PlaybackState.FlipPause);
        }
    }

    private void OnValidate()
    {
        if (texture != null)
        {
            setTexture(Application.isPlaying, true);
        }
        else
        {
            if (transform.childCount > 0)
            {
                texture = transform.GetChild(0).gameObject;
                setTexture(Application.isPlaying, true);
            }
        }
    }

    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Color gizmosColor = Color.white;
        switch(playbackState)
        {
            case PlaybackState.Pause:
                gizmosColor = new Color(1, .5f, 0f);
                break;

            case PlaybackState.Resume:
                gizmosColor = new Color(0, 1f, 0f);
                break;

            case PlaybackState.Stop:
                gizmosColor = new Color(1, 0f, 0f);
                break;

            case PlaybackState.FlipPause:
                gizmosColor = new Color(1, .75f, 0f);
                break;
        }

        foreach (GameObject go in triggers)
        {
            if (go == null) { continue; }

            Transform tr = go.transform;
            Vector3 triggerPos = transform.position;
            Vector3 objPos = tr.position;
            float halfHeight = (triggerPos.y - objPos.y) / 2f;
            Vector3 offset = Vector3.up * halfHeight;

            Handles.DrawBezier
            (
                triggerPos,
                objPos,
                triggerPos - offset,
                objPos + offset,
                gizmosColor,
                EditorGUIUtility.whiteTexture,
                1f
            );
        }
    }
    #endif
}
