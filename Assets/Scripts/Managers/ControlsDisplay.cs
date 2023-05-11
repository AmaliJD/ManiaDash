using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlsDisplay : MonoBehaviour
{
    public GameObject controls_button, controls_panel, vcam1, vcam2;
    public Image w, up, space, lmouse, dpadUp, controllerRight,
                 s, down, shift, rmouse, dpadDown, controllerDown, controllerLeft, controllerUp, controllerLT, controllerRT,
                 a, left, dpadLeft,
                 d, right, dpadRight,
                 delete;

    bool jump, move_left, move_right, crouch, holding_delete;
    bool open;

    public InputActions input;

    private void Awake()
    {
        if (input == null)
        {
            input = new InputActions();
        }
        input.Player.Enable();
    }

    void Update()
    {
        if (!open) return;

        jump = input.Player.Jump.ReadValue<float>() > 0;
        crouch = input.Player.Crouch.ReadValue<float>() >= 0.7f;
        move_left = input.Player.MovementHorizontal.ReadValue<float>() < 0;
        move_right = input.Player.MovementHorizontal.ReadValue<float>() > 0;
        holding_delete = Input.GetKey(KeyCode.Delete) && !jump && !crouch && !move_left && !move_right;

        w.color = jump ? Color.red : Color.white;
        up.color = jump ? Color.red : Color.white;
        space.color = jump ? Color.red : Color.white;
        lmouse.gameObject.SetActive(jump);
        //dpadDown.color = jump ? Color.red : Color.white;
        controllerUp.color = jump ? Color.red : Color.white;
        controllerRight.color = jump ? Color.red : Color.white;
        controllerLeft.color = jump ? Color.red : Color.white;
        controllerRT.color = jump ? Color.red : Color.white;
        //controllerRight.transform.GetChild(0).GetComponent<Image>().color = jump ? Color.red : Color.white;

        s.color = crouch ? Color.red : Color.white;
        down.color = crouch ? Color.red : Color.white;
        shift.color = crouch ? Color.red : Color.white;
        rmouse.gameObject.SetActive(crouch);
        dpadDown.color = crouch ? Color.red : Color.white;
        controllerDown.color = crouch ? Color.red : Color.white;
        //controllerLeft.transform.GetChild(0).GetComponent<Image>().color = crouch ? Color.red : Color.white;
        controllerLT.color = crouch ? Color.red : Color.white;

        a.color = move_left ? Color.red : Color.white;
        left.color = move_left ? Color.red : Color.white;
        dpadLeft.color = move_left ? Color.red : Color.white;

        d.color = move_right ? Color.red : Color.white;
        right.color = move_right ? Color.red : Color.white;
        dpadRight.color = move_right ? Color.red : Color.white;

        delete.color = holding_delete ? Color.red : Color.white;
    }

    public void Open(bool o)
    {
        open = o;
        if(open)
        {
            controls_button.SetActive(false);
            controls_panel.SetActive(true);
            vcam1.SetActive(false);
            vcam2.SetActive(true);
        }
        else
        {
            controls_button.SetActive(true);
            controls_panel.SetActive(false);
            vcam1.SetActive(true);
            vcam2.SetActive(false);
        }
    }
}
