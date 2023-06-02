using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputLineTrackerUI : MonoBehaviour
{
    public InputLineTracker tracker;
    public Toggle trackingPositionToggle;
    public Toggle trackingJumpToggle;
    public Toggle trackingCrouchToggle;
    public Slider eraseSlider;
    public Text eraseText;

    private void Awake()
    {
        LoadPlayerPrefs();
    }

    public void SetTrackingPosition()
    {
        tracker.trackMove = trackingPositionToggle.isOn;
    }

    public void SetTrackingJump()
    {
        tracker.trackJump = trackingJumpToggle.isOn;
    }

    public void SetTrackingCrouch()
    {
        tracker.trackCrouch = trackingCrouchToggle.isOn;
    }

    public void SliderChange()
    {
        tracker.keepLastDeaths = (int)eraseSlider.value;

        switch(eraseSlider.value)
        {
            case 0: eraseText.text = "Erase After Death"; break;
            case 1: eraseText.text = "Keep Last Tracks"; break;
            case 2: eraseText.text = "Don't Erase"; break;
        }
    }

    public void Clear()
    {
        tracker.ClearAll();
    }

    void LoadPlayerPrefs()
    {
        trackingJumpToggle.isOn = PlayerPrefs.GetInt("trackJump", 0) == 1;
        trackingCrouchToggle.isOn = PlayerPrefs.GetInt("trackCrouch", 0) == 1;
        trackingPositionToggle.isOn = PlayerPrefs.GetInt("trackMove", 1) == 1;
        eraseSlider.value = PlayerPrefs.GetInt("keepLastDeaths", 0);

        SetTrackingPosition();
        SetTrackingJump();
        SetTrackingCrouch();
        SetTrackingCrouch();
    }

    public void SavePlayerPrefs()
    {
        PlayerPrefs.SetInt("trackJump", trackingJumpToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("trackCrouch", trackingCrouchToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("trackMove", trackingPositionToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("keepLastDeaths", (int)eraseSlider.value);
        PlayerPrefs.Save();
    }

    private void OnApplicationQuit()
    {
        SavePlayerPrefs();
    }
}
