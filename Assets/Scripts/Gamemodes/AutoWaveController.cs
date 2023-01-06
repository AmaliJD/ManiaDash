using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Cinemachine;

public class AutoWaveController : PlayerController
{
    public Collider2D player_collider;
    public PolygonCollider2D wave_collider;

    public TrailRenderer trail, wave_trail, wave_trail2;

    public GameObject wave;

    private float jumpForce = 1f;
    private float posJump;

    private float moveX, grav_scale;
    private float smoothing;

    private float maxSpeed = 12f;

    public override void Awake2()
    {
        speed = getSpeed();
        moveX = speed;
        smoothing = .05f;
        v_Velocity = Vector3.zero;
        posJump = jumpForce;
        player_collider = GetComponent<Collider2D>();

        grav_scale = player_body.gravityScale;

        wave_collider.enabled = false;
        setRespawn(transform.position, reversed, mini);
        setRepawnSpeed(1f);

        //eyes = GameObject.Find("Icon_Eyes");
        //icon = eyes.transform.parent.gameObject;
        //setAnimation();
    }

    private void Start()
    {
        //bgmusic.Play();
        //player_body.freezeRotation = false;
    }

    public override void setAnimation()
    {
        wave_trail.Clear();
        wave_trail2.Clear();
        wave_trail.gameObject.SetActive(true);
        wave_trail.enabled = true;
        wave_trail.emitting = true;

        player_body.freezeRotation = true;
        transform.rotation = new Quaternion(0, 0, 0, 0);

        grounded_particles.Stop();
        ground_impact_particles.Stop();

        ChangeSize();

        player_body.gravityScale = 0f;
        if (reversed) { player_body.gravityScale *= -1; }
        grav_scale = player_body.gravityScale;

        icon.transform.localScale = new Vector3(0f, 0f, 1f);
        icon.transform.localPosition = new Vector3(1f, 1f, 0);
        wave.SetActive(true);
        icon.SetActive(false);

        trail.transform.localPosition = new Vector3(0f, 0f, 0);
    }

    public override void ChangeSize()
    {
        if (mini)
        {
            transform.localScale = new Vector2(.47f, .47f);
            jumpForce = 2f;
        }
        else
        {
            transform.localScale = new Vector2(1.05f, 1.05f);
            jumpForce = 1f;
        }

        posJump = jumpForce;
    }

