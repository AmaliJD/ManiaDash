using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevTools : MonoBehaviour
{
    private GameManager gamemanager;
    private TimeManager timemanager;
    private PlayerControllerV2 player;

    private void Awake()
    {
        gamemanager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        timemanager = GameObject.FindGameObjectWithTag("Master").GetComponent<TimeManager>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControllerV2>();
    }
}
