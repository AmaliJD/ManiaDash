using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class PlayerRecorder : MonoBehaviour
{
    public ColorReference ghost_player_color_1;
    public ColorReference ghost_player_color_2;

    public GameObject ghostPlayer;
    private GameObject player;
    private IconController iconController;

    private Color pc1, pc2;
    private int icon_index;

    private Transform playerIconParent;
    private Transform ghostPlayerIconParent;

    private Transform playerIcon;
    private Transform ghostPlayerIcon;

    private GameObject ghostPlayerCube, ghostPlayerShip, ghostPlayerUfo, ghostPlayerWave, ghostPlayerBall, ghostPlayerJetpack, ghostPlayerSpider, ghostPlayerCopter;

    private PlayerControllerV2 playerController;
    private Rigidbody2D playerBody, ghostPlayerBody;

    private Animator CopterAnim, SpiderAnim;

    [HideInInspector]
    public bool start = false;

    [HideInInspector]
    public bool canceled = false;

    private List<PlayerPositionData> position_data;
    private PlayerPositionData[] currentGhost;
    private PlayerPositionData[] newGhost;

    private GameManager gamemanager;
    private int levelNumber;
    private int index = 1;

    private bool prevMini;
    private PlayerControllerV2.Gamemode prevState;
    private float[] prevPosition;

    private bool playback;
    private WaitForSeconds wait;

    [System.Serializable]
    public class PlayerPositionData
    {
        public float[] position;
        //public float[] velocity;
        public float[] rotation;
        public float[] icon_position;
        public float[] icon_scale;
        public int iconParentScaleRev;
        public PlayerControllerV2.Gamemode state;
        public bool dead;
        public bool mini;
        public bool grounded;
        public float copterAnimSpeed;
        public float spiderAnimSpeed;
        public int spiderAnimType;

        public float[] pc1;
        public float[] pc2;
        public int icon_index;
        public float time;
        public bool uptodate;

        public PlayerPositionData(Rigidbody2D rb, Quaternion rot, Transform tr, PlayerControllerV2 pc) // Constructor
        {
            position = new float[] { rb.position.x, rb.position.y };
            //velocity = new float[] { rb.velocity.x, rb.velocity.y };
            rotation = new float[] { rot.x, rot.y, rot.z, rot.w };
            icon_position = new float[] { tr.localPosition.x, tr.localPosition.y };
            icon_scale = new float[] { tr.localScale.x, tr.localScale.y };
            iconParentScaleRev = (int)Mathf.Sign(tr.parent.localScale.x);
            state = pc.getMode();
            dead = pc.getDead();
            grounded = pc.getGrounded();
            mini = pc.getMini();
            copterAnimSpeed = pc.getCopterAnimSpeed();
            spiderAnimSpeed = pc.getSpiderAnimSpeed();
            spiderAnimType = pc.getSpiderAnimType();
        }

        public PlayerPositionData(Color col1, Color col2, int idx, float tim) // Constructor
        {
            pc1 = new float[] { col1.r, col1.g, col1.b };
            pc2 = new float[] { col2.r, col2.g, col2.b };
            icon_index = idx;
            time = tim;
        }
    }

    private void Awake()
    {
        gamemanager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        levelNumber = gamemanager.getLevelNumber();

        player = GameObject.FindGameObjectWithTag("Player");
        playerIconParent = player.transform.GetChild(0);
        playerIcon = playerIconParent.transform.GetChild(0);
        iconController = playerIcon.GetComponent<IconController>();
        playerController = player.GetComponent<PlayerControllerV2>();
        playerBody = player.GetComponent<Rigidbody2D>();

        ghostPlayerIconParent = ghostPlayer.transform.GetChild(0);
        ghostPlayerIcon = ghostPlayerIconParent.transform.GetChild(0);
        ghostPlayerBody = ghostPlayer.GetComponent<Rigidbody2D>();

        ghostPlayerShip = ghostPlayerIcon.GetChild(30).gameObject;
        ghostPlayerUfo = ghostPlayerIcon.GetChild(31).gameObject;
        ghostPlayerWave = ghostPlayerIcon.GetChild(32).gameObject;
        ghostPlayerBall = ghostPlayerIcon.GetChild(33).gameObject;
        ghostPlayerJetpack = ghostPlayerIcon.GetChild(34).gameObject;
        ghostPlayerSpider = ghostPlayerIcon.GetChild(35).gameObject;
        ghostPlayerCopter = ghostPlayerIcon.GetChild(36).gameObject;

        CopterAnim = ghostPlayerCopter.GetComponent<Animator>();
        SpiderAnim = ghostPlayerSpider.GetComponent<Animator>();

        /*ghostPlayerShip.SetActive(false);
        ghostPlayerUfo.SetActive(false);
        ghostPlayerWave.SetActive(false);
        ghostPlayerBall.SetActive(false);
        ghostPlayerJetpack.SetActive(false);
        ghostPlayerSpider.SetActive(false);
        ghostPlayerCopter.SetActive(false);*/

        pc1 = iconController.p1;
        pc2 = iconController.p2;
        icon_index = iconController.getIconIndex();

        position_data = new List<PlayerPositionData>(10000);
        LoadData();

        position_data.Add(new PlayerPositionData(pc1, pc2, icon_index, float.MaxValue));

        if (gamemanager.record_playback == 1) { playback = true; }
        if (currentGhost != null)
        {
            ghost_player_color_1.Set(new Color(currentGhost[0].pc1[0], currentGhost[0].pc1[1], currentGhost[0].pc1[2], .5f));
            ghost_player_color_2.Set(new Color(currentGhost[0].pc2[0], currentGhost[0].pc2[1], currentGhost[0].pc2[2], .7f));

            ghostPlayerCube = ghostPlayerIcon.GetChild(currentGhost[0].icon_index).gameObject;
            ghostPlayerCube.gameObject.SetActive(true);

            /*for (int i = 0; i < 30; i++)
            {
                if (i != currentGhost[0].icon_index) { ghostPlayerIcon.GetChild(i).gameObject.SetActive(false); }
            }*/
        }
        else
        {
            playback = false;
        }
        ghostPlayer.SetActive(playback);

        wait = new WaitForSeconds(0.001f);
    }

    public void Begin()
    {
        start = true;
        if(levelNumber != 0) { StartCoroutine(RecordPlayer()); }
    }

    private IEnumerator RecordPlayer()
    {
        while (start)
        {
            //if (!start) { return; }

            if(!canceled)
            {
                position_data.Add(new PlayerPositionData(playerBody, playerIconParent.rotation, playerIcon.transform, playerController));
            }

            /*ghostPlayerBody.position = playerBody.position;
            ghostPlayerBody.velocity = playerBody.velocity;
            ghostPlayerIcon.rotation = playerIcon.rotation;

            ghostPlayerIcon.gameObject.SetActive(!playerController.getDead());*/

            if(currentGhost != null && gamemanager.getTime() > currentGhost[0].time + 2)
            {
                canceled = true;
            }

            if (currentGhost != null && index < currentGhost.Length && playback)
            {
                //Debug.Log(index + "/" + currentGhost.Length + "      " + ghostPlayerBody.position);
                ghostPlayerBody.MovePosition(new Vector2(currentGhost[index].position[0], currentGhost[index].position[1]));
                //ghostPlayerBody.velocity = new Vector2(currentGhost[index].velocity[0], currentGhost[index].velocity[1]);
                //ghostPlayerBody.transform.position = new Vector2(currentGhost[index].position[0], currentGhost[index].position[1]);
                ghostPlayerIconParent.rotation = new Quaternion(currentGhost[index].rotation[0], currentGhost[index].rotation[1], currentGhost[index].rotation[2], currentGhost[index].rotation[3]);
                ghostPlayerIconParent.gameObject.SetActive(!currentGhost[index].dead);

                ghostPlayerIcon.localPosition = new Vector2(currentGhost[index].icon_position[0], currentGhost[index].icon_position[1]);
                ghostPlayerIcon.localScale = new Vector2(currentGhost[index].icon_scale[0], currentGhost[index].icon_scale[1]);

                if(currentGhost[index].state == PlayerControllerV2.Gamemode.copter || currentGhost[index].state == PlayerControllerV2.Gamemode.auto_copter) { CopterAnim.speed = currentGhost[index].copterAnimSpeed; }

                if ((currentGhost[index].state == PlayerControllerV2.Gamemode.spider || currentGhost[index].state == PlayerControllerV2.Gamemode.auto_spider))
                {
                    if (!currentGhost[index].grounded)
                    {
                        switch (currentGhost[index].spiderAnimType)
                        {
                            case 0:
                                SpiderAnim.ResetTrigger("run");
                                SpiderAnim.ResetTrigger("jump");
                                SpiderAnim.ResetTrigger("curl");
                                SpiderAnim.SetTrigger("stop");
                                break;

                            case 1:
                                SpiderAnim.ResetTrigger("run");
                                SpiderAnim.ResetTrigger("curl");
                                SpiderAnim.ResetTrigger("stop");
                                SpiderAnim.SetTrigger("jump");
                                SpiderAnim.Play("jump", -1, 0f);
                                break;

                            case 2:
                                SpiderAnim.ResetTrigger("run");
                                SpiderAnim.ResetTrigger("jump");
                                SpiderAnim.ResetTrigger("stop");
                                SpiderAnim.SetTrigger("curl");
                                break;

                            default:
                                break;
                        }
                    }
                    else
                    {
                        if (prevPosition == currentGhost[index].position)
                        {
                            SpiderAnim.ResetTrigger("curl");
                            SpiderAnim.ResetTrigger("jump");
                            SpiderAnim.ResetTrigger("run");
                            SpiderAnim.SetTrigger("stop");
                        }
                        else
                        {
                            SpiderAnim.ResetTrigger("curl");
                            SpiderAnim.ResetTrigger("jump");
                            SpiderAnim.ResetTrigger("stop");
                            SpiderAnim.SetTrigger("run");
                        }
                    }

                    SpiderAnim.speed = currentGhost[index].spiderAnimSpeed;
                }

                if (currentGhost[index].mini != prevMini) { SetSize(); }
                if (currentGhost[index].state != prevState) { SetState(); }

                ghostPlayerIconParent.localScale = new Vector3(currentGhost[index].iconParentScaleRev * Mathf.Abs(ghostPlayerIconParent.localScale.x), ghostPlayerIconParent.localScale.y, 1);

                prevMini = currentGhost[index].mini;
                prevState = currentGhost[index].state;
                prevPosition = currentGhost[index].position;

                index++;
            }

            if(canceled && currentGhost != null && index >= currentGhost.Length) { yield break; }

            yield return new WaitForFixedUpdate();
        }
    }

    private void SetSize()
    {
        ghostPlayer.transform.localScale = Vector2.one * (currentGhost[index].mini ? .44f : 1f);
        ghostPlayerIconParent.localScale = Vector2.one * (currentGhost[index].mini ? 1.1f : 1.05f);
    }

    private void SetState()
    {
        PlayerControllerV2.Gamemode gamemode = currentGhost[index].state;
        switch (gamemode)
        {
            case PlayerControllerV2.Gamemode.cube:
            case PlayerControllerV2.Gamemode.auto_cube:
            case PlayerControllerV2.Gamemode.ship:
            case PlayerControllerV2.Gamemode.copter:
            case PlayerControllerV2.Gamemode.auto_copter:
                ghostPlayerCube.transform.localPosition = Vector3.zero;
                ghostPlayerCube.transform.localScale = Vector3.one;
                break;

            case PlayerControllerV2.Gamemode.auto_ship:
                ghostPlayerCube.transform.localPosition = new Vector3(.037f, .232f, 0);
                ghostPlayerCube.transform.localScale = new Vector3(.7f, .7f, 0);
                break;

            case PlayerControllerV2.Gamemode.ufo:
            case PlayerControllerV2.Gamemode.auto_ufo:
                ghostPlayerCube.transform.localPosition = new Vector3(0, 0.147f, 0);
                ghostPlayerCube.transform.localScale = new Vector3(.7f, .7f, 0);
                break;
        }

        ghostPlayerCube.SetActive(!(gamemode == PlayerControllerV2.Gamemode.ball || gamemode == PlayerControllerV2.Gamemode.auto_ball
                    || gamemode == PlayerControllerV2.Gamemode.spider || gamemode == PlayerControllerV2.Gamemode.auto_spider
                    || gamemode == PlayerControllerV2.Gamemode.wave || gamemode == PlayerControllerV2.Gamemode.auto_wave));

        ghostPlayerJetpack.SetActive(gamemode == PlayerControllerV2.Gamemode.ship);
        ghostPlayerShip.SetActive(gamemode == PlayerControllerV2.Gamemode.auto_ship);
        ghostPlayerUfo.SetActive(gamemode == PlayerControllerV2.Gamemode.ufo || gamemode == PlayerControllerV2.Gamemode.auto_ufo);
        ghostPlayerWave.SetActive(gamemode == PlayerControllerV2.Gamemode.wave || gamemode == PlayerControllerV2.Gamemode.auto_wave);
        ghostPlayerBall.SetActive(gamemode == PlayerControllerV2.Gamemode.ball || gamemode == PlayerControllerV2.Gamemode.auto_ball);
        ghostPlayerSpider.SetActive(gamemode == PlayerControllerV2.Gamemode.spider || gamemode == PlayerControllerV2.Gamemode.auto_spider);
        ghostPlayerCopter.SetActive(gamemode == PlayerControllerV2.Gamemode.copter || gamemode == PlayerControllerV2.Gamemode.auto_copter);
    }

    private void LoadData()
    {
        string path = Application.persistentDataPath + "/LevelRecordingData/level_" + levelNumber + "_best.dat";
        
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            currentGhost = formatter.Deserialize(stream) as PlayerPositionData[];
            stream.Close();
        }
        else
        {
            currentGhost = null;
            Debug.LogError("No Level " + levelNumber + " Recording Data Found");
        }

        if (!Directory.Exists(Application.persistentDataPath + "/LevelRecordingData"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/LevelRecordingData");
        }
    }

    public void SaveData()
    {
        string path = Application.persistentDataPath + "/LevelRecordingData/level_" + levelNumber + "_best.dat";

        if (!Directory.Exists(Application.persistentDataPath + "/LevelRecordingData"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/LevelRecordingData");
        }

        position_data[0].time = gamemanager.getTime();
        newGhost = position_data.ToArray();
        if(currentGhost != null && newGhost[0].time > currentGhost[0].time) { return; }

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream;
        stream = new FileStream(path, FileMode.Create);
        formatter.Serialize(stream, newGhost);
        stream.Close();
    }
}
