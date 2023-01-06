using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Cinemachine;

public class BallController : PlayerController
{
    public Collider2D player_collider;
    public CircleCollider2D circle_collider;

    public TrailRenderer trail;

    public GameObject ball;

    private float jumpForce = 15f;
    private float posJump;

    private float moveX, grav_scale;
    private float smoothing;

    private float maxSpeed = 15 * 1.6f;

    private bool orbJumped = false;

    public override void Awake2()
    {
        speed = getSpeed();
        moveX = speed;
        smoothing = .15f;//0.05f
        v_Velocity = Vector3.zero;
        posJump = jumpForce;
        player_collider = GetComponent<Collider2D>();

        grav_scale = player_body.gravityScale;

        circle_collider.enabled = false;
        setRespawn(transform.position, reversed, mini);
        setRepawnSpeed(1f);

        //eyes = GameObject.Find("Icon_Eyes");
        //setAnimation();
    }

    private void Start()
    {
        //bgmusic.Play();
        //player_body.freezeRotation = false;
    }

    public override void setAnimation()
    {
        player_body.freezeRotation = true;
        player_body.gravityScale = 8f;
        if (reversed) { player_body.gravityScale *= -1; }
        grav_scale = player_body.gravityScale;

        grounded_particles.gameObject.transform.localPosition = new Vector3(0, -.52f, 0);
        ground_impact_particles.gameObject.transform.localPosition = new Vector3(0, -.52f, 0);

        grounded_particles.gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
        ground_impact_particles.gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);

        ChangeSize();

        icon.transform.localScale = new Vector3(1f, 1f, 1f);
        icon.transform.localPosition = new Vector3(0, 0, 0);
        ball.SetActive(true);
        icon.SetActive(false);

