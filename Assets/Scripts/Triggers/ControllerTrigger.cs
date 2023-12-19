using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ControllerTrigger : MonoBehaviour
{
    public enum Mode { cube, auto, ship, auto_ship, ufo, auto_ufo, wave, auto_wave, ball, auto_ball, spider, auto_spider, copter, auto_copter, spring, auto_spring, robot, auto_robot }
    public Mode mode;

    [Range(-1, 3)]
    public int reverse;

    private GameManager gamemanager;
    private bool inuse = false;
    public bool oneuse = false;
    public bool show_portal = true;
    public bool startmusic = false;
    public bool forceStart = false;
    public bool restartmusic = false;

    public GameObject texture, cube_portal, ship_portal, ufo_portal, wave_portal, ball_portal, spider_portal, copter_portal, springboard_portal, robot_portal;

    // Start is called before the first frame update
    void Awake()
    {
        gamemanager = GameObject.FindObjectOfType<GameManager>();

        texture.SetActive(false);

        cube_portal?.SetActive(show_portal && (mode.ToString().Equals("cube") || mode.ToString().Equals("auto")));
        ship_portal?.SetActive(show_portal && (mode.ToString().Equals("ship") || mode.ToString().Equals("auto_ship")));
        ufo_portal?.SetActive(show_portal && (mode.ToString().Equals("ufo") || mode.ToString().Equals("auto_ufo")));
        wave_portal?.SetActive(show_portal && (mode.ToString().Equals("wave") || mode.ToString().Equals("auto_wave")));
        ball_portal?.SetActive(show_portal && (mode.ToString().Equals("ball") || mode.ToString().Equals("auto_ball")));
        spider_portal?.SetActive(show_portal && (mode.ToString().Equals("spider") || mode.ToString().Equals("auto_spider")));
        copter_portal?.SetActive(show_portal && (mode.ToString().Equals("copter") || mode.ToString().Equals("auto_copter")));
        springboard_portal?.SetActive(show_portal && (mode == Mode.spring) || (mode == Mode.auto_spring));
        robot_portal?.SetActive(show_portal && (mode == Mode.robot) || (mode == Mode.auto_robot));
    }

    private void changeGamemode()
    {
        //gamemanager.setPlayerController(mode.ToString(), startmusic, restartmusic);
    }

    private void setMusicOnChangeGamemode()
    {
        gamemanager.setMusicOnGamemodeChange(startmusic, forceStart, restartmusic);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && !inuse)
        {
            inuse = true;
            //changeGamemode();
            setMusicOnChangeGamemode();
            inuse = false;

            if (oneuse)
            {
                Destroy(gameObject);
            }
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        cube_portal?.SetActive(show_portal && (mode.ToString().Equals("cube") || mode.ToString().Equals("auto")));
        ship_portal?.SetActive(show_portal && (mode.ToString().Equals("ship") || mode.ToString().Equals("auto_ship")));
        ufo_portal?.SetActive(show_portal && (mode.ToString().Equals("ufo") || mode.ToString().Equals("auto_ufo")));
        wave_portal?.SetActive(show_portal && (mode.ToString().Equals("wave") || mode.ToString().Equals("auto_wave")));
        ball_portal?.SetActive(show_portal && (mode.ToString().Equals("ball") || mode.ToString().Equals("auto_ball")));
        spider_portal?.SetActive(show_portal && (mode.ToString().Equals("spider") || mode.ToString().Equals("auto_spider")));
        copter_portal?.SetActive(show_portal && (mode.ToString().Equals("copter") || mode.ToString().Equals("auto_copter")));
        springboard_portal?.SetActive(show_portal && (mode == Mode.spring) || (mode == Mode.auto_spring));
        robot_portal?.SetActive(show_portal && (mode == Mode.robot) || (mode == Mode.auto_robot));
    }
#endif
}
