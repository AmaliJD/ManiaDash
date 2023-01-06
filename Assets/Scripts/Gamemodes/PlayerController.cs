using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class PlayerController : MonoBehaviour
{
    // The different 'protected' variables are separated based on how they functioned in their original scripts

    // SHARED "PRIVATE" VARIABLES
    protected static GameObject eyes;
    protected static GameObject icon;

    protected static ParticleSystem grav_particles;
    protected static ParticleSystem antigrav_particles;
    protected static ParticleSystem speed0_particles;
    protected static ParticleSystem speed1_particles;
    protected static ParticleSystem speed2_particles;
    protected static ParticleSystem speed3_particles;
    protected static ParticleSystem speed4_particles;

    protected static Rigidbody2D player_body, player_rot;
    protected Animator Cube_Anim;

    protected AudioSource bgmusic;

    protected GameManager gamemanager;

    protected Vector3 v_Velocity; // ref Velocity

    protected RaycastHit2D headHit;

    protected SpiderOrbTrail SpOrTr;

    protected const float MAIN_GRAVITY_CUBE = 9.81f, MINI_GRAVITY_CUBE = 9.81f,
                            MAIN_GRAVITY_SHIP = 2.7f, MINI_GRAVITY_SHIP = 2.7f,
                            MAIN_GRAVITY_UFO = 9.81f, MINI_GRAVITY_UFO = 9.81f,
                            MAIN_GRAVITY_WAVE = 9.81f, MINI_GRAVITY_WAVE = 9.81f,
                            MAIN_GRAVITY_BALL = 9.81f, MINI_GRAVITY_BALL = 9.81f,
                            MAIN_GRAVITY_SPIDER = 9.81f, MINI_GRAVITY_SPIDER = 9.81f,
                            MAIN_GRAVITY_COPTER = 9.81f, MINI_GRAVITY_COPER = 9.81f;

    protected const float CROUCH_JUMP_OPENING = 0f, CROUCH_JUMP_WINDOW = .1f; //.22 .1
    protected const float CROUCH_JUMP_STRENGTH_MULTIPLIER = .99f, CROUCH_JUMP_SPEED = 15f; //1.05 *1.4

    // SHARED STATIC VARIABLES
    static public InputActions input;
    static public bool prevJumpNotReleased,prevCrouchNotReleased;
    static public bool jump = false, jump_ground = false, reversed = false, downjump = false, released = false;
    static public bool grounded = false, prev_grounded = false, fromGround = false, checkGrounded = true;
    static public bool dead = false, check_death = false, able = true;
    static public float disableJumpTimer = 0;

    static public float prev_z_rotation;
    static public float gravityMultiplier = 1f;

    static public float prev_velocity = 0;
    static public float negate = 1, regate = 1;

    static public bool respawn_rev = false, respawn_mini = false;
    static public bool restartmusic = false;

    static public bool crouch = false, crouched = true, holdjump = false, enablecrouchjump = false;
    static public float crouchtimer = 0, groundBufferTime = 0, chargeTeleportTimer = 0, chargeTeleportTimerC = 0;
    static public float crouchJumpTimer = 0, offCrouchTimer = 0;
    static public bool crouchJump, crouchJumpUsed, crouchReleased = true;
    static public int crouchJumpCounter;

    static public bool facingright = true, upright = true, mini = false, goingUp;

    static public GameObject OrbTouched = null, ChargedTeleportCTouched = null;
    static public bool yellow = false, blue = false, red = false, pink = false, green = false, black = false, purple = false, teleorb = false, triggerorb = false;
    static public bool yellow_j = false, red_j = false, blue_j = false, pink_j = false, green_j = false, black_j = false, purple_j = false, teleorb_j = false, triggerorb_j = false;
    static public bool yellow_p = false, blue_p = false, red_p = false, pink_p = false, green_p;
    static public bool grav = false, gravN = false, gravC = false, teleA = false, chargedTeleA = false;
    static public Vector3 teleB, chargedTeleB, respawn, teleOrb_translate;
    static public Vector2 chargedTeleportVelocity = Vector2.zero;

    static public bool freeFallMode = false;

    static public List<Rigidbody2D> MovingObjectVelocities;
    static public Vector2 movingVelocity;

    static public float speed, speed0 = 40f, speed1 = 55f /*50*/, speed2 = 75f, speed3 = 90f, speed4 = 110f, respawn_speed;

    public void resetStaticVariables()
    {
        jump = false;
        jump_ground = false;
        reversed = false;
        downjump = false;
        released = false;
        disableJumpTimer = 0;

        grounded = false;
        prev_grounded = false;
        fromGround = false;
        checkGrounded = true;

        dead = false;
        check_death = false;
        able = true;

        respawn_rev = false;
        respawn_mini = false;
        restartmusic = false;

        crouch = false;
        crouched = true;
        crouchtimer = 0;
        groundBufferTime = 0;
        crouchJumpTimer = 0;
        offCrouchTimer = 0;
        crouchJump = false;
        crouchJumpUsed = false;
        crouchReleased = true;
        crouchJumpCounter = 0;
        chargeTeleportTimer = 0;
        chargeTeleportTimerC = 0;

        gravityMultiplier = 1f;

        facingright = true;
        upright = true;
        mini = false;

        restartmusic = false;
        reversed = false;

        yellow = false; blue = false; red = false; pink = false; green = false; black = false; purple = false;
        yellow_j = false; red_j = false; blue_j = false; pink_j = false; green_j = false; black_j = false; purple_j = false;
        yellow_p = false; blue_p = false; red_p = false; pink_p = false; green_p = false;
        grav = false; gravN = false; teleA = false;

        freeFallMode = false;

        chargedTeleportVelocity = Vector2.zero;

        speed0 = 40f; speed1 = 55f; speed2 = 75f; speed3 = 90f; speed4 = 110f;

        prevJumpNotReleased = false;
        prevCrouchNotReleased = false;
    }

    // PROTECTED "PUBLIC" VARIABLES
    [SerializeField] protected GameObject player_renderer;

    [SerializeField] protected LayerMask groundLayer;
    [SerializeField] protected LayerMask deathLayer;

    [SerializeField] protected ParticleSystem grounded_particles;
    [SerializeField] protected ParticleSystem ground_impact_particles;
    /*[SerializeField] protected ParticleSystem grav_particles;
    [SerializeField] protected ParticleSystem antigrav_particles;
    [SerializeField] protected ParticleSystem speed0_particles;
    [SerializeField] protected ParticleSystem speed1_particles;
    [SerializeField] protected ParticleSystem speed2_particles;
    [SerializeField] protected ParticleSystem speed3_particles;
    [SerializeField] protected ParticleSystem speed4_particles;*/
    [SerializeField] protected ParticleSystem death_particles;
    [SerializeField] protected AudioSource death_sfx;
    [SerializeField] protected TrailRenderer spiderorb_trail;
    [SerializeField] protected PulseTrigger pulse_trigger_p1, pulse_trigger_p2;

    // SHARED METHODS
    private void Awake()
    {
        if (input == null)
        {
            input = new InputActions();
            //input.Player.SetCallbacks(this);
        }
        input.Player.Enable();

        if (spiderorb_trail.GetComponent<SpiderOrbTrail>() == null)
        {
            spiderorb_trail.gameObject.AddComponent<SpiderOrbTrail>();
        }
        SpOrTr = spiderorb_trail.GetComponent<SpiderOrbTrail>();

        MovingObjectVelocities = new List<Rigidbody2D>();
        movingVelocity = Vector2.zero;

        gamemanager = FindObjectOfType<GameManager>();

        player_body = GetComponent<Rigidbody2D>();
        player_rot = transform.GetChild(0).GetComponent<Rigidbody2D>();
        Cube_Anim = player_renderer.transform.GetComponent<Animator>();

        eyes = GameObject.Find("Icon_Eyes");
        eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
        eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
        eyes.transform.Find("Eyes_Wide").gameObject.SetActive(false);
        eyes.transform.Find("Eyes_Normal").gameObject.SetActive(true);

        speed0_particles = GameObject.Find("Speed 0 Particles").GetComponent<ParticleSystem>();
        speed1_particles = GameObject.Find("Speed 1 Particles").GetComponent<ParticleSystem>();
        speed2_particles = GameObject.Find("Speed 2 Particles").GetComponent<ParticleSystem>();
        speed3_particles = GameObject.Find("Speed 3 Particles").GetComponent<ParticleSystem>();
        speed4_particles = GameObject.Find("Speed 4 Particles").GetComponent<ParticleSystem>();
        grav_particles = GameObject.Find("Normal Gravity Particles").GetComponent<ParticleSystem>();
        antigrav_particles = GameObject.Find("Reverse Gravity Particles").GetComponent<ParticleSystem>();

        setRespawn(transform.position, reversed, mini);
        setRepawnSpeed(1);

        v_Velocity = Vector3.zero;
        speed = speed1;
        
        Awake2();
    }

    // DO METHODS
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

    public void forceRespawn()
    {
        transform.position = respawn;
    }

    public void TurnOffEverything()
    {
        grounded_particles.Stop();
        ground_impact_particles.Stop();

        Cube_Anim.ResetTrigger("Crouch");
        Cube_Anim.ResetTrigger("Squash");
        Cube_Anim.ResetTrigger("Stretch");
        Cube_Anim.SetTrigger("Default");
    }

    public void playSpeedParticles(int s)
    {
        if (PlayerPrefs.GetInt("screen_particles") == 0) { return; }
        switch (s)
        {
            case 0: speed0_particles.Play(); break;
            case 1: speed1_particles.Play(); break;
            case 2: speed2_particles.Play(); break;
            case 3: speed3_particles.Play(); break;
            case 4: speed4_particles.Play(); break;
        }
    }

    public void playGravityParticles()
    {
        if (PlayerPrefs.GetInt("screen_particles") == 0) { return; }
        switch (reversed)
        {
            case true: grav_particles.Play(); break;
            case false: antigrav_particles.Play(); break;
        }
    }

    public void Interpolate(short rot, short lin)
    {
        //rot = 0; lin = 0;

        if(transform.rotation.z != prev_z_rotation)
        {
            switch(rot)
            {
                case 0:
                    if (player_body.interpolation != RigidbodyInterpolation2D.None)
                        player_body.interpolation = RigidbodyInterpolation2D.None;

                    //Debug.Log("None");

                    break;

                case 1:
                    if (player_body.interpolation != RigidbodyInterpolation2D.Extrapolate)
                        player_body.interpolation = RigidbodyInterpolation2D.Extrapolate;

                    //Debug.Log("Extrapolate");

                    break;

                default:
                    if (player_body.interpolation != RigidbodyInterpolation2D.Interpolate)
                        player_body.interpolation = RigidbodyInterpolation2D.Interpolate;

                    //Debug.Log("Interpolate");

                    break;
            }
        }
        else
        {
            switch (lin)
            {
                case 0:
                    if (player_body.interpolation != RigidbodyInterpolation2D.None)
                        player_body.interpolation = RigidbodyInterpolation2D.None;

                    //Debug.Log("None");

                    break;

                case 1:
                    if (player_body.interpolation != RigidbodyInterpolation2D.Extrapolate)
                        player_body.interpolation = RigidbodyInterpolation2D.Extrapolate;

                    //Debug.Log("Extrapolate");

                    break;

                default:
                    if (player_body.interpolation != RigidbodyInterpolation2D.Interpolate)
                        player_body.interpolation = RigidbodyInterpolation2D.Interpolate;

                    //Debug.Log("Interpolate");

                    break;
            }
        }

        prev_z_rotation = transform.rotation.z;
    }

    // SET METHODS
    public void setIcons(GameObject i)
    {
        icon = i;
    }
    public void setBGMusic(AudioSource audio)
    {
        bgmusic = audio;
    }
    public void setRestartMusicOnDeath(bool r)
    {
        restartmusic = r;
    }
    public void setAble(bool a)
    {
        able = a;
    }
    public void setMini(bool m)
    {
        mini = m;
        ChangeSize();
    }
    public void setJump(bool j, bool jb)
    {
        jump = j;
        jump_ground = jb;
    }
    public void setIconRigidbody(bool k)
    {
        player_rot.simulated = k;
    }

    public void setIconRigidbody()
    {
        player_rot.simulated = getMode() == "cube";
        player_rot.interpolation = RigidbodyInterpolation2D.Interpolate;
        if (getMode() != "cube")
        {
            player_rot.rotation = player_body.rotation;
        }
    }

    // GET METHODS
    public bool isDead()
    {
        return dead;
    }

    public bool getJump()
    {
        return jump;
    }

    public bool getMini()
    {
        return mini;
    }
    public bool getAble()
    {
        return able;
    }
    public bool getRestartMusicOnDeath()
    {
        return restartmusic;
    }

    public Vector3 getRespawn()
    {
        return respawn;
    }

    // OnCollision
    protected void OnTriggerStay2D(Collider2D collision)
    {
        if (!enabled) { return; }
        if (collision.gameObject.tag == "TeleOrb")
        {
            OrbTouched = collision.gameObject;
            teleorb = true;

            teleOrb_translate = collision.gameObject.GetComponent<OrbComponent>().getTeleport().position - collision.gameObject.GetComponent<OrbComponent>().transform.position;
            teleOrb_translate.z = 0;
        }
        if (collision.gameObject.tag == "YellowOrb")
        {
            yellow = true;
            OrbTouched = collision.gameObject;
        }
        if (collision.gameObject.tag == "PinkOrb")
        {
            pink = true;
            OrbTouched = collision.gameObject;
        }
        if (collision.gameObject.tag == "RedOrb")
        {
            red = true;
            OrbTouched = collision.gameObject;
        }
        if (collision.gameObject.tag == "GreenOrb")
        {
            green = true;
            OrbTouched = collision.gameObject;
        }
        if (collision.gameObject.tag == "BlackOrb")
        {
            black = true;
            OrbTouched = collision.gameObject;
        }
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (!enabled) { return; }
        if (collision.gameObject.tag == "TriggerOrb")
        {
            triggerorb = true;
            OrbTouched = collision.gameObject;
        }
        if (collision.gameObject.tag == "PortalGravity")
        {
            grav = true;
            if (!reversed) { playGravityParticles(); }
        }
        if (collision.gameObject.tag == "PortalGravityN")
        {
            gravN = true;
            if (reversed) { playGravityParticles(); }
        }
        if (collision.gameObject.tag == "PortalGravityC")
        {
            gravC = true;
            playGravityParticles();
        }
        if (collision.gameObject.tag == "TeleportA")
        {
            teleA = true;

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

            teleB = tb.position - t.position;

            float distAX = transform.position.x - t.position.x;
            float offsetBX = distAX * ((Mathf.Abs((tb.rotation.eulerAngles.z - t.rotation.eulerAngles.z)) % 180)/90f);
            teleB.x -= offsetBX;

            float distAY = transform.position.y - t.position.y;
            float offsetBY = distAY * ((Mathf.Abs((tb.rotation.eulerAngles.z - t.rotation.eulerAngles.z)) % 180) / 90f);
            teleB.y -= offsetBY;

            teleB.z = 0;
        }
        if (collision.gameObject.tag == "ChargedTeleportA")
        {
            chargedTeleA = true;

            Vector3 inputVelocity = player_body.velocity;

            Transform t = collision.gameObject.transform;

            Transform tb = t;
            foreach (Transform tr in t)
            {
                if (tr.tag == "ChargedTeleportB")
                {
                    tb = tr.GetComponent<Transform>();
                    break;
                }
            }

            chargedTeleB = tb.position - t.position;

            float distAX = transform.position.x - t.position.x;
            float offsetBX = distAX * ((Mathf.Abs((tb.rotation.eulerAngles.z - t.rotation.eulerAngles.z)) % 180) / 90f);
            chargedTeleB.x -= offsetBX;

            float distAY = transform.position.y - t.position.y;
            float offsetBY = distAY * ((Mathf.Abs((tb.rotation.eulerAngles.z - t.rotation.eulerAngles.z)) % 180) / 90f);
            chargedTeleB.y -= offsetBY;

            chargedTeleB.z = 0;

            float inDirection = Vector3.Dot(inputVelocity, t.right);
            float crossDirection = Vector3.Dot(inputVelocity, Vector2.Perpendicular(((Vector2)(-t.right))));//inputVelocity.magnitude - inDirection;
            float crossScale = Mathf.Abs(Vector3.Dot(Vector2.Perpendicular(((Vector2)(-t.right))), Vector2.Perpendicular(((Vector2)(-tb.right)))));
            chargedTeleportVelocity = ((Vector2)(-tb.right) * inDirection) + Vector2.Perpendicular(((Vector2)(-t.right))) * crossDirection * crossScale;

            /*Quaternion halfTurn = Quaternion.Euler(0.0f, 180.0f, 0.0f);
            Vector3 relativeVel = t.InverseTransformDirection(inputVelocity);
            relativeVel = halfTurn * relativeVel;
            chargedTeleportVelocity = tb.TransformDirection(relativeVel);*/

            //chargedTeleportVelocity = Quaternion.Euler(0, 0, 180 + tb.transform.localRotation.eulerAngles.z) * inputVelocity;
            //Debug.Log(t.right + "   " + -tb.right);
            //Debug.Log(inDirection + "   " + crossDirection + "    " + crossScale);
            chargeTeleportTimer = 1.2f;
        }
        if (collision.gameObject.tag == "ChargedTeleportC")
        {
            if (ChargedTeleportCTouched != null && collision.gameObject == ChargedTeleportCTouched && chargeTeleportTimerC > 0) return;
            chargedTeleA = true;

            Vector3 inputVelocity = player_body.velocity;

            Transform t = collision.gameObject.transform;

            Transform tb = t;
            if(t.parent != null && t.parent.tag == "ChargedTeleportC")
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

            chargedTeleB = tb.position - t.position;

            float distAX = transform.position.x - t.position.x;
            float offsetBX = distAX * ((Mathf.Abs((tb.rotation.eulerAngles.z - t.rotation.eulerAngles.z)) % 180) / 90f);
            chargedTeleB.x -= offsetBX;

            float distAY = transform.position.y - t.position.y;
            float offsetBY = distAY * ((Mathf.Abs((tb.rotation.eulerAngles.z - t.rotation.eulerAngles.z)) % 180) / 90f);
            chargedTeleB.y -= offsetBY;

            chargedTeleB.z = 0;

            float inDirection = Vector3.Dot(inputVelocity, t.right);
            float crossDirection = Vector3.Dot(inputVelocity, Vector2.Perpendicular(((Vector2)(-t.right))));//inputVelocity.magnitude - inDirection;
            float crossScale = Mathf.Abs(Vector3.Dot(Vector2.Perpendicular(((Vector2)(-t.right))), Vector2.Perpendicular(((Vector2)(-tb.right)))));
            chargedTeleportVelocity = ((Vector2)(-tb.right) * inDirection) + Vector2.Perpendicular(((Vector2)(-t.right))) * crossDirection * crossScale;

            /*Quaternion halfTurn = Quaternion.Euler(0.0f, 180.0f, 0.0f);
            Vector3 relativeVel = t.InverseTransformDirection(inputVelocity);
            relativeVel = halfTurn * relativeVel;
            chargedTeleportVelocity = tb.TransformDirection(relativeVel);*/

            chargeTeleportTimer = 1.2f;
            chargeTeleportTimerC = .1f;
            ChargedTeleportCTouched = tb.gameObject;
        }
        if (collision.gameObject.tag == "PurpleOrb")
        {
            purple = true;
            OrbTouched = collision.gameObject;
        }
        if (collision.gameObject.tag == "BlueOrb")
        {
            blue = true;
            OrbTouched = collision.gameObject;
        }
        if (collision.gameObject.tag == "YellowPad")
        {
            yellow_p = true;
        }
        if (collision.gameObject.tag == "PinkPad")
        {
            pink_p = true;
        }
        if (collision.gameObject.tag == "RedPad")
        {
            red_p = true;
        }
        if (collision.gameObject.tag == "GreenPad")
        {
            green_p = true;
            playGravityParticles();
        }
        if (collision.gameObject.tag == "BluePad")
        {
            blue_p = true;
            playGravityParticles();
        }
        if (collision.gameObject.tag == "Speed0x")
        {
            speed = speed0;
            playSpeedParticles(0);
        }
        if (collision.gameObject.tag == "Speed1x")
        {
            speed = speed1;
            playSpeedParticles(1);
        }
        if (collision.gameObject.tag == "Speed2x")
        {
            speed = speed2;
            playSpeedParticles(2);
        }
        if (collision.gameObject.tag == "Speed3x")
        {
            speed = speed3;
            playSpeedParticles(3);
        }
        if (collision.gameObject.tag == "Speed4x")
        {
            speed = speed4;
            playSpeedParticles(4);
        }
        if (collision.gameObject.tag == "Mini")
        {
            mini = true;
            ChangeSize();
        }
        if (collision.gameObject.tag == "Mega")
        {
            mini = false;
            ChangeSize();
        }
    }

    protected void OnTriggerExit2D(Collider2D collision)
    {
        if (!enabled) { return; }
        if (collision.gameObject.tag == "TriggerOrb")
        {
            triggerorb = false;
        }
        if (collision.gameObject.tag == "TeleOrb")
        {
            teleorb = false;
        }
        if (collision.gameObject.tag == "YellowOrb")
        {
            yellow = false;
        }
        if (collision.gameObject.tag == "BlueOrb")
        {
            blue = false;
        }
        if (collision.gameObject.tag == "PinkOrb")
        {
            pink = false;
        }
        if (collision.gameObject.tag == "RedOrb")
        {
            red = false;
        }
        if (collision.gameObject.tag == "GreenOrb")
        {
            green = false;
        }
        if (collision.gameObject.tag == "BlackOrb")
        {
            black = false;
        }
        if (collision.gameObject.tag == "PurpleOrb")
        {
            purple = false;
        }
        if (collision.gameObject.tag == "YellowPad")
        {
            yellow_p = false;
        }
        if (collision.gameObject.tag == "GreenPad")
        {
            green_p = false;
        }
        if (collision.gameObject.tag == "PinkPad")
        {
            pink_p = false;
        }
        if (collision.gameObject.tag == "RedPad")
        {
            red_p = false;
        }
        if (collision.gameObject.tag == "BluePad")
        {
            blue_p = false;
        }
    }

    protected void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "MovingObject")
        {
            //gameObject.transform.SetParent(collision.gameObject.transform);
            //player_body.AddForce(collision.gameObject.GetComponent<Rigidbody2D>().velocity / (crouch?3:.9f), ForceMode2D.Force);
            if(!MovingObjectVelocities.Contains(collision.gameObject.GetComponent<Rigidbody2D>()))
            {
                MovingObjectVelocities.Add(collision.gameObject.GetComponent<Rigidbody2D>());
            }
        }
    }

    protected void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "MovingObject")
        {
            //gameObject.transform.SetParent(null);
            //MovingObjectVelocity = Vector3.zero;
            MovingObjectVelocities.Remove(collision.gameObject.GetComponent<Rigidbody2D>());
        }
    }

    // ABSTRACT METHOD ----------------------------------------------------------
    public abstract void Awake2();
    public abstract void setAnimation();
    public abstract void ChangeSize();
    public abstract void Move();
    public abstract void Jump();
    public abstract void Pad();
    public abstract void Portal();
    public abstract void Flip();
    public abstract void Respawn();
    public abstract void setRespawn(Vector3 pos, bool rev, bool min);
    public abstract void resetBooleans();
    public abstract void setRepawnSpeed(float s);
    public abstract void setSpeed(float s);
    public abstract float getSpeed();
    public abstract void resetColliders();
    public abstract void setColliders();
    public abstract string getMode();

    public class SpiderOrbTrail : MonoBehaviour
    {
        public void Activate(TrailRenderer trail, GameObject player)
        {
            StopAllCoroutines();
            StartCoroutine(animateTrail(trail, player));
        }

        IEnumerator animateTrail(TrailRenderer trail, GameObject player)
        {
            trail.emitting = true;

            float time = 0, T = 0.1f;
            while (time <= T)
            {
                time += Time.deltaTime;
                yield return null;
            }

            trail.emitting = false;
        }
    }
}