        trail.transform.localPosition = new Vector3(0, 0, 0);
    }
    public override void ChangeSize()
    {
        if (mini)
        {
            grounded_particles.startLifetime = .15f;
            ground_impact_particles.startLifetime = .15f;
            grounded_particles.transform.localScale = new Vector2(.47f, .47f);
            ground_impact_particles.transform.localScale = new Vector2(.47f, .47f);

            transform.localScale = new Vector2(.47f, .47f);
            jumpForce = 13f;
        }
        else
        {
            grounded_particles.startLifetime = .3f;
            ground_impact_particles.startLifetime = .3f;
            grounded_particles.transform.localScale = new Vector2(1, 1f);
            ground_impact_particles.transform.localScale = new Vector2(1f, 1f);

            transform.localScale = new Vector2(1.05f, 1.05f);
            jumpForce = 15f;
        }

        posJump = jumpForce;
    }

    void Update()
    {
        if (able)
        {
            // CHECK IF DEAD
            dead = /*Physics2D.IsTouchingLayers(player_collider, deathLayer) || */Physics2D.IsTouchingLayers(circle_collider, deathLayer);
            //grounded = Physics2D.Raycast(player_body.transform.position, Vector2.down, .51f, groundLayer);

            // CHECK IF GROUNDED
            if (reversed)
            {
                grounded = Physics2D.BoxCast(player_body.transform.position, new Vector2((mini ? .4f : .95f), .1f), 0f, Vector2.up, .51f, groundLayer) && checkGrounded
                        && (Physics2D.IsTouchingLayers(player_collider, groundLayer) || Physics2D.IsTouchingLayers(circle_collider, groundLayer));
                regate = -1;

                grounded_particles.gravityModifier = -Mathf.Abs(grounded_particles.gravityModifier);
                ground_impact_particles.gravityModifier = -Mathf.Abs(ground_impact_particles.gravityModifier);

                grounded_particles.gameObject.transform.rotation = Quaternion.Euler(0, 0, 180);
                ground_impact_particles.gameObject.transform.rotation = Quaternion.Euler(0, 0, 180);
            }
            else
            {//.9
                grounded = Physics2D.BoxCast(player_body.transform.position, new Vector2((mini ? .4f : .95f), .1f), 0f, Vector2.down, .51f, groundLayer) && checkGrounded
                        && (Physics2D.IsTouchingLayers(player_collider, groundLayer) || Physics2D.IsTouchingLayers(circle_collider, groundLayer));
                regate = 1;

                grounded_particles.gravityModifier = Mathf.Abs(grounded_particles.gravityModifier);
                ground_impact_particles.gravityModifier = Mathf.Abs(ground_impact_particles.gravityModifier);

                grounded_particles.gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
                ground_impact_particles.gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
            }

            // IF GROUNDED --> TURN OFF TRAIL
            /*
            if (grounded && (!red_p && !yellow_p && !blue_p && !pink_p))
            {
                trail.emitting = false;
                animator.SetBool("Jump", false);
                animator.SetBool("Orb", false);
            }*/

            // LIMIT Y SPEED
            if (player_body.velocity.y > maxSpeed)
            {
                player_body.velocity = new Vector2(player_body.velocity.x, maxSpeed);
            }
            else if (player_body.velocity.y < -maxSpeed)
            {
                player_body.velocity = new Vector2(player_body.velocity.x, -maxSpeed);
            }


            // Movement Speed
            moveX = input.Player.MovementHorizontal.ReadValue<float>() * speed;

            // Grounded Particles
            if (grounded && (Mathf.Abs(player_body.velocity.x) > .2f || jump))
            {
                if (!grounded_particles.isPlaying)
                {
                    grounded_particles.Play();
                }
            }
            else
            {
                grounded_particles.Stop();
            }

            if ((prev_grounded && !grounded) || (!prev_grounded && grounded && prev_velocity > 10f))
            {
                ground_impact_particles.Play();
            }

            // JUMP!
            if (input.Player.Jump.triggered)//Input.GetButtonDown("Jump") || Input.GetKeyDown("space") || Input.GetMouseButtonDown(0))
            {
                jump = true;
            }

            // RELEASE JUMP
            if (prevJumpNotReleased && input.Player.Jump.ReadValue<float>() == 0)//Input.GetButtonUp("Jump") || Input.GetKeyUp("space") || Input.GetMouseButtonUp(0))
            {
                jump = false;
            }

            if (chargeTeleportTimer > 0)
            {
                chargeTeleportTimer -= Time.deltaTime;
            }
            if (chargeTeleportTimerC > 0)
            {
                chargeTeleportTimerC -= Time.deltaTime;
            }

            // CHANGE JUMP DIRECTION WHEN REVERSED
            if (reversed)
            {
                jumpForce = -posJump;
            }
            else
            {
                jumpForce = posJump;
            }

            // IF DEAD --> RESPAWN
            if (dead)
            {
                Respawn();
            }

            prevJumpNotReleased = input.Player.Jump.triggered || input.Player.Jump.ReadValue<float>() == 1;
        }
    }

    void FixedUpdate()
    {
        // one job and one job only. MOVE
        if (able)
        {
            Move();
            Interpolate(-1, -1);
        }
    }

    public override void Move()
    {
        // If the input is moving the player right and the player is facing left...
        if ((!reversed && speed > 0 && !facingright && grounded) || (reversed && speed < 0 && !facingright && grounded))
        {
            // ... flip the player.
            negate = 1;
            facingright = !facingright;
            //Flip();
        }
        // Otherwise if the input is moving the player left and the player is facing right...
        else if ((!reversed && speed < 0 && facingright && grounded) || (reversed && speed > 0 && facingright && grounded))
        {
            // ... flip the player.
            negate = -1;
            facingright = !facingright;
            //Flip();
        }

        // movement controls
        //Vector2 targetVelocity = new Vector2(moveX * Time.fixedDeltaTime * 10f, player_body.velocity.y);
        //player_body.velocity = targetVelocity;
        bool cancelCharge = true; float clampedTimer = Mathf.Clamp(chargeTeleportTimer, 0, 1);
        if (chargeTeleportTimer > 0 && ((!grounded && (Mathf.Abs(chargedTeleportVelocity.x) * clampedTimer) > Mathf.Abs(moveX)) || chargeTeleportTimer > 1)) cancelCharge = false;

        //Vector2 targetVelocity = new Vector2(moveX * Time.fixedDeltaTime * 10f, player_body.velocity.y);
        Vector2 targetVelocity = new Vector2((cancelCharge ? moveX : (moveX * (1 - clampedTimer) + chargedTeleportVelocity.x * clampedTimer)) * Time.fixedDeltaTime * 10f, player_body.velocity.y);

        if (Mathf.Abs(targetVelocity.x) > Mathf.Abs(player_body.velocity.x) || !grounded)
        {
            //player_body.velocity = Vector3.SmoothDamp(player_body.velocity, targetVelocity, ref v_Velocity, smoothing * .7f); //.7
            player_body.velocity = Vector3.SmoothDamp(player_body.velocity, targetVelocity, ref v_Velocity, smoothing * ((!grounded && !cancelCharge) ? 5 : .7f));
        }
        else
        {
            player_body.velocity = Vector3.SmoothDamp(player_body.velocity, targetVelocity, ref v_Velocity, smoothing * 1.1f); //1.1
        }

        /*
        if (Mathf.Abs(targetVelocity.x) > Mathf.Abs(player_body.velocity.x) || !grounded)
        {
            player_body.velocity = Vector3.SmoothDamp(player_body.velocity, targetVelocity, ref v_Velocity, smoothing * .7f);
        }
        else
        {
            player_body.velocity = Vector3.SmoothDamp(player_body.velocity, targetVelocity, ref v_Velocity, smoothing * 1.5f);
        }*/

        Rotate();
        //Eyes();
        Pad();      // check if hit pad
        Jump();     // check if jumping
        Portal();   // check if on portal

        // IF GROUNDED --> TURN OFF TRAIL
        if (grounded && Mathf.Abs(player_body.velocity.y) <= 0.01 && (!red_p && !yellow_p && !blue_p && !pink_p))
        {
            trail.emitting = false;
        }
    }

    //private bool resetRotation = true;

    public void Rotate()
    {
        float step = -player_body.velocity.x * (mini ? 1.5f : .9f);
        if (grounded) { orbJumped = false; }

        if (reversed)
        {
            step = player_body.velocity.x * (mini ? 1.5f : .9f);
        }

        if (grounded || !orbJumped)
        {
            player_body.rotation = player_body.rotation + step;
        }
        else
        {
            if (Mathf.Abs(player_body.velocity.x) > 0.02f)
            {
                player_body.rotation = player_body.rotation - step * .7f;
            }
            else
            {
                player_body.rotation = player_body.rotation + step - (speed * .2f);
            }
        }

        /*if (grounded || !orbJumped)
        {
            Vector3 newAngle = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z + step);
            transform.rotation = Quaternion.Euler(newAngle);
        }
        else
        {
            if (Mathf.Abs(player_body.velocity.x) > 0.02f)
            {
                Vector3 newAngle = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z - step * .7f);
                transform.rotation = Quaternion.Euler(newAngle);
            }
            else
            {
                Vector3 newAngle = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z - (speed * .2f));
                transform.rotation = Quaternion.Euler(newAngle);
            }
        }*/

        int rev = reversed ? -1 : 1;
        float cos = rev*-Mathf.Cos(Mathf.Deg2Rad * transform.localRotation.eulerAngles.z);
        float sin = rev*-Mathf.Sin(Mathf.Deg2Rad * transform.localRotation.eulerAngles.z);

        grounded_particles.gameObject.transform.localPosition = new Vector3(sin*.52f, cos*.52f, 0);
        ground_impact_particles.gameObject.transform.localPosition = new Vector3(sin*.52f, cos*.52f, 0);

        //Debug.Log("Rot: " + transform.localRotation.eulerAngles.z + "\tSin: " + sin * .52f + "\tCos: " + cos * .52f);
    }

    /*public void Eyes()
    {
        int rev = 1;
        if (reversed) { rev = -1; }
        eyes.transform.localPosition = Vector3.Lerp(eyes.transform.localPosition, new Vector3(rev * (moveX / 800), 0 * rev * (player_body.velocity.y / 80), 0), .4f);

        if (!grounded)
        {
            return;
        }
        else
        {
            if ((int)Mathf.Abs(transform.rotation.eulerAngles.z / 90) == 0)
            {
                eyes.transform.Find("Eyes_Wide").gameObject.SetActive(false);
                eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
                eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
                eyes.transform.Find("Eyes_Normal").gameObject.SetActive(true);
            }
            else if ((int)Mathf.Abs(transform.rotation.eulerAngles.z / 90) == 1)
            {
                eyes.transform.Find("Eyes_Wide").gameObject.SetActive(false);
                eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
                eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
                eyes.transform.Find("Eyes_Squint").gameObject.SetActive(true);
            }
            else if ((int)Mathf.Abs(transform.rotation.eulerAngles.z / 90) == 2)
            {
                eyes.transform.Find("Eyes_Wide").gameObject.SetActive(false);
                eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
                eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
                eyes.transform.Find("Eyes_Squint").gameObject.SetActive(true);
            }
            else if ((int)Mathf.Abs(transform.rotation.eulerAngles.z / 90) == 3)
            {
                eyes.transform.Find("Eyes_Wide").gameObject.SetActive(false);
                eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
                eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
                eyes.transform.Find("Eyes_Irked").gameObject.SetActive(true);
            }
        }
    }*/

    public override void Jump()
    {
        OrbComponent orbscript = null;
        if (OrbTouched != null) { orbscript = OrbTouched.GetComponent<OrbComponent>(); }

        if (teleorb && jump)
        {
            Vector3 positionDelta = (transform.position + teleOrb_translate) - transform.position;
            jump = false;
            teleorb = false;

            if (OrbTouched != null)
            {
                orbscript.Pulse();
            }

            player_body.transform.position += teleOrb_translate;
            CinemachineVirtualCamera activeCamera = gamemanager.getActiveCamera();
            activeCamera.GetCinemachineComponent<CinemachineFramingTransposer>().OnTargetObjectWarped(transform.GetChild(0), positionDelta);
        }

        if (triggerorb && jump)
        {
            triggerorb = false;
            SpawnTrigger spawn = OrbTouched.GetComponent<SpawnTrigger>();
            StartCoroutine(spawn.Begin());

            if (OrbTouched != null)
            {
                orbscript.Pulse();
            }

            jump = !spawn.cancelJump;
            triggerorb = spawn.cancelJump;
        }

        if (yellow && jump)
        {
            chargeTeleportTimer = 0;
            jump = false;
            yellow = false;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * 1.2f);
            trail.emitting = true;

            if (grav) { grav = false; }
            if (gravN) { gravN = false; }

            orbJumped = true;

            if (OrbTouched != null)
            {
                orbscript.Pulse();
            }
        }
        else if (pink && jump)
        {
            chargeTeleportTimer = 0;
            jump = false;
            pink = false;
            trail.emitting = true;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce);

            if (grav) { grav = false; }
            if (gravN) { gravN = false; }

            orbJumped = true;

            if (OrbTouched != null)
            {
                orbscript.Pulse();
            }
        }
        else if (red && jump)
        {
            chargeTeleportTimer = 0;
            jump = false;
            red = false;
            trail.emitting = true;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * 1.4f);

            if (grav) { grav = false; }
            if (gravN) { gravN = false; }

            orbJumped = true;

            if (OrbTouched != null)
            {
                orbscript.Pulse();
            }
        }
        else if (blue && jump)
        {
            chargeTeleportTimer = 0;
            jump = false;
            blue = false;
            trail.emitting = true;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * .4f);

            playGravityParticles();
            reversed = !reversed;
            player_body.gravityScale *= -1;
            grav_scale *= -1;
            grounded = false;

            if (grav) { grav = false; }
            if (gravN) { gravN = false; }

            if (OrbTouched != null)
            {
                orbscript.Pulse();
            }
        }
        else if (green && jump)
        {
            chargeTeleportTimer = 0;
            jump = false;
            green = false;
            playGravityParticles();
            reversed = !reversed;

            if (reversed)
            {
                jumpForce = -posJump;
            }
            else
            {
                jumpForce = posJump;
            }
            trail.emitting = true;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * 1.2f);
            player_body.gravityScale *= -1;
            grav_scale *= -1;

            if (grav) { grav = false; }
            if (gravN) { gravN = false; }

            orbJumped = true;

            if (OrbTouched != null)
            {
                orbscript.Pulse();
            }
        }
        else if (black && jump)
        {
            chargeTeleportTimer = 0;
            black = false;
            jump = false;
            trail.emitting = true;
            player_body.velocity = new Vector2(player_body.velocity.x, -jumpForce * 1.2f);

            if (OrbTouched != null)
            {
                orbscript.Pulse();
            }
        }
        else if (purple && jump)
        {
            chargeTeleportTimer = 0;
            int rev = 1;
            if (reversed) { rev = -1; }
            RaycastHit2D groundhit, deathhit;

            bool connect = true;

            if (!reversed)
            {
                groundhit = Physics2D.BoxCast(player_body.transform.position + new Vector3(0, .2f, 0), new Vector2(circle_collider.bounds.size.x * .5f, .1f), 0f, Vector2.up, 30, groundLayer);
                deathhit = Physics2D.BoxCast(player_body.transform.position + new Vector3(0, .2f, 0), new Vector2(circle_collider.bounds.size.x * .5f, .1f), 0f, Vector2.up, 30, deathLayer);
            }
            else
            {
                groundhit = Physics2D.BoxCast(player_body.transform.position + new Vector3(0, -.2f, 0), new Vector2(circle_collider.bounds.size.x * .5f, .1f), 0f, -Vector2.up, 30, groundLayer);
                deathhit = Physics2D.BoxCast(player_body.transform.position + new Vector3(0, -.2f, 0), new Vector2(circle_collider.bounds.size.x * .5f, .1f), 0f, -Vector2.up, 30, deathLayer);
            }

            //bool head = grounded && Physics2D.BoxCast(player_body.transform.position, new Vector2(.95f, .1f), 0f, rev * Vector2.up, .01f, groundLayer);
            if (deathhit.collider != null && (deathhit.distance <= groundhit.distance || groundhit.distance == 0))
            {
                player_body.velocity = new Vector2(player_body.velocity.x, 0);
                spiderorb_trail.emitting = true;
                SpOrTr.Activate(spiderorb_trail, gameObject);
                playGravityParticles();
                //Debug.Log(deathhit.distance);
                reversed = !reversed;
                player_body.gravityScale *= -1;
                grav_scale *= -1;
                transform.position = new Vector2(transform.position.x, transform.position.y + rev * (deathhit.distance - (mini ? 0f : .1f)));
            }
            else if (groundhit.collider != null)
            {
                player_body.velocity = new Vector2(player_body.velocity.x, 0);
                spiderorb_trail.emitting = true;
                SpOrTr.Activate(spiderorb_trail, gameObject);
                playGravityParticles();
                //Debug.Log(groundhit.distance - .5f);
                reversed = !reversed;
                player_body.gravityScale *= -1;
                grav_scale *= -1;
                transform.position = new Vector2(transform.position.x, transform.position.y + rev * (groundhit.distance - (mini ? .1f : .3f)));
            }
            else
            {
                //spider_trail.emitting = true;
                connect = false;
            }

            //Debug.DrawLine(transform.position + new Vector3(1, 0, 0), transform.position + new Vector3(1, rev * (groundhit.distance - .5f), 0), Color.red);

            pulse_trigger_p1.Enter();
            pulse_trigger_p2.Enter();
            if (!connect) return;

            purple = false;
            jump = false;
            trail.emitting = true;

            if (grav) { grav = false; }
            if (gravN) { gravN = false; }

            if (OrbTouched != null)
            {
                orbscript.Pulse();
            }
        }
        else if (grounded && jump)
        {
            jump = false;
            blue = false;
            trail.emitting = false;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * .4f);
            reversed = !reversed;
            player_body.gravityScale *= -1;
            grav_scale *= -1;
            grounded = false;

            if (grav) { grav = false; }
            if (gravN) { gravN = false; }

            orbJumped = true;
        }
    }

    public override void Pad()
    {
        if (yellow_p)
        {
            //yellow_p = false;
            checkGrounded = false;
            grounded = false;
            jump = false;

            //animator.SetBool("Orb", true);
            //jump = false;
            trail.emitting = true;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * 1.3f);
            yellow_p = false;

            checkGrounded = true;
            orbJumped = true;
        }
        else if (pink_p)
        {
            checkGrounded = false;
            grounded = false;
            jump = false;

            //jump = false;
            trail.emitting = true;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * .9f);
            pink_p = false;

            checkGrounded = true;
            orbJumped = true;
        }
        else if (red_p)
        {
            checkGrounded = false;
            grounded = false;
            jump = false;

            trail.emitting = true;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * 1.6f);
            red_p = false;

            checkGrounded = true;
            orbJumped = true;
        }
        else if (blue_p)
        {

            checkGrounded = false;
            blue_p = false;
            grounded = false;
            jump = false;

            trail.emitting = true;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * .4f);
            reversed = !reversed;
            player_body.gravityScale *= -1;
            grav_scale *= -1;

            checkGrounded = true;
        }
    }

    public override void Portal()
    {
        if (grav)
        {
            grav = false;

            if (!reversed)
            {
                reversed = true;
                jumpForce = -posJump;
                trail.emitting = true;
                /*if (Mathf.Abs(player_body.velocity.y) > maxSpeed * .6f)
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .6f);
                }
                else
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .8f);
                }*/

                if (player_body.velocity.y <= -10f)
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, -10f);
                }

                player_body.gravityScale = -Mathf.Abs(player_body.gravityScale);
                grav_scale = player_body.gravityScale;
            }
        }
        else if (gravN)
        {
            gravN = false;

            if (reversed)
            {
                reversed = false;
                jumpForce = posJump;
                trail.emitting = true;
                /*if (Mathf.Abs(player_body.velocity.y) > maxSpeed * .6f)
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .6f);
                }
                else
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .8f);
                }*/

                if (player_body.velocity.y >= 10f)
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, 10f);
                }

                player_body.gravityScale = Mathf.Abs(player_body.gravityScale);
                grav_scale = player_body.gravityScale;
            }
        }
        else if (gravC)
        {
            gravC = false;
            fromGround = false;
            released = false;

            if (reversed)
            {
                reversed = false;
                jumpForce = posJump;
                trail.emitting = true;
                /*if (Mathf.Abs(player_body.velocity.y) > maxSpeed * .6f)
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .6f);
                }
                else
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .8f);
                }*/

                if (player_body.velocity.y >= 10f)
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, 10f);
                }

                player_body.gravityScale = Mathf.Abs(player_body.gravityScale);
                grav_scale = player_body.gravityScale;
            }
            else if (!reversed)
            {
                reversed = true;
                jumpForce = -posJump;
                trail.emitting = true;
                /*if (Mathf.Abs(player_body.velocity.y) > maxSpeed * .6f)
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .6f);
                }
                else
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .8f);
                }*/

                if (player_body.velocity.y <= -10f)
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, -10f);
                }

                player_body.gravityScale = -Mathf.Abs(player_body.gravityScale);
                grav_scale = player_body.gravityScale;
            }
        }
        else if (teleA)
        {
            Vector3 positionDelta = (transform.position + teleB) - transform.position;
            //trail.emitting = false;
            //trail.Clear();
            trail.enabled = true;
            teleA = false;
            player_body.transform.position += teleB;
            //trail.enabled = true;

            //CinemachineVirtualCamera activeCamera = gamemanager.getActiveCamera();
            //activeCamera.GetCinemachineComponent<CinemachineFramingTransposer>().OnTargetObjectWarped(transform.GetChild(0), positionDelta);
        }
        else if (chargedTeleA)
        {
            trail.emitting = false;
            //trail.Clear();
            trail.enabled = true;
            chargedTeleA = false;
            player_body.transform.position += chargedTeleB;
            //Debug.Log("PlayerVY: " + player_body.velocity + "   OutVY: " + chargedTeleportVelocity);
            player_body.velocity = chargedTeleportVelocity;
            released = false;
            trail.emitting = true;
        }
    }


    // COROUTUNES
    // none needed

    public override void Flip()
    {
        // Switch the way the player is labelled as facing.
        //facingright = !facingright;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    public override void Respawn()
    {
        able = false;
        if (restartmusic) { bgmusic.Stop(); }

        grounded_particles.Stop();
        ground_impact_particles.Stop();

        player_collider.enabled = false;
        circle_collider.enabled = false;
        StopAllCoroutines();
        player_body.velocity = Vector2.zero;
        trail.emitting = false;
        jump = false;
        yellow = false; pink = false; red = false; green = false; blue = false; purple = false; black = false;
        reversed = respawn_rev;
        mini = respawn_mini;
        ChangeSize();

        if (reversed)
        {
            player_body.gravityScale = -Mathf.Abs(player_body.gravityScale);
            grav_scale = player_body.gravityScale;
            transform.rotation = new Quaternion(0, 0, 180, 0);
        }
        else
        {
            player_body.gravityScale = Mathf.Abs(player_body.gravityScale);
            grav_scale = player_body.gravityScale;
            transform.rotation = new Quaternion(0, 0, 0, 0);
        }

        player_renderer.SetActive(false);
        //player_renderer.enabled = false;
        //death_animation.GetComponent<SpriteRenderer>().enabled = true;
        death_particles.Play();
        death_sfx.PlayOneShot(death_sfx.clip, gamemanager.sfx_volume);
        player_body.gravityScale = 0;

        Invoke("reposition", 1f);
        //player_body.transform.position += respawn - transform.position;

    }

    public void reposition()
    {
        Vector3 positionDelta = respawn - transform.position;
        player_body.transform.position += respawn - transform.position;
        player_collider.enabled = true;

        CinemachineVirtualCamera activeCamera = gamemanager.getActiveCamera();
        activeCamera.GetCinemachineComponent<CinemachineFramingTransposer>().OnTargetObjectWarped(transform.GetChild(0), positionDelta);

        undead();
        //Invoke("undead", .5f);
    }

    public void undead()
    {
        player_collider.enabled = false;
        speed = respawn_speed;
        //player_collider.enabled = true;
        circle_collider.enabled = true;
        player_renderer.SetActive(true);
        //player_renderer.enabled = true;
        player_body.gravityScale = grav_scale;

        //bgmusic.volume = 1;
        if (restartmusic)
        {
            gamemanager.setToNewBGMusic();
            bgmusic.Play();
        }

        //Vector2 targetVelocity = new Vector2(speed * Time.fixedDeltaTime * 10f, player_body.velocity.y);
        //player_body.velocity = targetVelocity;

        dead = false;
        able = true;
    }

    public override void setRespawn(Vector3 pos, bool rev, bool min)
    {
        respawn = pos;
        respawn_rev = rev;
        respawn_mini = min;
        respawn.z = transform.position.z;
    }

    public override void resetBooleans()
    {
        StopAllCoroutines();
        reversed = false; jump = false; yellow = false; pink = false; red = false; green = false; blue = false; purple = false; black = false; teleA = false;
    }

    public override void setRepawnSpeed(float s)
    {
        if (s == 0) { respawn_speed = speed0; }
        else if (s == 1) { respawn_speed = speed1; }
        else if (s == 2) { respawn_speed = speed2; }
        else if (s == 3) { respawn_speed = speed3; }
        else if (s == 4) { respawn_speed = speed4; }
    }

    public override void setSpeed(float s)
    {
        if (s == 0 || s == speed0) { speed = speed0; }
        else if (s == 1 || s == speed1) { speed = speed1; }
        else if (s == 2 || s == speed2) { speed = speed2; }
        else if (s == 3 || s == speed3) { speed = speed3; }
        else if (s == 4 || s == speed4) { speed = speed4; }
    }

    public override float getSpeed()
    {
        return speed;
    }

    public override void resetColliders()
    {
        //player_collider.isTrigger = false;
        ball.SetActive(false);

        player_collider.enabled = true;
        circle_collider.enabled = false;
        circle_collider.radius = 0.5f;
    }
    public override void setColliders()
    {
        player_collider.enabled = false;
        //player_collider.isTrigger = true;
        circle_collider.enabled = true;
        circle_collider.radius = 0.475f;
    }

    public override string getMode()
    {
        return "ball";
    }
}