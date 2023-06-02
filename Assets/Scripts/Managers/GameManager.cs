using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cinemachine;
using Cinemachine.PostFX;
using UnityEngine.Experimental.Rendering.Universal;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using PlayFab;
using PlayFab.ClientModels;
using System.Linq;


public class GameManager : MonoBehaviour
{
    public int levelNumber;

    public InputActions input;

    private Transform canvas;

    private Text ManaCount;
    private Text DiamondCount;
    private Text Timer;
    private Text ManaScore;
    private Text DiamondScore;
    private Text TimeScore;
    private Text FPS;
    private Text Saving;
    Text ExitText, ReplayText;
    private GameObject Pause_Menu;
    private GameObject Menu1;
    private GameObject Menu2;
    private GameObject WindowedResolution;
    
    private Button Menu_Button;
    private Button Restart_Button;
    private Button Options_Button;
    private Button Fullscreen_Button;
    private Button Particles_Button;
    private Button Effects_Button;
    private Button Res1440_Button;
    private Button Res1080_Button;
    private Button Res720_Button;
    private Toggle fixedJumpToggle, holdJumpToggle, hideUIToggle;
    private bool prevFixed, prevHold, prevHideUI;

    private Text MusicText;
    private Text SfxText;
    private Slider MusicSlider;
    private Slider SfxSlider;

    private Text restartWarningText;
    private Slider restartWarningSlider;
    private Image restartWarningSliderFill;

    private Animator UIAnimator;
    private GameObject UIIntroSignal;
    private GameObject UIRestartSignal;
    private GameObject UIEndSignal;

    public bool showCursorInGame;

    private CinemachineBrain main_camera_brain;
    private float aspectratio = 16f / 9f;
    private int prev_width, prev_height;

    //public ColorReference[] color_channels;
    //private Color[] channel_colors;

    private float time = 0, timediff = 0;
    private bool shortcuts_enabled = false, game = false, paused = false, start = false, halt = false, endscreen = false, addtimediff = true;

    private GameObject effects;
    private GameObject globallight;
    private GameObject playerlight;
    private GameObject player;
    public MusicSource bgmusic;
    private MusicSource newbgmusic;

    private PlayerControllerV2 newPlayer;
    private TimeManager timeManager;
    private PlayerRecorder recorder;
    private ClicksPerSecond cps_controller;

    private Checkpoint_Controller checkpointcontroller;

    private IconController iconcontroller;
    private GameObject icon;

    private int mana_count = 0;
    private int diamond_count = 0;
    private int coins_gotten = 0;
    private int[] coin_count = new int[3];
    public Coin[] Coins;
    public GameObject[] coin1On, ghostCoin1On, coin2On, ghostCoin2On, coin3On, ghostCoin3On;
    private GameObject[] CoinIcons;
    public AudioSource coinget;
    public AudioSource diamondget;

    [Range(0f,1f)] [HideInInspector]
    public float music_volume, sfx_volume;

    [Range(0, 1)] [HideInInspector]
    public int fixedJump, holdJump;

    [Range(0, 1)] [HideInInspector]
    public int record_playback, record_playback_100;

    private int min = 0, sec = 0, milli = 0, m = 0, s = 0;

    private bool postfxon = true;
    private List<GameObject> highdetail;
    private Dictionary<Light2D, Vector2> highdetaillights;

    private float deltaTime = 0.0f;
    private float endgame_timer = 0;

    private bool hideUI;
    private GameObject collectablesUI, textUI;

    private bool drawOn, hideRenderers;
    private DrawColliders drawColliders;
    public GameObject lineTrackerObj;

    private bool showStats;
    private LevelDevTools levelDevTools;
    private GameObject statsObj, trackerObj;

    private GameObject[] initialList;
    private List<CinemachineVirtualCamera> cameraList;
    CinemachineVirtualCamera activeCamera;

    public MoveTrigger movex, movey;
    public GameObject End;
    public MenuState page;

    private MusicTrigger activeMusicTrigger;
    private GlobalData data;

    private void LevelStartedEvent()
    {
        if (IsLoggedIn)
        {
            PlayFabClientAPI.WritePlayerEvent(new WriteClientPlayerEventRequest()
            {
                Body = new Dictionary<string, object>() {
                { "LevelNumber", levelNumber },
                { "CurrentBest", data.level_times[levelNumber] },
                { "CurrentBest 100%", data.level_times_allcoins[levelNumber] }
            },
                EventName = "level_started"
            },
            result => Debug.Log("Level " + levelNumber + " Opened"),
            error => Debug.LogError(error.GenerateErrorReport()));
        }
        else
        {
            Debug.LogError("Not Logged In");
        }
    }

    private void LevelCompletedEvent(bool allcoins)
    {
        if (IsLoggedIn)
        {
            PlayFabClientAPI.WritePlayerEvent(new WriteClientPlayerEventRequest()
            {
                Body = new Dictionary<string, object>() {
                { "LevelNumber", levelNumber },
                { "100%", allcoins },
                { "CurrentBest", allcoins ? data.level_times_allcoins[levelNumber] : data.level_times[levelNumber] },
                { "Time", time },
                { "Diamonds", diamond_count },
                { "FPS", fps },
                { "Max CPS", cps_controller.getMaxCPS() },
                { "Ave CPS", cps_controller.getAveCPS() },
                { "Jumps", newPlayer.getJumpCount() },
                { "Deaths", newPlayer.getDeathCount() },
                { "Fixed Jump", newPlayer.fixedJumpHeight },
                { "Hold Jump", newPlayer.cubeHoldJump }
            },
                EventName = "level_completed"
            },
            result => Debug.Log("Level " + levelNumber + " Completed"),
            error => Debug.LogError(error.GenerateErrorReport()));
        }
        else
        {
            Debug.LogError("Not Logged In");
        }
    }

    private void LevelExited()
    {
        if (IsLoggedIn)
        {
            PlayFabClientAPI.WritePlayerEvent(new WriteClientPlayerEventRequest()
            {
                Body = new Dictionary<string, object>() {
                { "LevelNumber", levelNumber },
                { "Time", time },
                { "Max CPS", cps_controller.getMaxCPS() },
                { "Ave CPS", cps_controller.getAveCPS() },
                { "Jumps", newPlayer.getJumpCount() },
                { "Deaths", newPlayer.getDeathCount() }
            },
                EventName = "level_exited"
            },
            result => Debug.Log("Level " + levelNumber + " Exited"),
            error => Debug.LogError(error.GenerateErrorReport()));
        }
        else
        {
            Debug.LogError("Not Logged In");
        }
    }

    public bool IsLoggedIn
    {
        get
        {
            return PlayFabClientAPI.IsClientLoggedIn();
        }
    }

