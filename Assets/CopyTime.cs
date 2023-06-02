using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CopyTime : MonoBehaviour
{
    private GameManager gamemanager;

    public TextMeshPro text;

    private void Awake()
    {
        gamemanager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    void Update()
    {
        text.text = "-Time-\n" + gamemanager.getFormattedTime();
    }
}
