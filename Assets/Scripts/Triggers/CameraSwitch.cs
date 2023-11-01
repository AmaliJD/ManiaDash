using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Cinemachine.PostFX;

public class CameraSwitch : MonoBehaviour
{
    public CinemachineVirtualCamera activeCamera;
    //public bool resetLookAhead;
    private List<CinemachineVirtualCamera> cameraList;
    //private GameObject[] initialList;
    private GameManager gamemanager;

    private void Awake()
    {
        gamemanager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        cameraList = new List<CinemachineVirtualCamera>();
        //initialList = GameObject.FindGameObjectsWithTag("Camera");

        cameraList = gamemanager.getCameraList();

        /*
        int i = 0;
        foreach (GameObject g in initialList)
        {
            cameraList.Add(g.GetComponent<CinemachineVirtualCamera>());
            cameraList[i].gameObject.SetActive(true);
            cameraList[i].Priority = 5;
        }*/
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Activate();
            /*if(resetLookAhead)
            {
                activeCamera.enabled = false;
                activeCamera.Follow = activeCamera.Follow;
                activeCamera.enabled = true;
            }*/
        }
    }

    public void Activate()
    {
        if (gamemanager.getpostfx())
        {
            if (activeCamera.VirtualCameraGameObject.GetComponent<CinemachineVolumeSettings>() != null)
            {
                activeCamera.VirtualCameraGameObject.GetComponent<CinemachineVolumeSettings>().enabled = true;
            }
        }
        else
        {
            if (activeCamera.VirtualCameraGameObject.GetComponent<CinemachineVolumeSettings>() != null)
            {
                activeCamera.VirtualCameraGameObject.GetComponent<CinemachineVolumeSettings>().enabled = false;
            }
        }

        foreach (CinemachineVirtualCamera c in cameraList)
        {
            if (c.Priority == 10) { c.Priority = 8; }
            else if (c.Priority == 8) { c.Priority = 7; }
            else if (c.Priority == 7 && c == activeCamera)
            {
                activeCamera.enabled = false;
                activeCamera.Follow = activeCamera.Follow;
                activeCamera.enabled = true;
            }

        }

        activeCamera.Priority = 10;
    }
}
