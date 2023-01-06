using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Cinemachine;

public class CopterController : PlayerController
{
    public Collider2D player_collider;

    public TrailRenderer trail;

    public ParticleSystem windUp, windDown;

    public Transform Copter_Anim;
    public GameObject copter;

    private float jumpForce = 10f; //15 when at 80
    private float posJump;

    private float moveX, grav_scale;
    private float smoothing;

    private float time;

    private float maxSpeed = 15f;

    public override void Awake2()
    {
        speed = getSpeed();
        moveX = speed;
        smoothing = .05f;
        v_Velocity = Vector3.zero;
        posJump = jumpForce;
        player_collider = GetComponent<Collider2D>();

        grav_scale = player_body.gravityScale;
        windUp.Stop();
        windDown.Stop();
        //icon = eyes.transform.parent.gameObject;
        //setAnimation();

        setRespawn(transform.position, reversed, mini);
        setRepawnSpeed(1f);
    }

    private void Start()
    {
        //bgmusic.Play();
        //player_body.freezeRotation = false;
    }

    public override void setAnimation()
    {
        //trail.gameObject.SetActive(false);

        player_body.freezeRotation = true;
        transform.rotation = new Quaternion(0, 0, 0, 0);

        player_body.gravityScale = 0f; //2.7f
        //goingUp = false;
        if (reversed) { player_body.gravityScale *= -1;}
        grav_scale = player_body.gravityScale;

        grounded_particles.gameObject.transform.localPosition = new Vector3(0, -.52f, 0);
        ground_impact_particles.gameObject.transform.localPosition = new Vector3(0, -.52f, 0);

        grounded_particles.gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
        ground_impact_particles.gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);

        ChangeSize();

        icon.transform.localScale = new Vector3(1f, 1f, 1f);
        icon.transform.localPosition = new Vector3(0f, 0f, 0);
        copter.SetActive(true);
        icon.SetActive(true);

        eyes = GameObject.Find("Icon_Eyes");

        eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
        eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
        eyes.transform.Find("Eyes_Wide").gameObject.SetActive(false);
        eyes.transform.Find("Eyes_Normal").gameObject.SetActive(true);
    }
    public override void ChangeSize()
    {
        int rev = reversed ? -1 : 1;
        if (mini)
        {
            grounded_particles.startLifetime = .15f;
            ground_impact_particles.startLifetime = .15f;
            grounded_particles.transform.localScale = new Vector2(.47f, .47f);
            ground_impact_particles.transform.localScale = new Vector2(.47f, .47f);

            transform.localScale = new Vector2(.47f, rev * .47f);
            jumpForce = 13f;
        }
        else
        {
            grounded_particles.startLifetime = .3f;
            ground_impact_particles.startLifetime = .3f;
            grounded_particles.transform.localScale = new Vector2(1, 1f);
            ground_impact_particles.transform.localScale = new Vector2(1f, 1f);

            transform.localScale = new Vector2(1.05f, rev * 1.05f);
            jumpForce = 10f;
        }

        posJump = jumpForce;
    }

    void Update()
    {
        if (able)
        {
            // CHECK IF DEAD
            dead = /*Physics2D.IsTouchingLayers(player_collider, deathLayer) || */Physics2D.IsTouchingLayers(player_collider, deathLayer);
            //grounded = Physics2D.Raycast(player_body.transform.position, Vector2.down, .51f, groundLayer);

            // CHECK IF GROUNDED
            //grounded = checkGrounded && Physics2D.IsTouchingLayers(player_collider, groundLayer);

            if (reversed)
            {
                grounded = Physics2D.BoxCast(player_body.transform.position, new Vector2(mini ? .45f : .95f, .1f), 0f, Vector2.up, .51f, groundLayer) && checkGrounded
                        && Physics2D.IsTouchingLayers(player_collider, groundLayer);

                regate = -1;
                grounded_particles.gravityModifier = -Mathf.Abs(grounded_particles.gravityModifier);
                ground_impact_particles.gravityModifier = -Mathf.Abs(ground_impact_particles.gravityModifier);
            }
            else
            {//.9
                grounded = Physics2D.BoxCast(player_body.transform.position, new Vector2(mini ? .45f : .95f, .1f), 0f, Vector2.down, .51f, groundLayer) && checkGrounded
                        && Physics2D.IsTouchingLayers(player_collider, groundLayer);

                regate = 1;
                grounded_particles.gravityModifier = Mathf.Abs(grounded_particles.gravityModifier);
                ground_impact_particles.gravityModifier = Mathf.Abs(ground_impact_particles.gravityModifier);
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

            if (grounded && (Mathf.Abs(player_body.velocity.x) > .1f || jump))
            {
                if (!grounded_particles.isPlaying)
                {
                    grounded_particles.Play();
                }
            }
            else
            {
                if (grounded_particles.isPlaying) { grounded_particles.Stop(); }
            }

            if (!prev_grounded && grounded)
            {
                ground_impact_particles.Play();
            }

            // JUMP!
            if (input.Player.Jump.triggered)//Input.GetButtonDown("Jump") || Input.GetKeyDown("space") || Input.GetMouseButtonDown(0))
            {
                if (triggerorb) { triggerorb_j = true; }
                if (teleorb) { teleorb_j = true; }
                if (yellow) { yellow_j = true; }
                if (red) { red_j = true; }
                if (pink) { pink_j = true; }
                if (blue) { blue_j = true; }
                if (green) { green_j = true; }
                if (black) { black_j = true; }
                if (purple) { purple_j = true; }

                jump = true;
            }

            // RELEASE JUMP
            if (prevJumpNotReleased && input.Player.Jump.ReadValue<float>() == 0)//Input.GetButtonUp("Jump") || Input.GetKeyUp("space") || Input.GetMouseButtonUp(0))
            {
                jump = false;
            }

            // CHANGE JUMP DIRECTION WHEN REVERSED
            if (reversed)
            {
                jumpForce = -posJump;
                //windUp.gravityModifier = -1;
                //windDown.gravityModifier = -1;
            }
            else
            {
                jumpForce = posJump;
                //windUp.gravityModifier = 1;
                //windDown.gravityModifier = 1;
            }

            // IF DEAD --> RESPAWN
            if (dead)
            {
                dead = false;
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
            Interpolate(1, 1);
        }
    }

    public override void Move()
    {
        player_body.velocity -= movingVelocity;
        // If the input is moving the player right and the player is facing left...
        if (!reversed && !upright)
        {
            // ... flip the player.
            negate = 1;
            upright = !upright;
            Flip();
        }
        // Otherwise if the input is moving the player left and the player is facing right...
        else if (reversed && upright)
        {
            // ... flip the player.
            negate = -1;
            upright = !upright;
            Flip();
        }

        // movement controls
        Vector2 targetVelocity = new Vector2(moveX * Time.fixedDeltaTime * 10f, player_body.velocity.y);
        //player_body.velocity = targetVelocity;

        bool grounded_temp = checkGrounded && Physics2D.IsTouchingLayers(player_collider, groundLayer);
        if (Mathf.Abs(targetVelocity.x) > Mathf.Abs(player_body.velocity.x))
        {
            player_body.velocity = Vector3.SmoothDamp(player_body.velocity, targetVelocity, ref v_Velocity, smoothing * (grounded_temp ? 4f : 9f)); //4
        }
        else
        {
            player_body.velocity = Vector3.SmoothDamp(player_body.velocity, targetVelocity, ref v_Velocity, smoothing * (grounded_temp ? .4f : 9f)); //.4
        }

        movingVelocity = MovingObjectVelocities.Count > 0 ? MovingObjectVelocities[MovingObjectVelocities.Count - 1].velocity : Vector2.zero;
        player_body.velocity += movingVelocity;

        Rotate();
        Eyes();
        Jump();     // check if jumping
        Pad();      // check if hit pad
        Portal();   // check if on portal

        // IF GROUNDED --> TURN OFF TRAIL
        if (grounded)
        {
            Copter_Anim.GetComponent<Animator>().speed = 1.34f;
            if (windUp.isPlaying) { windUp.Stop(); }
            if (windDown.isPlaying) { windDown.Stop(); }

            trail.emitting = false;
        }
        else
        {
            //grounded_particles.Stop();
            //ground_impact_particles.Stop();
            Copter_Anim.GetComponent<Animator>().speed = Mathf.Lerp(Copter_Anim.GetComponent<Animator>().speed, Mathf.Abs(maxSpeed / 2.5f), .3f);
            if ((goingUp && !reversed) || (!goingUp && reversed))
            {
                if (!windDown.isPlaying) { windDown.Play(); }
                if (windUp.isPlaying) { windUp.Stop(); }
            }
            else
            {
                if (!windUp.isPlaying) { windUp.Play(); }
                if (windDown.isPlaying) { windDown.Stop(); }
            }
        }
    }

    //private bool resetRotation = true;

    public void Rotate()
    {
        //Debug.Log(Mathf.Abs(transform.rotation.eulerAngles.z % 90) <= .001f);
        bool grounded_temp = checkGrounded && Physics2D.IsTouchingLayers(player_collider, groundLayer);

        if (grounded_temp)
        {
            player_body.freezeRotation = true;
            //transform.rotation = new Quaternion(0, 0, 0, 0);
            player_body.rotation = 0;
        }
        else if (player_body.velocity.x >= 0)
        {
            int rev = reversed ? -1 : 1;
            player_body.freezeRotation = false;
            //Vector3 newAngle = new Vector3(0, 0, rev * - player_body.velocity.x);
            //transform.rotation = Quaternion.Euler(newAngle);

            float newAngle = rev * -player_body.velocity.x;
            player_body.rotation = newAngle;
        }
        else
        {
            int rev = reversed ? -1 : 1;
            player_body.freezeRotation = false;
            //Vector3 newAngle = new Vector3(0, 0, rev * -player_body.velocity.x);
            //transform.rotation = Quaternion.Euler(newAngle);

            float newAngle = rev * -player_body.velocity.x;
            player_body.rotation = newAngle;
        }
    }

    public void Eyes()
    {
        int rev = 1;
        if (reversed) { rev = -1; }
        eyes.transform.localPosition = Vector3.Lerp(eyes.transform.localPosition, new Vector3((moveX / 800), rev * (player_body.velocity.y / 500), 0), .4f);
    }

    public override void Jump()
    {
        //trailUp.emitting = true;
        //trailDown.emitting = true;
        OrbComponent orbscript = null;
        if (OrbTouched != null) { orbscript = OrbTouched.GetComponent<OrbComponent>(); }

        if (maxSpeed != 15)
        {
            maxSpeed = Mathf.Lerp(maxSpeed, 15, time);
            time += 1f * Time.deltaTime;

            if (time > 1.0f)
            {
                time = 0.0f;
            }
        }

        if (teleorb_j && jump)
        {
            Vector3 positionDelta = (transform.position + teleOrb_translate) - transform.position;

            teleorb_j = false;
            teleorb = false;
            jump = false;

            if (OrbTouched != null)
            {
                orbscript.Pulse();
            }

            player_body.transform.position += teleOrb_translate;

            CinemachineVirtualCamera activeCamera = gamemanager.getActiveCamera();
            activeCamera.GetCinemachineComponent<CinemachineFramingTransposer>().OnTargetObjectWarped(transform.GetChild(0), positionDelta);
        }

        if (triggerorb_j && jump)
        {
            triggerorb_j = false;
            triggerorb = false;
            //jump = false;
            SpawnTrigger spawn = OrbTouched.GetComponent<SpawnTrigger>();
            StartCoroutine(spawn.Begin());

            if (OrbTouched != null)
            {
                orbscript.Pulse();
            }

            jump = !spawn.cancelJump;
            triggerorb = spawn.cancelJump;
        }

        if (yellow_j)
        {
            yellow = false;
            yellow_j = false;
            jump = false;
            //yellow_count++;
            //if (yellow_count >= 0) { yellow_j = false; yellow_count = 0; }
            trail.emitting = true;

            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(true);

            maxSpeed = Mathf.Abs(jumpForce) * 1.5f;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * 1.5f);
            time = 0;

            if (OrbTouched != null)
            {
                orbscript.Pulse();
            }
        }
        else if (red_j)
        {
            red = false;
            red_j = false;
            jump = false;

            trail.emitting = true;

            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(true);

            maxSpeed = Mathf.Abs(jumpForce) * 1.85f;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * 1.85f);
            time = 0;

            if (OrbTouched != null)
            {
                orbscript.Pulse();
            }
        }
        else if (pink_j)
        {
            pink = false;
            pink_j = false;
            jump = false;

            trail.emitting = true;

            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(true);

            maxSpeed = 15f;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * 1f);
            time = 0;

            if (OrbTouched != null)
            {
                orbscript.Pulse();
            }
        }
        else if (blue_j)
        {
            blue = false;
            blue_j = false;
            jump = false;
            //yellow_count++;
            //if (yellow_count >= 0) { green_j = false; yellow_count = 0; }
            trail.emitting = true;

            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(true);

            playGravityParticles();

            reversed = !reversed;
            goingUp = !goingUp;

            maxSpeed = 15f;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * .7f);

            player_body.gravityScale *= -1;
            grav_scale *= -1;
            time = 0;

            if (OrbTouched != null)
            {
                orbscript.Pulse();
            }
        }
        else if (green_j)
        {
            green = false;
            green_j = false;
            jump = false;
            //yellow_count++;
            //if (yellow_count >= 0) { green_j = false; yellow_count = 0; }
            trail.emitting = true;

            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(true);

            playGravityParticles();

            reversed = !reversed;
            goingUp = ((goingUp && reversed) || (!goingUp && !reversed)) ? reversed : !goingUp;

            if (reversed)
            {
                jumpForce = -posJump;
            }
            else
            {
                jumpForce = posJump;
            }

            maxSpeed = Mathf.Abs(jumpForce) * 1.5f;

            player_body.gravityScale *= -1;
            grav_scale *= -1;

            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * 1.5f);
            time = 0;

            if (OrbTouched != null)
            {
                orbscript.Pulse();
            }
        }
        else if (black_j)
        {
            black = false;
            black_j = false;
            jump = false;
            //black_count++;
            //if (black_count >= 5) { black_j = false; black_count = 0; }
            trail.emitting = true;

            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(true);

            maxSpeed = Mathf.Abs(jumpForce) * 2.4f;
            player_body.velocity = new Vector2(player_body.velocity.x, 0f);
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * -2.4f);
            time = 0;

            if (OrbTouched != null)
            {
                orbscript.Pulse();
            }
        }
        else if (purple_j)
        {
            int rev = 1;
            if (reversed) { rev = -1; }
            RaycastHit2D groundhit, deathhit;

            bool connect = true;

            if (!reversed)
            {
                groundhit = Physics2D.BoxCast(player_body.transform.position + new Vector3(0, .2f, 0), new Vector2(player_collider.bounds.size.x * .5f, .1f), 0f, Vector2.up, 30, groundLayer);
                deathhit = Physics2D.BoxCast(player_body.transform.position + new Vector3(0, .2f, 0), new Vector2(player_collider.bounds.size.x * .5f, .1f), 0f, Vector2.up, 30, deathLayer);
            }
            else
            {
                groundhit = Physics2D.BoxCast(player_body.transform.position + new Vector3(0, -.2f, 0), new Vector2(player_collider.bounds.size.x * .5f, .1f), 0f, -Vector2.up, 30, groundLayer);
                deathhit = Physics2D.BoxCast(player_body.transform.position + new Vector3(0, -.2f, 0), new Vector2(player_collider.bounds.size.x * .5f, .1f), 0f, -Vector2.up, 30, deathLayer);
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
                transform.position = new Vector2(transform.position.x, transform.position.y + rev * (deathhit.distance - (mini ? .1f : .3f)));
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
            purple_j = false;
            goingUp = !reversed;

            if (grav) { grav = false; }
            if (gravN) { gravN = false; }

            if (OrbTouched != null)
            {
                orbscript.Pulse();
            }
        }

        if (jump)
        {
            jump = false;
            goingUp = !goingUp;
            trail.emitting = false;

            //player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y + 3f);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(true);
        }

        player_body.AddForce(new Vector2(0, goingUp ? (mini ? 60f : 40f) : (mini ? -60f : -40f)));
        //int add = goingUp != player_body.velocity.y >= 0 ? (int)(.1f * Mathf.Abs(player_body.velocity.y)) : 0;//prevgoingUp != goingUp ? 20 : 0;
        //player_body.AddForce(new Vector2(0, goingUp ? 80f + add : -80f - add));
    }

    public override void Pad()
    {
        if (maxSpeed != 15)
        {
            maxSpeed = Mathf.Lerp(maxSpeed, 15, time);
            time += 1f * Time.deltaTime;

            if (time > 1.0f)
            {
                time = 0.0f;
            }
        }

        if (yellow_p)
        {
            yellow_p = false;
            trail.emitting = true;

            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(true);

            maxSpeed = Mathf.Abs(jumpForce) * 1.7f;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * 1.7f);
            time = 0;
        }
        else if (red_p)
        {
            red_p = false;
            trail.emitting = true;

            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(true);

            maxSpeed = Mathf.Abs(jumpForce) * 2f;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * 2f);
            time = 0;
        }
        else if (pink_p)
        {
            pink_p = false;
            trail.emitting = true;

            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(true);

            maxSpeed = Mathf.Abs(jumpForce) * 1.2f;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * 1.2f);
            time = 0;
        }
        else if (blue_p)
        {
            blue_p = false;
            trail.emitting = true;

            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(true);

            reversed = !reversed;
            goingUp = !goingUp;

            maxSpeed = Mathf.Abs(jumpForce) * 1.2f;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * 1.2f);

            player_body.gravityScale *= -1;
            grav_scale *= -1;
            time = 0;
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
                goingUp = !goingUp;
                jumpForce = -posJump;

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
                goingUp = !goingUp;
                jumpForce = posJump;

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

            if (reversed)
            {
                reversed = false;
                goingUp = !goingUp;
                jumpForce = posJump;

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
                goingUp = !goingUp;
                jumpForce = -posJump;

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
            //trailUp.enabled = true;
            //trailDown.emitting = true;
            teleA = false;
            player_body.transform.position += teleB;
            //trail.enabled = true;
            CinemachineVirtualCamera activeCamera = gamemanager.getActiveCamera();
            activeCamera.GetCinemachineComponent<CinemachineFramingTransposer>().OnTargetObjectWarped(transform.GetChild(0), positionDelta);
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
        //theScale.y *= -1;
        theScale.y = upright ? Mathf.Abs(theScale.y) : -Mathf.Abs(theScale.y);
        transform.localScale = theScale;
    }

    public override void Respawn()
    {
        able = false;
        if (restartmusic) { bgmusic.Stop(); }

        player_collider.enabled = false;
        StopAllCoroutines();
        player_body.velocity = Vector2.zero;

        grounded_particles.Stop();
        ground_impact_particles.Stop();
        windUp.Stop();
        windDown.Stop();

        jump = false;

        yellow = false; pink = false; red = false; green = false; blue = false; black = false;
        reversed = respawn_rev;
        mini = respawn_mini;
        ChangeSize();

        if (reversed)
        {
            upright = false;
            player_body.gravityScale = -Mathf.Abs(player_body.gravityScale);
            grav_scale = player_body.gravityScale;
            transform.rotation = new Quaternion(0, 0, 180, 0);
        }
        else
        {
            upright = true;
            player_body.gravityScale = Mathf.Abs(player_body.gravityScale);
            grav_scale = player_body.gravityScale;
            transform.rotation = new Quaternion(0, 0, 0, 0);
        }

        player_renderer.SetActive(false);
        //player_renderer.enabled = false;
        //death_animation.GetComponent<SpriteRenderer>().enabled = true;
        death_particles.Play();
        death_sfx.PlayOneShot(death_sfx.clip, gamemanager.sfx_volume);
        player_body.velocity = Vector3.zero;
        player_body.gravityScale = 0;
        goingUp = false;

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
        player_collider.enabled = true;
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
        reversed = false; jump = false; yellow = false; pink = false; red = false; green = false; blue = false; purple = false;  black = false; teleA = false;
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
        copter.SetActive(false);
        //trailUp.gameObject.SetActive(false);
        trail.gameObject.SetActive(true);
        windUp.Stop();
        windDown.Stop();

        //player_collider.isTrigger = false;
        player_collider.enabled = true;
    }
    public override void setColliders()
    {
        player_collider.enabled = true;
        //player_collider.isTrigger = true;
    }

    public void setGoingUp(bool b)
    {
        goingUp = b;
    }

    public bool getGoingUp()
    {
        return goingUp;
    }

    public override string getMode()
    {
        return "copter";
    }
}