    public void StopMusicTrigger(MusicTrigger newTrigger)
    {
        if (activeMusicTrigger != newTrigger)
        {
            if(activeMusicTrigger != null) activeMusicTrigger.StopAllCoroutines();
            activeMusicTrigger = newTrigger;
        }
    }

    private void Awake()
    {
        //Debug.Log("Total Objects: " + FindObjectsOfType<GameObject>().Length);

        Cursor.visible = showCursorInGame;
        //QualitySettings.vSyncCount = 1;
        //Application.targetFrameRate = 60;
        fps = Screen.currentResolution.refreshRate;

        if(fps != 60)
        {
            float fpsRatio = getFPSRatio();
            //var forceFields = Resources.FindObjectsOfTypeAll<ParticleSystemForceField>();
            //var forceFields = GameObject.FindObjectsOfType<ParticleSystemForceField>();
            var forceFields = FindAllObjectsInScene().Where(x => x.GetComponent<ParticleSystemForceField>() != null).ToList().ConvertAll(r => r.GetComponent<ParticleSystemForceField>());
            foreach(ParticleSystemForceField ff in forceFields)
            {
                ff.gravity = ff.gravity.constant / fpsRatio;
            }
        }

        List<GameObject> FindAllObjectsInScene()
        {
            Scene activeScene = SceneManager.GetActiveScene();

            GameObject[] rootObjects = activeScene.GetRootGameObjects();

            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            List<GameObject> objectsInScene = new List<GameObject>();

            for (int j = 0; j < rootObjects.Length; j++)
            {
                objectsInScene.Add(rootObjects[j]);
            }

            for (int k = 0; k < allObjects.Length; k++)
            {
                if (allObjects[k].transform.root)
                {
                    for (int i2 = 0; i2 < rootObjects.Length; i2++)
                    {
                        if (allObjects[k].transform.root == rootObjects[i2].transform && allObjects[k] != rootObjects[i2])
                        {
                            objectsInScene.Add(allObjects[k]);
                            break;
                        }
                    }
                }
            }
            return objectsInScene;
        }


        if (input == null)
        {
            input = new InputActions();
        }
        input.Menu.Enable();

        main_camera_brain = Camera.main.GetComponent<CinemachineBrain>();
        drawColliders = Camera.main.GetComponent<DrawColliders>();

        // CANVAS
        canvas = GameObject.FindGameObjectWithTag("Canvas").transform;

        // parents
        collectablesUI = canvas.Find("Collectables Grid").gameObject;
        textUI = canvas.Find("Text").gameObject;
        Transform collectablesScoreUI = canvas.Find("Collectables Score Grid");

        // text
        ManaCount = collectablesUI.transform.Find("Mana Count").GetComponent<Text>();
        DiamondCount = collectablesUI.transform.Find("Diamond Count").GetComponent<Text>();
        Timer = textUI.transform.Find("Time Text").GetComponent<Text>();
        FPS = textUI.transform.Find("Fps Text").GetComponent<Text>();

        ManaScore = collectablesScoreUI.transform.Find("Mana Count").GetComponent<Text>();
        DiamondScore = collectablesScoreUI.transform.Find("Diamond Count").GetComponent<Text>();

        TimeScore = canvas.transform.Find("Time Score Text").GetComponent<Text>();
        Saving = canvas.transform.Find("Saving_Continue Text").GetComponent<Text>();

        // pause
        statsObj = canvas.GetChild(3).gameObject;
        trackerObj = canvas.GetChild(4).gameObject;
        Pause_Menu = canvas.GetChild(5).gameObject;
        Menu1 = Pause_Menu.transform.GetChild(1).gameObject;
        Menu2 = Pause_Menu.transform.GetChild(2).gameObject;
        Menu_Button = Menu1.transform.GetChild(0).GetComponent<Button>();
        Restart_Button = Menu1.transform.GetChild(1).GetComponent<Button>();
        Options_Button = Menu1.transform.GetChild(2).GetComponent<Button>();
        Fullscreen_Button = Menu2.transform.GetChild(1).GetComponent<Button>();
        Particles_Button = Menu2.transform.GetChild(2).GetComponent<Button>();
        Effects_Button = Menu2.transform.GetChild(3).GetComponent<Button>();

        fixedJumpToggle = Menu2.transform.Find("FixedJumpToggle").GetComponent<Toggle>();
        holdJumpToggle = Menu2.transform.Find("HoldJumpToggle").GetComponent<Toggle>();
        hideUIToggle = Menu2.transform.Find("HideUIToggle").GetComponent<Toggle>();

        MusicText = Menu2.transform.Find("Music Value Text").GetComponent<Text>();
        SfxText = Menu2.transform.Find("Sfx Value Text").GetComponent<Text>();
        MusicSlider = Menu2.transform.Find("Music Slider").GetComponent<Slider>();
        SfxSlider = Menu2.transform.Find("Sfx Slider").GetComponent<Slider>();

        // restart
        Transform restartWarning = canvas.Find("Restart Warning");
        restartWarningText = restartWarning.GetChild(0).GetComponent<Text>();
        restartWarningSlider = restartWarning.GetChild(1).GetComponent<Slider>();

        // animator
        UIAnimator = canvas.GetComponent<Animator>();
        UIIntroSignal = canvas.Find("Black").gameObject;
        UIRestartSignal = canvas.Find("Restart Signal").gameObject;
        UIEndSignal = canvas.Find("End Signal").gameObject;

        // coin
        Transform coinicongrid = canvas.Find("Coin Grid");
        CoinIcons = new GameObject[3];
        CoinIcons[0] = coinicongrid.GetChild(0).gameObject;
        CoinIcons[1] = coinicongrid.GetChild(1).gameObject;
        CoinIcons[2] = coinicongrid.GetChild(2).gameObject;

        LoadPrefs();

        Resources.UnloadUnusedAssets();
        prev_width = Screen.width;
        prev_height = Screen.height;

        highdetail = new List<GameObject>();
        highdetaillights = new Dictionary<Light2D, Vector2>();

        restartWarningSliderFill = restartWarningSlider.transform.GetChild(1).GetChild(0).GetComponent<Image>();

        Restart_Button.onClick.AddListener(/*Restart*/StartRestart);
        Options_Button.onClick.AddListener(Options);
        Menu_Button.onClick.AddListener(ReturnToMenu);
        Fullscreen_Button.onClick.AddListener(ToggleFullscreen);
        Particles_Button.onClick.AddListener(ToggleParticles);
        Effects_Button.onClick.AddListener(TogglePostProcessing);

        // DevTools
        levelDevTools = FindObjectOfType<LevelDevTools>();


        // ------------------------------------------------

        //Res1440_Button.onClick.AddListener(() => { SetResolution(1440); });
        //Res1080_Button.onClick.AddListener(() => { SetResolution(1080); });
        //Res720_Button.onClick.AddListener(() => { SetResolution(720); });

        setButtonOn(Fullscreen_Button, Screen.fullScreen);
        Fullscreen_Button.GetComponentInChildren<Text>().text = "FULLSCREEN: " + (Screen.fullScreen ? "ON" : "OFF");

        setButtonOn(Particles_Button, PlayerPrefs.GetInt("screen_particles") == 1 ? true : false);
        Particles_Button.GetComponentInChildren<Text>().text = "SCREEN PARTICLES: " + (PlayerPrefs.GetInt("screen_particles") == 1 ? "ON" : "OFF");

        setButtonOn(Effects_Button, postfxon);
        Effects_Button.GetComponentInChildren<Text>().text = "POST PROCESSING: " + (postfxon ? "ON" : "OFF");

        MusicSlider.value = Mathf.Pow(music_volume, 2f / 3f);
        SfxSlider.value = Mathf.Pow(sfx_volume, 2f / 3f);

        Saving.text = "";

        ExitText = Saving.transform.GetChild(0).GetComponent<Text>();
        ExitText.text = "";
        ReplayText = Saving.transform.GetChild(1).GetComponent<Text>();
        ReplayText.text = "";

        LoadData();
        //LevelStartedEvent();

        int i = 0;
        for (int j = 0; j < coin_count.Length; j++)
        {
            CoinIcons[j].GetComponent<Image>().color = new Color(1, 1, 1, coin_count[j] == 1 ? 1 : 0);

            Coins[i].gameObject.SetActive(coin_count[j] == 0);
            Coins[i+1].gameObject.SetActive(coin_count[j] == 1);

            if (i == 0)
            {
                foreach (GameObject obj in coin1On) { obj.SetActive(coin_count[j] == 0); }
                foreach (GameObject obj in ghostCoin1On) { obj.SetActive(coin_count[j] == 1); }
            }
            else if (i == 2)
            {
                foreach (GameObject obj in coin2On) { obj.SetActive(coin_count[j] == 0); }
                foreach (GameObject obj in ghostCoin2On) { obj.SetActive(coin_count[j] == 1); }
            }
            else if (i == 4)
            {
                foreach (GameObject obj in coin3On) { obj.SetActive(coin_count[j] == 0); }
                foreach (GameObject obj in ghostCoin3On) { obj.SetActive(coin_count[j] == 1); }
            }


            i += 2;
        }

        player = GameObject.FindGameObjectWithTag("Player");
        GameObject master = GameObject.FindGameObjectWithTag("Master");
        newPlayer = player.GetComponent<PlayerControllerV2>();
        timeManager = master.GetComponent<TimeManager>();
        recorder = master.GetComponent<PlayerRecorder>();
        cps_controller = master.GetComponent<ClicksPerSecond>();
        //playerlight = GameObject.Find("Player Light Bright"); playerlight.SetActive(false);
        effects = GameObject.Find("EFFECTS");
        globallight = GameObject.Find("Global Light");

        /*cubecontroller = player.GetComponent<CubeController>();
        autocontroller = player.GetComponent<AutoController>();

        shipcontroller = player.GetComponent<ShipController>();
        autoshipcontroller = player.GetComponent<AutoShipController>();

        ufocontroller = player.GetComponent<UfoController>();
        autoufocontroller = player.GetComponent<AutoUfoController>();

        wavecontroller = player.GetComponent<WaveController>();
        autowavecontroller = player.GetComponent<AutoWaveController>();

        ballcontroller = player.GetComponent<BallController>();
        autoballcontroller = player.GetComponent<AutoBallController>();

        spidercontroller = player.GetComponent<SpiderController>();
        autospidercontroller = player.GetComponent<AutoSpiderController>();

        coptercontroller = player.GetComponent<CopterController>();
        autocoptercontroller = player.GetComponent<AutoCopterController>();*/

        //------------------------------------------------------------------------------------------------
        //playercontroller = cubecontroller;
        checkpointcontroller = FindObjectOfType<Checkpoint_Controller>();

        newPlayer.setBGMusic(bgmusic.audio);
        newbgmusic = bgmusic;

        iconcontroller = FindObjectOfType<IconController>();
        icon = iconcontroller.getIcon();

        //playercontroller.setIcons(icon);
        //playercontroller.resetStaticVariables();

        // ------------------------------------------------------------------------------------------------
        /*channel_colors = new Color[color_channels.Length];
        int i = 0;
        foreach(ColorReference c in color_channels)
        {
            channel_colors[i] = c.channelcolor;
            if(c.refer != null) { channel_colors[i] = c.refer.channelcolor; }
            i++;
        }*/

        // camera list ------------------------
        cameraList = new List<CinemachineVirtualCamera>();
        initialList = GameObject.FindGameObjectsWithTag("Camera");

        i = 0;
        foreach (GameObject g in initialList)
        {
            cameraList.Add(g.GetComponent<CinemachineVirtualCamera>());
            cameraList[i].gameObject.SetActive(true);
            //cameraList[i].Priority = 5;
        }

        /*if (postfxon && main_camera_brain.ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CinemachineVolumeSettings>() != null)
        {
            main_camera_brain.ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CinemachineVolumeSettings>().enabled = true;
            effects.SetActive(true);
        }
        else
        {
            main_camera_brain.ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CinemachineVolumeSettings>().enabled = false;
            effects.SetActive(false);
        }*/

        newPlayer.fixedJumpHeight = fixedJumpToggle.isOn;
        newPlayer.cubeHoldJump = holdJumpToggle.isOn;
        setToggleOn(fixedJumpToggle, fixedJumpToggle.isOn);
        setToggleOn(holdJumpToggle, holdJumpToggle.isOn);
        setToggleOn(hideUIToggle, hideUI);
    }

