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
}
