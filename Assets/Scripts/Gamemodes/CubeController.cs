using System.Collections;
using UnityEngine;
using Cinemachine;

public class CubeController : PlayerController
{
    public Collider2D player_collider, crouch_collider;

    public TrailRenderer trail;

    public GameObject copter;
    public GameObject jetpack;
    public GameObject ship;
    public GameObject ufo;
    public GameObject wave;
    public GameObject ball;
    public GameObject spider;

    private float jumpForce = 21f;//20f;
    private float posJump;

    private float moveX, grav_scale;
    private float smoothing;

    private float time = 0;

    private float maxSpeed = 110f;

    private Vector3 impact_position = Vector3.zero;

    public ParticleSystem leftspark, rightspark, centerspark;

    public override void Awake2()
    {
        speed = speed1;
        moveX = 0;
        smoothing = .05f;
        v_Velocity = Vector3.zero;
        posJump = jumpForce;
        player_collider = GetComponent<Collider2D>();

        grav_scale = player_body.gravityScale;

        crouch_collider.enabled = false;
        setRespawn(transform.position, reversed, mini);
        setRepawnSpeed(1f);

        setAnimation();
    }

    
    private void Start()
    {
        /*if (!bgmusic.isPlaying)
        {
            bgmusic.Play();
        }*/
    }

    public override void setAnimation()
    {
        player_body.freezeRotation = true;
        player_body.gravityScale = MAIN_GRAVITY_CUBE * gravityMultiplier;
        if (reversed) { player_body.gravityScale *= -1; }
        grav_scale = player_body.gravityScale;

        grounded_particles.gameObject.transform.localPosition = new Vector3(0, -.52f, 0);
        ground_impact_particles.gameObject.transform.localPosition = new Vector3(0, -.52f, 0);

        grounded_particles.gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
        ground_impact_particles.gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);

        ChangeSize();

        icon.transform.localScale = new Vector3(1f, 1f, 1f);
        icon.transform.localPosition = new Vector3(0, 0, 0);
        copter.SetActive(false);
        jetpack.SetActive(false);
        ship.SetActive(false);
        ufo.SetActive(false);
        ball.SetActive(false);
        spider.SetActive(false);
        wave.SetActive(false);
        icon.SetActive(true);

        trail.transform.localPosition = new Vector3(0, 0, 0);

        Cube_Anim.ResetTrigger("Crouch");
        Cube_Anim.ResetTrigger("Squash");
        Cube_Anim.ResetTrigger("DeepSquash");
        Cube_Anim.ResetTrigger("Stretch");
        Cube_Anim.SetTrigger("Default");

        eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
        eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
        eyes.transform.Find("Eyes_Wide").gameObject.SetActive(false);
        eyes.transform.Find("Eyes_Normal").gameObject.SetActive(true);
    }

    public override void ChangeSize()
    {
        int rev = reversed ? -1 : 1;
        bool currMini = transform.localScale.x < .5;

        if (mini)
        {
            grounded_particles.startLifetime = .15f;
            ground_impact_particles.startLifetime = .15f;
            grounded_particles.transform.localScale = new Vector2(.47f, .47f);
            ground_impact_particles.transform.localScale = new Vector2(.47f, .47f);
            transform.localScale = new Vector2(.47f, .47f);
            transform.position = transform.position - new Vector3(0, (currMini ? 0 : 1 ) * rev * .29f, 0);
            jumpForce = 16.5f;
        }
        else
        {
            grounded_particles.startLifetime = .3f;
            ground_impact_particles.startLifetime = .3f;
            grounded_particles.transform.localScale = new Vector2(1, 1f);
            ground_impact_particles.transform.localScale = new Vector2(1f, 1f);
            transform.localScale = new Vector2(1.05f, 1.05f);
            transform.position = transform.position + new Vector3(0, (!currMini ? 0 : 1) * rev * .29f, 0);
            jumpForce = 21f;
        }

        posJump = jumpForce;
        if (reversed) { jumpForce *= -1; }
    }

    void Update()
    {
        able = false;
        if (able)
        {
            // CHECK IF DEAD
            dead = Physics2D.IsTouchingLayers(player_collider, deathLayer) || Physics2D.IsTouchingLayers(crouch_collider, deathLayer);
            //grounded = Physics2D.Raycast(player_body.transform.position, Vector2.down, .51f, groundLayer);

            // CHECK IF GROUNDED
            if (reversed)
            {
                grounded = Physics2D.BoxCast(player_body.transform.position, new Vector2(mini ? .45f : .95f, .1f), 0f, Vector2.up, .51f, groundLayer) && checkGrounded
                        && (Physics2D.IsTouchingLayers(player_collider, groundLayer) || Physics2D.IsTouchingLayers(crouch_collider, groundLayer));
                regate = -1;
                grounded_particles.gravityModifier = -Mathf.Abs(grounded_particles.gravityModifier);
                ground_impact_particles.gravityModifier = -Mathf.Abs(ground_impact_particles.gravityModifier);
            }
            else
            {//.9
                grounded = Physics2D.BoxCast(player_body.transform.position, new Vector2(mini ? .45f : .95f, .1f), 0f, Vector2.down, .51f, groundLayer) && checkGrounded
                        && (Physics2D.IsTouchingLayers(player_collider, groundLayer) || Physics2D.IsTouchingLayers(crouch_collider, groundLayer));
                regate = 1;
                grounded_particles.gravityModifier = Mathf.Abs(grounded_particles.gravityModifier);
                ground_impact_particles.gravityModifier = Mathf.Abs(ground_impact_particles.gravityModifier);
            }

            //Debug.Log("Grounded: " + grounded);

            bool touchingGround = (Physics2D.IsTouchingLayers(player_collider, groundLayer) || Physics2D.IsTouchingLayers(crouch_collider, groundLayer));
            // IF GROUNDED --> TURN OFF TRAIL
            if (touchingGround && !prev_grounded)
            {
                if (player_body.rotation != (reversed ? 180 : 0))
                {
                    player_body.rotation = reversed ? 180 : 0;
                }

                crouchJumpUsed = false;

                //trail.emitting = false;
                eyes.transform.Find("Eyes_Normal").gameObject.SetActive(true);
                eyes.transform.Find("Eyes_Wide").gameObject.SetActive(false);
                eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
                eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);

                if (player_body.velocity.y < 1f)
                {
                    if (prev_velocity >= 40)
                    {
                        Cube_Anim.ResetTrigger("Crouch");
                        Cube_Anim.ResetTrigger("Default");
                        Cube_Anim.ResetTrigger("Stretch");
                        Cube_Anim.ResetTrigger("Squash");
                        Cube_Anim.ResetTrigger("DeepSquash");
                        Cube_Anim.Play("HeightDeepSquash", -1, 0f);
                    }
                    else if (prev_velocity >= 12)
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
            else if(!touchingGround)
            {
                if (crouchtimer < 0.01f)
                    crouchtimer = 0.01f;
            }

            if(grounded) trail.emitting = false;

            // set cube upright when touching ground
            /*bool touchingGround = (Physics2D.IsTouchingLayers(player_collider, groundLayer) || Physics2D.IsTouchingLayers(crouch_collider, groundLayer));
            if (touchingGround && player_body.rotation != (reversed ? 180 : 0))
            {
                player_body.rotation = reversed ? 180 : 0;
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
            //moveX = Input.GetAxisRaw("Horizontal") * speed;
            moveX = input.Player.MovementHorizontal.ReadValue<float>() * speed;

            // Grounded Particles
            if(grounded && (Mathf.Abs(player_body.velocity.x) > .1f || jump))
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
            if (gameObject.scene.IsValid() && (input.Player.Jump.triggered || Input.GetButtonDown("Jump") || Input.GetKeyDown("space") || Input.GetMouseButtonDown(0)))
            {
                jump = true;
                jump_ground = true;
                released = false;
                fromGround = ((grounded || time < .07f) && jump);

                if (!reversed && player_body.velocity.y - (Mathf.Clamp(movingVelocity.y, 0, Mathf.Abs(movingVelocity.y))) <= 1)
                {
                    downjump = true;
                }
                else if (reversed && player_body.velocity.y - (Mathf.Clamp(movingVelocity.y, -Mathf.Abs(movingVelocity.y), 0)) >= -1)
                {
                    downjump = true;
                }
                else
                {
                    downjump = false;
                }
            }

            //Debug.Log(jumpStarted);

            // RELEASE JUMP
            if (prevJumpNotReleased && input.Player.Jump.ReadValue<float>() == 0)//Input.GetButtonUp("Jump") || Input.GetKeyUp("space") || Input.GetMouseButtonUp(0))
            {
                jump = false;
                jump_ground = false;
                released = true;
            }

            float hitDist = mini ? 0 : .65f;
            if (!reversed)
            {
                headHit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y - .2f), Vector2.up, hitDist, groundLayer);
            }
            else
            {
                headHit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y + .2f), -Vector2.up, hitDist, groundLayer);
            }
            int rev = reversed ? -1 : 1;
            Debug.DrawLine(transform.position - new Vector3(-1, rev * .2f, 0), transform.position + new Vector3(1, rev * hitDist, 0), Color.red);
            //Debug.Log("headHit: " + headHit.distance);

            //Debug.Log("Crouch Value: " + input.Player.MovementVertical.ReadValue<float>());
            // CROUCH
            if (gameObject.scene.IsValid() &&
                (input.Player.Crouch.ReadValue<float>() >= .7f || input.Player.MovementVertical.ReadValue<float>() <= -.7f || Input.GetAxisRaw("Vertical") <= -.7f || Input.GetKey(KeyCode.LeftShift) || Input.GetMouseButton(1) || headHit.distance > 0))
            {
                crouch = true;
            }
            else// if(prevCrouchNotReleased && input.Player.Crouch.ReadValue<float>() == 0)
            {
                crouch = false;
                crouchReleased = true;
                //crouchtimer = 0;
                //offCrouchTimer = 0;
            }

            if (crouched)
            {
                offCrouchTimer = 0;
                crouchJumpTimer += Time.deltaTime;
            }
            else
            {
                crouchtimer = 0;
                offCrouchTimer += Time.deltaTime;
            }

            //crouchtimer += Time.deltaTime;
            //Debug.Log("CROUCHTIME: " + crouchtimer);
            crouchJump = (crouchJumpTimer >= CROUCH_JUMP_OPENING && crouchJumpTimer < CROUCH_JUMP_WINDOW + CROUCH_JUMP_OPENING) && (crouched || offCrouchTimer < .2f) && crouchReleased;
            /*if (crouchJump)
            {
                if (moveX > .1f)
                {
                    leftspark.Play();
                }
            }*/

            //Debug.Log(crouchJump);
            if(chargeTeleportTimer > 0)
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
                dead = false;
                Respawn();
            }

            prev_grounded = grounded;
            prev_velocity = Mathf.Abs(player_body.velocity.y);

            time += Time.deltaTime;
            if(prev_grounded) { time = 0; }

            prevJumpNotReleased = input.Player.Jump.triggered || input.Player.Jump.ReadValue<float>() == 1;
            prevCrouchNotReleased = input.Player.Crouch.triggered || input.Player.Crouch.ReadValue<float>() == 1;
            //prevCrouchJump = crouchJump;

            //Debug.Log(crouchJump + "    " + cJReady);
        }
    }

    void FixedUpdate()
    {
        if(able)
        {
            Move();
            Interpolate(-1, -1);
        }

        //Debug.Log(able + " : " + player_body.velocity);
    }

    public override void Move()
    {
        crouchtimer += Time.fixedDeltaTime;
        if(disableJumpTimer > 0) disableJumpTimer -= Time.fixedDeltaTime;
        //player_body.velocity -= MovingObjectVelocities.Count > 0 ? movingVelocity : Vector2.zero;
        player_body.velocity -= new Vector2(movingVelocity.x, movingVelocity.y);

        // If the input is moving the player right and the player is facing left...
        if ((!reversed && moveX > 0 && !facingright) || (reversed && moveX < 0 && !facingright && (fromGround || grounded)))
        {
            // ... flip the player.
            negate = 1;
            facingright = !facingright;
        }
        // Otherwise if the input is moving the player left and the player is facing right...
        else if ((!reversed && moveX < 0 && facingright) || (reversed && moveX > 0 && facingright && (fromGround || grounded)))
        {
            // ... flip the player.
            negate = -1;
            facingright = !facingright;
        }
        //Debug.Log("CROUCH TIMER: " + ((crouch && crouchtimer >= 0.01f) && grounded));
        // if crouching, change movement controls
        if (headHit.distance > 0)
        {
            crouch_collider.enabled = true;
            player_collider.enabled = false;

            Cube_Anim.SetTrigger("Crouch");

            crouched = true;

            if (!grounded)
            {
                Vector2 targetVelocity = new Vector2(moveX * Time.fixedDeltaTime * 10f, player_body.velocity.y);
                player_body.velocity = Vector3.SmoothDamp(player_body.velocity, targetVelocity, ref v_Velocity, 0);
            }
        }
        else if ((crouch && crouchtimer >= 0.01f) && grounded)
        {
            if (!crouched)
            {
                Cube_Anim.ResetTrigger("Default");
                Cube_Anim.ResetTrigger("Squash");
                Cube_Anim.ResetTrigger("Stretch");
                Cube_Anim.ResetTrigger("DeepSquash");
                Cube_Anim.SetTrigger("Crouch");
            }

            moveX = 0;
            crouch_collider.enabled = true;
            player_collider.enabled = false;
            Vector2 targetVelocity = new Vector2(moveX * Time.fixedDeltaTime * 10f, Mathf.Clamp(player_body.velocity.y, -maxSpeed, maxSpeed));

            if (!crouched)
            {
                player_body.velocity = Vector3.SmoothDamp(player_body.velocity * 1.6f, targetVelocity, ref v_Velocity, smoothing * 7f / gravityMultiplier);
                crouched = true;
                crouchJumpTimer = 0;
            }
            else
            {
                player_body.velocity = Vector3.SmoothDamp(player_body.velocity, targetVelocity, ref v_Velocity, smoothing * 7f / gravityMultiplier);
            }

            //crouched = true;
            //player_body.velocity = Vector3.SmoothDamp(player_body.velocity, targetVelocity, ref v_Velocity, smoothing * 7);
            //speed 55      *7      *1      *1.5
        }
        else
        {
            if (crouched)
            {
                crouched = false;
                Cube_Anim.ResetTrigger("Crouch");
                Cube_Anim.ResetTrigger("Squash");
                Cube_Anim.ResetTrigger("DeepSquash");
                Cube_Anim.ResetTrigger("Stretch");
                Cube_Anim.SetTrigger("Default");
            }

            player_collider.enabled = true;
            crouch_collider.enabled = false;

            bool cancelCharge = true; float clampedTimer = Mathf.Clamp(chargeTeleportTimer, 0, 1);
            if (chargeTeleportTimer > 0 && ((!grounded && (Mathf.Abs(chargedTeleportVelocity.x) * clampedTimer) > Mathf.Abs(moveX)) || chargeTeleportTimer > 1)) cancelCharge = false;

            Vector2 targetVelocity = new Vector2((cancelCharge ? moveX : (moveX * (1 - clampedTimer) + chargedTeleportVelocity.x * clampedTimer)) * Time.fixedDeltaTime * 10f, player_body.velocity.y);
            if (crouchJumpUsed)
            {
                int rev = moveX > 0 ? 1 : -1;
                rev = moveX != 0 ? rev : 0;
                targetVelocity = new Vector2((speed + CROUCH_JUMP_SPEED) * rev * Time.fixedDeltaTime * 10f, player_body.velocity.y);

                launched = true;

                if (rev == 0) crouchJumpUsed = false;
            }
            //Vector2 targetVelocity = new Vector2(moveX * Time.fixedDeltaTime * 10f, player_body.velocity.y);

            if (player_body.velocity != targetVelocity)
            {
                if (Mathf.Abs(targetVelocity.x) > Mathf.Abs(player_body.velocity.x) || !grounded)
                {
                    player_body.velocity = Vector3.SmoothDamp(player_body.velocity, targetVelocity, ref v_Velocity, smoothing * ((!grounded && !cancelCharge) ? 5 : .7f) / gravityMultiplier); // .7
                }
                else
                {
                    player_body.velocity = Vector3.SmoothDamp(player_body.velocity, targetVelocity, ref v_Velocity, smoothing * 1.1f / gravityMultiplier); //1.1
                }
            }
            //prev_TargetVelocity = new Vector2(player_body.velocity.x, player_body.velocity.y);
        }

        movingVelocity = MovingObjectVelocities.Count > 0 ? MovingObjectVelocities[MovingObjectVelocities.Count-1].velocity : Vector2.zero;
        //player_body.velocity += movingVelocity;
        player_body.velocity += new Vector2(movingVelocity.x, movingVelocity.y);

        if (crouchJump && enablecrouchjump)
        {
            crouchJumpCounter++;
            if (crouchJumpCounter == 1)
            {
                if (Mathf.Max(player_body.velocity.x, input.Player.MovementHorizontal.ReadValue<float>()) > .1f)//(input.Player.MovementHorizontal.ReadValue<float>() > .1f)
                {
                    cJReady = true;
                    if (!reversed) { leftspark.Play(); }
                    else { rightspark.Play(); }
                }
                else if (Mathf.Min(player_body.velocity.x, input.Player.MovementHorizontal.ReadValue<float>()) < -.1f)
                {
                    cJReady = true;
                    if (!reversed) { rightspark.Play(); }
                    else { leftspark.Play(); }
                }
                else
                {
                    cJReady = true;
                    centerspark.Play();
                }
            }
        }
        else
        {
            cJReady = false;
            crouchJumpCounter = 0;
        }

        /*if(Mathf.Abs(chargedTeleportVelocity.magnitude) > .5f)
        {
            chargedTeleportVelocity *= 0.98f;
        }
        else
        {
            chargedTeleportVelocity = Vector3.zero;
        }*/

        //Debug.Log(chargedTeleportVelocity);

        Eyes();
        Jump();
        Pad();      // check if hit pad
        Portal();   // check if on portal
        Rotate();
    }

    private bool cJReady;
    private bool launched = false;
    public void Rotate()
    {
        bool touchingGround = (Physics2D.IsTouchingLayers(player_collider, groundLayer) || Physics2D.IsTouchingLayers(crouch_collider, groundLayer));
        //Debug.Log("Rotation: " + player_body.rotation);

        if(grounded && !crouchJumpUsed)
            launched = false;

        if (touchingGround)
        {
            player_body.rotation = reversed ? 180 : 0;
        }
        else if (!grounded && launched)
        {
            float step = 12.5f;
            int rev = reversed ? -1 : 1;

            if(touchingGround)
                launched = false;

            if (Mathf.Abs(player_body.velocity.x) < 2)
            {
                player_body.rotation = Mathf.Lerp(player_body.rotation, reversed ? 180 : 0, .04f);
            }
            else
            {
                if (player_body.velocity.x >= 0)
                {
                    player_body.rotation = Mathf.Lerp(player_body.rotation, player_body.rotation - step * rev, .8f);
                }
                else
                {
                    player_body.rotation = Mathf.Lerp(player_body.rotation, player_body.rotation + step * rev, .8f);
                }
            }

            if (player_body.rotation > 360) player_body.rotation -= 360;
            else if (player_body.rotation < -360) player_body.rotation += 360;
        }
        else if(!grounded && !launched)
        {
            float step = 12.5f;
            if(reversed && player_body.rotation != 180)
            {
                if(player_body.velocity.x >= 0)
                {
                    player_body.rotation = Mathf.Lerp(player_body.rotation, player_body.rotation + step, .8f);
                }
                else
                {
                    player_body.rotation = Mathf.Lerp(player_body.rotation, player_body.rotation - step, .8f);
                }
            }
            else if (!reversed && player_body.rotation != 0)
            {
                if (player_body.velocity.x >= 0)
                {
                    player_body.rotation = Mathf.Lerp(player_body.rotation, player_body.rotation - step, .8f);
                }
                else
                {
                    player_body.rotation = Mathf.Lerp(player_body.rotation, player_body.rotation + step, .8f);
                }
            }
        }
    }

    public void Eyes()
    {
        int rev = 1;
        if (reversed) { rev = -1; }
        eyes.transform.localPosition = Vector3.Lerp(eyes.transform.localPosition, new Vector3(rev * (moveX/800), 0 * rev * (player_body.velocity.y/200), 0), .4f);
    }

    public override void Jump()
    {
        jumpForce += movingVelocity.y;
        //Debug.Log((grounded || time < .08f));

        OrbComponent orbscript = null;
        float multiplier = 1;
        if (OrbTouched != null) { orbscript = OrbTouched.GetComponent<OrbComponent>(); multiplier = orbscript.multiplier; }

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

        if(triggerorb && jump)
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
            launched = true;
            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(true);

            fromGround = false;
            released = false;
            jump = false;
            yellow = false;
            crouchJumpUsed = false;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * 1.1f * multiplier);
            trail.emitting = true;
            //StartCoroutine(RotateArc(Vector3.forward, negate * -30.0f, 0.5f));

            if (grav) { grav = false; }
            if (gravN) { gravN = false; }

            if(OrbTouched != null)
            {
                orbscript.Pulse();
            }
        }
        else if (pink && jump)
        {
            chargeTeleportTimer = 0;
            launched = true;
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(true);

            fromGround = false;
            released = false;
            jump = false;
            pink = false;
            crouchJumpUsed = false;
            trail.emitting = true;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * .95f * multiplier);
            //StartCoroutine(RotateArc(Vector3.forward, negate * -25.0f, 0.5f));

            if (grav) { grav = false; }
            if (gravN) { gravN = false; }

            if (OrbTouched != null)
            {
                orbscript.Pulse();
            }
        }
        else if (red && jump)
        {
            chargeTeleportTimer = 0;
            launched = true;
            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(true);

            fromGround = false;
            released = false;
            jump = false;
            red = false;
            crouchJumpUsed = false;
            trail.emitting = true;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * 1.45f * multiplier);

            if (grav) { grav = false; }
            if (gravN) { gravN = false; }

            if (OrbTouched != null)
            {
                orbscript.Pulse();
            }
        }
        else if (blue && jump)
        {
            chargeTeleportTimer = 0;
            launched = false;
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(true);

            playGravityParticles();

            fromGround = false;
            released = false;
            jump = false;
            blue = false;
            crouchJumpUsed = false;
            trail.emitting = true;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * .4f * multiplier);
            reversed = !reversed;
            player_body.gravityScale *= -1;
            grav_scale *= -1;
            grounded = false;
            //StartCoroutine(RotateAround(Vector3.forward, regate * negate * -180.0f, 0.4f));

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
            launched = false;
            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(true);

            playGravityParticles();

            fromGround = false;
            released = false;
            jump = false;
            green = false;
            crouchJumpUsed = false;
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
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * multiplier);
            player_body.gravityScale *= -1;
            grav_scale *= -1;

            //StartCoroutine(RotateAround(Vector3.forward, regate * negate * 180.0f, 0.5f));

            if (grav) { grav = false; }
            if (gravN) { gravN = false; }

            if (OrbTouched != null)
            {
                orbscript.Pulse();
            }
        }
        else if (black && jump)
        {
            chargeTeleportTimer = 0;
            launched = false;
            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(true);

            fromGround = false;
            black = false;
            released = false;
            jump = true;
            crouchJumpUsed = false;
            downjump = true;
            trail.emitting = true;
            player_body.velocity = new Vector2(player_body.velocity.x, -jumpForce * 1.1f * multiplier);

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

            if(grounded) purple = false;
            if (!connect) return;
            launched = false;

            fromGround = false;
            released = false;
            jump = false;
            purple = false;
            trail.emitting = true;

            if (grav) { grav = false; }
            if (gravN) { gravN = false; }

            if (OrbTouched != null)
            {
                orbscript.Pulse();
            }
        }
        else if (disableJumpTimer <= 0 && (grounded || time < .08f) && (holdjump ? jump_ground : jump && downjump))
        {
            time = 1;
            Cube_Anim.ResetTrigger("Crouch");
            Cube_Anim.ResetTrigger("Default");
            Cube_Anim.ResetTrigger("Squash");
            Cube_Anim.ResetTrigger("DeepSquash");
            Cube_Anim.ResetTrigger("Stretch");
            Cube_Anim.Play("HeightStretch", -1, 0f);
            trail.emitting = false;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * (crouchJump && cJReady/*(Mathf.Max(Mathf.Abs(player_body.velocity.x), Mathf.Abs(input.Player.MovementHorizontal.ReadValue<float>())) > .1f)*/ ? CROUCH_JUMP_STRENGTH_MULTIPLIER : 1));
            grounded = false;

            //Debug.Log((int)jumpForce + "    " + (int)movingVelocity.y);

            if (crouchJump && cJReady)//(Mathf.Max(Mathf.Abs(player_body.velocity.x), Mathf.Abs(input.Player.MovementHorizontal.ReadValue<float>()))) > .1f)
            {
                crouchJumpUsed = true;
                crouchReleased = false;
                cJReady = false;
            }
            else
            {
                launched = false;
            }

            /* fuck me
            if (Mathf.Abs(moveX) > 0)
            {
                checkGrounded = false;
                StartCoroutine(RotateArc(Vector3.forward, negate * -10.0f, 0.2f));
                checkGrounded = true;
            }//*/

            fromGround = true;
            jump = false;
            downjump = holdjump;
        }
        else if (fromGround && ((!reversed && released && player_body.velocity.y > 0) || (reversed && released && player_body.velocity.y < 0)))
        {
            Cube_Anim.ResetTrigger("Crouch");
            Cube_Anim.ResetTrigger("Default");
            Cube_Anim.ResetTrigger("Squash");
            Cube_Anim.ResetTrigger("DeepSquash");
            Cube_Anim.ResetTrigger("Stretch");
            //Cube_Anim.SetTrigger("Default");
            player_body.velocity /= 2;
            released = false;
            fromGround = false;
        }
        
        jumpForce -= movingVelocity.y;
    }

    public override void Pad()
    {
        jumpForce += movingVelocity.y;
        if (yellow_p)
        {
            launched = true;
            yellow_p = false;
            checkGrounded = false;
            grounded = false;
            jump = false;

            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(true);

            fromGround = false;
            released = false;
            trail.emitting = true;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * 1.4f);

            checkGrounded = true;
        }
        else if (pink_p)
        {
            launched = true;
            pink_p = false;
            checkGrounded = false;
            grounded = false;
            jump = false;

            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(true);

            fromGround = false;
            released = false;
            trail.emitting = true;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * .9f);

            checkGrounded = true;
        }
        else if (red_p)
        {
            launched = true;
            red_p = false;
            checkGrounded = false;
            grounded = false;
            jump = false;

            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(true);

            fromGround = false;
            released = false;
            trail.emitting = true;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * 1.6f);
            grounded = false;

            checkGrounded = true;
        }
        else if (blue_p && disableJumpTimer <= 0)
        {
            launched = false;
            blue_p = false;
            checkGrounded = false;
            grounded = false;
            fromGround = false;
            released = false;

            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(true);

            trail.emitting = true;
            player_body.velocity = new Vector2(player_body.velocity.x, jumpForce * .4f);
            reversed = !reversed;
            player_body.gravityScale *= -1;
            grav_scale *= -1;

            //StartCoroutine(RotateAround(Vector3.forward, regate * negate * -180.0f, 0.4f));
            checkGrounded = true;
            //disableJumpTimer = .5f;
            disableJumpTimer = .2f;
        }
        if (green_p && disableJumpTimer <= 0)
        {
            launched = true;
            green_p = false;
            checkGrounded = false;
            grounded = false;
            jump = false;

            eyes.transform.Find("Eyes_Normal").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Irked").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Squint").gameObject.SetActive(false);
            eyes.transform.Find("Eyes_Wide").gameObject.SetActive(true);

            fromGround = false;
            released = false;
            trail.emitting = true;
            reversed = !reversed;
            player_body.gravityScale *= -1;
            grav_scale *= -1;
            player_body.velocity = new Vector2(player_body.velocity.x, -jumpForce * 1.5f);

            checkGrounded = true;
            disableJumpTimer = .2f;
        }
        jumpForce -= movingVelocity.y;
    }

    public override void Portal()
    {
        if (grav)
        {
            grav = false;

            if (!reversed)
            {
                fromGround = false;
                released = false;

                reversed = true;
                jumpForce = -posJump;
                trail.emitting = true;
                /*
                if (Mathf.Abs(player_body.velocity.y) > maxSpeed * .6f)
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .6f);
                }
                else
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .8f);
                }*/
                if (player_body.velocity.y <= -15f)
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, -15f);
                }

                player_body.gravityScale = -Mathf.Abs(player_body.gravityScale);
                grav_scale = player_body.gravityScale;
                //StartCoroutine(RotateAround(Vector3.forward, regate * negate * -180.0f, 0.5f));
            }
        }
        else if (gravN)
        {
            gravN = false;

            if (reversed)
            {
                fromGround = false;
                released = false;

                reversed = false;
                jumpForce = posJump;
                trail.emitting = true;
                /*
                if (Mathf.Abs(player_body.velocity.y) > maxSpeed * .6f)
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .5f);
                }
                else
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .75f);
                }*/
                if (player_body.velocity.y >= 15f)
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, 15f);
                }

                player_body.gravityScale = Mathf.Abs(player_body.gravityScale);
                grav_scale = player_body.gravityScale;
                //StartCoroutine(RotateAround(Vector3.forward, regate * negate * -180.0f, 0.5f));
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
                /*
                if (Mathf.Abs(player_body.velocity.y) > maxSpeed * .6f)
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .5f);
                }
                else
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .75f);
                }*/
                if (player_body.velocity.y >= 15f)
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, 15f);
                }

                player_body.gravityScale = Mathf.Abs(player_body.gravityScale);
                grav_scale = player_body.gravityScale;
                //StartCoroutine(RotateAround(Vector3.forward, regate * negate * -180.0f, 0.5f));
            }
            else if (!reversed)
            {
                reversed = true;
                jumpForce = -posJump;
                trail.emitting = true;
                /*
                if (Mathf.Abs(player_body.velocity.y) > maxSpeed * .6f)
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .6f);
                }
                else
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .8f);
                }*/
                if (player_body.velocity.y <= -15f)
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, -15f);
                }

                player_body.gravityScale = -Mathf.Abs(player_body.gravityScale);
                grav_scale = player_body.gravityScale;
                //StartCoroutine(RotateAround(Vector3.forward, regate * negate * -180.0f, 0.5f));
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

            CinemachineVirtualCamera activeCamera = gamemanager.getActiveCamera();
            activeCamera.GetCinemachineComponent<CinemachineFramingTransposer>().OnTargetObjectWarped(transform.GetChild(0), positionDelta);
        }
        else if (chargedTeleA)
        {
            Vector3 positionDelta = (transform.position + teleB) - transform.position;

            trail.emitting = false;
            //trail.Clear();
            trail.enabled = true;
            chargedTeleA = false;
            player_body.transform.position += chargedTeleB;
            //Debug.Log("PlayerVY: " + player_body.velocity + "   OutVY: " + chargedTeleportVelocity);
            player_body.velocity = chargedTeleportVelocity;
            released = false;
            trail.emitting = true;

            CinemachineVirtualCamera activeCamera = gamemanager.getActiveCamera();
            activeCamera.GetCinemachineComponent<CinemachineFramingTransposer>().OnTargetObjectWarped(transform.GetChild(0), positionDelta);
        }
    }

    public override void Flip()
    {
        // Switch the way the player is labelled as facing.
        facingright = !facingright;

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

        player_body.velocity = Vector2.zero;
        crouch_collider.enabled = false;
        player_collider.enabled = false;
        StopAllCoroutines();
        trail.emitting = false;
        jump = false;
        jump_ground = false;
        yellow = false; pink = false; red = false; green = false; blue = false; black = false; purple = false;
        reversed = respawn_rev;
        mini = respawn_mini;
        ChangeSize();

        if (reversed)
        {
            player_body.gravityScale = -Mathf.Abs(player_body.gravityScale);
            grav_scale = player_body.gravityScale;
            //transform.rotation = new Quaternion(0, 0, 180, 0);
            player_body.rotation = 180;
        }
        else
        {
            player_body.gravityScale = Mathf.Abs(player_body.gravityScale);
            grav_scale = player_body.gravityScale;
            //transform.rotation = new Quaternion(0, 0, 0, 0);
            player_body.rotation = 0;
        }

        //player_renderer.enabled = false;
        player_renderer.SetActive(false);
        //death_animation.GetComponent<SpriteRenderer>().enabled = true;
        death_particles.Play();
        death_sfx.PlayOneShot(death_sfx.clip, gamemanager.sfx_volume);
        player_body.gravityScale = 0;

        Invoke("undead", 1);
    }

    public void undead()
    {
        speed = respawn_speed;
        Vector3 positionDelta = respawn - transform.position;
        transform.position = respawn;

        CinemachineVirtualCamera activeCamera = gamemanager.getActiveCamera();
        activeCamera.GetCinemachineComponent<CinemachineFramingTransposer>().OnTargetObjectWarped(transform.GetChild(0), positionDelta); //activeCamera.Follow;
        /*int numVcams = CinemachineCore.Instance.VirtualCameraCount;
        for (int i = 0; i < numVcams; ++i)
        {
            CinemachineCore.Instance.GetVirtualCamera(i).OnTargetObjectWarped(transform, posDelta);
        }*/

        crouch = false;
        Cube_Anim.ResetTrigger("Crouch");
        Cube_Anim.ResetTrigger("Squash");
        Cube_Anim.ResetTrigger("DeepSquash");
        Cube_Anim.ResetTrigger("Stretch");
        Cube_Anim.SetTrigger("Default");
        Cube_Anim.transform.localPosition = new Vector3(0, 0, 0);

        player_renderer.SetActive(true);
        player_collider.enabled = true;
        player_body.gravityScale = grav_scale;

        //bgmusic.volume = 1;
        if (restartmusic)
        {
            gamemanager.setToNewBGMusic();
            bgmusic.Play();
        }

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
        reversed = false;  yellow = false; pink = false; red = false; green = false; blue = false; black = false; purple = false; teleA = false;
    }

    public override void setRepawnSpeed(float s)
    {
        if(s == 0) { respawn_speed = speed0; }
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
        player_collider.enabled = false;
        crouch_collider.enabled = false;

        Cube_Anim.ResetTrigger("Crouch");
        Cube_Anim.ResetTrigger("Squash");
        Cube_Anim.ResetTrigger("DeepSquash");
        Cube_Anim.ResetTrigger("Stretch");
        Cube_Anim.SetTrigger("Default");
    }

    public override void setColliders()
    {
        player_collider.enabled = true;
        crouch_collider.enabled = false;
        //transform.rotation = new Quaternion(0, 0, 0, 0);
        player_body.rotation = 0;
    }
    public override string getMode()
    {
        return "cube";
    }
}

/*public class SpiderOrbTrail : MonoBehaviour
{
    public void Activate(TrailRenderer trail, GameObject player)
    {
        StopAllCoroutines();
        StartCoroutine(animateTrail(trail, player));
    }

    IEnumerator animateTrail(TrailRenderer trail, GameObject player)
    {
        trail.transform.localPosition = Vector3.zero;
        trail.emitting = true;
        Vector3 pos = player.transform.position;
        yield return null;
        trail.transform.position = pos;
        yield return null;
        trail.transform.localPosition = Vector3.zero;

        float time = 0, T = 0.2f;
        while (time <= T)
        {
            time += Time.deltaTime;
            yield return null;
        }

        trail.emitting = false;
    }
}*/