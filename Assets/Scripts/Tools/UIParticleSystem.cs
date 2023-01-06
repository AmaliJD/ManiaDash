using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIParticleSystem : MonoBehaviour
{
    public enum Anchor
    {
        Center,
        Top,
        TopRight,
        Right,
        BottomRight,
        Bottom,
        BottomLeft,
        Left,
        TopLeft,
        Player
    }

    public Anchor anchor;

    private Transform player;
    private Camera mainCam;
    private int xOff, yOff;
    void Start()
    {
        mainCam = Camera.main;
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void LateUpdate()
    {
        switch (anchor.ToString())
        {
            case "Center":
                xOff = (int)(.5f * mainCam.pixelWidth); yOff = (int)(.5f * mainCam.pixelHeight); break;

            case "Top":
                xOff = (int)(.5f * mainCam.pixelWidth); yOff = mainCam.pixelHeight; break;

            case "TopRight":
                xOff = mainCam.pixelWidth; yOff = mainCam.pixelHeight; break;

            case "Right":
                xOff = mainCam.pixelWidth; yOff = (int)(.5f * mainCam.pixelHeight); break;

            case "BottomRight":
                xOff = mainCam.pixelWidth; yOff = 0; break;

            case "Bottom":
                xOff = (int)(.5f * mainCam.pixelWidth); yOff = 0; break;

            case "BottomLeft":
                xOff = 0; yOff = 0; break;

            case "Left":
                xOff = 0; yOff = (int)(.5f * mainCam.pixelHeight); break;

            case "TopLeft":
                xOff = 0; yOff = mainCam.pixelHeight; break;

            case "Player":
                xOff = (int)mainCam.WorldToScreenPoint(player.position).x; yOff = (int)mainCam.WorldToScreenPoint(player.position).y; break;
        }

        transform.localScale = new Vector3(mainCam.orthographicSize / 10, mainCam.orthographicSize / 10, 1);
        transform.position = mainCam.ScreenToWorldPoint(new Vector3(xOff, yOff, 20));
    }
}