    void Update()
    {
        if (able)
        {
            // CHECK IF DEAD
            dead = check_death && (Physics2D.IsTouchingLayers(wave_collider, deathLayer) || Mathf.Abs(player_body.velocity.x) <= 0.01);
            //grounded = Physics2D.Raycast(player_body.transform.position, Vector2.down, .51f, groundLayer);

            // CHECK IF GROUNDED
            grounded = checkGrounded && Physics2D.IsTouchingLayers(wave_collider, groundLayer);
            if (grounded) freeFallMode = false;

            if (reversed)
            {
                regate = -1;
            }
            else
            {//.9
                regate = 1;
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
            moveX = speed;

            // JUMP!
            if (input.Player.Jump.triggered)//Input.GetButtonDown("Jump") || Input.GetKeyDown("space") || Input.GetMouseButtonDown(0))
            {
                if (blue) { blue_j = true; }
                if (green) { green_j = true; }
                if (purple) { purple_j = true; }
                if (triggerorb) { triggerorb_j = true; }
                if (teleorb) { teleorb_j = true; }
                if (yellow) { yellow_j = true; }

                jump = true;
            }

            // RELEASE JUMP
            if (prevJumpNotReleased && input.Player.Jump.ReadValue<float>() == 0)//Input.GetButtonUp("Jump") || Input.GetKeyUp("space") || Input.GetMouseButtonUp(0))
            {
                jump = false;

                blue_j = false;
                green_j = false;
                purple_j = false;
                triggerorb_j = false;
                teleorb_j = false;
                yellow_j = false;
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

            prevJumpNotReleased = input.Player.Jump.triggered || input.Player.Jump.ReadValue<float>() == 1;
        }
    }

    void FixedUpdate()
    {
        // one job and one job only. MOVE
        if (able)
        {
            Move();
            Interpolate(0, 0);
        }
    }

    public override void Move()
    {
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
        player_body.velocity = targetVelocity;

        /*
        if (Mathf.Abs(targetVelocity.x) > Mathf.Abs(player_body.velocity.x) || !grounded)
        {
            player_body.velocity = Vector3.SmoothDamp(player_body.velocity, targetVelocity, ref v_Velocity, smoothing * .7f);
        }
        else
        {
            player_body.velocity = Vector3.SmoothDamp(player_body.velocity, targetVelocity, ref v_Velocity, smoothing * 1.5f);
        }*/
        if(freeFallMode)
        {
            int rev = reversed ? -1 : 1;
            player_body.gravityScale = rev * 6;
            grav_scale = player_body.gravityScale;
        }
        else
        {
            player_body.gravityScale = 0;
            grav_scale = 0;
        }

        Rotate();
        //Eyes();
        Jump();     // check if jumping
        Pad();      // check if hit pad
        Portal();   // check if on portal

        // IF GROUNDED --> TURN OFF TRAIL
        if (grounded && Mathf.Abs(player_body.velocity.y) <= 0.01 && (!red_p && !yellow_p && !blue_p && !pink_p))
        {
            trail.emitting = false;
        }

        check_death = true;
    }

    //private bool resetRotation = true;

    public void Rotate()
    {
        int pos = 45, neg = 315;
        if (mini) { pos = 60; neg = 300; }
        if (reversed) { int temp = pos; pos = neg; neg = temp; }

        if (!freeFallMode)
        {
            if (grounded)
            {
                //player_body.freezeRotation = false;
                //transform.rotation = new Quaternion(0, 0, 0, 0);
                player_body.rotation = 0;
                //transform.rotation = Quaternion.Lerp(transform.rotation, new Quaternion(0, 0, 0, 0), .5f);
            }
            else if (jump)
            {
                //Vector3 newAngle = new Vector3(0, 0, pos);
                //transform.rotation = Quaternion.Euler(newAngle);
                player_body.rotation = pos;
            }
            else
            {
                //Vector3 newAngle = new Vector3(0, 0, neg);
                //transform.rotation = Quaternion.Euler(newAngle);
                player_body.rotation = -pos;
            }
        }
        else
        {
            player_body.rotation = Vector2.SignedAngle(new Vector2(1, 0), player_body.velocity);
        }
    }

    /*public void Eyes()
    {
        int rev = 1;
        //if (reversed) { rev = -1; }
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

        trail.emitting = true;

        if (teleorb_j)
        {
            Vector3 positionDelta = (transform.position + teleOrb_translate) - transform.position;

            teleorb = false;
            teleorb_j = false;

            if (OrbTouched != null)
            {
                orbscript.Pulse();
            }

            player_body.transform.position += teleOrb_translate;
            CinemachineVirtualCamera activeCamera = gamemanager.getActiveCamera();
            activeCamera.GetCinemachineComponent<CinemachineFramingTransposer>().OnTargetObjectWarped(transform.GetChild(0), positionDelta);
        }

        if (triggerorb_j)
        {
            triggerorb = false;
            triggerorb_j = false;
            SpawnTrigger spawn = OrbTouched.GetComponent<SpawnTrigger>();
            StartCoroutine(spawn.Begin());

            if (OrbTouched != null)
            {
                orbscript.Pulse();
            }

            jump = !spawn.cancelJump;
        }

        if (blue_j)
        {
            blue = false;
            blue_j = false;

            playGravityParticles();

            reversed = !reversed;

            player_body.gravityScale *= -1;
            grav_scale *= -1;

            if (OrbTouched != null)
            {
                orbscript.Pulse();
            }
        }
        else if(green_j)
        {
            playGravityParticles();
            reversed = !reversed;

            int rev_ = 1;
            if (reversed) { rev_ = -1; }
            freeFallMode = true;

            player_body.gravityScale = rev_ * 6;
            grav_scale = player_body.gravityScale;

            player_body.velocity = new Vector2(player_body.velocity.x, rev_ * 25);

            green = false;
            green_j = false;
            jump = false;

            if (OrbTouched != null)
            {
                orbscript.Pulse();
            }
        }
        else if (purple_j)
        {
            int rev_ = 1;
            if (reversed) { rev_ = -1; }
            RaycastHit2D groundhit, deathhit;

            bool connect = true;

            if (!reversed)
            {
                groundhit = Physics2D.BoxCast(player_body.transform.position + new Vector3(0, .2f, 0), new Vector2(wave_collider.bounds.size.x * .5f, .1f), 0f, Vector2.up, 30, groundLayer);
                deathhit = Physics2D.BoxCast(player_body.transform.position + new Vector3(0, .2f, 0), new Vector2(wave_collider.bounds.size.x * .5f, .1f), 0f, Vector2.up, 30, deathLayer);
            }
            else
            {
                groundhit = Physics2D.BoxCast(player_body.transform.position + new Vector3(0, -.2f, 0), new Vector2(wave_collider.bounds.size.x * .5f, .1f), 0f, -Vector2.up, 30, groundLayer);
                deathhit = Physics2D.BoxCast(player_body.transform.position + new Vector3(0, -.2f, 0), new Vector2(wave_collider.bounds.size.x * .5f, .1f), 0f, -Vector2.up, 30, deathLayer);
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
                transform.position = new Vector2(transform.position.x, transform.position.y + rev_ * (deathhit.distance - (mini ? .1f : .3f)));
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
                transform.position = new Vector2(transform.position.x, transform.position.y + rev_ * (groundhit.distance - (mini ? .1f : .3f)));
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

            if (grav) { grav = false; }
            if (gravN) { gravN = false; }

            if (OrbTouched != null)
            {
                orbscript.Pulse();
            }
        }
        else if (yellow_j)
        {
            int rev_ = 1;
            if (reversed) { rev_ = -1; }
            freeFallMode = true;

            player_body.gravityScale = rev_ * 6;
            grav_scale = player_body.gravityScale;

            player_body.velocity = new Vector2(player_body.velocity.x, rev_ * 25);

            yellow = false;
            yellow_j = false;
            jump = false;

            if (OrbTouched != null)
            {
                orbscript.Pulse();
            }
        }

        int rev = 1;
        if (reversed) { rev = -1; }

        if (jump)
        {
            player_body.velocity = new Vector2(player_body.velocity.x, posJump * rev * player_body.velocity.x);
            freeFallMode = false;
        }
        else if(!freeFallMode)
        {
            player_body.velocity = new Vector2(player_body.velocity.x, posJump * rev * -player_body.velocity.x);
        }
    }

    public override void Pad()
    {
        if (blue_p)
        {
            blue_p = false;

            reversed = !reversed;

            player_body.gravityScale *= -1;
            grav_scale *= -1;
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
                /*
                if (Mathf.Abs(player_body.velocity.y) > maxSpeed * .6f)
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .6f);
                }
                else
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .8f);
                }*/

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
                /*
                if (Mathf.Abs(player_body.velocity.y) > maxSpeed * .6f)
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .5f);
                }
                else
                {
                    player_body.velocity = new Vector2(player_body.velocity.x, player_body.velocity.y * .75f);
                }*/

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
                jumpForce = posJump;
                trail.emitting = true;

                player_body.gravityScale = Mathf.Abs(player_body.gravityScale);
                grav_scale = player_body.gravityScale;
            }
            else if (!reversed)
            {
                reversed = true;
                jumpForce = -posJump;
                trail.emitting = true;

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

            CinemachineVirtualCamera activeCamera = gamemanager.getActiveCamera();
            activeCamera.GetCinemachineComponent<CinemachineFramingTransposer>().OnTargetObjectWarped(transform.GetChild(0), positionDelta);
        }
    }


