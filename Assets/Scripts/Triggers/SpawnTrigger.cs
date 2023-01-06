using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTrigger : MonoBehaviour
{
    public enum ExecutionType
    {
        Sequential, Parallel
    };
    public ExecutionType exe;

    public float wait;
    private WaitForSeconds waitForSeconds;
    public GameObject[] triggers;
    public float[] delay;
    public bool loop;
    public int loopNumberOfTimes;
    public bool TriggerOrb;
    public bool cancelJump;
    public bool oneuse;
    public bool onDeathStop, onCheckpointStop;
    public bool ignorefinish, disableColliderOnFinish;

    private bool inuse = false, finished = false;

    private GameManager gamemanager;    
    private PlayerControllerV2 player;    

    private void Awake()
    {
        if (!TriggerOrb) { gameObject.transform.GetChild(0).gameObject.SetActive(false); }
        waitForSeconds = new WaitForSeconds(wait);

        gamemanager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControllerV2>();
    }

    public ExecutionType getExeType()
    {
        return exe;
    }

    public IEnumerator CheckDeadOrCheckpoint()
    {
        bool died = false, check = false;
        int checkCount = player.getCheckpointCount();

        while (true)
        {
            if (player.getDead() == true && !died) { died = true; }
            if (player.getCheckpointCount() != checkCount && !check) { check = true; }
            if ((onDeathStop && died) || (onCheckpointStop && check)) { inuse = false; finished = true; GetComponent<Collider2D>().enabled = true;  StopAllCoroutines(); }

            yield return null;
        }
    }

    public IEnumerator Begin()
    {
        int i = 0, f = 1;

        if (inuse && oneuse) yield break;
        if (oneuse) inuse = true;
        inuse = true;

        if(wait > 0)
        {
            yield return waitForSeconds;
        }

        if ((onDeathStop || onCheckpointStop) && !loop)
        {
            StartCoroutine(CheckDeadOrCheckpoint());
        }

        if (exe == (ExecutionType)0) //Sequential
        {
            while (i < triggers.Length)
            {
                float d1 = delay[i], time = 0, d2 = 0;
                GameObject trigger = triggers[i];

                //Debug.Log("Executing: " +  i + "    Pre Wait: " + delay[i]);
                /*while (time < d1)
                {
                    time += Time.deltaTime;
                    yield return null;
                }*/
                if (d1 > 0) yield return new WaitForSeconds(d1);

                if (trigger != null)
                {
                    switch (trigger.tag)
                    {
                        // MOVE TRIGGER
                        case "MoveTrigger":
                            MoveTrigger move = trigger.GetComponent<MoveTrigger>();
                            RotateTrigger rotate = trigger.GetComponent<RotateTrigger>();
                            ScaleTrigger scale = trigger.GetComponent<ScaleTrigger>();

                            if (move != null)
                            {
                                d2 = move.getDuration();
                                time = 0;

                                StartCoroutine(move.Move());

                                /*while (time <= d2 && !ignorefinish)
                                {
                                    time += Time.deltaTime;
                                    yield return null;
                                }*/
                                if (d2 > 0 && !ignorefinish) yield return new WaitForSeconds(d2);
                                while (move.getFinished() != true && !ignorefinish)
                                {
                                    yield return null;
                                }

                                move.StopAllCoroutines();
                            }
                            else if (rotate != null)
                            {
                                d2 = rotate.getDuration();
                                time = 0;

                                StartCoroutine(rotate.Move());

                                /*while (time <= d2 && !ignorefinish)
                                {
                                    time += Time.deltaTime;
                                    yield return null;
                                }*/
                                if (d2 > 0 && !ignorefinish) yield return new WaitForSeconds(d2);
                                while (rotate.getFinished() != true && !ignorefinish)
                                {
                                    yield return null;
                                }

                                rotate.StopAllCoroutines();
                            }
                            else if (scale != null)
                            {
                                d2 = scale.getDuration();
                                time = 0;

                                StartCoroutine(scale.Move());

                                /*while (time <= d2 && !ignorefinish)
                                {
                                    time += Time.deltaTime;
                                    yield return null;
                                }*/
                                if (d2 > 0 && !ignorefinish) yield return new WaitForSeconds(d2);
                                while (scale.getFinished() != true && !ignorefinish)
                                {
                                    yield return null;
                                }

                                scale.StopAllCoroutines();
                            }

                            /*d2 = move.getDuration();
                            time = 0;

                            //Debug.Log("Trigger " +  i);
                            StartCoroutine(move.Move());

                            while (time <= d2 && !ignorefinish)
                            {
                                time += Time.deltaTime;
                                yield return null;
                            }
                            while (move.getFinished() != true && !ignorefinish)
                            {
                                //Debug.Log("Waiting to finish");
                                yield return null;
                            }

                            move.StopAllCoroutines();*/
                            break;

                        // SPAWN TRIGGER
                        case "SpawnTrigger":
                            SpawnTrigger spawn = trigger.GetComponent<SpawnTrigger>();
                            spawn.StopAllCoroutines();
                            StartCoroutine(spawn.Begin());

                            if (spawn.exe == ExecutionType.Parallel)
                            {
                                while (spawn.getFinished() != true && !ignorefinish)
                                {
                                    //Debug.Log("Waiting to finish");
                                    yield return null;
                                }
                            }

                            break;

                        // MUSIC TRIGGER
                        case "MusicTrigger":
                            MusicTrigger music = trigger.GetComponent<MusicTrigger>();
                            d2 = music.getDuration();
                            time = 0;

                            // Volume
                            if (music.mode == MusicTrigger.Mode.volume)
                            {
                                StartCoroutine(music.setMusicVolume());

                                while (music.getFinished() != true && !ignorefinish)
                                {
                                    //Debug.Log("Waiting to finish");
                                    yield return null;
                                }
                            }

                            // Change Song
                            else if (music.mode == MusicTrigger.Mode.music)
                            {
                                music.setBGMusic();
                            }

                            music.StopAllCoroutines();
                            break;

                        // TOGGLE TRIGGER
                        case "ToggleTrigger":
                            ToggleTrigger toggle = trigger.GetComponent<ToggleTrigger>();
                            StartCoroutine(toggle.Toggle());
                            while (toggle.getFinished() != true && !ignorefinish)
                            {
                                //Debug.Log("Waiting to finish");
                                yield return null;
                            }
                            break;

                        // COLOR TRIGGER
                        case "ColorTrigger":
                            ColorTrigger color = trigger.GetComponent<ColorTrigger>();
                            float dc = color.duration + color.delay;
                            color.SpawnActivate();
                            /*while (!color.getFinished() && !ignorefinish)
                            {
                                yield return null;
                            }*/
                            /*float tc = 0;
                            while (tc <= dc)
                            {
                                tc += Time.deltaTime;
                                yield return null;
                            }*/
                            if (dc > 0 && !ignorefinish) yield return new WaitForSeconds(dc);
                            break;

                        // PULSE TRIGGER
                        case "PulseTrigger":
                            PulseTrigger pulse = trigger.GetComponent<PulseTrigger>();
                            float dp = pulse.duration + pulse.fadein + pulse.hold;
                            pulse.SpawnActivate();
                            /*while (!pulse.getFinished() && !ignorefinish)
                            {
                                yield return null;
                            }*/
                            /*float tp = 0;
                            while (tp <= dp)
                            {
                                tp += Time.deltaTime;
                                yield return null;
                            }*/
                            if (dp > 0 && !ignorefinish) yield return new WaitForSeconds(dp);
                            break;

                        // SHAKE TRIGGER
                        case "ShakeTrigger":
                            ShakeTrigger shake = trigger.GetComponent<ShakeTrigger>();
                            shake.SpawnActivate();
                            break;

                        // RANDOM TRIGGER
                        case "RandomTrigger":
                            RandomTrigger random = trigger.GetComponent<RandomTrigger>();
                            random.SpawnActivate();
                            break;

                        // SFX TRIGGER
                        case "SfxTrigger":
                            SfxTrigger sfx = trigger.GetComponent<SfxTrigger>();
                            sfx.Play();
                            break;

                        default:
                            break;
                    }
                }

                i++;

                if (i == triggers.Length && loop && (loopNumberOfTimes != 0 ? f < loopNumberOfTimes : true))
                {
                    i = 0;
                    f++;
                }

                yield return null;
            }
        }


        else if (exe == (ExecutionType)1) //Parallel
        {
            finished = false;
            float longestDelay = 0;
            f = 0;
            bool died = false, check = false;
            int checkCount = player.getCheckpointCount();

            do
            {
                died = false;
                i = 0;
                while (i < triggers.Length)
                {
                    GameObject trigger = triggers[i];

                    if (trigger != null)
                    {
                        switch (trigger.gameObject.tag)
                        {
                            // MOVE TRIGGER
                            case "MoveTrigger":

                                MoveTrigger move = trigger.GetComponent<MoveTrigger>();
                                RotateTrigger rotate = trigger.GetComponent<RotateTrigger>();
                                ScaleTrigger scale = trigger.GetComponent<ScaleTrigger>();

                                if (move != null)
                                {
                                    longestDelay = Mathf.Max(move.getDuration(), longestDelay);
                                    StartCoroutine(move.Move());
                                }
                                else if (rotate != null)
                                {
                                    longestDelay = Mathf.Max(rotate.getDuration(), longestDelay);
                                    StartCoroutine(rotate.Move());
                                }
                                else if (scale != null)
                                {
                                    longestDelay = Mathf.Max(scale.getDuration(), longestDelay);
                                    StartCoroutine(scale.Move());
                                }


                                break;

                            // SPAWN TRIGGER
                            case "SpawnTrigger":
                                SpawnTrigger spawn = trigger.GetComponent<SpawnTrigger>();
                                spawn.StopAllCoroutines();
                                StartCoroutine(spawn.Begin());
                                break;

                            // MUSIC TRIGGER
                            case "MusicTrigger":
                                MusicTrigger music = trigger.GetComponent<MusicTrigger>();
                                longestDelay = Mathf.Max(music.getDuration(), longestDelay);

                                // Volume
                                if (music.mode == MusicTrigger.Mode.volume)
                                {
                                    StartCoroutine(music.setMusicVolume());
                                }

                                // Change Song
                                else if (music.mode == MusicTrigger.Mode.music)
                                {
                                    music.setBGMusic();
                                }
                                break;

                            // TOGGLE TRIGGER
                            case "ToggleTrigger":
                                ToggleTrigger toggle = trigger.GetComponent<ToggleTrigger>();
                                longestDelay = Mathf.Max((toggle.on_targets.Length + toggle.off_targets.Length) * Time.fixedDeltaTime * 10, longestDelay);
                                StartCoroutine(toggle.Toggle());
                                break;

                            // COLOR TRIGGER
                            case "ColorTrigger":
                                ColorTrigger color = trigger.GetComponent<ColorTrigger>();
                                color.SpawnActivate();
                                break;

                            // PULSE TRIGGER
                            case "PulseTrigger":
                                PulseTrigger pulse = trigger.GetComponent<PulseTrigger>();
                                pulse.SpawnActivate();
                                break;

                            // SHAKE TRIGGER
                            case "ShakeTrigger":
                                ShakeTrigger shake = trigger.GetComponent<ShakeTrigger>();
                                shake.SpawnActivate();
                                break;

                            // RANDOM TRIGGER
                            case "RandomTrigger":
                                RandomTrigger random = trigger.GetComponent<RandomTrigger>();
                                random.SpawnActivate();
                                break;

                            // SFX TRIGGER
                            case "SfxTrigger":
                                SfxTrigger sfx = trigger.GetComponent<SfxTrigger>();
                                sfx.Play();
                                break;

                            default:
                                break;
                        }
                    }                    

                    i++;
                }                

                float delayParallel = ignorefinish ? delay[0] : longestDelay;
                float delayTime = 0;

                if (!(onDeathStop || onCheckpointStop))
                {
                    if (delayParallel > 0)
                    {
                        yield return new WaitForSeconds(delayParallel);
                    }
                }
                else
                {
                    if (delayParallel > 0)
                    {
                        while (delayTime < delayParallel)
                        {
                            if (player.getDead() == true && !died) { died = true; }
                            if (player.getCheckpointCount() != checkCount && !check) { check = true; }
                            if ((onDeathStop && died) || (onCheckpointStop && check)) { inuse = false; finished = true; yield break; }
                            delayTime += Time.deltaTime;
                            yield return null;
                        }
                    }
                }

                f++;

            } while (loop && (loopNumberOfTimes != 0 ? f < loopNumberOfTimes : true));

            finished = true;
            if (disableColliderOnFinish) GetComponent<Collider2D>().enabled = false;
        }

        //inuse = false;
    }

    public void Activate()
    {
        //inuse = true;
        StartCoroutine(Begin());        
    }

    public void ActivateIfNotInUse()
    {
        if (!inuse)
        {
            StartCoroutine(Begin());
        }
    }

    public bool getFinished()
    {
        return finished;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && !inuse && !TriggerOrb)
        {
            //inuse = true;
            StartCoroutine(Begin());
            //if(oneuse && GetComponent<Collider2D>() != null) { GetComponent<Collider2D>().enabled = false; }
            //Activate();
        }
    }
}
