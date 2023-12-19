using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartPos : MonoBehaviour
{
    [System.Serializable]
    public struct InitObj
    {
        public GameObject obj;
        public Vector2 move;
        public bool centerOnStartPos;
        public bool disable;
    }

    public bool mini;
    public PlayerControllerV2.Gamemode gamemode;

    [Range(0, 4)]
    public int speed;

    public InitObj[] InitObjects;

    private void Awake()
    {
        PlayerControllerV2 playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControllerV2>();
        playerController.transform.position = playerController.transform.position.SetXY(new Vector2(transform.position.x, transform.position.y));
    
        foreach (var o in InitObjects)
        {
            o.obj.transform.position = o.centerOnStartPos ? o.obj.transform.position.SetXY(new Vector2(transform.position.x, transform.position.y)) : o.obj.transform.position;
            o.obj.transform.Translate(o.move, Space.World);
            o.obj.gameObject.SetActive(o.disable);
        }

        if(mini)
        {
            playerController.setMini(mini);
            playerController.ChangeSize();
        }

        if(gamemode != PlayerControllerV2.Gamemode.cube)
        {
            playerController.setGamemode((int)gamemode);
        }

        playerController.setSpeed(speed);
    }
}
