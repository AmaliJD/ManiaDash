using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartPos : MonoBehaviour
{
    private void Awake()
    {
        PlayerControllerV2 playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControllerV2>();
        playerController.transform.position = playerController.transform.position.SetXY(new Vector2(transform.position.x, transform.position.y));
    }
}