    // COROUTUNES
    // none needed

    public override void Flip()
    {
        // Switch the way the player is labelled as facing.
        //facingRight = !facingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.y *= -1;
        transform.localScale = theScale;
    }

    public override void Respawn()
    {
        able = false;
        check_death = false;
        if (restartmusic) { bgmusic.Stop(); }
        player_collider.enabled = false;
        wave_collider.enabled = false;
        StopAllCoroutines();
        player_body.velocity = Vector2.zero;
        trail.emitting = false;
        jump = false;

        yellow = false; pink = false; red = false; green = false; blue = false; black = false;
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
        wave_trail.Clear();
        wave_trail2.Clear();
        //player_collider.enabled = true;

        CinemachineVirtualCamera activeCamera = gamemanager.getActiveCamera();
        activeCamera.GetCinemachineComponent<CinemachineFramingTransposer>().OnTargetObjectWarped(transform.GetChild(0), positionDelta);

        undead();

        //Invoke("undead", .5f);
    }

    public void undead()
    {
        if (!enabled)
        {
            Debug.Log("KMY SORRY");
            jump = false;
            dead = true;
            player_renderer.SetActive(true);
            speed = respawn_speed;
            resetColliders();
            return;
        }

        player_collider.enabled = false;
        speed = respawn_speed;
        //player_collider.enabled = true;
        wave_collider.enabled = true;
        player_renderer.SetActive(true);
        //player_renderer.enabled = true;
        player_body.gravityScale = grav_scale;

        //bgmusic.volume = 1;
        if (restartmusic)
        {
            gamemanager.setToNewBGMusic();
            bgmusic.Play();
        }

        Vector2 targetVelocity = new Vector2(speed * Time.fixedDeltaTime * 10f, player_body.velocity.y);
        player_body.velocity = targetVelocity;

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
        player_collider.isTrigger = false;
        player_collider.enabled = false;
        wave_collider.enabled = false;

        wave.SetActive(false);

        wave_trail.gameObject.SetActive(false);
        wave_trail.enabled = false;
        wave_trail.emitting = false;
        wave_trail2.enabled = false;
        wave_trail2.emitting = false;
    }
    public override void setColliders()
    {
        player_collider.enabled = false;
        check_death = false;
        //player_collider.isTrigger = true;
        wave_collider.enabled = true;

        wave_trail.gameObject.SetActive(true);
        wave_trail.enabled = true;
        wave_trail.emitting = true;
        wave_trail2.enabled = true;
        wave_trail2.emitting = true;
    }

    public override string getMode()
    {
        return "auto_wave";
    }
}