    public void addHighDetail(GameObject obj)
    {
        highdetail.Add(obj);
    }

    public void addHighDetailLights(Light2D light, Vector2 intensities)
    {
        highdetaillights.Add(light, intensities);
    }

    private void Start()
    {
        /*if (!bgmusic.isPlaying)
        {
            bgmusic.Play();
        }*/
        //SetPostProcessing(postfxon);
        //playercontroller.forceRespawn();
    }

    public List<CinemachineVirtualCamera> getCameraList()
    {
        return cameraList;
    }

    public CinemachineVirtualCamera getActiveCamera()
    {
        foreach (CinemachineVirtualCamera c in cameraList)
        {
            if (c.Priority == 10) { activeCamera = c; break; }
        }

        return activeCamera;
    }

    public Transform getPlayerTransform()
    {
        return player.transform;
    }

    /*void resetColorChannels()
    {
        int i = 0;
        foreach (ColorReference c in color_channels)
        {
            c.Set(channel_colors[i]);
            i++;
        }
    }*/

    // Button Functions
    public void StartRestart()
    {
        paused = false;
        //Time.timeScale = 1;
        timeManager.setScale(0, 1);

        levelDevTools.setTimeSlider(1);
        timeManager.setScale(4, 1);

        //playercontroller.setAble(false);
        //playercontroller.stopBGMusic();
        newPlayer.setAble(false);
        newPlayer.stopBGMusic();

        SavePrefs();

        UIAnimator.Play("UI_Restart_Sequence");
    }
    public void Restart()
    {
        //playercontroller.resetStaticVariables();
        //resetColorChannels();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        //Time.timeScale = 1.0f;
        timeManager.setScale(0, 1);
    }
    public void Options()
    {
        Menu1.SetActive(false);
        Menu2.SetActive(true);
    }
    public void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
        if(!Screen.fullScreen)
        {
            Screen.SetResolution(Screen.resolutions[Screen.resolutions.Length - 1].width, Screen.resolutions[Screen.resolutions.Length - 1].height, true);
        }
        setButtonOn(Fullscreen_Button, !Screen.fullScreen);
        Fullscreen_Button.GetComponentInChildren<Text>().text = "FULLSCREEN: " + (!Screen.fullScreen ? "ON" : "OFF");
        setButtonOn(Res1440_Button, false); setButtonOn(Res1080_Button, false); setButtonOn(Res720_Button, false);
    }
    public void ToggleParticles()
    {
        PlayerPrefs.SetInt("screen_particles", PlayerPrefs.GetInt("screen_particles") == 1 ? 0 : 1);
        setButtonOn(Particles_Button, PlayerPrefs.GetInt("screen_particles") == 1 ? true : false);
        Particles_Button.GetComponentInChildren<Text>().text = "SCREEN PARTICLES: " + (PlayerPrefs.GetInt("screen_particles") == 1 ? "ON" : "OFF");
    }
    public void SetResolution(int width)
    {
        switch(width)
        {
            case 1440:
                Screen.fullScreen = false; Screen.SetResolution(2560, 1440, true);
                setButtonOn(Res1440_Button, true); setButtonOn(Res1080_Button, false); setButtonOn(Res720_Button, false);
                setButtonOn(Fullscreen_Button, false);
                Fullscreen_Button.GetComponentInChildren<Text>().text = "FULLSCREEN: OFF"; break;
            case 1080:
                Screen.fullScreen = false; Screen.SetResolution(1920, 1080, true);
                setButtonOn(Res1440_Button, false); setButtonOn(Res1080_Button, true); setButtonOn(Res720_Button, false);
                setButtonOn(Fullscreen_Button, false);
                Fullscreen_Button.GetComponentInChildren<Text>().text = "FULLSCREEN: OFF"; break;
            case 720:
                Screen.fullScreen = false; Screen.SetResolution(1280, 720, true); 
                setButtonOn(Res1440_Button, false); setButtonOn(Res1080_Button, false); setButtonOn(Res720_Button, true);
                setButtonOn(Fullscreen_Button, false);
                Fullscreen_Button.GetComponentInChildren<Text>().text = "FULLSCREEN: OFF"; break;
            default:
                break;
        }
        
    }

    public void SetPostProcessing(bool set)
    {
        effects.SetActive(set);
        foreach (GameObject obj in highdetail)
        {
            if (obj != null) obj.SetActive(set);
        }
        foreach (KeyValuePair<Light2D, Vector2> light_intensities in highdetaillights)
        {
            if (light_intensities.Key != null)
            {
                light_intensities.Key.intensity = set ? light_intensities.Value.x : light_intensities.Value.y;
            }
        }
        //Debug.Log(main_camera_brain.ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CinemachineVolumeSettings>() == null);
        postfxon = set;
        if (postfxon && main_camera_brain.ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CinemachineVolumeSettings>() != null)
        {
            main_camera_brain.ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CinemachineVolumeSettings>().enabled = true;
        }
        else
        {
            main_camera_brain.ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CinemachineVolumeSettings>().enabled = false;
        }

        setButtonOn(Effects_Button, postfxon);
        Effects_Button.GetComponentInChildren<Text>().text = "POST PROCESSING: " + (postfxon ? "ON" : "OFF");
    }
    public void TogglePostProcessing()
    {
        effects.SetActive(!effects.activeSelf);
        foreach (GameObject obj in highdetail)
        {
            if (obj != null) obj.SetActive(!postfxon);
        }
        foreach (KeyValuePair<Light2D, Vector2> light_intensities in highdetaillights)
        {
            if (light_intensities.Key != null)
            {
                light_intensities.Key.intensity = !postfxon ? light_intensities.Value.x : light_intensities.Value.y;
            }
        }
        //Debug.Log(main_camera_brain.ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CinemachineVolumeSettings>() == null);
        postfxon = !postfxon;
        if (postfxon && main_camera_brain.ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CinemachineVolumeSettings>() != null)
        {
            main_camera_brain.ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CinemachineVolumeSettings>().enabled = true;
        }
        else
        {
            main_camera_brain.ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CinemachineVolumeSettings>().enabled = false;
        }

        setButtonOn(Effects_Button, postfxon);
        Effects_Button.GetComponentInChildren<Text>().text = "POST PROCESSING: " + (postfxon ? "ON" : "OFF");
    }
    public void setButtonOn(Button button, bool on)
    {
        if(on)
        {
            ColorBlock colorblock = button.colors;
            colorblock.normalColor = new Color32(255, 255, 255, 255);
            colorblock.highlightedColor = new Color32(245, 245, 245, 112);
            colorblock.pressedColor = new Color32(200, 200, 200, 255);
            //colorblock.selectedColor = new Color32(245, 245, 245, 255);
            colorblock.disabledColor = new Color32(128, 128, 128, 255);

            button.colors = colorblock;
            button.GetComponentInChildren<Text>().color = Color.black;
        }
        else
        {
            ColorBlock colorblock = button.colors;
            colorblock.normalColor = new Color32(255, 255, 242, 13);
            colorblock.highlightedColor = new Color32(245, 245, 245, 112);
            colorblock.pressedColor = new Color32(200, 200, 200, 255);
            //colorblock.selectedColor = new Color32(245, 245, 245, 255);
            colorblock.disabledColor = new Color32(128, 128, 128, 255);

            button.colors = colorblock;
            button.GetComponentInChildren<Text>().color = Color.white;
        }
    }
    public void setToggleOn(Toggle toggle, bool on)
    {
        if (on)
        {
            ColorBlock colorblock = toggle.colors;
            colorblock.normalColor = new Color32(255, 255, 255, 255);
            colorblock.highlightedColor = new Color32(245, 245, 245, 112);
            colorblock.pressedColor = new Color32(200, 200, 200, 255);
            //colorblock.selectedColor = new Color32(245, 245, 245, 255);
            colorblock.disabledColor = new Color32(128, 128, 128, 255);

            toggle.colors = colorblock;
            //button.GetComponentInChildren<Text>().color = Color.black;
        }
        else
        {
            ColorBlock colorblock = toggle.colors;
            colorblock.normalColor = new Color32(255, 255, 242, 60);
            colorblock.highlightedColor = new Color32(245, 245, 245, 112);
            colorblock.pressedColor = new Color32(200, 200, 200, 255);
            //colorblock.selectedColor = new Color32(245, 245, 245, 255);
            colorblock.disabledColor = new Color32(128, 128, 128, 255);

            toggle.colors = colorblock;
            //button.GetComponentInChildren<Text>().color = Color.white;
        }
    }
    void OnApplicationQuit()
    {
        Resources.UnloadUnusedAssets();
    }
    public void ReturnToMenu()
    {
        SavePrefs();
        //Application.Quit();
        //Time.timeScale = 1.0f;
        timeManager.setScale(0, 1);
        LevelExited();
        page.State = 2;
        levelDevTools.setTimeSlider(1);
        timeManager.setScale(4, 1);
        SceneManager.LoadScene("Main Menu");
    }
    void Update()
    {
        if (restartWarningSlider.value > 0)
        {
            restartWarningSlider.value -= Time.unscaledDeltaTime;
            restartWarningText.color = new Color(1, 1, 1, Mathf.Pow(restartWarningSlider.value, .4f));
            restartWarningSliderFill.color = new Color(1, 1, 1, Mathf.Pow(restartWarningSlider.value, .4f));
        }
        else
        {
            restartWarningText.color = Color.clear;
            restartWarningSliderFill.color = Color.clear;
        }

        if (UIRestartSignal.activeSelf) { Restart(); }
        if (UIIntroSignal.activeSelf)
        {
            //playercontroller.forceRespawn();
            //playercontroller.setAble(false);
            return;
        }
        if (!start)
        {
            start = true;
            SetPostProcessing(postfxon);
            //playercontroller.playBGMusic();
            //playercontroller.setAble(true);
            //playercontroller.forceRespawn();
            newPlayer.playBGMusic();
            newPlayer.setAble(true);
            //recorder.start = true;
            recorder.Begin();
        }

        if (Input.GetKeyDown(KeyCode.F11))
        {
            if (!Screen.fullScreen)
            {
                Screen.SetResolution(Screen.resolutions[Screen.resolutions.Length - 1].width, Screen.resolutions[Screen.resolutions.Length - 1].height, true);
                Screen.fullScreen = true;
            }
            else
            {
                Screen.fullScreen = !Screen.fullScreen;
            }
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            hideUI = !hideUI;
            hideUIToggle.isOn = hideUI;

            //collectablesUI.SetActive(!hideUI);
            //textUI.SetActive(!hideUI);
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            showStats = !showStats;
            levelDevTools.enabled = showStats;
            statsObj.SetActive(showStats);

            trackerObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, showStats ? -360 : -455, 0);

            Cursor.visible = !paused ? showStats || lineTrackerObj.activeSelf : true;
            showCursorInGame = showStats || lineTrackerObj.activeSelf;

            //if (!showStats) { timeManager.setScale(4, 1); levelDevTools.setTimeSlider(1); }
        }

        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            if (timeManager.getScale(4) == 0) { levelDevTools.FrameStep(); }
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            drawOn = !drawOn;
            drawColliders.enabled = drawOn;
            if (drawOn) { drawColliders.GetColliders(); }
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            hideRenderers = !hideRenderers;
            drawColliders.HideRenderers(hideRenderers);
        }

        if (Input.GetKeyDown(KeyCode.F4))
        {
            lineTrackerObj.SetActive(!lineTrackerObj.activeSelf);
            trackerObj.SetActive(lineTrackerObj.activeSelf);

            trackerObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, showStats ? -360 : -455, 0);

            Cursor.visible = !paused ? lineTrackerObj.activeSelf || showStats : true;
            showCursorInGame = lineTrackerObj.activeSelf || showStats;
        }

        if (input.Menu.Restart.triggered/*Input.GetKeyDown("r")*/ && !halt)
        {
            if (restartWarningSlider.value <= 0)
            {
                restartWarningSlider.value = restartWarningSlider.maxValue;
            }
            else
            {
                StartRestart();
            }
        }

        if(endscreen)
        {
            if (Input.GetKeyDown("space") && !halt)
            {
                StartRestart();
            }
            if (/*Input.GetKeyDown("escape")*/input.Menu.Esc.triggered && !halt)
            {
                ReturnToMenu();
            }
        }

        // SOUND
        music_volume = Mathf.Pow(MusicSlider.value, 1.5f);
        sfx_volume = Mathf.Pow(SfxSlider.value, 1.5f);
        MusicText.text = "Music: " + (int)(MusicSlider.value * 100);
        SfxText.text = "Sfx: " + (int)(SfxSlider.value * 100);
        bgmusic.audio.volume = bgmusic.realVolume * music_volume;

        if (!game)
        {
            // PAUSE MENU
            if (input.Menu.Esc.triggered || (paused && input.Menu.Back.triggered)/*Input.GetKeyDown("escape")*/)
            {
                if (!paused)
                {
                    Cursor.visible = true;
                    paused = !paused;
                    Pause_Menu.SetActive(true);
                    bgmusic.Pause();
                    //Time.timeScale = 0;
                    timeManager.setScale(0, 0);

                    restartWarningSlider.value = 0;
                }
                else if (paused)
                {
                    if (Menu1.activeSelf)
                    {
                        Cursor.visible = showCursorInGame;
                        paused = !paused;
                        Pause_Menu.SetActive(false);
                        //Time.timeScale = 1;
                        timeManager.setScale(0, 1);
                        bgmusic.Play();
                    }
                    else
                    {
                        Menu1.SetActive(true);
                        Menu2.SetActive(false);
                    }
                }
            }

            if(paused)
            {
                MenuNavigation();
            }

            if(prevFixed != fixedJumpToggle.isOn)
            {
                fixedJump = fixedJumpToggle.isOn ? 1 : 0;
                newPlayer.fixedJumpHeight = fixedJumpToggle.isOn;
                setToggleOn(fixedJumpToggle, fixedJumpToggle.isOn);
            }
            if (prevHold != holdJumpToggle.isOn)
            {
                holdJump = holdJumpToggle.isOn ? 1 : 0;
                newPlayer.cubeHoldJump = holdJumpToggle.isOn;
                setToggleOn(holdJumpToggle, holdJumpToggle.isOn);
            }
            if (prevHideUI != hideUIToggle.isOn)
            {
                hideUI = hideUIToggle.isOn;

                collectablesUI.SetActive(!hideUI);
                textUI.SetActive(!hideUI);

                setToggleOn(hideUIToggle, hideUIToggle.isOn);
            }

            // UI TEXT
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

            if (Time.deltaTime != 0)
            {
                milli = (int)(time * 1000) % 1000;
                sec = (int)(time);
                min = (int)(sec / 60);
            

                milli -= (1000 * m);
                sec -= (60 * s);
            

                if (milli == 1000) { m++; };
                if (sec == 60) { s++; };
            }

            ManaCount.text = "x" + mana_count;
            DiamondCount.text = "x" + diamond_count;
            Timer.text = "Time: " + min + " : " + (sec < 10 ? "0" : "") + sec + " : " + (milli < 100 ? "0" : "") + (milli < 10 ? "0" : "") + milli;
            FPS.text = Mathf.RoundToInt((1 / deltaTime)) + " FPS\n" + Time.timeScale + "x speed";
            fps = (fps + Mathf.RoundToInt((1 / deltaTime))) / 2;
            //fps = (int)(1 / Time.deltaTime);

            ManaScore.text = ManaCount.text;
            DiamondScore.text = DiamondCount.text;
            TimeScore.text = min + " : " + (sec < 10 ? "0" : "") + sec + " : " + (milli < 100 ? "0" : "") + (milli < 10 ? "0" : "") + milli;

            //time += Time.unscaledDeltaTime * timeManager.getScale(0);
            //time += deltaTime * timeManager.getScale(0);
            time += Time.deltaTime != 0 ? (1 / Time.timeScale) * Time.deltaTime : 0;
        }
        else if(addtimediff)
        {
            addtimediff = false;
            milli = (int)(time * 1000) % 1000;
            sec = (int)(time);
            min = (int)(sec / 60);

            milli -= (1000 * m);
            sec -= (60 * s);

            if (milli == 1000) { m++; };
            if (sec == 60) { s++; };

            Timer.text = "Time: " + min + " : " + (sec < 10 ? "0" : "") + sec + " : " + (milli < 100 ? "0" : "") + (milli < 10 ? "0" : "") + milli;
            TimeScore.text = min + " : " + (sec < 10 ? "0" : "") + sec + " : " + (milli < 100 ? "0" : "") + (milli < 10 ? "0" : "") + milli;
        }

        prev_height = Screen.height;
        prev_width = Screen.width;
        prevFixed = fixedJumpToggle.isOn;
        prevHold = holdJumpToggle.isOn;
        prevHideUI = hideUIToggle.isOn;
    }

    int fps;
    public int getFPS()
    {
        return fps;
    }

    public float getFPSRatio()
    {
        return ((float)fps) / 60f;
    }

    void MenuNavigation()
    {
        if(Menu1.activeSelf)
        {
            if ((input.Menu.Up.triggered || input.Menu.Down.triggered || input.Menu.Left.triggered || input.Menu.Right.triggered)
            && (EventSystem.current.currentSelectedGameObject != Restart_Button.gameObject)
            && (EventSystem.current.currentSelectedGameObject != Menu_Button.gameObject)
            && (EventSystem.current.currentSelectedGameObject != Options_Button.gameObject))
            {
                EventSystem.current.SetSelectedGameObject(Menu_Button.gameObject);
            }
        }
        else
        {
            if ((input.Menu.Up.triggered || input.Menu.Down.triggered || input.Menu.Left.triggered || input.Menu.Right.triggered)
            && (EventSystem.current.currentSelectedGameObject != Fullscreen_Button.gameObject)
            && (EventSystem.current.currentSelectedGameObject != Particles_Button.gameObject)
            && (EventSystem.current.currentSelectedGameObject != Effects_Button.gameObject)
            && (EventSystem.current.currentSelectedGameObject != MusicSlider.gameObject)
            && (EventSystem.current.currentSelectedGameObject != SfxSlider.gameObject)
            && (EventSystem.current.currentSelectedGameObject != fixedJumpToggle.gameObject)
            && (EventSystem.current.currentSelectedGameObject != holdJumpToggle.gameObject)
            && (EventSystem.current.currentSelectedGameObject != hideUIToggle.gameObject))
            {
                EventSystem.current.SetSelectedGameObject(Effects_Button.gameObject);
            }
        }
    }

    private void LateUpdate()
    {
        if(game)
        {
            if (!endscreen) { halt = true; }
            player.GetComponent<Collider2D>().enabled = true;
            player.GetComponent<Collider2D>().isTrigger = true;

            Rigidbody2D playerbody = player.GetComponent<Rigidbody2D>();

            //playercontroller.setAble(false);
            //playercontroller.TurnOffEverything();
            //playercontroller.enabled = false;
            newPlayer.setAble(false);
            newPlayer.enabled = false;

            playerbody.interpolation = RigidbodyInterpolation2D.Extrapolate;
            playerbody.velocity = Vector2.zero;
            playerbody.gravityScale = 0;
            //playerbody.rotation = Mathf.Lerp(playerbody.rotation, playerbody.rotation - 10, .6f);

            Vector3 newRotation = new Vector3(0, 0, newPlayer.getIconTransform().localEulerAngles.z - (360));
            newPlayer.getIconTransform().localEulerAngles = Vector3.MoveTowards(newPlayer.getIconTransform().localEulerAngles, newRotation, 300 * Time.deltaTime);
        }
    }

    private void LoadData()
    {
        string path = Application.persistentDataPath + "/savedata.gja";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            data = formatter.Deserialize(stream) as GlobalData;

            coin_count[0] = data.levels_completed_and_coins[levelNumber, 1];
            coin_count[1] = data.levels_completed_and_coins[levelNumber, 2];
            coin_count[2] = data.levels_completed_and_coins[levelNumber, 3];
            stream.Close();
        }
        else
        {
            Debug.LogError("No Save File Found");
        }
    }

    private void SaveData()
    {
        string path = Application.persistentDataPath + "/savedata.gja";
        BinaryFormatter formatter = new BinaryFormatter();
        GlobalData data = new GlobalData();
        FileStream stream;

        if (File.Exists(path))
        {
            
            stream = new FileStream(path, FileMode.Open);
            data = formatter.Deserialize(stream) as GlobalData;
            stream.Close();
        }

        bool allcoins = true;
        for (int i = 0; i < coin_count.Length; i++)
        {
            if(coin_count[i] == 2)
                coins_gotten++;

            if (coin_count[i] == 2 || coin_count[i] == 3)
            {
                coin_count[i] = 1;
            }
            else
            {
                allcoins = false;
            }
        }

        LevelCompletedEvent(allcoins);
        data.SaveLevelData(levelNumber, coin_count, time, allcoins ? time : float.MaxValue, diamond_count, coins_gotten);

        stream = new FileStream(path, FileMode.Create);
        formatter.Serialize(stream, data);
        stream.Close();
    }

    public IEnumerator countScore()
    {
        while(!UIEndSignal.activeSelf)
        {
            yield return null;
        }

        endscreen = true;
        Saving.gameObject.SetActive(true);
        Saving.text = "Saving...";
        for (int i = 0; i < coin_count.Length; i++)
        {
            if (coin_count[i] == 2 || coin_count[i] == 3)
            {
                CoinIcons[i].transform.localScale = new Vector3(3, 3, 1);
                Image coinimage = CoinIcons[i].GetComponent<Image>();
                coinimage.color = new Color(1, 1, 1, 0);

                yield return null;

                float timer = 0;

                coinget.PlayOneShot(coinget.clip, sfx_volume);
                while (coinimage.color.a < 1f)
                {
                    CoinIcons[i].transform.localScale = Vector3.Lerp(new Vector3(3, 3, 1), new Vector3(1, 1, 1), timer / .2f);
                    coinimage.color = Color.Lerp(new Color(1, 1, 1, 0), new Color(1, 1, 1, 1), timer / .2f);
                    timer += Time.deltaTime;

                    yield return null;
                }

                //CoinIcons[i].transform.localScale = new Vector3(1, 1, 1);
                coinimage.color =new Color(1, 1, 1, 1);

                timer = 0;
                while(timer < .1f)
                {
                    timer += Time.deltaTime;
                    yield return new WaitForEndOfFrame();
                }
            }
        }
        
        int diamondgain = 0;
        for(int i = mana_count; i > 0; i--)
        {
            float timer = 0;
            while (timer < .001f)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            mana_count--;
            diamondgain++;

            if(diamondgain % 25 == 0)
            {
                diamond_count++;
                diamondget.PlayOneShot(diamondget.clip, sfx_volume);
            }

            ManaScore.text = "x" + mana_count;
            DiamondScore.text = "x" + diamond_count;
        }

        SaveData();
        recorder.start = false;
        recorder.SaveData();
        Saving.text = "Save Complete!";
        ExitText.text = "Menu (Esc)";
        ReplayText.text = "Retry (Space)";
        halt = false;
    }

    /*public PlayerController getController()
    {
        if (cubecontroller.isActiveAndEnabled)
        {
            playercontroller = cubecontroller;
            return playercontroller;
        }
        else if (autocontroller.isActiveAndEnabled)
        {
            playercontroller = autocontroller;
            return playercontroller;
        }
        else if (shipcontroller.isActiveAndEnabled)
        {
            playercontroller = shipcontroller;
            return playercontroller;
        }
        else if (autoshipcontroller.isActiveAndEnabled)
        {
            playercontroller = autoshipcontroller;
            return playercontroller;
        }
        else if (ufocontroller.isActiveAndEnabled)
        {
            playercontroller = ufocontroller;
            return playercontroller;
        }
        else if (autoufocontroller.isActiveAndEnabled)
        {
            playercontroller = autoufocontroller;
            return playercontroller;
        }
        else if (wavecontroller.isActiveAndEnabled)
        {
            playercontroller = wavecontroller;
            return playercontroller;
        }
        else if (autowavecontroller.isActiveAndEnabled)
        {
            playercontroller = autowavecontroller;
            return playercontroller;
        }
        else if (ballcontroller.isActiveAndEnabled)
        {
            playercontroller = ballcontroller;
            return playercontroller;
        }
        else if (autoballcontroller.isActiveAndEnabled)
        {
            playercontroller = autoballcontroller;
            return playercontroller;
        }
        else if (spidercontroller.isActiveAndEnabled)
        {
            playercontroller = spidercontroller;
            return playercontroller;
        }
        else if (autospidercontroller.isActiveAndEnabled)
        {
            playercontroller = autospidercontroller;
            return playercontroller;
        }
        else if (coptercontroller.isActiveAndEnabled)
        {
            playercontroller = coptercontroller;
            return playercontroller;
        }
        else if (autocoptercontroller.isActiveAndEnabled)
        {
            playercontroller = autocoptercontroller;
            return playercontroller;
        }
        else
        {
            return playercontroller;
        }

        return playercontroller;
    }*/

    public PlayerControllerV2 getController()
    {
        return newPlayer;
    }

    public void setMusicOnGamemodeChange(bool startmusic, bool forceStart, bool restartmusic)
    {
        if (bgmusic != newbgmusic && startmusic)
        {
            bgmusic.Stop();
            bgmusic = newbgmusic;
        }
        else
        {
            startmusic = forceStart;
        }

        if (startmusic)
        {
            //Debug.Log("START MUSIC");
            newPlayer.setBGMusic(bgmusic.audio);
            //bgmusic.Stop();
            bgmusic.Play();
            //bgmusic.volume = music_volume;
            bgmusic.realVolume = 1;
            bgmusic.audio.volume = music_volume;
        }

        newPlayer.setRestartMusic(restartmusic);
    }

    /*public void setPlayerController(string mode, bool startmusic, bool restartmusic)
    {
        //bool reversed = playercontroller.getReversed();
        //bool mini = playercontroller.getMini();
        //float speed;

        if(mode.Equals(playercontroller.getMode()))
        {
            //Debug.Log("Equals");
            if (bgmusic != newbgmusic && startmusic)
            {
                bgmusic.Stop();
                bgmusic = newbgmusic;
            }
            else
            {
                startmusic = false;
            }

            return;
        }

        if (mode != playercontroller.getMode())
        {
            //Debug.Log("curr: " + playercontroller.getMode() + "    new: " + mode);
            if (bgmusic != newbgmusic && startmusic)
            {
                bgmusic.Stop();
                bgmusic = newbgmusic;
            }

            playercontroller.enabled = false;
            playercontroller.setAble(false);
            bool jumpVal = playercontroller.getJump();
            playercontroller.setJump(false, false);
            //reversed = playercontroller.getReversed();
            //speed = playercontroller.getSpeed();
            //mini = playercontroller.getMini();
            playercontroller.resetColliders();
            //playercontroller.setVariables(false, false, false);

            if (playercontroller.isDead())
            {
                //reversed = checkpointcontroller.getReversed();
                //speed = checkpointcontroller.getSpeed();
            }

            switch(mode)
            {
                case "cube": playercontroller = cubecontroller; break;
                case "auto": playercontroller = autocontroller; break;
                case "ship": playercontroller = shipcontroller; break;
                case "auto_ship": playercontroller = autoshipcontroller; break;
                case "ufo": playercontroller = ufocontroller; break;
                case "auto_ufo": playercontroller = autoufocontroller; break;
                case "wave": playercontroller = wavecontroller; break;
                case "auto_wave": playercontroller = autowavecontroller; break;
                case "ball": playercontroller = ballcontroller; break;
                case "auto_ball": playercontroller = autoballcontroller; break;
                case "spider": playercontroller = spidercontroller; break;
                case "auto_spider": playercontroller = autospidercontroller; break;
                case "copter":
                    bool gu = false;
                    if (playercontroller == autocoptercontroller)
                    {
                        gu = autocoptercontroller.getGoingUp();
                    }
                    playercontroller = coptercontroller; coptercontroller.setGoingUp(gu); break;
                case "auto_copter":
                    gu = false;
                    if (playercontroller == coptercontroller)
                    {
                        gu = coptercontroller.getGoingUp();
                    }
                    playercontroller = autocoptercontroller; autocoptercontroller.setGoingUp(gu); break;
            }

            //playercontroller.setIconRigidbody();
            playercontroller.setAble(false);
            playercontroller.setColliders();
            playercontroller.setIcons(icon);
            playercontroller.setBGMusic(bgmusic.audio);
            //playercontroller.setSpeed(speed);
            playercontroller.setRestartMusicOnDeath(restartmusic);

            if (startmusic)
            {
                bgmusic.Play();
                //bgmusic.volume = music_volume;
                bgmusic.realVolume = 1;
                bgmusic.audio.volume = music_volume;
            }
            /*
            if (checkpointcontroller.getIndex() != -1)
            {
                playercontroller.setRespawn(checkpointcontroller.getTransform(), checkpointcontroller.getReversed(), checkpointcontroller.getMini());
                playercontroller.setRepawnSpeed(checkpointcontroller.getSpeed());
            }


            //playercontroller.setVariables((Input.GetButton("Jump") || Input.GetKey("space")), reversed, mini);
            playercontroller.setAnimation();
            playercontroller.enabled = true;
            playercontroller.setAble(true);
            playercontroller.setJump(jumpVal, false);

            return;
        }
    }*/

    public void playBGMusic(float playvolume)
    {
        if (bgmusic != newbgmusic)
        {
            bgmusic.Stop();
            bgmusic = newbgmusic;
        }

        //playercontroller.setBGMusic(bgmusic.audio);
        newPlayer.setBGMusic(bgmusic.audio);

        //bgmusic.volume = playvolume * music_volume;
        bgmusic.realVolume = playvolume;
        bgmusic.audio.volume = bgmusic.realVolume * music_volume;
        bgmusic.Play();
    }

    public void setBGMusic(MusicSource music)
    {
        newbgmusic = music;
    }

    public void setToNewBGMusic()
    {
        bgmusic.Stop();
        bgmusic = newbgmusic;

        //playercontroller.setBGMusic(bgmusic.audio);
        newPlayer.setBGMusic(bgmusic.audio);

        bgmusic.realVolume = 1;
        bgmusic.audio.volume = music_volume;
    }

    public MusicSource getBGMusic()
    {
        return bgmusic;
    }

    public void incrementManaCount(int amt)
    {
        mana_count += amt;
    }

    public int getManaCount()
    {
        return mana_count;
    }

    public void incrementDiamondCount(int amt)
    {
        diamond_count += amt;
    }

    public int getDiamondCount()
    {
        return diamond_count;
    }

    public void incrementCoinCount(int num, bool check, bool ghost)
    {
        if (check) { coin_count[num - 1] = -1 + (ghost?-1:0); }
        else { coin_count[num - 1] = 2 + (ghost ? 1 : 0); }
    }

    public int[] getCoinCount()
    {
        return coin_count;
    }

    public void resolveCoins(bool proceed)
    {
        for (int i = 0; i < coin_count.Length; i++)
        {
            if(coin_count[i] == -1)
            {
                coin_count[i] = proceed ? 2 : 0;
                if (!proceed)
                {
                    Coins[i*2].resetCoin();
                }
                else
                {
                    Destroy(Coins[i * 2].gameObject);
                }
            }
            else if (coin_count[i] == -2)
            {
                coin_count[i] = proceed ? 3 : 1;
                if (!proceed)
                {
                    Coins[(i * 2) + 1].resetCoin();
                }
                else
                {
                    Destroy(Coins[(i * 2) + 1].gameObject);
                    //CoinIcons[i * 2].gameObject.SetActive(false);
                    //CoinIcons[(i * 2) + 1].gameObject.SetActive(true);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            game = true;
            StartCoroutine(movex.Move());
            StartCoroutine(movey.Move());
        }
    }

    public bool getpostfx()
    {
        return postfxon;
    }

    public float getTime()
    {
        return time;
    }

    public string getFormattedTime()
    {
        return min + ":" + (sec < 10 ? "0" : "") + sec + ":" + (milli < 100 ? "0" : "") + (milli < 10 ? "0" : "") + milli;
    }

    public int getLevelNumber()
    {
        return levelNumber;
    }

    public Animator GetUIAnimator()
    {
        return UIAnimator;
    }

    public bool isPaused()
    {
        return paused;
    }

    public bool getGameEnd()
    {
        return game;
    }

    public void setTime(float t)
    {
        time = t;
    }

    // PLAYER PREFERENCES
    public void SavePrefs()
    {
        PlayerPrefs.SetFloat("music_volume", music_volume);
        PlayerPrefs.SetFloat("sfx_volume", sfx_volume);
        PlayerPrefs.SetInt("post_proccessing_on", postfxon ? 1 : 0);
        PlayerPrefs.SetInt("fixed_jump", fixedJump);
        PlayerPrefs.SetInt("hold_jump", holdJump);
        PlayerPrefs.SetInt("hide_ui", hideUI ? 1 : 0);
        PlayerPrefs.SetInt("record_playback", record_playback);
        PlayerPrefs.SetInt("record_playback_100", record_playback_100);
        trackerObj.GetComponent<InputLineTrackerUI>().SavePlayerPrefs();
        PlayerPrefs.Save();
    }

    public void LoadPrefs()
    {
        music_volume = PlayerPrefs.GetFloat("music_volume", .8f);
        sfx_volume = PlayerPrefs.GetFloat("sfx_volume", 1);
        postfxon = PlayerPrefs.GetInt("post_proccessing_on", 1) == 1 ? true : false;
        fixedJump = PlayerPrefs.GetInt("fixed_jump", 0);
        holdJump = PlayerPrefs.GetInt("hold_jump", 0);
        hideUI = PlayerPrefs.GetInt("hide_ui", 0) == 1;

        record_playback = PlayerPrefs.GetInt("record_playback", 0);
        record_playback_100 = PlayerPrefs.GetInt("record_playback_100", 0);

        fixedJumpToggle.isOn = fixedJump == 1 ? true : false;
        holdJumpToggle.isOn = holdJump == 1 ? true : false;
        hideUIToggle.isOn = hideUI;
        prevFixed = fixedJumpToggle.isOn;
        prevHold = holdJumpToggle.isOn;
        prevHideUI = hideUI;
        collectablesUI.SetActive(!hideUI);
        textUI.SetActive(!hideUI);
    }
}
