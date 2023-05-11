using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using Shapes2D;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerControllerV2 : MonoBehaviour
{
    // INPUT
    private InputActions input;
    private GameManager gamemanager;
    private TimeManager timeManager;

    // EVENTS
    public delegate void DeathAction();
    public static event DeathAction OnDeath;

    public delegate void RespawnAction();
    public static event RespawnAction OnRespawn;

    // MODE
    public enum Gamemode
    {
        cube, auto_cube,
        ship, auto_ship,
        ufo, auto_ufo,
        wave, auto_wave,
        ball, auto_ball,
        spider, auto_spider,
        copter, auto_copter,
        spring, auto_spring,
        robot, auto_robot
    }
    public Gamemode gamemode;

    // ICON
    [SerializeField]
    private GameObject ship, jetpack, ufo, wave, ball, spider, copter;

    // 'PUBLIC' REFS
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask deathLayer;
    [SerializeField] private PulseTrigger pulse_trigger_p1, pulse_trigger_p2;

    [Header("Animation Curves")]
    [SerializeField] private AnimationCurve shipAccelerationCurve;
    [SerializeField] private AnimationCurve shipDeccelerationCurve;

    [Header("Colliders")]
    [SerializeField] private BoxCollider2D player_collider;
    [SerializeField] private BoxCollider2D crouch_collider;
    private bool crouchCollider;

    [Header("Icon")]
    [SerializeField] private Transform iconParent;
    [SerializeField] private Shape sdshape;

    [Header("Particles")]
    [SerializeField] private ParticleSystem grounded_particles;
    [SerializeField] private ParticleSystem ground_impact_particles;
    [SerializeField] private ParticleSystem death_particles;
    [SerializeField] private ParticleSystem[] speed_particles;
    [SerializeField] private ParticleSystem[] gravity_particles;
    [SerializeField] private ParticleSystem dash_particles;
    [SerializeField] private ParticleSystem[] flame_burst;
    [SerializeField] private ParticleSystem flame_stream;
    [SerializeField] private ParticleSystem flame_spray;
    [SerializeField] private ParticleSystem windUp;
    [SerializeField] private ParticleSystem windDown;

    [Header("Trails")]
    [SerializeField] private TrailRenderer main_trail;
    [SerializeField] private TrailRenderer spider_trail;
    [SerializeField] private GameObject jepack_trails;
    [SerializeField] private GameObject wave_trail;
    [SerializeField] private GameObject auto_wave_trail;

    [Header("Effects")]
    [SerializeField] private GameObject dash_effect;
    [SerializeField] private GameObject dash_parent;
    [SerializeField] private Material dash_flame_material;
    [SerializeField] private Transform grounded_particles_parent;

    [Header("Animators")]
    [SerializeField] Animator Spider_Anim;
    [SerializeField] Animator Copter_Anim;

    [Header("Audio")]
    [SerializeField] private AudioSource death_sfx;
    private AudioSource bgmusic;
    private bool restartMusic;

    private SpiderTrail SpOrTr;

    // PRIVATE REFS
    private Rigidbody2D player_body;
    private Animator Cube_Anim;
    private Transform icons;
    private GameObject eyesParent, icon;
    private GameObject[] eyes;
    private int eyeType, spiderAnimType = 0, spiderAnimSpeed = 1;
    private GameObject OrbTouched, PadTouched, PortalTouched;
    private GameObject ChargedTeleportCTouched;
    private Camera mainCamera;

    // STATE
    private bool able, dead, mini, launched;
    public int reverseDir;
    private float firstFramesBuffer, groundBuffer;
    private const float FRAMEBUFFERTIME = 0.1f, GROUNDBUFFERTIME = 0.1f;

    // STATS
    private int jumpCount, deathCount, checkpointCount;
    private float runningCPS, clicks;

    private class GamemodeConstants
    {
        public readonly bool auto, dashSpin, orbBuffer, cancelJump;
        public readonly float jumpForce, miniJumpForce, maxSpeed, /*posMaxSpeed, */gravityStrength;
        public readonly float accelerationNG, acceleration, deceleration, chargedAcceleration;
        public readonly float yellowOrbMultiplier, redOrbMultiplier, pinkOrbMultiplier,
                                blueOrbMultiplier, greenOrbMultiplier, blackOrbMultiplier;
        public readonly float yellowPadMultiplier, redPadMultiplier, pinkPadMultiplier,
                                bluePadMultiplier, greenPadMultiplier, blackPadMultiplier;

        public float posMaxSpeed;

        public float boxX, boxY, boxR, jboxX, jboxY, jboxR;
        

        public GamemodeConstants(bool au, bool ds, bool ob, bool cj,
                                float bx, float by, float br, float jbx, float jby, float jbr,
                                float jF, float mjF, float mS, float pmS, float gS, float accNG, float acc, float dec, float cacc,
                                float rOM, float yOM, float pOM, float bOM, float gOM, float bbOM,
                                float rPM, float yPM, float pPM, float bPM, float gPM, float bbPM)
        {
            auto = au;
            dashSpin = ds;
            orbBuffer = ob;
            cancelJump = cj;

            boxX = bx;
            boxY = by;
            boxR = br;
            jboxX = jbx;
            jboxY = jby;
            jboxR = jbr;

            jumpForce = jF;
            miniJumpForce = mjF;
            maxSpeed = mS;
            posMaxSpeed = pmS;
            gravityStrength = gS;

            accelerationNG = accNG;
            acceleration = acc;
            deceleration = dec;
            chargedAcceleration = cacc;

            redOrbMultiplier = rOM;
            yellowOrbMultiplier = yOM;
            pinkOrbMultiplier = pOM;
            blueOrbMultiplier = bOM;
            greenOrbMultiplier = gOM;
            blackOrbMultiplier = bbOM;

            redPadMultiplier = rPM;
            yellowPadMultiplier = yPM;
            pinkPadMultiplier = pPM;
            bluePadMultiplier = bPM;
            greenPadMultiplier = gPM;
            blackPadMultiplier = bbPM;
        }
    }
    private Dictionary<Gamemode, GamemodeConstants> gamemodeConstants;

    public struct RespawnVals
    {
        public Vector3 position;
        public int gravityDirection;
        public int speed;
        public bool mini;
        public int reverseDir;
    }
    public RespawnVals respawn;

    // CONSTANTS
    private readonly float[] SPEEDS = { 40f, 55f, 75f, 90f, 110f };
    private const float CROUCHTIMER = 0.01f, CHARGEDTIMER = 1.2f, CHARGEDSMOOTH = 5f / 0.7f, COYOTETIME = .085f, SDTIME = 3;
    private const float SHIP_ACCELERATE_CURVE_TIME = .08f;
    private float sdtimer;

    // SAVED VALUES FOR RESPAWN
    private int respawnSpeed;
    private float respawnOrientation, respawnGamemode;

    // Orbs Pads Portals Effectors
    private bool yellow_orb, blue_orb, red_orb, pink_orb, green_orb, black_orb, purple_orb, purple_dash, dash_orb, black_dash, inDash, cw_orb, ccw_orb, tele_orb, trigger_orb, rebound_orb, super_orb;
    private bool yellow_pad, blue_pad, red_pad, pink_pad, green_pad, black_pad, purple_pad, rebound_pad, pad;
    private Vector2 tele_orb_translate, teleBDelta, teleBChargedDelta, chargedTeleportVelocity, dashDirection;
    private bool gravportal_down, gravportal_up, gravportal_flip, teleportalA, teleportal_charged, dashDisable;
    private float chargeTeleportTimer, chargeTeleportTimerC, overrideDashSpeed, dashAngle, dashDisableTimer;

    // JUMP
    private bool jump, jump_air, jump_orb, jump_hold, cancel_jump, jump_released, jump_from_ground, falling;
    private float coyoteTimer, holdJumpTimer, releaseJumpTimer;

    // CROUCH
    private bool crouch = false, isCrouched = false, previousIsCrouched = false;
    private float crouchedTimer = 0;
    private RaycastHit2D headHit;

    // MOTION
    private float moveX, moveY;
    private float prevMoveX;
    private int speed = 1;
    private Vector2 ref_Velocity;
    private float velocityComponentX, velocityComponentY, prev_velocityComponentY;
    private Vector2 velocityVectorX, velocityVectorY;
    private bool addForce;
    private Vector2 additionalForce;
    private float posMaxSpeed, posMaxSpeedTimer;
    private List<Rigidbody2D> MovingObjectVelocities;
    private Vector2 movingObjectVelocity;
    private bool goingUp;
    private bool springDown;

    // GROUND
    private bool grounded = false, prev_grounded = false, touchingGround = false, checkGrounded = true, ceiling;

    // ORIENTATION    
    private Vector2 gravityOrientation, forwardOrientation, previousDirection;

    [Header("Gravity Orientation")]
    public int gravityDirection = 0;
    private int prevGravityDirection = 0;
    public bool normalBaseOrientation = true, facingForward = true;

    [Header("Cube Jump")]
    public bool fixedJumpHeight;
    public bool cubeHoldJump;

    // TRIGGERS TO RESET
    private List<MoveObject> movetriggers;
    public void AddMoveTriggers(MoveObject mo) { if (!movetriggers.Contains(mo) && mo.resetOnDeathPerCheckpoint) { movetriggers.Add(mo); } }
    public void ResetMoveTriggers() { foreach(MoveObject mo in movetriggers) { mo.ResetTrigger(); } }
    public void ClearMoveTriggers() { movetriggers.Clear(); }

    private List<RotateObject> rotatetriggers;
    public void AddRotateTriggers(RotateObject ro) { if (!rotatetriggers.Contains(ro) && ro.resetOnDeathPerCheckpoint) { rotatetriggers.Add(ro); } }
    public void ResetRotateTriggers() { foreach (RotateObject ro in rotatetriggers) { ro.ResetTrigger(); } }
    public void ClearRotateTriggers() { rotatetriggers.Clear(); }

    private List<ScaleObject> scaletriggers;
    public void AddScaleTriggers(ScaleObject so) { if (!scaletriggers.Contains(so) && so.resetOnDeathPerCheckpoint) { scaletriggers.Add(so); } }
    public void ResetScaleTriggers() { foreach (ScaleObject so in scaletriggers) { so.ResetTrigger(); } }
    public void ClearScaleTriggers() { scaletriggers.Clear(); }

    private void Awake()
    {
        // INPUT
        if (input == null)
        {
            input = new InputActions();
        }
        input.Player.Enable();

        gamemanager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        timeManager = GameObject.FindGameObjectWithTag("Master").GetComponent<TimeManager>();

        // ASSIGNMENT
        player_body = GetComponent<Rigidbody2D>();
        icons = iconParent.GetChild(0);

        icon = icons.GetComponent<IconController>().getIcon();
        eyesParent = icon.transform.Find("Icon_Body").Find("Icon_Eyes").gameObject;
        eyes = new GameObject[4];
        eyes[0] = eyesParent.transform.Find("Eyes_Normal").gameObject;
        eyes[1] = eyesParent.transform.Find("Eyes_Wide").gameObject;
        eyes[2] = eyesParent.transform.Find("Eyes_Squint").gameObject;
        eyes[3] = eyesParent.transform.Find("Eyes_Irked").gameObject;
        eyes[0].SetActive(true);
        eyes[1].SetActive(false);
        eyes[2].SetActive(false);
        eyes[3].SetActive(false);

        Cube_Anim = icons.GetComponent<Animator>();
        spider_trail.gameObject.AddComponent<SpiderTrail>();
        SpOrTr = spider_trail.GetComponent<SpiderTrail>();

        gamemodeConstants = new Dictionary<Gamemode, GamemodeConstants>();
        gamemodeConstants.Add(Gamemode.cube, new GamemodeConstants(
                                                        false, true, true, true,
                                                        0.85f, 0.85f, 0.06f, 0.2f, 0.2f, 0.385f,
                                                        21f, 16.5f, 110, 1000, 9.81f, .035f, .035f, .055f, .25f,
                                                        1.45f, 1.1f, .95f, .4f, 1f, 1.1f,
                                                        1.6f, 1.4f, .9f, .4f, 1.5f, 1.1f));
        /*gamemodeConstants.Add(Gamemode.auto_cube, new GamemodeConstants(
                                                        true, true, true, false,
                                                        0.2f, 0.2f, 0.385f, 0.1f, 0.1f, 0.435f,
                                                        19.21f, 15f, 31.5f, 1000, 9.6f, 0f, 0f, 0f, 0f,
                                                        1.4f, 1.1f, .94f, .4f, 1.1f, 1.1f,
                                                        1.7f, 1.55f, .9f, .4f, 1.6f, 1.1f));*/
        /*gamemodeConstants.Add(Gamemode.auto_cube, new GamemodeConstants(
                                                        true, true, true, false,
                                                        0.75f, 0.75f, 0.11f, 0.2f, 0.2f, 0.385f,
                                                        20.8f, 16.8f, 31.5f, 1000, 9.81f, 0f, 0f, 0f, 0f,
                                                        1.34f, 1.08f, .94f, .38f, 1.08f, 1f,
                                                        1.62f, 1.48f, .9f, .38f, 1.5f, 1f));*/
        gamemodeConstants.Add(Gamemode.auto_cube, new GamemodeConstants(
                                                        true, true, true, false,
                                                        0.2f, 0.2f, 0.385f, 0.1f, 0.1f, 0.435f,
                                                        20.5f, 16.8f, 31.5f, 1000, 9.6f, 0f, 0f, 0f, 0f,
                                                        1.32f, 1.04f, .89f, .35f, 1.05f, 1.05f,
                                                        1.62f, 1.5f, .85f, .35f, 1.55f, 1.05f));
        gamemodeConstants.Add(Gamemode.ship, new GamemodeConstants(
                                                        false, false, false, false,
                                                        0.75f, 0.75f, 0.11f, 0.2f, 0.2f, 0.385f,
                                                        10f, 7f, 12f, 12f, 0f, .3f, .2f, .1f, .25f,
                                                        1.65f, 1.3f, 1f, .7f, 1.3f, 2.4f,
                                                        1.55f, 1.2f, 1f, 1.2f, 1.2f, 2.4f));
        gamemodeConstants.Add(Gamemode.auto_ship, new GamemodeConstants(
                                                        true, false, false, false,
                                                        0.6f, 0.75f, 0.11f, 0.16f, 0.2f, 0.385f,
                                                        10f, 7f, 12f, 12f, 0f, 0f, 0f, 0f, 0f,
                                                        1.65f, 1.3f, 1f, .7f, 1.3f, 2.4f,
                                                        1.55f, 1.2f, 1f, 1.2f, 1.2f, 2.4f));
        gamemodeConstants.Add(Gamemode.ufo, new GamemodeConstants(
                                                        false, false, false, true,
                                                        0.60f, 0.75f, 0.11f, 0.05f, 0.2f, 0.385f,
                                                        12.5f, 10f, 17f, 17f, 5f, .05f, .05f, .05f, .25f,
                                                        1.75f, 1.3f, .78f, .4f, 1.3f, 1.2f,
                                                        1.9f, 1.4f, .9f, .4f, 1.3f, 2f));
        gamemodeConstants.Add(Gamemode.auto_ufo, new GamemodeConstants(
                                                        true, false, false, true,
                                                        0.60f, 0.75f, 0.11f, 0.05f, 0.2f, 0.385f,
                                                        12.5f, 10f, 17f, 17f, 5f, 0f, 0f, 0f, 0f,
                                                        1.6f, 1.3f, .78f, .4f, 1.3f, 1.2f,
                                                        1.9f, 1.4f, .9f, .4f, 1.3f, 2f));
        gamemodeConstants.Add(Gamemode.wave, new GamemodeConstants(
                                                        false, false, false, true,
                                                        0.2f, 0.2f, 0.11f, 0.2f, 0.2f, 0.11f,
                                                        15f, 13f, 100f, 100f, 0, 0, 0, 0, .25f,
                                                        1.4f, 1.2f, .9f, 0, 1.2f, 0,
                                                        1.6f, 1.3f, 1f, 0, 1.3f, 0));
        gamemodeConstants.Add(Gamemode.auto_wave, new GamemodeConstants(
                                                        true, false, false, true,
                                                        0.2f, 0.2f, 0.11f, 0.2f, 0.2f, 0.11f,
                                                        15f, 13f, 100f, 100f, 0, 0, 0, 0, 0f,
                                                        1.4f, 1.2f, .9f, 0, 1.2f, 0,
                                                        1.6f, 1.3f, 1f, 0, 1.3f, 0));
        gamemodeConstants.Add(Gamemode.ball, new GamemodeConstants(
                                                        false, true, true, true,
                                                        0.75f, 0.75f, 0.11f, 0.1f, 0.1f, 0.435f,
                                                        15f, 13f, 24f, 24f, 8f, .105f, .105f, .165f, .75f,
                                                        1.4f, 1.2f, 1f, .4f, 1.2f, 1.2f,
                                                        1.6f, 1.3f, .9f, .4f, 1.3f, 2f));
        gamemodeConstants.Add(Gamemode.auto_ball, new GamemodeConstants(
                                                        true, true, true, true,
                                                        0.75f, 0.75f, 0.11f, 0.1f, 0.1f, 0.435f,
                                                        15f, 13f, 24f, 24f, 8f, .105f, .105f, .165f, 0f,
                                                        1.4f, 1.2f, 1f, .4f, 1.2f, 1.2f,
                                                        1.6f, 1.3f, .9f, .4f, 1.3f, 2f));
        gamemodeConstants.Add(Gamemode.spider, new GamemodeConstants(
                                                        false, false, true, true,
                                                        0.6f, 0.75f, 0.11f, 0.05f, 0.2f, 0.385f,
                                                        17f, 14f, 27.2f, 27.2f, 8f, 0, 0, 0, .25f,
                                                        1.4f, 1.2f, 1f, .4f, 1.2f, 1.6f,
                                                        1.6f, 1.3f, .9f, .4f, 1.3f, 2f));
        gamemodeConstants.Add(Gamemode.auto_spider, new GamemodeConstants(
                                                        true, false, true, true,
                                                        0.6f, 0.75f, 0.11f, 0.05f, 0.2f, 0.385f,
                                                        17f, 14f, 27.2f, 27.2f, 8f, 0, 0, 0, 0f,
                                                        1.4f, 1.2f, 1f, .4f, 1.2f, 1.6f,
                                                        1.6f, 1.3f, .9f, .4f, 1.3f, 2f));
        gamemodeConstants.Add(Gamemode.copter, new GamemodeConstants(
                                                        false, false, false, true,
                                                        0.75f, 0.75f, 0.11f, 0.2f, 0.2f, 0.385f,
                                                        10f, 13f, 15f, 15f, 0, .45f, .2f, .02f, .25f,
                                                        1.85f, 1.5f, 1f, .7f, 1.5f, 2.4f,
                                                        2f, 1.7f, 1.2f, 1.2f, 1.7f, 2.4f));
        gamemodeConstants.Add(Gamemode.auto_copter, new GamemodeConstants(
                                                        true, false, false, true,
                                                        0.75f, 0.75f, 0.11f, 0.2f, 0.2f, 0.385f,
                                                        10f, 13f, 15f, 15f, 0, 0, 0, 0, 0f,
                                                        1.85f, 1.5f, 1f, .7f, 1.5f, 2.4f,
                                                        2f, 1.7f, 1.2f, 1.2f, 1.7f, 2.4f));

        gamemodeConstants.Add(Gamemode.spring, new GamemodeConstants(
                                                        false, false, true, true,
                                                        0.85f, 0.85f, 0.06f, 0.2f, 0.2f, 0.385f,
                                                        20f, 22f, 110, 1000, 6.5f, .035f, .035f, .055f, .25f,
                                                        1.45f, 1.1f, .95f, .4f, 1f, 1.1f,
                                                        1.6f, 1.4f, .9f, .4f, 1.5f, 1.1f));
        gamemodeConstants.Add(Gamemode.auto_spring, new GamemodeConstants(
                                                        true, true, true, true,
                                                        0.85f, 0.85f, 0.06f, 0.2f, 0.2f, 0.385f,
                                                        21f, 16.5f, 110, 1000, 9.81f, .035f, .035f, .055f, .25f,
                                                        1.45f, 1.1f, .95f, .4f, 1f, 1.1f,
                                                        1.6f, 1.4f, .9f, .4f, 1.5f, 1.1f));
        gamemodeConstants.Add(Gamemode.robot, new GamemodeConstants(
                                                        false, true, true, true,
                                                        0.85f, 0.85f, 0.06f, 0.2f, 0.2f, 0.385f,
                                                        21f, 16.5f, 110, 1000, 9.81f, .035f, .035f, .055f, .25f,
                                                        1.45f, 1.1f, .95f, .4f, 1f, 1.1f,
                                                        1.6f, 1.4f, .9f, .4f, 1.5f, 1.1f));
        gamemodeConstants.Add(Gamemode.auto_robot, new GamemodeConstants(
                                                        true, true, true, true,
                                                        0.85f, 0.85f, 0.06f, 0.2f, 0.2f, 0.385f,
                                                        21f, 16.5f, 110, 1000, 9.81f, .035f, .035f, .055f, .25f,
                                                        1.45f, 1.1f, .95f, .4f, 1f, 1.1f,
                                                        1.6f, 1.4f, .9f, .4f, 1.5f, 1.1f));
        /* GAMEMODE CONSTANTS HELP
         * auto         dash spin           orb buffer  cancel jump
         * box x        box y               box r       jumpBox x       jumpBox y   jumpBox r
         * jump force   mini jump force     maxSpeed    pos maxSpeed    gravity     acc air     accel   decel   chargeAcc
         * redOrb       yellowOrb           pinkOrb     blueOrb         greenOrb    blackOrb
         * redPad       yellowPad           pinkPad     bluePad         greenPad    blackPad
         */

        mainCamera = Camera.main;

        setGamemode((int)gamemode);
        resetBaseDirection();

        ref_Velocity = Vector3.zero;
        gravityOrientation = new Vector2(0, -1);
        forwardOrientation = new Vector2(1, 0);

        MovingObjectVelocities = new List<Rigidbody2D>();
        movingObjectVelocity = Vector2.zero;

        // TRIGGER LISTS
        movetriggers = new List<MoveObject>();
        rotatetriggers = new List<RotateObject>();
        scaletriggers = new List<ScaleObject>();
    }
    private void Start()
    {
        respawn.mini = mini;
        respawn.position = transform.position;
        respawn.speed = speed;
        respawn.gravityDirection = gravityDirection;
        respawn.reverseDir = reverseDir;

        //able = true;
    }

    public void setAble(bool a)
    {
        able = a;
    }

    public void setRestartMusic(bool r)
    {
        restartMusic = r;
    }

    public int GetGravityDirection()
    {
        return gravityDirection;
    }

    public void setGamemode(int i)
    {
        Gamemode prevGamemode = gamemode;
        gamemode = (Gamemode)i;
        switch(gamemode)
        {
            case Gamemode.cube:
            case Gamemode.auto_cube:
                icon.transform.localPosition = Vector3.zero;
                icon.transform.localScale = Vector3.one;
                main_trail.transform.localPosition = Vector3.zero;
                goingUp = false;
                break;
            case Gamemode.ship:
                icon.transform.localPosition = Vector3.zero;
                icon.transform.localScale = Vector3.one;
                main_trail.transform.localPosition = Vector3.zero;
                goingUp = false;
                break;
            case Gamemode.auto_ship:
                icon.transform.localPosition = new Vector3(.037f, .232f, 0);
                icon.transform.localScale = new Vector3(.7f, .7f, 0);
                main_trail.transform.localPosition = new Vector3(-.75f, -.22f, 0);
                flame_stream.Play();
                goingUp = false;

                float gravityBase = 0;
                float base0 = gravityDirection * -90, base1 = (4 - gravityDirection) * 90, base2 = (gravityDirection + 4) * -90, base3 = ((4 - gravityDirection) + 4) * 90;
                float currRotation = iconParent.rotation.eulerAngles.z;
                float minDiff = Mathf.Min(Mathf.Abs(currRotation - base0), Mathf.Abs(currRotation - base1), Mathf.Abs(currRotation - base2), Mathf.Abs(currRotation - base3));
                if (minDiff == Mathf.Abs(currRotation - base0))
                {
                    gravityBase = base0;
                }
                else if (minDiff == Mathf.Abs(currRotation - base1))
                {
                    gravityBase = base1;
                }
                else if (minDiff == Mathf.Abs(currRotation - base2))
                {
                    gravityBase = base2;
                }
                else if (minDiff == Mathf.Abs(currRotation - base3))
                {
                    gravityBase = base3;
                }

                Vector3 baseRotation = new Vector3(0, 0, gravityBase);

                float vY = Vector2.Dot(player_body.velocity, -gravityOrientation);
                float vX = Vector2.Dot(player_body.velocity, forwardOrientation);

                if (grounded || ceiling)
                {
                    iconParent.localEulerAngles = baseRotation;
                }
                else if (vY > 0)
                {
                    Vector3 newRotation = new Vector3(0, 0, (normalBaseOrientation ? 1 : -1) * (Mathf.Rad2Deg * Mathf.Atan(vY / vX)) + baseRotation.z);
                    iconParent.localEulerAngles = newRotation;
                }
                else if (vY < 0)
                {
                    Vector3 newRotation = new Vector3(0, 0, (normalBaseOrientation ? 1 : -1) * (360 + Mathf.Rad2Deg * Mathf.Atan(vY / vX)) + baseRotation.z);
                    iconParent.localEulerAngles = newRotation;
                }
                break;
            case Gamemode.ufo:
            case Gamemode.auto_ufo:
                icon.transform.localPosition = new Vector3(0, 0.147f, 0);
                icon.transform.localScale = new Vector3(.7f, .7f, 0);
                main_trail.transform.localPosition = Vector3.zero;
                goingUp = false;
                break;
            case Gamemode.ball:
            case Gamemode.auto_ball:
            case Gamemode.wave:
            case Gamemode.auto_wave:
                wave_trail.GetComponent<TrailRenderer>().emitting = true;
                main_trail.transform.localPosition = Vector3.zero;
                goingUp = false;
                break;
            case Gamemode.spider:
            case Gamemode.auto_spider:
                main_trail.transform.localPosition = Vector3.zero;
                Spider_Anim.ResetTrigger("curl");
                Spider_Anim.ResetTrigger("run");
                Spider_Anim.ResetTrigger("jump");
                Spider_Anim.SetTrigger("stop");
                goingUp = false;
                break;
            case Gamemode.copter:
            case Gamemode.auto_copter:
                icon.transform.localPosition = Vector3.zero;
                icon.transform.localScale = Vector3.one;
                main_trail.transform.localPosition = Vector3.zero;
                main_trail.emitting = false;
                break;
        }

        if(gamemode != Gamemode.cube)
        {
            crouch_collider.enabled = false;
            crouch = false;
        }

        icon.SetActive(!(gamemode == Gamemode.ball || gamemode == Gamemode.auto_ball
                    || gamemode == Gamemode.spider || gamemode == Gamemode.auto_spider
                    || gamemode == Gamemode.wave || gamemode == Gamemode.auto_wave));

        jetpack.SetActive(gamemode == Gamemode.ship);
        ship.SetActive(gamemode == Gamemode.auto_ship);

        jepack_trails.SetActive(gamemode == Gamemode.ship);        
        main_trail.gameObject.SetActive(gamemode != Gamemode.ship);

        flame_stream.gameObject.SetActive(gamemode == Gamemode.auto_ship);

        ufo.SetActive(gamemode == Gamemode.ufo || gamemode == Gamemode.auto_ufo);
        wave.SetActive(gamemode == Gamemode.wave || gamemode == Gamemode.auto_wave);
        wave_trail.SetActive(gamemode == Gamemode.wave || (gamemode == Gamemode.auto_wave && prevGamemode == Gamemode.wave));
        auto_wave_trail.SetActive(gamemode == Gamemode.auto_wave);
        if (gamemode == Gamemode.auto_wave && prevGamemode == Gamemode.wave) { wave_trail.GetComponent<TrailRenderer>().emitting = false; }

        ball.SetActive(gamemode == Gamemode.ball || gamemode == Gamemode.auto_ball);
        spider.SetActive(gamemode == Gamemode.spider || gamemode == Gamemode.auto_spider);
        copter.SetActive(gamemode == Gamemode.copter || gamemode == Gamemode.auto_copter);

        windUp.gameObject.SetActive(gamemode == Gamemode.copter || gamemode == Gamemode.auto_copter);
        windDown.gameObject.SetActive(gamemode == Gamemode.copter || gamemode == Gamemode.auto_copter);

        posMaxSpeed = gamemodeConstants[gamemode].posMaxSpeed;
        firstFramesBuffer = FRAMEBUFFERTIME;

        iconParent.localScale = new Vector3(Mathf.Abs(iconParent.localScale.x), Mathf.Abs(iconParent.localScale.y), 1);
    }

    void changeGravityDirection()
    {
        gravityOrientation = new Vector2((gravityDirection - 2) % 2, (gravityDirection - 1) % 2);
        forwardOrientation = normalBaseOrientation ? new Vector2((1 - gravityDirection) % 2, (gravityDirection - 2) % 2) : new Vector2((gravityDirection - 1) % 2, (2 - gravityDirection) % 2);

        float crouchWidth = .4f, crouchOffset = .285f;
        crouch_collider.offset = new Vector2(((gravityDirection - 2) % 2) * crouchOffset, ((gravityDirection - 1) % 2) * crouchOffset);
        crouch_collider.size = new Vector2((gravityDirection % 2 == 0) ? 0.98f : crouchWidth, (gravityDirection % 2 == 1) ? 0.98f : crouchWidth);
    }

    void resetBaseDirection()
    {
        float cameraRotation = mainCamera.transform.eulerAngles.z % 360;
        if (cameraRotation <= 22.5f || cameraRotation > 337.5f) { normalBaseOrientation = gravityDirection != 2; }
        else if (cameraRotation <= 67.5f && cameraRotation > 22.5f) { normalBaseOrientation = gravityDirection != 2 && gravityDirection != 1; }

        else if (cameraRotation <= 112.5 && cameraRotation > 67.5f) { normalBaseOrientation = gravityDirection != 1; }
        else if (cameraRotation <= 157.5f && cameraRotation > 112.5) { normalBaseOrientation = gravityDirection != 1 && gravityDirection != 0; }

        else if (cameraRotation <= 202.5 && cameraRotation > 157.5f) { normalBaseOrientation = gravityDirection != 0; }
        else if (cameraRotation <= 247.5f && cameraRotation > 202.5) { normalBaseOrientation = gravityDirection != 0 && gravityDirection != 3; }

        else if (cameraRotation <= 292.5f && cameraRotation > 247.5f) { normalBaseOrientation = gravityDirection != 3; }
        else if (cameraRotation <= 337.5f || cameraRotation > 292.5f) { normalBaseOrientation = gravityDirection != 3 && gravityDirection != 2; }

        //normalBaseOrientation = gravityDirection != topGravityDirection;
    }

    int GetOppositeGravity()
    {
        int oppositeGrav = 2;
        float cameraRotation = Camera.main.transform.eulerAngles.z % 360;
        if (cameraRotation <= 22.5f || cameraRotation > 337.5f)
        {
            oppositeGrav = 2;
        }
        else if (cameraRotation <= 67.5f && cameraRotation > 22.5f)
        {
            if(gravityDirection == 0 || gravityDirection == 2) { oppositeGrav = 2; }
            else if (gravityDirection == 3 || gravityDirection == 1) { oppositeGrav = 1; }
        }

        else if (cameraRotation <= 112.5 && cameraRotation > 67.5f)
        {
            oppositeGrav = 1;
        }
        else if (cameraRotation <= 157.5f && cameraRotation > 112.5)
        {
            if (gravityDirection == 3 || gravityDirection == 1) { oppositeGrav = 1; }
            else if (gravityDirection == 2 || gravityDirection == 0) { oppositeGrav = 0; }
        }

        else if (cameraRotation <= 202.5 && cameraRotation > 157.5f)
        {
            oppositeGrav = 0;
        }
        else if (cameraRotation <= 247.5f && cameraRotation > 202.5)
        {
            if (gravityDirection == 2 || gravityDirection == 0) { oppositeGrav = 0; }
            else if (gravityDirection == 1 || gravityDirection == 3) { oppositeGrav = 3; }
        }

        else if (cameraRotation <= 292.5f && cameraRotation > 247.5f)
        {
            oppositeGrav = 3;
        }
        else if (cameraRotation <= 337.5f || cameraRotation > 292.5f)
        {
            if (gravityDirection == 1 || gravityDirection == 3) { oppositeGrav = 3; }
            else if (gravityDirection == 0 || gravityDirection == 2) { oppositeGrav = 2; }
        }

        return oppositeGrav;
    }

    bool sameSign(float num1, float num2)
    {
        return num1 > 0 && num2 > 0 || num1 < 0 && num2 < 0;
    }

    Vector2 PositiveVector2(Vector2 v)
    {
        return new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));
    }

    void ChangeSize()
    {
        bool currMini = transform.localScale.x < .5;

        if (mini && !currMini)
        {
            grounded_particles.startLifetime = .15f;
            ground_impact_particles.startLifetime = .15f;
            grounded_particles.transform.localScale = new Vector2(.47f, .47f);
            ground_impact_particles.transform.localScale = new Vector2(.47f, .47f);

            transform.localScale = Vector2.one * .44f;
            iconParent.localScale = Vector2.one * 1.1f;
            //player_collider.edgeRadius = 0.05f;
            if (grounded) player_body.position += gravityOrientation * .3f;
        }
        else if(!mini && currMini)
        {
            grounded_particles.startLifetime = .3f;
            ground_impact_particles.startLifetime = .3f;
            grounded_particles.transform.localScale = new Vector2(1, 1f);
            ground_impact_particles.transform.localScale = new Vector2(1f, 1f);

            transform.localScale = Vector2.one;
            iconParent.localScale = Vector2.one * 1.05f;
            //player_collider.edgeRadius = 0.11f;
            if (grounded) player_body.position -= gravityOrientation * .3f;
        }
    }

    private void Update()
    {
        if (gamemanager.isPaused()) { return; }
        if (gameObject.scene.IsValid() && input.Player.Jump.ReadValue<float>() == 1)
        {
            jump_hold = true;
        }

        if (!able) return;

        // MOTION
        moveX = input.Player.MovementHorizontal.ReadValue<float>() * SPEEDS[speed];
        moveY = input.Player.MovementVertical.ReadValue<float>() * SPEEDS[speed];

        // GROUNDED
        if(!gamemodeConstants[gamemode].auto && (!sameSign(moveX, prevMoveX) || (prevMoveX != 0 && moveX == 0)))
        {
            resetBaseDirection();
            changeGravityDirection();
        }
        else if(gamemodeConstants[gamemode].auto && prevGravityDirection != gravityDirection)
        {
            resetBaseDirection();
            changeGravityDirection();
        }

        if (prevGravityDirection != gravityDirection)
        {
            changeGravityDirection();
        }

        float width = gamemodeConstants[gamemode].boxX + 2 * gamemodeConstants[gamemode].boxR;
        //if (crouch_collider.enabled) { width = .985f; }
        touchingGround = (Physics2D.IsTouchingLayers(player_collider, groundLayer) || Physics2D.IsTouchingLayers(crouch_collider, groundLayer));
        grounded = checkGrounded && touchingGround && Physics2D.BoxCast(player_body.position, new Vector2(mini ? width * .44f : width, .1f), gravityDirection % 2 == 0 ? 0 : 90, gravityOrientation, (mini ? .23f : .51f), groundLayer);
        ceiling = checkGrounded && touchingGround && Physics2D.BoxCast(player_body.position, new Vector2(mini ? width * .44f : width, .1f), gravityDirection % 2 == 0 ? 0 : 90, -gravityOrientation, (mini ? .23f : .51f), groundLayer);
        //&& (gamemode != Gamemode.copter && gamemode != Gamemode.auto_copter);
        
        if (!prev_grounded && grounded && gamemode == Gamemode.cube)
        {
            if (velocityComponentY < 1f)
            {
                if (prev_velocityComponentY <= -40)
                {
                    Cube_Anim.ResetTrigger("Crouch");
                    Cube_Anim.ResetTrigger("Default");
                    Cube_Anim.ResetTrigger("Stretch");
                    Cube_Anim.ResetTrigger("Squash");
                    Cube_Anim.ResetTrigger("DeepSquash");
                    Cube_Anim.Play("HeightDeepSquash", -1, 0f);
                }
                else if (prev_velocityComponentY <= -12)
                {
                    Cube_Anim.ResetTrigger("Crouch");
                    Cube_Anim.ResetTrigger("Default");
                    Cube_Anim.ResetTrigger("Stretch");
                    Cube_Anim.ResetTrigger("DeepSquash");
                    Cube_Anim.ResetTrigger("Squash");
                    Cube_Anim.Play("HeightSquash", -1, 0f);
                }
            }
        }

        if(grounded)
        {
            main_trail.emitting = false;
            launched = false;

            //Copter_Anim.speed = 1.34f;
            Copter_Anim.speed = Mathf.MoveTowards(Copter_Anim.speed, 1.34f, 10 * Time.deltaTime);
            if (windUp.isPlaying || windDown.isPlaying) { windUp.Stop(); windDown.Stop(); }
        }
        else
        {
            Copter_Anim.speed = Mathf.MoveTowards(Copter_Anim.speed, Mathf.Abs(gamemodeConstants[Gamemode.copter].maxSpeed / 2.5f), 10 * Time.deltaTime);
            if(goingUp && (!windUp.isPlaying || windDown.isPlaying)) { windUp.Play(); windDown.Stop(); }
            else if (!goingUp && (!windDown.isPlaying ||  windUp.isPlaying)) { windUp.Stop(); windDown.Play(); }
        }
        if (gamemode == Gamemode.auto_ship || gamemode == Gamemode.auto_ufo)
        {
            main_trail.emitting = true;
        }

        float yVel = Vector2.Dot(player_body.velocity, -gravityOrientation);
        yVel -= Vector2.Dot(movingObjectVelocity, -gravityOrientation);

        //bool closeToGrounded = checkGrounded && Physics2D.BoxCast(player_body.position, new Vector2(mini ? width * .44f : width, .1f), gravityDirection % 2 == 0 ? 0 : 90, gravityOrientation, (mini ? .215f : .487f), groundLayer);
        if (gamemode == Gamemode.ball || gamemode == Gamemode.auto_ball)
        {
            if (grounded) { yVel = -2; }
            else { yVel = 2; }
            //yVel = 2;
        }

        if (!(gamemode == Gamemode.cube && crouchCollider))
        {
            player_collider.offset = Vector2.zero;
            if (yVel <= 0.001f && !inDash)
            {
                player_collider.size = PositiveVector2(forwardOrientation) * gamemodeConstants[gamemode].boxX + PositiveVector2(gravityOrientation) * gamemodeConstants[gamemode].boxY;
                player_collider.edgeRadius = gamemodeConstants[gamemode].boxR * (!mini ? 1 : .44f);

                if (grounded && moveX == 0 && gamemode == Gamemode.cube)
                {
                    player_collider.size = PositiveVector2(forwardOrientation) * .97f + PositiveVector2(gravityOrientation) * .97f;
                    player_collider.edgeRadius = 0;
                }
            }
            else
            {
                player_collider.size = PositiveVector2(forwardOrientation) * gamemodeConstants[gamemode].jboxX + PositiveVector2(gravityOrientation) * gamemodeConstants[gamemode].jboxY;
                player_collider.edgeRadius = gamemodeConstants[gamemode].jboxR * (!mini ? 1 : .44f);
            }
        }

        // DEAD
        dead = Physics2D.IsTouchingLayers(player_collider, deathLayer) || Physics2D.IsTouchingLayers(crouch_collider, deathLayer);
        if (gamemodeConstants[gamemode].auto && reverseDir != 0 && !dashDisable && firstFramesBuffer <= 0)
        {
            dead = dead || (Mathf.Abs(Vector2.Dot(player_body.velocity, forwardOrientation)) <= .2f && touchingGround);            
        }

        if (Input.GetKeyDown(KeyCode.Delete))
        {
            sdtimer = 0;
        }
        if (Input.GetKey(KeyCode.Delete) && input.Player.MovementHorizontal.ReadValue<float>() == 0 && input.Player.MovementVertical.ReadValue<float>() == 0)
        {
            sdtimer += Time.unscaledDeltaTime;
            sdshape.settings.endAngle = Mathf.Clamp(SDTIME - sdtimer, .001f, SDTIME) / (SDTIME / 360);
            if (sdtimer > .5f) { sdshape.settings.fillColor = new Color(1, 0, 0, Mathf.MoveTowards(sdshape.settings.fillColor.a, 1, Time.unscaledDeltaTime)); }
            if (sdtimer >= SDTIME) { dead = true; }
        }
        else
        {
            sdtimer = 0;
            sdshape.settings.fillColor = new Color(1, 0, 0, Mathf.MoveTowards(sdshape.settings.fillColor.a, 0, 3*Time.unscaledDeltaTime));
        }

        // PARTICLES
        if (grounded && (Mathf.Abs(Vector2.Dot(player_body.velocity, forwardOrientation) - Vector2.Dot(movingObjectVelocity, forwardOrientation)) > .1f))
        {
            if(!grounded_particles.isPlaying)
                grounded_particles.Play();
        }
        else
        {
            grounded_particles.Stop();
        }

        if ((prev_grounded && !grounded) || (!prev_grounded && grounded && prev_velocityComponentY < 10f))
        {
            ground_impact_particles.Play();
        }

        // JUMP
        if (gameObject.scene.IsValid() && (input.Player.Jump.triggered || Input.GetButtonDown("Jump") || Input.GetKeyDown("space") || Input.GetKeyDown(KeyCode.I) || Input.GetMouseButtonDown(0)))
        {
            jump = true;
            jump_air = !grounded;
            jump_orb = yellow_orb || pink_orb || red_orb || purple_orb || blue_orb || green_orb || ccw_orb || cw_orb || blue_orb || dash_orb || black_dash || rebound_orb || purple_dash || trigger_orb || tele_orb || super_orb;
            jump_released = false;

            if(gamemode == Gamemode.ship)
            {
                flame_burst[0].Play();
                flame_burst[1].Play();
            }
            else if(gamemode == Gamemode.auto_ship)
            {
                flame_spray.Play();
            }
            else if(gamemode == Gamemode.wave)
            {
                launched = false;
            }

            jumpCount++;
        }

        // RELEASE JUMP
        if (input.Player.Jump.ReadValue<float>() == 0 || Input.GetButtonUp("Jump") || Input.GetKeyUp("space") || Input.GetKeyUp(KeyCode.I) || Input.GetMouseButtonUp(0))
        {
            jump = false;
            jump_air = false;
            jump_hold = false;
            jump_orb = false;
            jump_released = true;
            cancel_jump = false;

            inDash = false;
            dash_particles.Stop();
            if (dash_effect.activeSelf) { StartCoroutine(DashFlameDissipate()); }
            //dash_effect.SetActive(false);

            if (flame_burst[0].isPlaying || flame_burst[1].isPlaying || flame_spray.isPlaying)
            {
                flame_burst[0].Stop();
                flame_burst[1].Stop();
                flame_spray.Stop();
            }
        }

        // HEAD HIT
        float hitDist = mini ? 0 : .65f;
        headHit = Physics2D.Raycast((new Vector2(player_body.position.x, player_body.position.y)) + (.2f*gravityOrientation), -gravityOrientation, hitDist, groundLayer);

        // CROUCH
        if (gameObject.scene.IsValid() && (input.Player.Crouch.ReadValue<float>() >= .7f || input.Player.MovementVertical.ReadValue<float>() <= -.7f || Input.GetAxisRaw("Vertical") <= -.7f || Input.GetKey(KeyCode.LeftShift) || Input.GetMouseButton(1) || headHit.distance > 0))
        {
            crouch = true;
            if (gamemode == Gamemode.wave)
            {
                launched = false;
            }
        }
        else//if(input.Player.Crouch.ReadValue<float>() == 0)
        {
            crouch = false;
            isCrouched = false;
        }
        if(!grounded) { isCrouched = false; }

        if (gamemode == Gamemode.wave)
        {
            if (Input.GetMouseButton(0) || input.Player.Jump.ReadValue<float>() > 0f) moveY = Mathf.Abs(SPEEDS[speed]);
            else if (Input.GetMouseButton(1) || input.Player.Crouch.ReadValue<float>() > 0f) moveY = -Mathf.Abs(SPEEDS[speed]);
        }

        if(gamemode == Gamemode.auto_ship)
        {
            gamemodeConstants[Gamemode.auto_ship].posMaxSpeed = mini ? 15f : 12f;
            if (!flame_stream.isPlaying) { flame_stream.Play(); }
        }

        // TIMERS
        coyoteTimer += Time.deltaTime;
        //if (prev_grounded) { coyoteTimer = jump_from_ground ? COYOTETIME * 2 : 0; }
        if (grounded) { coyoteTimer = 0; }
        if(chargeTeleportTimerC > 0) { chargeTeleportTimerC -= Time.deltaTime; }

        if (jump_hold) { releaseJumpTimer = 0; holdJumpTimer += Time.deltaTime; }
        else { holdJumpTimer = 0; releaseJumpTimer += Time.deltaTime; }


        // PREVIOUS CHECKS
        prevGravityDirection = gravityDirection;
        prev_grounded = grounded;
        prev_velocityComponentY = velocityComponentY;
        prevMoveX = moveX;


        if (dead)
        {
            Die();
        }
    }

    private void LateUpdate()
    {
        if(able)
        {
            RotateUpdate();
        }
    }

    private void RotateUpdate()
    {
        float gravityBase = 0;
        float base0 = gravityDirection * -90, base1 = (4 - gravityDirection) * 90, base2 = (gravityDirection + 4) * -90, base3 = ((4 - gravityDirection) + 4) * 90;
        float currRotation = iconParent.rotation.eulerAngles.z;
        float minDiff = Mathf.Min(Mathf.Abs(currRotation - base0), Mathf.Abs(currRotation - base1), Mathf.Abs(currRotation - base2), Mathf.Abs(currRotation - base3));
        if (minDiff == Mathf.Abs(currRotation - base0))
        {
            gravityBase = base0;
        }
        else if (minDiff == Mathf.Abs(currRotation - base1))
        {
            gravityBase = base1;
        }
        else if (minDiff == Mathf.Abs(currRotation - base2))
        {
            gravityBase = base2;
        }
        else if (minDiff == Mathf.Abs(currRotation - base3))
        {
            gravityBase = base3;
        }

        Vector3 baseRotation = new Vector3(0, 0, gravityBase);

        if (inDash)
        {
            if (gamemodeConstants[gamemode].dashSpin)
            {
                float rev = Vector2.Dot(velocityVectorX, forwardOrientation) >= 0 ? -1 : 1;
                Vector3 newRotation = new Vector3(0, 0, iconParent.localEulerAngles.z + (360 * rev));
                iconParent.localEulerAngles = Vector3.MoveTowards(iconParent.localEulerAngles, newRotation, 700 * Time.deltaTime);
            }
            else
            {
                iconParent.right = dashDirection;
                iconParent.localScale = new Vector3(Mathf.Abs(iconParent.localScale.x), iconParent.localScale.y, iconParent.localScale.z);
                //iconParent.rotation = Quaternion.Euler(0, 0, dashAngle);
            }

            spiderAnimType = 2;
            spiderAnimSpeed = 2;
        }
        else
        {
            switch (gamemode)
            {
                case Gamemode.cube:
                    if (launched)
                    {
                        float rev = normalBaseOrientation ? 1 : -1;
                        float amt = -input.Player.MovementHorizontal.ReadValue<float>();

                        Vector3 newRotation = new Vector3(0, 0, iconParent.localEulerAngles.z + (360 * amt * rev));
                        if (amt == 0 || touchingGround)
                        {
                            if (iconParent.localEulerAngles.z >= gravityBase && iconParent.localEulerAngles.z <= gravityBase + 180)
                            {
                                newRotation = new Vector3(0, 0, gravityBase);
                            }
                            else if (iconParent.localEulerAngles.z >= gravityBase + 180 && iconParent.localEulerAngles.z <= gravityBase + 360)
                            {
                                newRotation = new Vector3(0, 0, gravityBase + 360);
                            }
                            else if (iconParent.localEulerAngles.z <= gravityBase && iconParent.localEulerAngles.z >= gravityBase - 180)
                            {
                                newRotation = new Vector3(0, 0, gravityBase);
                            }
                            else if (iconParent.localEulerAngles.z <= gravityBase - 180 && iconParent.localEulerAngles.z >= gravityBase - 360)
                            {
                                newRotation = new Vector3(0, 0, gravityBase - 360);
                            }

                            iconParent.localEulerAngles = Vector3.MoveTowards(iconParent.localEulerAngles, newRotation, 300 * Time.deltaTime);
                            break;
                        }

                        iconParent.localEulerAngles = Vector3.MoveTowards(iconParent.localEulerAngles, newRotation, 500 * Time.deltaTime);
                    }
                    else
                    {
                        iconParent.localEulerAngles = Vector3.MoveTowards(iconParent.localEulerAngles, baseRotation, (grounded ? 1200 : 500) * Time.deltaTime);
                    }
                    break;

                case Gamemode.auto_cube:
                    if (grounded)
                    {
                        float rotation = iconParent.localEulerAngles.z;
                        float difference = rotation % 90;
                        if (difference >= 40)
                        {
                            iconParent.localEulerAngles = new Vector3(0, 0, Mathf.MoveTowardsAngle(rotation, rotation + (90f - difference), 500 * Time.deltaTime));
                        }
                        else
                        {
                            iconParent.localEulerAngles = new Vector3(0, 0, Mathf.MoveTowardsAngle(rotation, rotation - difference, 500 * Time.deltaTime));
                        }
                    }
                    else
                    {
                        float rev = normalBaseOrientation ? -1 : 1;
                        Vector3 newRotation = new Vector3(0, 0, iconParent.localEulerAngles.z + (360 * rev * reverseDir));
                        iconParent.localEulerAngles = Vector3.MoveTowards(iconParent.localEulerAngles, newRotation, 450 * Time.deltaTime);
                    }                                        
                    break;

                case Gamemode.ship:
                    iconParent.localEulerAngles = Vector3.MoveTowards(iconParent.localEulerAngles, baseRotation, (grounded ? 700 : 400) * Time.deltaTime);
                    break;

                case Gamemode.auto_ship:
                    float vY = Vector2.Dot(player_body.velocity, -gravityOrientation);
                    float vX = Vector2.Dot(player_body.velocity, forwardOrientation);
                    float maxDiff = vX != 0 ? (Mathf.Rad2Deg * Mathf.Atan(gamemodeConstants[gamemode].posMaxSpeed / vX)) : 90;
                    //Debug.Log(Vector3.Angle(baseRotation, iconParent.localEulerAngles));
                    //Debug.Log(baseRotation + "   " + iconParent.localEulerAngles + "   " + Mathf.DeltaAngle(baseRotation.z, iconParent.localEulerAngles.z));
                    
                    if (grounded || ceiling)
                    {
                        bool overAngle = Mathf.Abs(Mathf.DeltaAngle(baseRotation.z, iconParent.localEulerAngles.z)) > maxDiff;//!AngleWithinRange(iconParent.localEulerAngles.z, baseRotation.z, maxDiff);
                        bool midAngle = Mathf.Abs(Mathf.DeltaAngle(baseRotation.z, iconParent.localEulerAngles.z)) <= 60;
                        float rotateSpeed = !overAngle ? 400 : 64000;
                        if(midAngle && overAngle) { rotateSpeed = 1200f; }
                        iconParent.localEulerAngles = new Vector3(0, 0, Mathf.MoveTowardsAngle(iconParent.localEulerAngles.z, baseRotation.z, rotateSpeed * Time.deltaTime));
                    }
                    else if (jump_hold)
                    {
                        //bool overAngle = iconParent.localEulerAngles.z > baseRotation.z + maxDiff || iconParent.localEulerAngles.z < baseRotation.z - maxDiff;    
                        bool overAngle = Mathf.Abs(Mathf.DeltaAngle(baseRotation.z, iconParent.localEulerAngles.z)) > maxDiff;
                        Vector3 newRotation = new Vector3(0, 0, (normalBaseOrientation ? 1 : -1) * (Mathf.Rad2Deg * Mathf.Atan(vY / vX)) + baseRotation.z);
                        float rotateSpeed = !overAngle ? 185/*182*/ : 64000;
                        iconParent.localEulerAngles = new Vector3(0, 0, Mathf.MoveTowardsAngle(iconParent.localEulerAngles.z, newRotation.z, rotateSpeed * Time.deltaTime));
                    }
                    else if (!jump_hold)
                    {
                        //bool overAngle = iconParent.localEulerAngles.z > baseRotation.z + maxDiff || iconParent.localEulerAngles.z < baseRotation.z - maxDiff;
                        bool overAngle = Mathf.Abs(Mathf.DeltaAngle(baseRotation.z, iconParent.localEulerAngles.z)) > maxDiff;
                        Vector3 newRotation = new Vector3(0, 0, (normalBaseOrientation ? 1 : -1) * (360 + Mathf.Rad2Deg * Mathf.Atan(vY / vX)) + baseRotation.z);
                        float rotateSpeed = !overAngle ? 140 : 64000;
                        //iconParent.localEulerAngles = newRotation;
                        //iconParent.localEulerAngles = Vector3.MoveTowards(iconParent.localEulerAngles, newRotation, 300 * Time.deltaTime);
                        iconParent.localEulerAngles = new Vector3(0, 0, Mathf.MoveTowardsAngle(iconParent.localEulerAngles.z, newRotation.z, rotateSpeed * Time.deltaTime));
                    }

                    int rev_ = reverseDir * (normalBaseOrientation ? 1 : -1);
                    if (rev_ == 1) { iconParent.localScale = new Vector2(Mathf.Abs(iconParent.localScale.x), iconParent.localScale.y); }
                    else if (rev_ == -1) { iconParent.localScale = new Vector2(-Mathf.Abs(iconParent.localScale.x), iconParent.localScale.y); }
                    break;

                case Gamemode.ufo:                    
                    float rev_ufo = normalBaseOrientation ? 1 : -1;
                    float y_ufo = Vector2.Dot(player_body.velocity, -gravityOrientation) > 0.01f ? 1 : 0;
                    float amt_ufo = input.Player.MovementHorizontal.ReadValue<float>();
                    float newRotation_ufo = baseRotation.z - (y_ufo * amt_ufo * rev_ufo * SPEEDS[speed] / 3.8f);
                    bool overAngle_ufo = iconParent.localEulerAngles.z > baseRotation.z + SPEEDS[speed] / 3.5f || iconParent.localEulerAngles.z < baseRotation.z - SPEEDS[speed] / 3.5f;

                    iconParent.localEulerAngles = new Vector3(0, 0, Mathf.MoveTowardsAngle(iconParent.localEulerAngles.z, newRotation_ufo, (!overAngle_ufo ? 100 : 4800) * Time.deltaTime));
                    break;

                case Gamemode.auto_ufo:
                    rev_ufo = normalBaseOrientation ? 1 : -1;
                    y_ufo = Vector2.Dot(player_body.velocity, -gravityOrientation) > 0.01f ? 1 : 0;
                    newRotation_ufo = baseRotation.z - (y_ufo * rev_ufo * reverseDir * SPEEDS[speed] / 3.8f);
                    overAngle_ufo = iconParent.localEulerAngles.z > baseRotation.z + SPEEDS[speed] / 3.5f || iconParent.localEulerAngles.z < baseRotation.z - SPEEDS[speed] / 3.5f;

                    iconParent.localEulerAngles = new Vector3(0, 0, Mathf.MoveTowardsAngle(iconParent.localEulerAngles.z, newRotation_ufo, (!overAngle_ufo ? 100 : 4800) * Time.deltaTime));
                    break;

                case Gamemode.ball:
                case Gamemode.auto_ball:
                    if (!launched)
                    {
                        float rev = normalBaseOrientation ? -1 : 1;
                        vX = Vector2.Dot(player_body.velocity, forwardOrientation);
                        Vector3 newRotation = new Vector3(0, 0, iconParent.localEulerAngles.z + rev * vX);
                        iconParent.localEulerAngles = Vector3.MoveTowards(iconParent.localEulerAngles, newRotation, SPEEDS[speed] * 18 * Time.deltaTime);
                    }
                    else
                    {
                        float rev = normalBaseOrientation ? 1 : -1;
                        vX = Vector2.Dot(player_body.velocity, forwardOrientation);
                        Vector3 newRotation = new Vector3(0, 0, iconParent.localEulerAngles.z + rev * vX);
                        iconParent.localEulerAngles = Vector3.MoveTowards(iconParent.localEulerAngles, newRotation, SPEEDS[speed] * 12 * Time.deltaTime);
                    }
                    break;

                case Gamemode.wave:
                case Gamemode.auto_wave:
                    if (player_body.velocity != Vector2.zero)
                    {
                        //Vector3 newAngle = new Vector3(0, 0, (Mathf.Rad2Deg * Mathf.Atan(player_body.velocity.y / player_body.velocity.x)));
                        //iconParent.localEulerAngles = newAngle;
                        //iconParent.right = player_body.velocity.normalized;

                        //iconParent.right = Vector3.MoveTowards(iconParent.right, player_body.velocity.normalized, 50 * Time.deltaTime);

                        iconParent.localEulerAngles = new Vector3(0, 0, Mathf.MoveTowardsAngle(iconParent.localEulerAngles.z, Vector2.SignedAngle(new Vector2(1, 0), player_body.velocity.normalized), 1000 * Time.deltaTime));
                    }
                    break;

                case Gamemode.spider:
                    if (moveX != 0)
                    {
                        float rev = Mathf.Sign(moveX) * (normalBaseOrientation ? 1 : -1);
                        iconParent.localScale = new Vector3(rev * Mathf.Abs(iconParent.localScale.x), Mathf.Abs(iconParent.localScale.y), 1);
                    }

                    if (grounded)
                    {
                        iconParent.localEulerAngles = baseRotation;
                    }
                    else
                    {
                        iconParent.localEulerAngles = new Vector3(0, 0, Mathf.MoveTowardsAngle(iconParent.localEulerAngles.z, baseRotation.z, 600 * Time.deltaTime));
                    }
                    break;

                case Gamemode.auto_spider:
                    vX = Vector2.Dot(player_body.velocity, forwardOrientation);
                    if (vX != 0)
                    {
                        float rev = Mathf.Sign(vX) * (normalBaseOrientation ? 1 : -1);
                        iconParent.localScale = new Vector3(rev * Mathf.Abs(iconParent.localScale.x), Mathf.Abs(iconParent.localScale.y), 1);
                    }

                    if (grounded)
                    {
                        iconParent.localEulerAngles = baseRotation;
                    }
                    else
                    {
                        iconParent.localEulerAngles = new Vector3(0, 0, Mathf.MoveTowardsAngle(iconParent.localEulerAngles.z, baseRotation.z, 600 * Time.deltaTime));
                    }
                    break;

                case Gamemode.copter:
                case Gamemode.auto_copter:
                    vX = Vector2.Dot(player_body.velocity, forwardOrientation);
                    if (touchingGround)
                    {
                        iconParent.localEulerAngles = new Vector3(0, 0, Mathf.MoveTowardsAngle(iconParent.localEulerAngles.z, baseRotation.z, 500 * Time.deltaTime));
                    }
                    else
                    {
                        bool overAngle_copter = iconParent.localEulerAngles.z > baseRotation.z + (SPEEDS[speed] * Time.fixedDeltaTime * 10f)
                                            || iconParent.localEulerAngles.z < baseRotation.z - (SPEEDS[speed] * Time.fixedDeltaTime * 10f);
                        float rev = (normalBaseOrientation ? 1 : -1);
                        iconParent.localEulerAngles = new Vector3(0, 0, Mathf.MoveTowardsAngle(iconParent.localEulerAngles.z, baseRotation.z - rev * vX, (!overAngle_copter ? 300 : 1200) * Time.deltaTime));
                    }
                    break;
            }
        }

        if (iconParent.rotation.eulerAngles.z >= 360) { iconParent.rotation = Quaternion.Euler(new Vector3(0, 0, iconParent.rotation.eulerAngles.z - 360)); }
        else if (iconParent.rotation.eulerAngles.z <= -360) { iconParent.rotation = Quaternion.Euler(new Vector3(0, 0, iconParent.rotation.eulerAngles.z + 360)); }

        int gravDir = gravityDirection % 2 == 0 ? gravityDirection : (gravityDirection + 2) % 4;
        grounded_particles_parent.localRotation = Quaternion.Euler(new Vector3(0, 0, -iconParent.localEulerAngles.z + (gravDir * 90)));
        if (grounded_particles_parent.eulerAngles.z >= 360) { grounded_particles_parent.rotation = Quaternion.Euler(new Vector3(0, 0, grounded_particles_parent.eulerAngles.z - 360)); }
        else if (grounded_particles_parent.eulerAngles.z <= -360) { grounded_particles_parent.rotation = Quaternion.Euler(new Vector3(0, 0, grounded_particles_parent.eulerAngles.z + 360)); }
    }

    /*bool AngleWithinRange(float angle, float baseAngle, float range)
    {
        bool within = false;

        //if(Mathf.Sign(angle) == Mathf.Sign(baseAngle + range))
        Vector3.Angle()

        return within;
    }*/

    private void FixedUpdate()
    {
        if (able)
        {
            Move();
            Portal();
            Jump();
            Pad();
            Extra();
            Animate();
            Forces();
        }
    }

    private void Move()
    {
        player_body.velocity -= movingObjectVelocity;
        velocityComponentX = Vector2.Dot(player_body.velocity, forwardOrientation);
        velocityComponentY = Vector2.Dot(player_body.velocity, -gravityOrientation);
        velocityVectorX = velocityComponentX * forwardOrientation;
        velocityVectorY = velocityComponentY * -gravityOrientation;

        //Vector2 angularVelocity = MovingObjectVelocities.Count > 0 ? MovingObjectVelocities[MovingObjectVelocities.Count - 1].angularVelocity * Vector2.Perpendicular(player_body.position - MovingObjectVelocities[MovingObjectVelocities.Count - 1].position).normalized : Vector2.zero;

        movingObjectVelocity = MovingObjectVelocities.Count > 0 ? MovingObjectVelocities[MovingObjectVelocities.Count - 1].velocity : Vector2.zero;

        previousDirection = forwardOrientation * reverseDir;

        if (!isCrouched && previousIsCrouched)
        {
            crouchedTimer = 0;

            Cube_Anim.ResetTrigger("Crouch");
            Cube_Anim.ResetTrigger("Squash");
            Cube_Anim.ResetTrigger("DeepSquash");
            Cube_Anim.ResetTrigger("Stretch");
            Cube_Anim.SetTrigger("Default");
        }

        if (grounded) { crouchedTimer += Time.fixedDeltaTime; }
        else { crouchedTimer = CROUCHTIMER; }

        if (chargeTeleportTimer > 0) { chargeTeleportTimer -= Time.fixedDeltaTime; }
        if (dashDisableTimer > 0) { dashDisableTimer -= Time.fixedDeltaTime; }
        if (posMaxSpeedTimer > 0) { posMaxSpeedTimer -= 2*Time.fixedDeltaTime; } else { posMaxSpeedTimer = 0; }
        groundBuffer += Time.fixedDeltaTime;

        // DASH ORB
        if (inDash)
        {
            if(!gamemodeConstants[gamemode].auto)
            {
                
                float dashSpeed = (overrideDashSpeed == -1 ? SPEEDS[speed] : overrideDashSpeed);
                velocityVectorX = Vector2.Dot(dashDirection * dashSpeed * Time.fixedDeltaTime * 10f, forwardOrientation) * forwardOrientation;
                velocityVectorY = Vector2.Dot(dashDirection * dashSpeed * Time.fixedDeltaTime * 10f, -gravityOrientation) * -gravityOrientation;
            }
            else
            {
                //Debug.Log(dashDirection + "   " + forwardOrientation + "   " + Vector2.Dot(dashDirection, forwardOrientation) + "   " + (int)Vector2.Angle(dashDirection, forwardOrientation));
                if ((int)Vector2.Angle(dashDirection, forwardOrientation) == 90)
                {
                    
                    inDash = false;
                    dash_particles.Stop();
                    if (dash_effect.activeSelf) { StartCoroutine(DashFlameDissipate()); }
                }
                else
                {
                    int rev = normalBaseOrientation ? 1 : -1;
                    int gd = gravityDirection % 2 == 0 ? gravityDirection : (gravityDirection + 2) % 4;
                    float forwardDashSpeed = reverseDir * SPEEDS[speed] * Time.fixedDeltaTime * 10f;
                    velocityVectorY = (rev * forwardDashSpeed * (Mathf.Tan(Mathf.Deg2Rad * (dashAngle - 90 * gd)))) * -gravityOrientation;
                    velocityVectorX = forwardDashSpeed * forwardOrientation;
                }
            }
        }

        // CROUCHED
        else if (crouch && grounded && crouchedTimer >= CROUCHTIMER && gamemode == Gamemode.cube)
        {
            //crouch_collider.enabled = true;
            //player_collider.enabled = false;
            //player_collider.isTrigger = true;
            player_collider.size = crouch_collider.size;
            player_collider.offset = crouch_collider.offset;
            player_collider.edgeRadius = 0;
            crouchCollider = true;

            int rev = velocityComponentX >= 0 ? 1 : -1;
            float targetX = ((headHit.distance > 0) ? rev * SPEEDS[speed] : 0) * Time.fixedDeltaTime * 10f;
            Vector2 targetVelocityX = targetX * forwardOrientation;

            if (!isCrouched)
            {
                velocityVectorX = Vector2.SmoothDamp(velocityVectorX * 1.6f, targetVelocityX, ref ref_Velocity, .35f);
                isCrouched = true;

                Cube_Anim.ResetTrigger("Default");
                Cube_Anim.ResetTrigger("Squash");
                Cube_Anim.ResetTrigger("Stretch");
                Cube_Anim.ResetTrigger("DeepSquash");
                Cube_Anim.SetTrigger("Crouch");
            }
            else
            {
                velocityVectorX = Vector2.SmoothDamp(velocityVectorX, targetVelocityX, ref ref_Velocity, .35f);
            }
        }
        
        // DASH DISABLE
        else if(dashDisable)
        {
            if (dashDisableTimer <= 0) { dashDisable = false; }
        }

        // NORMAL MOVEMENT
        else
        {
            //player_collider.enabled = true;
            //player_collider.isTrigger = false;
            //crouch_collider.enabled = false;
            crouchCollider = false;

            if (!gamemodeConstants[gamemode].auto)
            {
                float clampedChargedTimer = Mathf.Clamp(chargeTeleportTimer, 0, 1);
                bool cancelCharge = !(chargeTeleportTimer > 0 && ((!grounded && (Mathf.Abs(Vector2.Dot(chargedTeleportVelocity, forwardOrientation)) * clampedChargedTimer) > Mathf.Abs(moveX)) || chargeTeleportTimer > 1));

                if (gamemode != Gamemode.wave)
                {
                    if(crouch && grounded && gamemode == Gamemode.spider) { moveX /= 3; }
                    float targetX = (cancelCharge ? moveX : (moveX * (1 - clampedChargedTimer) + Vector2.Dot(chargedTeleportVelocity, forwardOrientation) * clampedChargedTimer)) * Time.fixedDeltaTime * 10f;
                    Vector2 targetVelocityX = targetX * forwardOrientation;

                    if (velocityVectorX != targetVelocityX)
                    {
                        if (!grounded && !ceiling)
                        {
                            //velocityVectorX = Vector2.SmoothDamp(velocityVectorX, targetVelocityX, ref ref_Velocity, gamemodeConstants[gamemode].accelerationNG * ((!grounded && !cancelCharge) ? CHARGEDSMOOTH : 1));
                            velocityVectorX = Vector2.SmoothDamp(velocityVectorX, targetVelocityX, ref ref_Velocity, ((!grounded && !cancelCharge) ? gamemodeConstants[gamemode].chargedAcceleration : gamemodeConstants[gamemode].accelerationNG));
                        }
                        else
                        {
                            if (Mathf.Abs(targetX) > Mathf.Abs(velocityComponentX))
                            {
                                //velocityVectorX = Vector2.SmoothDamp(velocityVectorX, targetVelocityX, ref ref_Velocity, gamemodeConstants[gamemode].acceleration * ((!grounded && !cancelCharge) ? CHARGEDSMOOTH : 1));
                                velocityVectorX = Vector2.SmoothDamp(velocityVectorX, targetVelocityX, ref ref_Velocity, ((!grounded && !cancelCharge) ? gamemodeConstants[gamemode].chargedAcceleration : gamemodeConstants[gamemode].acceleration));
                            }
                            else
                            {
                                velocityVectorX = Vector2.SmoothDamp(velocityVectorX, targetVelocityX, ref ref_Velocity, gamemodeConstants[gamemode].deceleration);
                            }
                        }
                    }
                }
                else
                {
                    float targetX = (cancelCharge ? moveX : (moveX * (1 - clampedChargedTimer) + chargedTeleportVelocity.x * clampedChargedTimer)) * Time.fixedDeltaTime * 10f;
                    Vector2 targetVelocityX = new Vector2(targetX, 0);
                    //Vector2 targetVelocityX = targetX * forwardOrientation;

                    float targetY = (cancelCharge ? moveY : (moveY * (1 - clampedChargedTimer) + chargedTeleportVelocity.y * clampedChargedTimer)) * Time.fixedDeltaTime * 10f;
                    Vector2 targetVelocityY = new Vector2(0, targetY);
                    //Vector2 targetVelocityY = targetY * -gravityOrientation;

                    //velocityVectorX = Vector2.SmoothDamp(velocityVectorX, targetVelocityX, ref ref_Velocity, gamemodeConstants[gamemode].accelerationNG * ((!grounded && !cancelCharge) ? CHARGEDSMOOTH : 1));
                    velocityVectorX = Vector2.SmoothDamp(velocityVectorX, targetVelocityX, ref ref_Velocity, ((!grounded && !cancelCharge) ? gamemodeConstants[gamemode].chargedAcceleration : gamemodeConstants[gamemode].accelerationNG));

                    if (grounded || ceiling) { launched = false; }
                    if (!launched)
                    {
                        addForce = false;
                        //velocityVectorY = Vector2.SmoothDamp(velocityVectorX, targetVelocityY, ref ref_Velocity, gamemodeConstants[gamemode].accelerationNG * ((!grounded && !cancelCharge) ? CHARGEDSMOOTH : 1));
                        velocityVectorY = Vector2.SmoothDamp(velocityVectorX, targetVelocityY, ref ref_Velocity, ((!grounded && !cancelCharge) ? gamemodeConstants[gamemode].chargedAcceleration : gamemodeConstants[gamemode].accelerationNG));
                    }
                    else
                    {
                        addForce = true;
                        float strength = !mini ? 6f : 4f;
                        additionalForce = (strength * 9.81f) * gravityOrientation;
                    }
                }
            }
            else
            {
                float targetX = reverseDir * SPEEDS[speed] * Time.fixedDeltaTime * 10f;
                velocityVectorX = targetX * forwardOrientation;
            }
        }

        falling = velocityComponentY <= 0;
        posMaxSpeed = Mathf.Lerp(posMaxSpeed, gamemodeConstants[gamemode].posMaxSpeed, 1 - posMaxSpeedTimer);

        // PREVIOUS CHECKS
        previousIsCrouched = isCrouched;

        if (firstFramesBuffer > 0) { firstFramesBuffer -= Time.fixedDeltaTime; }
    }

    private void Portal()
    {
        if (gravportal_flip)
        {
            gravportal_flip = false;

            jump_released = false;
            launched = false;
            main_trail.emitting = true;
            if (Vector2.Dot(velocityVectorY, -gravityOrientation) <= -15f)
            {
                velocityVectorY = -15f * -gravityOrientation;
            }

            spiderAnimType = 2;
            spiderAnimSpeed = 1;

            gravityDirection = (gravityDirection + 2) % 4;
            normalBaseOrientation = !normalBaseOrientation;
            changeGravityDirection();
            playGravityParticles(gravityDirection);
        }
        else if (gravportal_down)
        {
            gravportal_down = false;
            if(gravityDirection == (GetOppositeGravity()+2) % 4) { return; }

            jump_released = false;
            launched = false;
            main_trail.emitting = true;
            if (Vector2.Dot(velocityVectorY, -gravityOrientation) <= -15f)
            {
                velocityVectorY = -15f * -gravityOrientation;
            }

            spiderAnimType = 2;
            spiderAnimSpeed = 1;

            gravityDirection = (GetOppositeGravity() + 2) % 4;
            normalBaseOrientation = !normalBaseOrientation;
            changeGravityDirection();
            playGravityParticles(gravityDirection);
        }
        else if (gravportal_up)
        {
            gravportal_up = false;
            if (gravityDirection == GetOppositeGravity()) { return; }

            jump_released = false;
            launched = false;
            main_trail.emitting = true;
            if (Vector2.Dot(velocityVectorY, -gravityOrientation) <= -15f)
            {
                velocityVectorY = -15f * -gravityOrientation;
            }

            spiderAnimType = 2;
            spiderAnimSpeed = 1;

            gravityDirection = GetOppositeGravity();
            normalBaseOrientation = !normalBaseOrientation;
            changeGravityDirection();
            playGravityParticles(gravityDirection);
        }
        else if (teleportalA)
        {
            teleportalA = false;
            //main_trail.emitting = true;
            Vector2 positionDelta = (player_body.position + teleBDelta) - player_body.position;
            player_body.transform.position += (Vector3)teleBDelta;

            if(PortalTouched != null && !PortalTouched.GetComponent<PortalComponent>().smoothTeleport)
            {
                gamemanager.getActiveCamera().GetCinemachineComponent<CinemachineFramingTransposer>().OnTargetObjectWarped(transform, positionDelta);
            }
        }
        else if (teleportal_charged)
        {
            teleportal_charged = false;
            //main_trail.emitting = true;
            Vector3 positionDelta = (player_body.position + teleBChargedDelta) - player_body.position;
            player_body.transform.position += (Vector3)teleBChargedDelta;

            if (!gamemodeConstants[gamemode].auto) { velocityVectorX = Vector2.Dot(chargedTeleportVelocity, forwardOrientation) * forwardOrientation; }
            velocityVectorY = Vector2.Dot(chargedTeleportVelocity, -gravityOrientation) * -gravityOrientation;

            if (PortalTouched != null && !PortalTouched.GetComponent<PortalComponent>().smoothTeleport)
            {
                gamemanager.getActiveCamera().GetCinemachineComponent<CinemachineFramingTransposer>().OnTargetObjectWarped(transform, positionDelta);
            }

            spiderAnimType = 1;
            spiderAnimSpeed = 2;

            jump_from_ground = false;
            main_trail.emitting = true;
        }
    }
    private void Jump()
    {
        float jumpForce = !mini ? gamemodeConstants[gamemode].jumpForce : gamemodeConstants[gamemode].miniJumpForce;
        jumpForce += Vector2.Dot(movingObjectVelocity, -gravityOrientation);

        if (jump_orb || (gamemodeConstants[gamemode].orbBuffer ? jump_air : false))
        {
            OrbComponent orbscript = null;
            float multiplier = 1;
            int changeDirection = 0;
            Vector2 direction = Vector2.zero;
            bool usedOrb = false;
            cancel_jump = false;
            if (OrbTouched != null)
            {
                orbscript = OrbTouched.GetComponent<OrbComponent>();
                multiplier = orbscript.multiplier;
                direction = orbscript.GetDirection();
                changeDirection = orbscript.reverse;
            }

            if (trigger_orb)
            {
                trigger_orb = false;
                SpawnTrigger spawn = OrbTouched.GetComponent<SpawnTrigger>();
                StartCoroutine(spawn.Begin());

                jump = !spawn.cancelJump;
                jump_orb = false;
                jump_air = false;
                trigger_orb = spawn.cancelJump;
                if (OrbTouched != null)
                {
                    orbscript.Pulse();

                    if (orbscript.keepOrbActive) { trigger_orb = orbscript.keepOrbActive; }
                }

                usedOrb = true;
            }
            else if (tele_orb)
            {
                Vector2 positionDelta = (player_body.position + tele_orb_translate) - player_body.position;
                player_body.transform.position += (Vector3)tele_orb_translate;

                tele_orb = false;
                jump_orb = false;
                jump_air = false;
                if (OrbTouched != null)
                {
                    orbscript.Pulse();
                }

                usedOrb = true;

                if(!orbscript.smoothTeleport)
                {
                    gamemanager.getActiveCamera().GetCinemachineComponent<CinemachineFramingTransposer>().OnTargetObjectWarped(transform, positionDelta);
                }
            }
            else if (super_orb)
            {
                velocityVectorY = multiplier * Vector2.up.Rotate(orbscript.forceDirection + (orbscript.superGravityDirection * -90) + 180);
                //posMaxSpeed = Mathf.Max(gamemodeConstants[gamemode].posMaxSpeed, jumpForce * gamemodeConstants[gamemode].yellowOrbMultiplier * multiplier);
                //posMaxSpeedTimer = 1;
                bool oppositeGravity = false;
                bool changedGravity = false;
                oppositeGravity = orbscript.superGravityDirection == (gravityDirection + 2) % 4;
                changedGravity = orbscript.superGravityDirection != gravityDirection;

                posMaxSpeed = Mathf.Max(gamemodeConstants[gamemode].posMaxSpeed, multiplier);
                posMaxSpeedTimer = 1;

                gravityDirection = orbscript.superGravityDirection;
                if(changedGravity)
                {
                    if (oppositeGravity) { normalBaseOrientation = !normalBaseOrientation; }
                    changeGravityDirection();
                    playGravityParticles(gravityDirection);
                }

                super_orb = false;
                jump_orb = false;
                jump_air = false;
                launched = true;
                groundBuffer = 0;
                grounded = false;
                dashDisable = false;
                main_trail.emitting = true;
                if (OrbTouched != null)
                {
                    orbscript.Pulse();
                }

                eyeType = 1;
                spiderAnimType = 1;
                spiderAnimSpeed = 2;

                chargeTeleportTimer = 0;
                usedOrb = true;
            }
            else if (rebound_orb)
            {
                velocityVectorY = -velocityVectorY;
                addForce = true;
                additionalForce = gamemodeConstants[gamemode].gravityStrength * 9.81f * -gravityOrientation;
                //velocityVectorX = -velocityVectorX;
                //chargedTeleportVelocity = -velocityVectorY + -velocityVectorX;
                //chargeTeleportTimer = CHARGEDTIMER;
                posMaxSpeed = Mathf.Max(gamemodeConstants[gamemode].posMaxSpeed, player_body.velocity.magnitude);
                posMaxSpeedTimer = 1;

                rebound_orb = false;
                jump_orb = false;
                jump_air = false;
                launched = true;
                groundBuffer = 0;
                grounded = false;
                dashDisable = false;
                main_trail.emitting = true;
                if (OrbTouched != null)
                {
                    orbscript.Pulse();
                }

                eyeType = 1;
                spiderAnimType = 1;
                spiderAnimSpeed = 2;

                chargeTeleportTimer = 0;
                usedOrb = true;
            }
            else if (red_orb)
            {
                velocityVectorY = (jumpForce * gamemodeConstants[gamemode].redOrbMultiplier * multiplier) * -gravityOrientation;
                posMaxSpeed = Mathf.Max(gamemodeConstants[gamemode].posMaxSpeed, jumpForce * gamemodeConstants[gamemode].redOrbMultiplier * multiplier);
                posMaxSpeedTimer = 1;

                red_orb = false;
                jump_orb = false;
                jump_air = false;
                launched = true;
                groundBuffer = 0;
                grounded = false;
                dashDisable = false;
                main_trail.emitting = true;
                if (OrbTouched != null)
                {
                    orbscript.Pulse();
                }

                eyeType = 1;
                spiderAnimType = 1;
                spiderAnimSpeed = 2;

                chargeTeleportTimer = 0;
                usedOrb = true;
            }
            else if (yellow_orb)
            {
                velocityVectorY = (jumpForce * gamemodeConstants[gamemode].yellowOrbMultiplier * multiplier) * -gravityOrientation;
                posMaxSpeed = Mathf.Max(gamemodeConstants[gamemode].posMaxSpeed, jumpForce * gamemodeConstants[gamemode].yellowOrbMultiplier * multiplier);
                posMaxSpeedTimer = 1;

                yellow_orb = false;
                jump_orb = false;
                jump_air = false;
                launched = true;
                groundBuffer = 0;
                grounded = false;
                dashDisable = false;
                main_trail.emitting = true;
                if (OrbTouched != null)
                {
                    orbscript.Pulse();
                }

                eyeType = 1;
                spiderAnimType = 1;
                spiderAnimSpeed = 2;

                chargeTeleportTimer = 0;
                usedOrb = true;
            }
            else if (pink_orb)
            {
                velocityVectorY = (jumpForce * gamemodeConstants[gamemode].pinkOrbMultiplier * multiplier) * -gravityOrientation;
                posMaxSpeed = Mathf.Max(gamemodeConstants[gamemode].posMaxSpeed, jumpForce * gamemodeConstants[gamemode].pinkOrbMultiplier * multiplier);
                posMaxSpeedTimer = 1;

                pink_orb = false;
                jump_orb = false;
                jump_air = false;
                launched = true;
                groundBuffer = 0;
                grounded = false;
                dashDisable = false;
                main_trail.emitting = true;
                if (OrbTouched != null)
                {
                    orbscript.Pulse();
                }

                eyeType = 0;
                spiderAnimType = 1;
                spiderAnimSpeed = 2;

                chargeTeleportTimer = 0;
                usedOrb = true;
            }
            else if (purple_dash)
            {
                RaycastHit2D groundhit, deathhit;

                bool connect = true;

                Collider2D collider = !crouch ? player_collider : crouch_collider;
                groundhit = Physics2D.Raycast(player_body.position, direction, 100, groundLayer);
                deathhit = Physics2D.Raycast(player_body.position, direction, 100, deathLayer);
                
                if (deathhit.collider != null && (deathhit.distance <= groundhit.distance || groundhit.distance == 0))
                {
                    velocityVectorY = Vector2.zero;
                    spider_trail.emitting = true;
                    SpOrTr.Activate(spider_trail, gameObject);
                    //playGravityParticles();
                    player_body.position = (player_body.position + ((deathhit.distance) * direction));
                    //gravityDirection = (gravityDirection + 2) % 4;
                    //normalBaseOrientation = !normalBaseOrientation;
                    //changeGravityDirection();
                }
                else if (groundhit.collider != null)
                {
                    velocityVectorY = Vector2.zero;
                    spider_trail.emitting = true;
                    SpOrTr.Activate(spider_trail, gameObject);

                    bool flipDirection = false;
                    if(groundhit.normal == new Vector2(0, 1))
                    {
                        if(gravityDirection == 2) normalBaseOrientation = !normalBaseOrientation;
                        gravityDirection = 0;
                    }
                    else if (groundhit.normal == new Vector2(1, 0))
                    {
                        if (gravityDirection == 3) normalBaseOrientation = !normalBaseOrientation;
                        gravityDirection = 1;
                    }
                    else if (groundhit.normal == new Vector2(0, -1))
                    {
                        if (gravityDirection == 0) normalBaseOrientation = !normalBaseOrientation;
                        gravityDirection = 2;
                    }
                    else if (groundhit.normal == new Vector2(-1, 0))
                    {
                        if (gravityDirection == 1) normalBaseOrientation = !normalBaseOrientation;
                        gravityDirection = 3;
                    }

                    if (flipDirection && (gamemode == Gamemode.spider || (gamemode == Gamemode.auto_spider && reverseDir == 0)))
                    {
                        if (moveX == 0)
                        {
                            iconParent.localScale = new Vector3(-iconParent.localScale.x, Mathf.Abs(iconParent.localScale.y), 1);
                        }
                    }

                    changeGravityDirection();
                    playGravityParticles(gravityDirection);
                    firstFramesBuffer = FRAMEBUFFERTIME;

                    player_body.position = (player_body.position + ((groundhit.distance - (mini ? .1f : .3f)) * direction));
                }
                else
                {
                    connect = false;
                }

                pulse_trigger_p1.Enter();
                pulse_trigger_p2.Enter();

                purple_dash = false;
                jump_orb = false;
                jump_air = false;
                dashDisable = false;
                main_trail.emitting = true;
                if (!connect) return;

                if (OrbTouched != null)
                {
                    orbscript.Pulse();
                }
                usedOrb = true;
            }
            else if (purple_orb)
            {
                RaycastHit2D groundhit, deathhit;

                bool connect = true;

                Collider2D collider = !crouch ? player_collider : crouch_collider;
                groundhit = Physics2D.BoxCast(player_body.position + (.2f * -gravityOrientation), new Vector2(Mathf.Abs(Vector2.Dot(collider.bounds.size, -gravityOrientation)) * .5f, .1f), 0f, -gravityOrientation, 30, groundLayer);
                deathhit = Physics2D.BoxCast(player_body.position + (.2f * -gravityOrientation), new Vector2(Mathf.Abs(Vector2.Dot(collider.bounds.size, -gravityOrientation)) * .5f, .1f), 0f, -gravityOrientation, 30, deathLayer);
                
                if (deathhit.collider != null && (deathhit.distance <= groundhit.distance || groundhit.distance == 0))
                {
                    velocityVectorY = Vector2.zero;
                    spider_trail.emitting = true;
                    SpOrTr.Activate(spider_trail, gameObject);
                    //playGravityParticles();
                    player_body.position = (player_body.position + ((deathhit.distance - 0/*(mini ? 0f : .1f)*/) * -gravityOrientation));
                    gravityDirection = (gravityDirection + 2) % 4;
                    normalBaseOrientation = !normalBaseOrientation;
                    changeGravityDirection();
                }
                else if (groundhit.collider != null)
                {
                    velocityVectorY = Vector2.zero;
                    spider_trail.emitting = true;
                    SpOrTr.Activate(spider_trail, gameObject);
                    //playGravityParticles();
                    player_body.position = (player_body.position + ((groundhit.distance - .2f/*(mini ? .1f : .3f)*/) * -gravityOrientation));
                    gravityDirection = (gravityDirection + 2) % 4;
                    normalBaseOrientation = !normalBaseOrientation;
                    changeGravityDirection();

                    if (gamemode == Gamemode.spider || (gamemode == Gamemode.auto_spider && reverseDir == 0))
                    {
                        if (moveX == 0)
                        {
                            iconParent.localScale = new Vector3(-iconParent.localScale.x, Mathf.Abs(iconParent.localScale.y), 1);
                        }
                    }
                }
                else
                {
                    connect = false;
                }

                pulse_trigger_p1.Enter();
                pulse_trigger_p2.Enter();

                purple_orb = false;
                jump_orb = false;
                jump_air = false;
                dashDisable = false;
                main_trail.emitting = true;
                if (!connect) return;

                if (OrbTouched != null)
                {
                    orbscript.Pulse();
                }
                usedOrb = true;
            }
            else if (blue_orb)
            {
                velocityVectorY = (jumpForce * gamemodeConstants[gamemode].blueOrbMultiplier * multiplier) * -gravityOrientation;
                posMaxSpeed = Mathf.Max(gamemodeConstants[gamemode].posMaxSpeed, jumpForce * gamemodeConstants[gamemode].blueOrbMultiplier * multiplier);
                posMaxSpeedTimer = 1;
                //gravityDirection = (gravityDirection+3) % 4;
                gravityDirection = (gravityDirection + 2) % 4;
                normalBaseOrientation = !normalBaseOrientation;
                changeGravityDirection();
                playGravityParticles(gravityDirection);

                blue_orb = false;
                jump_orb = false;
                jump_air = false;
                launched = false;

                groundBuffer = 0;
                grounded = false;
                dashDisable = false;
                main_trail.emitting = true;
                if (OrbTouched != null)
                {
                    orbscript.Pulse();
                }

                eyeType = 2;
                spiderAnimType = 2;
                spiderAnimSpeed = 2;

                chargeTeleportTimer = 0;
                usedOrb = true;
            }
            else if (cw_orb)
            {
                gravityDirection = (gravityDirection+1) % 4;
                changeGravityDirection();
                playGravityParticles(gravityDirection);

                cw_orb = false;
                jump_orb = false;
                jump_air = false;
                launched = false;
                dashDisable = false;
                main_trail.emitting = true;
                if (OrbTouched != null)
                {
                    orbscript.Pulse();
                }

                eyeType = 0;
                spiderAnimType = 0;
                spiderAnimSpeed = 1;

                chargeTeleportTimer = 0;
                usedOrb = true;
            }
            else if (ccw_orb)
            {
                gravityDirection = (gravityDirection + 3) % 4;
                changeGravityDirection();
                playGravityParticles(gravityDirection);

                ccw_orb = false;
                jump_orb = false;
                jump_air = false;
                launched = false;
                dashDisable = false;
                main_trail.emitting = true;
                if (OrbTouched != null)
                {
                    orbscript.Pulse();
                }

                eyeType = 0;
                spiderAnimType = 0;
                spiderAnimSpeed = 1;

                chargeTeleportTimer = 0;
                usedOrb = true;
            }
            else if (green_orb)
            {
                gravityDirection = (gravityDirection + 2) % 4;
                normalBaseOrientation = !normalBaseOrientation;
                changeGravityDirection();
                playGravityParticles(gravityDirection);
                velocityVectorY = (jumpForce * gamemodeConstants[gamemode].greenOrbMultiplier * multiplier) * -gravityOrientation;
                posMaxSpeed = Mathf.Max(gamemodeConstants[gamemode].posMaxSpeed, jumpForce * gamemodeConstants[gamemode].greenOrbMultiplier * multiplier);
                posMaxSpeedTimer = 1;

                green_orb = false;
                jump_orb = false;
                jump_air = false;
                launched = true;

                groundBuffer = 0;
                grounded = true;

                dashDisable = false;
                main_trail.emitting = true;
                if (OrbTouched != null)
                {
                    orbscript.Pulse();
                }

                eyeType = 2;
                spiderAnimType = 2;
                spiderAnimSpeed = 2;

                chargeTeleportTimer = 0;
                usedOrb = true;
            }
            else if (black_orb)
            {
                velocityVectorY = (jumpForce * -gamemodeConstants[gamemode].blackOrbMultiplier * multiplier) * -gravityOrientation;
                posMaxSpeed = Mathf.Max(gamemodeConstants[gamemode].posMaxSpeed, jumpForce * gamemodeConstants[gamemode].blackOrbMultiplier * multiplier);
                posMaxSpeedTimer = 1;

                black_orb = false;
                jump_orb = false;
                jump_air = false;
                launched = false;

                dashDisable = false;
                main_trail.emitting = true;
                if (OrbTouched != null)
                {
                    orbscript.Pulse();
                }

                eyeType = 1;
                spiderAnimType = 2;
                spiderAnimSpeed = 2;

                chargeTeleportTimer = 0;
                usedOrb = true;
            }
            else if (dash_orb)
            {
                if(orbscript.gravityDirection != -1 && gravityDirection != orbscript.gravityDirection)
                {
                    if (orbscript.gravityDirection == (gravityDirection + 2) % 4) { normalBaseOrientation = !normalBaseOrientation; }
                    gravityDirection = orbscript.gravityDirection;
                    changeGravityDirection();
                    playGravityParticles(gravityDirection);
                    firstFramesBuffer = FRAMEBUFFERTIME;
                }

                inDash = true;
                dashDirection = direction;
                dashAngle = orbscript.GetAngle();
                dash_particles.Play();
                StopCoroutine(DashFlameDissipate());
                dash_effect.SetActive(true);
                dash_parent.transform.eulerAngles = new Vector3(0, 0, dashAngle);
                //dash_parent.transform.right = direction;
                //dash_flame_material.SetFloat("_DistortionStrength", 1);
                dash_flame_material.SetFloat("_FireBottom", 1.3f);
                //dash_flame_material.SetFloat("_YStretch", 1.03f);

                if (!orbscript.overrideSpeed)
                {
                    overrideDashSpeed = -1;
                }
                else
                {
                    if(orbscript.dashSpeed < SPEEDS.Length)
                    {
                        overrideDashSpeed = SPEEDS[orbscript.dashSpeed];
                    }
                    else
                    {
                        overrideDashSpeed = (float)orbscript.dashSpeed;
                    }
                }

                pulse_trigger_p1.Enter();
                pulse_trigger_p2.Enter();

                dash_orb = false;
                jump_orb = false;
                jump_air = false;
                launched = false;
                dashDisable = false;
                //main_trail.emitting = true;
                if (OrbTouched != null)
                {
                    orbscript.Pulse();
                }

                eyeType = 2;
                spiderAnimType = 2;
                spiderAnimSpeed = 2;

                chargeTeleportTimer = 0;
                usedOrb = true;
            }
            else if (black_dash)
            {
                //if (!gamemodeConstants[gamemode].auto) { velocityVectorX = Vector2.Dot(40 * direction, forwardOrientation) * forwardOrientation; }
                velocityVectorX = Vector2.Dot(40 * direction, forwardOrientation) * forwardOrientation;
                velocityVectorY = Vector2.Dot(40 * direction, -gravityOrientation) * -gravityOrientation;

                if (orbscript.gravityDirection != -1 && gravityDirection != orbscript.gravityDirection)
                {
                    if (orbscript.gravityDirection == (gravityDirection + 2) % 4) { normalBaseOrientation = !normalBaseOrientation; }
                    gravityDirection = orbscript.gravityDirection;
                    changeGravityDirection();
                    playGravityParticles(gravityDirection);
                    firstFramesBuffer = FRAMEBUFFERTIME;
                }

                pulse_trigger_p1.Enter();
                pulse_trigger_p2.Enter();

                black_dash = false;
                jump_orb = false;
                jump_air = false;
                launched = true;

                groundBuffer = 0;
                grounded = false;

                main_trail.emitting = true;
                if (OrbTouched != null)
                {
                    orbscript.Pulse();
                }

                eyeType = 1;
                spiderAnimType = 1;
                spiderAnimSpeed = 2;

                dashDisable = true;
                //chargeTeleportTimer = CHARGEDTIMER;
                dashDisableTimer = 0.15f;
                chargeTeleportTimer = 0f;
                usedOrb = true;
            }

            if(usedOrb)
            {
                bool shrink_wave_trail = false;
                switch (changeDirection)
                {
                    case -1:
                        if (reverseDir == 0)
                        {
                            firstFramesBuffer = FRAMEBUFFERTIME;
                            shrink_wave_trail = true;
                        }
                        reverseDir = 0;
                        break;

                    case 1:
                        if (reverseDir == -1)
                        {
                            firstFramesBuffer = FRAMEBUFFERTIME;
                            shrink_wave_trail = true;
                        }
                        reverseDir = 1;
                        break;
                    case 2:
                        if (reverseDir == 1)
                        {
                            firstFramesBuffer = FRAMEBUFFERTIME;
                            shrink_wave_trail = true;
                        }
                        reverseDir = -1;
                        break;
                    case 3:
                        shrink_wave_trail = true;
                        firstFramesBuffer = FRAMEBUFFERTIME;
                        reverseDir = (reverseDir == 1 ? -1 : 1);
                        //timeManager.setScale(2, 1);
                        break;
                }

                if (shrink_wave_trail && gamemode == Gamemode.auto_wave)
                {
                    auto_wave_trail.GetComponent<TrailRenderer>().Clear();
                    auto_wave_trail.transform.GetChild(0).GetComponent<TrailRenderer>().Clear();
                }

                if (OrbTouched != null)
                {
                    cancel_jump = gamemodeConstants[gamemode].cancelJump;
                    if(gamemode == Gamemode.cube && cubeHoldJump) { cancel_jump = false; }
                    if (orbscript.overrideCancelJump) { cancel_jump = orbscript.cancel_jump; }
                    orbscript.Pulse();
                }

                if (previousDirection != forwardOrientation * reverseDir)
                {
                    Vector2 newDirection = forwardOrientation * reverseDir;
                    float Dot = Vector2.Dot(newDirection, previousDirection);
                    float inDirection = Vector2.Dot(OrbTouched.transform.position - transform.position, newDirection);

                    if(inDirection != 0)
                    {
                        timeManager.setScale(1, (inDirection > 0 ? 1.05f : .95f));
                        timeManager.setFade(1, 1, Mathf.Abs(inDirection));
                    }

                    previousDirection = newDirection;
                }

                jump_from_ground = false;
            }
        }

        //if (pad) { jump = false; jump_hold = false; }
        if((jump || jump_hold) && !(inDash || cancel_jump) && groundBuffer > GROUNDBUFFERTIME && !pad)
        {
            switch (gamemode)
            {
                case Gamemode.cube:
                    if ((!cubeHoldJump ? jump : jump_hold) && (grounded || coyoteTimer <= COYOTETIME))
                    {
                        velocityVectorY = jumpForce * -gravityOrientation;
                        grounded = false;
                        jump = false;
                        jump_orb = false;
                        jump_air = false;
                        jump_from_ground = true;

                        coyoteTimer = COYOTETIME * 2;

                        Cube_Anim.ResetTrigger("Crouch");
                        Cube_Anim.ResetTrigger("Default");
                        Cube_Anim.ResetTrigger("Squash");
                        Cube_Anim.ResetTrigger("DeepSquash");
                        Cube_Anim.ResetTrigger("Stretch");
                        Cube_Anim.Play("HeightStretch", -1, 0f);
                    }                    
                    break;
                
                case Gamemode.auto_cube:
                    if ((grounded || coyoteTimer + .045f <= COYOTETIME) && jump_hold)
                    {
                        velocityVectorY = jumpForce * -gravityOrientation;
                        grounded = false;
                        jump = false;
                        jump_orb = false;
                        jump_air = false;
                        jump_orb = false;
                        jump_from_ground = true;

                        coyoteTimer = COYOTETIME * 2;
                    }
                    break;

                case Gamemode.ship:
                    if (jump_hold)
                    {
                        addForce = true;
                        float strength = !mini ? 3.8f : 4.4f;
                        additionalForce = (strength * 9.81f) * -gravityOrientation;
                    }
                    break;

                case Gamemode.auto_ship:
                    if (jump_hold)
                    {
                        addForce = true;
                        float strength = (!mini ? 3.91f : 4.51f) * shipAccelerationCurve.Evaluate(Mathf.Clamp(holdJumpTimer / (SHIP_ACCELERATE_CURVE_TIME * (!mini ? 1 : .75f)), 0, 1));
                        //float strength = (!mini ? 3.91f : 4.51f);
                        //float strength = !mini ? 4.2f : 4.8f;
                        additionalForce = (strength * 9.81f) * -gravityOrientation;
                    }
                    break;

                case Gamemode.ufo:
                case Gamemode.auto_ufo:
                    if (jump)
                    {
                        velocityVectorY = jumpForce * -gravityOrientation;
                        ground_impact_particles.Play();
                        main_trail.emitting = true;
                        jump = false;
                    }
                    break;

                case Gamemode.ball:
                case Gamemode.auto_ball:
                    if (jump && (grounded || coyoteTimer + .02f <= COYOTETIME))
                    {
                        velocityVectorY = jumpForce * -gravityOrientation * .4f;
                        gravityDirection = (gravityDirection + 2) % 4;
                        normalBaseOrientation = !normalBaseOrientation;
                        changeGravityDirection();

                        jump = false;
                        jump_orb = false;
                        jump_air = false;
                        grounded = false;

                        coyoteTimer = COYOTETIME * 2;
                    }
                    break;

                case Gamemode.auto_wave:
                    if (jump)
                    {
                        jump = false;
                        launched = false;
                    }
                    if (jump_hold && !launched)
                    {
                        int mult = !mini ? 1 : 2;
                        velocityVectorY = mult * velocityVectorX.magnitude * -gravityOrientation;
                    }
                    break;

                case Gamemode.spider:
                case Gamemode.auto_spider:
                    if (jump && (grounded || coyoteTimer <= COYOTETIME))
                    {
                        RaycastHit2D groundhit, deathhit;

                        bool connect = true;

                        Collider2D collider = player_collider;
                        groundhit = Physics2D.BoxCast(player_body.position + (.2f * -gravityOrientation), new Vector2(Mathf.Abs(Vector2.Dot(collider.bounds.size, -gravityOrientation)) * .5f, .1f), 0f, -gravityOrientation, 30, groundLayer);
                        deathhit = Physics2D.BoxCast(player_body.position + (.2f * -gravityOrientation), new Vector2(Mathf.Abs(Vector2.Dot(collider.bounds.size, -gravityOrientation)) * .5f, .1f), 0f, -gravityOrientation, 30, deathLayer);

                        if (deathhit.collider != null && (deathhit.distance <= groundhit.distance || groundhit.distance == 0))
                        {
                            velocityVectorY = Vector2.zero;
                            spider_trail.emitting = true;
                            SpOrTr.Activate(spider_trail, gameObject);
                            //playGravityParticles();
                            player_body.position = (player_body.position + ((deathhit.distance - 0/*(mini ? 0f : .1f)*/) * -gravityOrientation));
                            gravityDirection = (gravityDirection + 2) % 4;
                            normalBaseOrientation = !normalBaseOrientation;
                            changeGravityDirection();
                        }
                        else if (groundhit.collider != null)
                        {
                            velocityVectorY = Vector2.zero;
                            spider_trail.emitting = true;
                            SpOrTr.Activate(spider_trail, gameObject);
                            //playGravityParticles();
                            player_body.position = (player_body.position + ((groundhit.distance - .2f/*(mini ? .1f : .3f)*/) * -gravityOrientation));
                            gravityDirection = (gravityDirection + 2) % 4;
                            normalBaseOrientation = !normalBaseOrientation;
                            changeGravityDirection();

                            if(moveX == 0)
                            {
                                iconParent.localScale = new Vector3(-iconParent.localScale.x, Mathf.Abs(iconParent.localScale.y), 1);
                            }
                        }
                        else
                        {
                            connect = false;
                        }

                        pulse_trigger_p1.Enter();
                        pulse_trigger_p2.Enter();

                        purple_orb = false;
                        jump = false;
                        dashDisable = false;
                        main_trail.emitting = true;
                    }
                    break;

                case Gamemode.copter:
                case Gamemode.auto_copter:
                    if(jump)
                    {
                        jump = false;
                        goingUp = !goingUp;
                        main_trail.emitting = false;
                    }
                    break;

                case Gamemode.spring:
                case Gamemode.auto_spring:
                    if (jump && !grounded && !springDown)
                    {
                        velocityVectorY = (jumpForce) * gravityOrientation * (mini ? 16f : 1);
                        //posMaxSpeed = Mathf.Max(gamemodeConstants[gamemode].posMaxSpeed, jumpForce);
                        //posMaxSpeedTimer = 1;

                        jump = false;
                        springDown = true;
                        main_trail.emitting = true;
                    }
                    break;
            }
        }
        else if(jump_released || !jump_hold || (gamemode == Gamemode.auto_wave ? launched : false))
        {
            switch (gamemode)
            {
                case Gamemode.cube:
                    if (jump_released && jump_from_ground && !falling && !fixedJumpHeight)
                    {
                        //player_body.velocity = (velocityComponentX * forwardOrientation) + ((velocityComponentY/2) * -gravityOrientation);
                        velocityVectorY = (velocityComponentY/2) * -gravityOrientation;
                        jump_released = false;
                        jump_from_ground = false;
                    }
                    break;

                case Gamemode.ship:
                    if (!jump_hold)
                    {
                        addForce = true;
                        //float strength = !mini ? 0f : .2f;
                        //additionalForce = (strength * gamemodeConstants[gamemode].gravityStrength) * gravityOrientation;
                        float strength = !mini ? 2.7f : 3f;
                        additionalForce = (strength * 9.81f) * gravityOrientation;

                        eyeType = 0;
                    }
                    break;

                case Gamemode.auto_ship:
                    if (!jump_hold)
                    {
                        addForce = true;
                        float strength = (!mini ? 3.0f : 3.4f) * shipDeccelerationCurve.Evaluate(Mathf.Clamp(releaseJumpTimer / (SHIP_ACCELERATE_CURVE_TIME * (!mini ? 1 : .75f)), 0, 1));
                        //float strength = (!mini ? 3.0f : 3.4f);
                        //float strength = !mini ? 3.2f : 3.6f;
                        additionalForce = (strength * 9.81f) * gravityOrientation;

                        eyeType = 0;
                    }
                    break;

                case Gamemode.auto_wave:
                    if (!jump_hold || launched)
                    {
                        if(!launched)
                        {
                            int mult = !mini ? 1 : 2;
                            velocityVectorY = mult * velocityVectorX.magnitude * gravityOrientation;
                        }
                        else
                        {
                            addForce = true;
                            float strength = !mini ? 6f : 4f;
                            additionalForce = (strength * 9.81f) * gravityOrientation;
                        }
                    }
                    break;

                default:
                    jump_from_ground = false;
                    break;
            }
        }

        if(!inDash && (gamemode == Gamemode.copter || gamemode == Gamemode.auto_copter))
        {
            addForce = true;
            float strength = !mini ? (gamemode == Gamemode.copter ? 40 : 45) : 60;
            int rev = goingUp ? 1 : -1;

            additionalForce = rev * strength * -gravityOrientation;
        }
    }
    private void Pad()
    {
        if(pad)
        {
            float jumpForce = !mini ? gamemodeConstants[gamemode].jumpForce : gamemodeConstants[gamemode].miniJumpForce;
            jumpForce += Vector2.Dot(movingObjectVelocity, -gravityOrientation);
            dashDisable = false;
            jump_from_ground = false;

            PadComponent padscript = null;
            int changeDirection = 0;
            if (PadTouched != null)
            {
                padscript = PadTouched.GetComponent<PadComponent>();
                changeDirection = padscript.reverse;
            }

            if(rebound_pad)
            {
                velocityVectorY = -velocityVectorY;
                addForce = true;
                additionalForce = gamemodeConstants[gamemode].gravityStrength * 9.81f * -gravityOrientation;
                posMaxSpeed = Mathf.Max(gamemodeConstants[gamemode].posMaxSpeed, player_body.velocity.magnitude);
                posMaxSpeedTimer = 1;

                rebound_pad = false;
                launched = true;
                groundBuffer = 0;
                grounded = false;
                main_trail.emitting = true;

                eyeType = 1;
                spiderAnimType = 1;
                spiderAnimSpeed = 2;

                //chargeTeleportTimer = 0;
                inDash = false;
                dash_particles.Stop();
                if (dash_effect.activeSelf) { StartCoroutine(DashFlameDissipate()); }
            }
            else if (red_pad)
            {
                velocityVectorY = (jumpForce * gamemodeConstants[gamemode].redPadMultiplier) * -gravityOrientation;
                posMaxSpeed = Mathf.Max(gamemodeConstants[gamemode].posMaxSpeed, jumpForce * gamemodeConstants[gamemode].redPadMultiplier);
                posMaxSpeedTimer = 1;

                red_pad = false;
                launched = true;
                groundBuffer = 0;
                main_trail.emitting = true;
                eyeType = 1;
                spiderAnimType = 2;
                spiderAnimSpeed = 1;
                inDash = false;
                dash_particles.Stop();
                if (dash_effect.activeSelf) { StartCoroutine(DashFlameDissipate()); }
            }
            if (yellow_pad)
            {
                velocityVectorY = (jumpForce * gamemodeConstants[gamemode].yellowPadMultiplier) * -gravityOrientation;
                posMaxSpeed = Mathf.Max(gamemodeConstants[gamemode].posMaxSpeed, jumpForce * gamemodeConstants[gamemode].yellowPadMultiplier);
                posMaxSpeedTimer = 1;

                yellow_pad = false;
                launched = true;
                groundBuffer = 0;
                main_trail.emitting = true;
                eyeType = 1;
                spiderAnimType = 2;
                spiderAnimSpeed = 1;
                inDash = false;
                dash_particles.Stop();
                if (dash_effect.activeSelf) { StartCoroutine(DashFlameDissipate()); }
            }
            if (pink_pad)
            {
                velocityVectorY = (jumpForce * gamemodeConstants[gamemode].pinkPadMultiplier) * -gravityOrientation;
                posMaxSpeed = Mathf.Max(gamemodeConstants[gamemode].posMaxSpeed, jumpForce * gamemodeConstants[gamemode].pinkPadMultiplier);
                posMaxSpeedTimer = 1;

                pink_pad = false;
                launched = true;
                groundBuffer = 0;
                main_trail.emitting = true;
                eyeType = 0;
                spiderAnimType = 2;
                spiderAnimSpeed = 1;
                inDash = false;
                dash_particles.Stop();
                if (dash_effect.activeSelf) { StartCoroutine(DashFlameDissipate()); }
            }
            if (purple_pad)
            {
                RaycastHit2D groundhit, deathhit;

                bool connect = true;

                Collider2D collider = !crouch ? player_collider : crouch_collider;
                groundhit = Physics2D.BoxCast(player_body.position + (.2f * -gravityOrientation), new Vector2(Mathf.Abs(Vector2.Dot(collider.bounds.size, -gravityOrientation)) * .5f, .1f), 0f, -gravityOrientation, 30, groundLayer);
                deathhit = Physics2D.BoxCast(player_body.position + (.2f * -gravityOrientation), new Vector2(Mathf.Abs(Vector2.Dot(collider.bounds.size, -gravityOrientation)) * .5f, .1f), 0f, -gravityOrientation, 30, deathLayer);
                
                if (deathhit.collider != null && (deathhit.distance <= groundhit.distance || groundhit.distance == 0))
                {
                    velocityVectorY = Vector2.zero;
                    spider_trail.emitting = true;
                    SpOrTr.Activate(spider_trail, gameObject);
                    //playGravityParticles();
                    player_body.position = (player_body.position + ((deathhit.distance - (mini ? 0f : .1f)) * -gravityOrientation));
                    gravityDirection = (gravityDirection + 2) % 4;
                    normalBaseOrientation = !normalBaseOrientation;
                    changeGravityDirection();
                    playGravityParticles(gravityDirection);
                }
                else if (groundhit.collider != null)
                {
                    velocityVectorY = Vector2.zero;
                    spider_trail.emitting = true;
                    SpOrTr.Activate(spider_trail, gameObject);
                    //playGravityParticles();
                    player_body.position = (player_body.position + ((groundhit.distance - (mini ? .1f : .3f)) * -gravityOrientation));
                    gravityDirection = (gravityDirection + 2) % 4;
                    normalBaseOrientation = !normalBaseOrientation;
                    changeGravityDirection();
                    playGravityParticles(gravityDirection);

                    if(gamemode == Gamemode.spider || (gamemode == Gamemode.auto_spider && reverseDir == 0))
                    {
                        if (moveX == 0)
                        {
                            iconParent.localScale = new Vector3(-iconParent.localScale.x, Mathf.Abs(iconParent.localScale.y), 1);
                        }
                    }
                }
                else
                {
                    //spider_trail.emitting = true;
                    connect = false;
                }

                pulse_trigger_p1.Enter();
                pulse_trigger_p2.Enter();

                purple_pad = false;
                main_trail.emitting = true;
            }
            if (blue_pad)
            {
                velocityVectorY = (jumpForce * gamemodeConstants[gamemode].bluePadMultiplier) * -gravityOrientation;
                posMaxSpeed = Mathf.Max(gamemodeConstants[gamemode].posMaxSpeed, jumpForce * gamemodeConstants[gamemode].bluePadMultiplier);
                posMaxSpeedTimer = 1;

                gravityDirection = (gravityDirection + 2) % 4;
                normalBaseOrientation = !normalBaseOrientation;
                changeGravityDirection();
                playGravityParticles(gravityDirection);

                blue_pad = false;
                launched = false;
                groundBuffer = 0;
                main_trail.emitting = true;
                eyeType = 2;
                spiderAnimType = 2;
                spiderAnimSpeed = 1;
                inDash = false;
                dash_particles.Stop();
                if (dash_effect.activeSelf) { StartCoroutine(DashFlameDissipate()); }
            }
            if (green_pad)
            {
                gravityDirection = (gravityDirection + 2) % 4;
                normalBaseOrientation = !normalBaseOrientation;
                changeGravityDirection();
                playGravityParticles(gravityDirection);
                velocityVectorY = (jumpForce * gamemodeConstants[gamemode].greenPadMultiplier) * -gravityOrientation;
                posMaxSpeed = Mathf.Max(gamemodeConstants[gamemode].posMaxSpeed, jumpForce * gamemodeConstants[gamemode].greenPadMultiplier);
                posMaxSpeedTimer = 1;

                green_pad = false;
                launched = true;
                groundBuffer = 0;
                main_trail.emitting = true;
                eyeType = 2;
                spiderAnimType = 2;
                spiderAnimSpeed = 1;
                inDash = false;
                dash_particles.Stop();
                if (dash_effect.activeSelf) { StartCoroutine(DashFlameDissipate()); }
            }
            if (black_pad)
            {
                velocityVectorY = (jumpForce * -gamemodeConstants[gamemode].blackPadMultiplier) * -gravityOrientation;
                posMaxSpeed = Mathf.Max(gamemodeConstants[gamemode].posMaxSpeed, jumpForce * gamemodeConstants[gamemode].blackPadMultiplier);
                posMaxSpeedTimer = 1;

                black_pad = false;
                launched = false;
                main_trail.emitting = true;
                eyeType = 1;
                spiderAnimType = 2;
                spiderAnimSpeed = 2;
                inDash = false;
                dash_particles.Stop();
                if (dash_effect.activeSelf) { StartCoroutine(DashFlameDissipate()); }
            }

            bool shrink_wave_trail = false;
            switch (changeDirection)
            {
                case -1:
                    if (reverseDir == 0)
                    {
                        firstFramesBuffer = FRAMEBUFFERTIME;
                        shrink_wave_trail = true;
                    }
                    reverseDir = 0; break;
                case 1:
                    if (reverseDir == -1)
                    {
                        firstFramesBuffer = FRAMEBUFFERTIME;
                        shrink_wave_trail = true;
                    }
                    reverseDir = 1; break;
                case 2:
                    if (reverseDir == 1)
                    {
                        firstFramesBuffer = FRAMEBUFFERTIME;
                        shrink_wave_trail = true;
                    }
                    reverseDir = -1; break;
                case 3:
                    shrink_wave_trail = true;
                    firstFramesBuffer = FRAMEBUFFERTIME;
                    reverseDir = (reverseDir == 1 ? -1 : 1); break;
            }

            if (shrink_wave_trail && gamemode == Gamemode.auto_wave)
            {
                auto_wave_trail.GetComponent<TrailRenderer>().Clear();
                auto_wave_trail.transform.GetChild(0).GetComponent<TrailRenderer>().Clear();
            }

            if (previousDirection != forwardOrientation * reverseDir)
            {
                Vector2 newDirection = forwardOrientation * reverseDir;
                float Dot = Vector2.Dot(newDirection, previousDirection);
                float inDirection = Vector2.Dot(PadTouched.transform.position - transform.position, newDirection);

                if (inDirection != 0)
                {
                    timeManager.setScale(1, (inDirection > 0 ? 1.05f : .95f));
                    timeManager.setFade(1, 1, Mathf.Abs(inDirection));
                }

                previousDirection = newDirection;
            }

            pad = false;
        }
    }

    private void Extra()
    {
        if ((gamemode == Gamemode.spring || gamemode == Gamemode.auto_spring) && grounded)
        {
            velocityVectorY = (gamemodeConstants[gamemode].jumpForce * (springDown ? 1.25f : 1)) * -gravityOrientation;
            posMaxSpeed = Mathf.Max(gamemodeConstants[gamemode].posMaxSpeed, gamemodeConstants[gamemode].jumpForce);
            posMaxSpeedTimer = 1;
            springDown = false;
            jump_orb = false;
            jump_air = false;
        }
    }

    private void Animate()
    {
        //if (touchingGround) eyeType = 0;
        if (grounded)
        {
            int gravDir = gravityDirection % 2 == 0 ? gravityDirection : (gravityDirection + 2) % 4;
            float rotationZ = Mathf.Abs((iconParent.rotation.eulerAngles.z - (gravDir * 90)) / 90);
            if ((rotationZ >= 0f && rotationZ < .5f) || (rotationZ >= 3.5 && rotationZ < 4f)) { eyeType = 0; }
            else if (rotationZ >= .5 && rotationZ < 1.5f) { eyeType = 2; }
            else if (rotationZ >= 1.5 && rotationZ < 2.5f) { if (eyeType != 2 && eyeType != 3) { eyeType = Random.Range(2, 4); } }
            else if (rotationZ >= 2.5 && rotationZ < 3.5f) { eyeType = 3; }
        }

        eyes[0].SetActive(eyeType == 0);
        eyes[1].SetActive(eyeType == 1);
        eyes[2].SetActive(eyeType == 2);
        eyes[3].SetActive(eyeType == 3);

        if(!gamemodeConstants[gamemode].auto)
        {
            float rev = normalBaseOrientation ? 1 : -1;
            eyesParent.transform.localPosition = Vector3.MoveTowards(eyesParent.transform.localPosition, new Vector3(rev * (moveX / 800), (velocityComponentY / 400), 0), Time.fixedDeltaTime);            
        }
        else
        {
            //Debug.Log(velocityVectorX + "  " + velocityVectorY);
            eyesParent.transform.position = Vector3.MoveTowards(eyesParent.transform.position, eyesParent.transform.parent.position + ((Vector3)velocityVectorX * (mini ? .44f : 1) ) / (Mathf.Clamp(velocityVectorX.x, 10, 15) * 15) + ((Vector3)velocityVectorY * (mini ? .44f : 1)) / (Mathf.Clamp(velocityVectorY.y, 10, 15) * 15), Time.fixedDeltaTime);
        }

        if(gamemode == Gamemode.spider && grounded && !inDash)
        {
            if (moveX == 0)
            {
                Spider_Anim.ResetTrigger("curl");
                Spider_Anim.ResetTrigger("jump");
                Spider_Anim.ResetTrigger("run");
                Spider_Anim.SetTrigger("stop");
                Spider_Anim.speed = 1;
            }
            else
            {
                Spider_Anim.ResetTrigger("curl");
                Spider_Anim.ResetTrigger("jump");
                Spider_Anim.ResetTrigger("stop");
                Spider_Anim.SetTrigger("run");
                Spider_Anim.speed = (Mathf.Abs(moveX) / 50) * (mini ? 1.5f : 1);
            }
        }
        else if (gamemode == Gamemode.auto_spider && grounded && !inDash)
        {
            float vX = Vector2.Dot(player_body.velocity, forwardOrientation);
            if (vX == 0)
            {
                Spider_Anim.ResetTrigger("curl");
                Spider_Anim.ResetTrigger("jump");
                Spider_Anim.ResetTrigger("run");
                Spider_Anim.SetTrigger("stop");
                Spider_Anim.speed = 1;
            }
            else
            {
                Spider_Anim.ResetTrigger("curl");
                Spider_Anim.ResetTrigger("jump");
                Spider_Anim.ResetTrigger("stop");
                Spider_Anim.SetTrigger("run");
                Spider_Anim.speed = (SPEEDS[speed] / 50) * (mini ? 1.5f : 1);
            }
        }

        if((gamemode == Gamemode.spider || gamemode == Gamemode.auto_spider) && !grounded)
        {
            SPT = spiderAnimType;
            switch (spiderAnimType)
            {
                case 0:
                    Spider_Anim.ResetTrigger("run");
                    Spider_Anim.ResetTrigger("jump");
                    Spider_Anim.ResetTrigger("curl");
                    Spider_Anim.SetTrigger("stop");
                    spiderAnimType = -1;
                    break;

                case 1:
                    Spider_Anim.ResetTrigger("run");
                    Spider_Anim.ResetTrigger("curl");
                    Spider_Anim.ResetTrigger("stop");
                    Spider_Anim.SetTrigger("jump");
                    Spider_Anim.Play("jump", -1, 0f);
                    spiderAnimType = -1;
                    break;

                case 2:
                    Spider_Anim.ResetTrigger("run");
                    Spider_Anim.ResetTrigger("jump");
                    Spider_Anim.ResetTrigger("stop");
                    Spider_Anim.SetTrigger("curl");
                    spiderAnimType = -1;
                    break;

                default:
                    break;
            }

            Spider_Anim.speed = spiderAnimSpeed;
        }
    }
    int SPT;

    private void Forces()
    {
        if (Vector2.Dot(velocityVectorY, -gravityOrientation) < -gamemodeConstants[gamemode].maxSpeed) { velocityVectorY = -gamemodeConstants[gamemode].maxSpeed * -gravityOrientation; }
        if (Vector2.Dot(velocityVectorY, -gravityOrientation) > posMaxSpeed) { velocityVectorY = posMaxSpeed * -gravityOrientation; }
        if (jump_hold && gamemodeConstants[gamemode].auto && !dashDisable) { velocityVectorX = reverseDir * SPEEDS[speed] * Time.fixedDeltaTime * 10f * forwardOrientation; }

        player_body.velocity = velocityVectorX + velocityVectorY + movingObjectVelocity;

        if (addForce)
        {
            player_body.AddForce(additionalForce);
            addForce = false;
        }

        if (!inDash)
        {
            player_body.AddForce((9.81f * gravityOrientation * gamemodeConstants[gamemode].gravityStrength));
        }
    }


    private void Die()
    {
        sdshape.settings.fillColor = Color.clear;
        sdtimer = 0;
        deathCount++;
        if (restartMusic) { bgmusic.Stop(); }

        timeManager.setScale(2, 1);
        timeManager.setScale(1, 1);

        able = false;
        //if (restartMusic) { bgmusic.Stop(); }
        inDash = false;
        dash_particles.Stop();
        if (dash_effect.activeSelf) { StartCoroutine(DashFlameDissipate()); }

        player_collider.enabled = false;
        crouch_collider.enabled = false;

        iconParent.gameObject.SetActive(false);
        main_trail.emitting = false;
        death_particles.Play();
        death_sfx.PlayOneShot(death_sfx.clip, gamemanager.sfx_volume);

        player_body.velocity = Vector2.zero;

        if (OnDeath != null) { OnDeath(); }

        StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(1);

        ResetTriggers();

        Vector3 positionDelta = respawn.position - transform.position;
        transform.position = respawn.position;
        gravityDirection = respawn.gravityDirection;
        speed = respawn.speed;
        reverseDir = respawn.reverseDir;
        mini = respawn.mini;

        gamemanager.getActiveCamera().GetCinemachineComponent<CinemachineFramingTransposer>().OnTargetObjectWarped(transform, positionDelta);

        ChangeSize();
        resetBaseDirection();
        changeGravityDirection();

        Cube_Anim.ResetTrigger("Crouch");        
        Cube_Anim.ResetTrigger("Squash");
        Cube_Anim.ResetTrigger("DeepSquash");
        Cube_Anim.ResetTrigger("Stretch");
        Cube_Anim.SetTrigger("Default");
        icons.localPosition = Vector3.zero;
        iconParent.localEulerAngles = new Vector3(0, 0, gravityDirection * -90);
        iconParent.gameObject.SetActive(true);

        player_collider.enabled = true;

        moveX = 0; moveY = 0;
        velocityVectorX = Vector2.zero;
        velocityVectorY = Vector2.zero;

        if(!gamemodeConstants[gamemode].auto)
        {
            player_body.velocity = Vector2.zero;
        }
        else
        {
            player_body.velocity = reverseDir * SPEEDS[speed] * Time.fixedDeltaTime * 10f * forwardOrientation;
        }

        auto_wave_trail.GetComponent<TrailRenderer>().Clear();
        auto_wave_trail.transform.GetChild(0).GetComponent<TrailRenderer>().Clear();

        resetVars();

        if (restartMusic)
        {
            gamemanager.setToNewBGMusic();
            bgmusic.Play();
        }

        if (OnRespawn != null) { OnRespawn(); }

        dead = false;
        able = true;
    }

    void resetVars()
    {
        jump = false;
        jump_from_ground = false;
        jump_orb = false;
        jump_orb = false;
        jump_air = false;
        jump_hold = false;
        jump_released = false;
        falling = false;

        goingUp = false;

        yellow_orb = false;
        blue_orb = false;
        red_orb = false;
        pink_orb = false;
        green_orb = false;
        black_orb = false;
        purple_orb = false;
        purple_dash = false;
        dash_orb = false;
        black_dash = false;
        inDash = false;
        cw_orb = false;
        ccw_orb = false;
        tele_orb = false;
        trigger_orb = false;
        dashDisable = false;
        super_orb = false;

        yellow_pad = false;
        blue_pad = false;
        red_pad = false;
        pink_pad = false;
        green_pad = false;
        black_pad = false;
        purple_pad = false;
        pad = false;

        gravportal_flip = false;
        gravportal_down = false;
        gravportal_up = false;
        teleportalA = false;
        teleportal_charged = false;

        coyoteTimer = COYOTETIME * 2;
        crouch = false;
        isCrouched = false;
        previousIsCrouched = false;
        crouchedTimer = 0;

        firstFramesBuffer = FRAMEBUFFERTIME;
        groundBuffer = 0;

        prevGravityDirection = gravityDirection;
        prev_grounded = false;
        prev_velocityComponentY = 0;
        prevMoveX = 0;
    }

    #region Trigger Enter
    protected void OnTriggerStay2D(Collider2D collision)
    {
        if (!enabled) { return; }
        switch(collision.gameObject.tag)
        {
            case "TeleOrb":
                tele_orb = true;
                OrbTouched = collision.gameObject;
                //tele_orb_translate = collision.gameObject.GetComponent<OrbComponent>().getTeleport().position - collision.gameObject.GetComponent<OrbComponent>().transform.position;
                tele_orb_translate = collision.gameObject.GetComponent<OrbComponent>().GetTeleportDisplacement();

                Vector2 playerToOrb = (Vector2)(transform.position - collision.gameObject.GetComponent<OrbComponent>().transform.position);
                tele_orb_translate -= playerToOrb.SetXY(
                        collision.gameObject.GetComponent<OrbComponent>().centerY ? 0 : playerToOrb.x,
                        collision.gameObject.GetComponent<OrbComponent>().centerX ? 0 : playerToOrb.y);
                break;
            case "SuperOrb":
                super_orb = true;
                OrbTouched = collision.gameObject;
                break;
            case "RedOrb":
                red_orb = true;
                OrbTouched = collision.gameObject;
                break;
            case "YellowOrb":
                yellow_orb = true;
                OrbTouched = collision.gameObject;
                break;
            case "PinkOrb":
                pink_orb = true;
                OrbTouched = collision.gameObject;
                break;
            case "GreenOrb":
                green_orb = true;
                OrbTouched = collision.gameObject;
                break;
            case "BlackOrb":
                black_orb = true;
                OrbTouched = collision.gameObject;
                break;
            case "PlayerSettings":
                PlayerSettingsTrigger pst = collision.GetComponent<PlayerSettingsTrigger>();
                if (pst.cancelJump)
                {
                    //jump = false;
                    //jump_hold = false;
                    //jump_air = false;
                    //jump_orb = false;
                    cancel_jump = true;
                }
                else if(pst.disableSelfDestruct)
                {
                    sdtimer = 0;
                }
                break;
        }
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (!enabled) { return; }
        switch(collision.gameObject.tag)
        {
            case "Controller":
                gamemode = (Gamemode)collision.GetComponent<ControllerTrigger>().mode;
                setGamemode((int)gamemode);

                if (gamemodeConstants[gamemode].auto)
                {
                    switch (collision.GetComponent<ControllerTrigger>().reverse)
                    {
                        case -1: if (reverseDir == 0) { firstFramesBuffer = FRAMEBUFFERTIME; } reverseDir = 0; break;
                        case 1: if (reverseDir == -1) { firstFramesBuffer = FRAMEBUFFERTIME; } reverseDir = 1; break;
                        case 2: if (reverseDir == 1) { firstFramesBuffer = FRAMEBUFFERTIME; } reverseDir = -1; break;
                        case 3: firstFramesBuffer = FRAMEBUFFERTIME; reverseDir = (reverseDir == 1 ? -1 : 1); break;
                    }
                }
                break;
            case "TriggerOrb":
                trigger_orb = true;
                OrbTouched = collision.gameObject;
                break;
            case "PortalGravityC":
                gravportal_flip = true;
                PortalTouched = collision.gameObject;
                break;
            case "PortalGravityN":
                gravportal_down = true;
                PortalTouched = collision.gameObject;
                break;
            case "PortalGravity":
                gravportal_up = true;
                PortalTouched = collision.gameObject;
                break;
            case "TeleportA":
                teleportalA = true;

                Transform t = collision.gameObject.transform;

                Transform tb = t;
                foreach (Transform tr in t)
                {
                    if (tr.tag == "TeleportB")
                    {
                        tb = tr.GetComponent<Transform>();
                        break;
                    }
                }

                teleBDelta = tb.position - t.position;

                float distAX = transform.position.x - t.position.x;
                float offsetBX = distAX * ((Mathf.Abs((tb.rotation.eulerAngles.z - t.rotation.eulerAngles.z)) % 180) / 90f);
                teleBDelta.x -= offsetBX;

                float distAY = transform.position.y - t.position.y;
                float offsetBY = distAY * ((Mathf.Abs((tb.rotation.eulerAngles.z - t.rotation.eulerAngles.z)) % 180) / 90f);
                teleBDelta.y -= offsetBY;

                PortalTouched = collision.gameObject;
                break;
            case "ChargedTeleportA":
                teleportal_charged = true;

                Vector3 inputVelocity = player_body.velocity;

                t = collision.gameObject.transform;

                tb = t;
                foreach (Transform tr in t)
                {
                    if (tr.tag == "ChargedTeleportB")
                    {
                        tb = tr.GetComponent<Transform>();
                        break;
                    }
                }

                teleBChargedDelta = tb.position - t.position;

                distAX = transform.position.x - t.position.x;
                offsetBX = distAX * ((Mathf.Abs((tb.rotation.eulerAngles.z - t.rotation.eulerAngles.z)) % 180) / 90f);
                teleBChargedDelta.x -= offsetBX;

                distAY = transform.position.y - t.position.y;
                offsetBY = distAY * ((Mathf.Abs((tb.rotation.eulerAngles.z - t.rotation.eulerAngles.z)) % 180) / 90f);
                teleBChargedDelta.y -= offsetBY;

                float inDirection = Vector3.Dot(inputVelocity, t.right);
                float crossDirection = Vector3.Dot(inputVelocity, Vector2.Perpendicular(((Vector2)(-t.right))));//inputVelocity.magnitude - inDirection;
                float crossScale = Mathf.Abs(Vector3.Dot(Vector2.Perpendicular(((Vector2)(-t.right))), Vector2.Perpendicular(((Vector2)(-tb.right)))));
                chargedTeleportVelocity = ((Vector2)(-tb.right) * inDirection) + Vector2.Perpendicular(((Vector2)(-t.right))) * crossDirection * crossScale;

                chargeTeleportTimer = CHARGEDTIMER;

                PortalTouched = collision.gameObject;

                if (inDash)
                {
                    dashDirection = -tb.right;
                    dash_parent.transform.eulerAngles = new Vector3(0, 0, tb.rotation.eulerAngles.z + 180);
                }

                break;
            case "ChargedTeleportC":
                if (ChargedTeleportCTouched != null && collision.gameObject == ChargedTeleportCTouched && chargeTeleportTimerC > 0) return;
                teleportal_charged = true;

                inputVelocity = player_body.velocity;

                t = collision.gameObject.transform;

                tb = t;
                if (t.parent != null && t.parent.tag == "ChargedTeleportC")
                {
                    tb = t.parent.GetComponent<Transform>();
                }
                else
                {
                    foreach (Transform tr in t)
                    {
                        if (tr.tag == "ChargedTeleportC")
                        {
                            tb = tr.GetComponent<Transform>();
                            break;
                        }
                    }
                }

                teleBChargedDelta = tb.position - t.position;

                distAX = transform.position.x - t.position.x;
                offsetBX = distAX * ((Mathf.Abs((tb.rotation.eulerAngles.z - t.rotation.eulerAngles.z)) % 180) / 90f);
                teleBChargedDelta.x -= offsetBX;

                distAY = transform.position.y - t.position.y;
                offsetBY = distAY * ((Mathf.Abs((tb.rotation.eulerAngles.z - t.rotation.eulerAngles.z)) % 180) / 90f);
                teleBChargedDelta.y -= offsetBY;

                inDirection = Vector3.Dot(inputVelocity, t.right);
                crossDirection = Vector3.Dot(inputVelocity, Vector2.Perpendicular(((Vector2)(-t.right))));//inputVelocity.magnitude - inDirection;
                crossScale = Mathf.Abs(Vector3.Dot(Vector2.Perpendicular(((Vector2)(-t.right))), Vector2.Perpendicular(((Vector2)(-tb.right)))));
                chargedTeleportVelocity = ((Vector2)(-tb.right) * inDirection) + Vector2.Perpendicular(((Vector2)(-t.right))) * crossDirection * crossScale;

                chargeTeleportTimer = CHARGEDTIMER;
                chargeTeleportTimerC = .1f;
                ChargedTeleportCTouched = tb.gameObject;

                PortalTouched = collision.gameObject;

                if (inDash)
                {
                    dashDirection = -tb.right;
                    dash_parent.transform.eulerAngles = new Vector3(0, 0, tb.rotation.eulerAngles.z + 180);
                }

                break;
            case "PurpleDash":
                purple_dash = true;
                OrbTouched = collision.gameObject;
                break;
            case "Dash":
                dash_orb = true;
                OrbTouched = collision.gameObject;
                break;
            case "BlackDash":
                black_dash = true;
                OrbTouched = collision.gameObject;
                break;
            case "ReboundOrb":
                rebound_orb = true;
                OrbTouched = collision.gameObject;
                break;
            case "PurpleOrb":
                purple_orb = true;
                OrbTouched = collision.gameObject;
                break;
            case "BlueOrb":
                blue_orb = true;
                OrbTouched = collision.gameObject;
                break;
            case "CWOrb":
                cw_orb = true;
                OrbTouched = collision.gameObject;
                break;
            case "CCWOrb":
                ccw_orb = true;
                OrbTouched = collision.gameObject;
                break;
            case "BlackPad":
                black_pad = true;
                PadTouched = collision.gameObject;
                pad = true;
                break;
            case "ReboundPad":
                rebound_pad = true;
                PadTouched = collision.gameObject;
                pad = true;
                break;
            case "PurplePad":
                purple_pad = true;
                PadTouched = collision.gameObject;
                pad = true;
                break;
            case "BluePad":
                blue_pad = true;
                PadTouched = collision.gameObject;
                pad = true;
                break;
            case "GreenPad":
                green_pad = true;
                PadTouched = collision.gameObject;
                pad = true;
                break;
            case "RedPad":
                red_pad = true;
                PadTouched = collision.gameObject;
                pad = true;
                break;
            case "YellowPad":
                yellow_pad = true;
                PadTouched = collision.gameObject;
                pad = true;
                break;
            case "PinkPad":
                pink_pad = true;
                PadTouched = collision.gameObject;
                pad = true;
                break;
            case "Speed0x":
                if (speed != 0) { playSpeedParticles(0); }
                speed = 0;
                break;
            case "Speed1x":
                if (speed != 1) { playSpeedParticles(1); }
                speed = 1;
                break;
            case "Speed2x":
                if (speed != 2) { playSpeedParticles(2); }
                speed = 2;
                break;
            case "Speed3x":
                if (speed != 3) { playSpeedParticles(3); }
                speed = 3;
                break;
            case "Speed4x":
                if (speed != 4) { playSpeedParticles(4); }
                speed = 4;
                break;
            case "Mini":
                mini = true;
                ChangeSize();
                break;
            case "Mega":
                mini = false;
                ChangeSize();
                break;
            case "PlayerSettings":
                PlayerSettingsTrigger pst = collision.GetComponent<PlayerSettingsTrigger>();
                if(pst.gravityDirection != -1 && !(gravityDirection == pst.gravityDirection))
                {
                    if (pst.gravityDirection == (gravityDirection + 2) % 4) { normalBaseOrientation = !normalBaseOrientation; }
                    gravityDirection = pst.gravityDirection;
                    changeGravityDirection();
                    playGravityParticles(gravityDirection);
                }
                if (gamemodeConstants[gamemode].auto && pst.reverseDirection != 0 && !(pst.reverseDirection == reverseDir))
                {
                    switch (pst.reverseDirection)
                    {
                        case -1: if (reverseDir == 0) { firstFramesBuffer = FRAMEBUFFERTIME; } reverseDir = 0; break;
                        case 1: if (reverseDir == -1) { firstFramesBuffer = FRAMEBUFFERTIME; } reverseDir = 1; break;
                        case 2: if (reverseDir == 1) { firstFramesBuffer = FRAMEBUFFERTIME; } reverseDir = -1; break;
                        case 3: firstFramesBuffer = FRAMEBUFFERTIME; reverseDir = (reverseDir == 1 ? -1 : 1); break;
                    }
                }
                if(pst.stopDash)
                {
                    inDash = false;
                    dash_particles.Stop();
                    if (dash_effect.activeSelf) { StartCoroutine(DashFlameDissipate()); }
                }
                if (pst.disableTrail)
                {
                    main_trail.emitting = false;
                }
                if (pst.setPosition != null)
                {
                    Vector3 newPosition = new Vector3(pst.setPosition.position.x, pst.setPosition.position.y, transform.position.z);
                    Vector3 positionDelta = newPosition - transform.position;

                    transform.position = newPosition;
                    gamemanager.getActiveCamera().GetCinemachineComponent<CinemachineFramingTransposer>().OnTargetObjectWarped(transform, positionDelta);
                    firstFramesBuffer = FRAMEBUFFERTIME;
                }
                break;            
        }
    }

    protected void OnTriggerExit2D(Collider2D collision)
    {
        if (!enabled) { return; }
        switch(collision.gameObject.tag)
        {
            case "TriggerOrb":
                trigger_orb = false;
                break;
            case "TeleOrb":
                tele_orb = false;
                break;
            case "SuperOrb":
                super_orb = false;
                break;
            case "YellowOrb":
                yellow_orb = false;
                break;
            case "RedOrb":
                red_orb = false;
                break;
            case "PinkOrb":
                pink_orb = false;
                break;
            case "PurpleDash":
                purple_dash = false;
                break;
            case "Dash":
                dash_orb = false;
                break;
            case "BlackDash":
                black_dash = false;
                break;
            case "ReboundOrb":
                rebound_orb = false;
                break;
            case "PurpleOrb":
                purple_orb = false;
                break;
            case "BlueOrb":
                blue_orb = false;
                break;
            case "GreenOrb":
                green_orb = false;
                break;
            case "BlackOrb":
                black_orb = false;
                break;
            case "YellowPad":
                yellow_pad = false;
                break;
            case "RedPad":
                red_pad = false;
                break;
            case "BluePad":
                blue_pad = false;
                break;
            case "PinkPad":
                pink_pad = false;
                break;
            case "GreenPad":
                green_pad = false;
                break;
            case "PurplePad":
                purple_pad = false;
                break;
            case "BlackPad":
                black_pad = false;
                break;
            case "CWOrb":
                cw_orb = false;
                break;
            case "CCWOrb":
                ccw_orb = false;
                break;
            case "PlayerSettings":
                PlayerSettingsTrigger pst = collision.GetComponent<PlayerSettingsTrigger>();
                if (pst.cancelJump)
                {
                    //jump = false;
                    //jump_hold = false;
                    //jump_air = false;
                    //jump_orb = false;
                    cancel_jump = false;
                }
                break;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!enabled) { return; }
        switch (collision.gameObject.tag)
        {
            case "MovingObject":
                if (!MovingObjectVelocities.Contains(collision.gameObject.GetComponent<Rigidbody2D>()))
                {
                    MovingObjectVelocities.Add(collision.gameObject.GetComponent<Rigidbody2D>());
                }
                break;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (!enabled) { return; }
        switch (collision.gameObject.tag)
        {
            case "MovingObject":
                MovingObjectVelocities.Remove(collision.gameObject.GetComponent<Rigidbody2D>());
                break;
        }
    }
    #endregion

    public void playSpeedParticles(int s)
    {
        if (PlayerPrefs.GetInt("screen_particles") == 0) { return; }
        speed_particles[s].Play();
    }

    public void playGravityParticles(int g)
    {
        //int reverseGrav = GetOppositeGravity();
        int normalGrav = (GetOppositeGravity() + 2) % 4;
        int diff = ((g - normalGrav) + 4) % 4;
        if (PlayerPrefs.GetInt("screen_particles") == 0) { return; }
        gravity_particles[diff].Play();
    }

    public class SpiderTrail : MonoBehaviour
    {
        public void Activate(TrailRenderer trail, GameObject player)
        {
            StopAllCoroutines();
            StartCoroutine(animateTrail(trail, player));
        }

        IEnumerator animateTrail(TrailRenderer trail, GameObject player)
        {
            trail.emitting = true;

            float time = 0, T = 0.08f;
            while (time <= T)
            {
                time += Time.deltaTime;
                yield return null;
            }

            trail.emitting = false;
        }
    }

    IEnumerator DashFlameDissipate()
    {
        float time = 0;
        while (time <= 1)
        {
            if(inDash)
            {
                dash_flame_material.SetFloat("_FireBottom", 1.3f);
                yield break;
            }
            //dash_flame_material.SetFloat("_DistortionStrength", Mathf.Lerp(1, -10, time));
            dash_flame_material.SetFloat("_FireBottom", Mathf.Lerp(1.3f, -1, time));
            //dash_flame_material.SetFloat("_YStretch", Mathf.Lerp(1.03f, 0, time));
            time += Time.deltaTime * 6;
            yield return null;
        }

        dash_effect.SetActive(inDash);
    }

    public void playBGMusic()
    {
        if (!bgmusic.isPlaying)
        {
            bgmusic.Play();
        }
    }
    public void stopBGMusic()
    {
        if (bgmusic.isPlaying)
        {
            bgmusic.Stop();
        }
    }
    public void setBGMusic(AudioSource audio)
    {
        bgmusic = audio;
    }

    public void setRespawn(Vector3 pos, int grav, int sd, bool min, int rev)
    {
        respawn.position = new Vector3(pos.x, pos.y, transform.position.z);
        respawn.gravityDirection = grav;
        respawn.speed = sd;
        respawn.mini = min;
        respawn.reverseDir = rev;
    }

    public void IncrementCheckpointCount(int add)
    {
        ClearTriggers();
        checkpointCount += add;
    }

    void ClearTriggers()
    {
        ClearMoveTriggers();
        ClearRotateTriggers();
        ClearScaleTriggers();
    }

    void ResetTriggers()
    {
        ResetMoveTriggers();
        ResetRotateTriggers();
        ResetScaleTriggers();
    }

    public Quaternion getIconRotation()
    {
        return iconParent.rotation;
    }

    public Transform getIconTransform()
    {
        return iconParent;
    }

    public int getCheckpointCount()
    {
        return checkpointCount;
    }

    public int getDeathCount()
    {
        return deathCount;
    }

    public int getJumpCount()
    {
        return jumpCount;
    }

    public bool getAble()
    {
        return able;
    }

    public bool getCrouched()
    {
        return isCrouched;
    }
    public bool getMini()
    {
        return mini;
    }

    public bool getDead()
    {
        return dead;
    }

    public bool getGrounded()
    {
        return grounded;
    }

    public float getCopterAnimSpeed()
    {
        return Copter_Anim.speed;
    }

    public float getSpiderAnimSpeed()
    {
        return Spider_Anim.speed;
    }

    public int getSpiderAnimType()
    {
        return SPT;
    }

    public float getVelocityComponentY()
    {
        return Vector2.Dot(player_body.velocity, -gravityOrientation);
    }

    public Vector2 getGravityOrientation()
    {
        return gravityOrientation;
    }

    public Vector2 getForwardOrientation()
    {
        return forwardOrientation;
    }

    public Gamemode getMode()
    {
        return gamemode;
    }

    public float getXVelocity()
    {
        float value = Vector2.Dot(player_body.velocity, forwardOrientation);
        return value;
    }

    public float getYVelocity()
    {
        float value = Vector2.Dot(player_body.velocity, -gravityOrientation);
        return value;
    }

    public Vector2 getForces()
    {
        Vector2 value = additionalForce + (gamemodeConstants[gamemode].gravityStrength * gravityOrientation);
        return value;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position + (.2f * (Vector3)gravityOrientation), transform.position + (.2f * (Vector3)gravityOrientation) + 0.65f * (Vector3)(-gravityOrientation));

        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(transform.position + (Vector3)(gravityOrientation*.51f), gravityDirection%2 == 0 ? new Vector2(mini ? .42f : .98f, .1f) : new Vector2(.1f, mini ? .42f : .98f));
    }
#endif
}
