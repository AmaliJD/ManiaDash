using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ShakeTrigger : MonoBehaviour
{
    public float intensity, frequency = 1, fadein, hold, fadeout;
    public bool holdOnStay, oneuse, STOPFADE, triggerColliderActivate;

    private GameManager gamemanager;
    private bool entered, inuse;

    private ShakeManager shakeManager;

    public enum Speed
    {
        x0, x1, x2, x3, x4
    }
    public Speed speed = Speed.x1;

    private void Awake()
    {
        gamemanager = FindObjectOfType<GameManager>();
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        shakeManager = FindObjectOfType<ShakeManager>();
    }

    IEnumerator Shake()
    {
        inuse = true;
        CinemachineBasicMultiChannelPerlin camera = gamemanager.getActiveCamera().GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        shakeManager.SetActive(this);

        //camera.m_AmplitudeGain = 0;
        if (STOPFADE)
        {
            camera.m_AmplitudeGain = 0; yield break;
        }

        float startingIntensity = camera.m_AmplitudeGain;
        float startingFrequency = camera.m_FrequencyGain;
        shakeManager.addPerlin(camera, startingIntensity, startingFrequency);

        float time = 0;
        while(time < fadein)
        {
            if(camera.VirtualCamera != gamemanager.getActiveCamera())
            {
                //camera.m_AmplitudeGain = shakeManager.getInitialAmplitude(camera);
                //camera.m_FrequencyGain = shakeManager.getInitialFrequency(camera);
                camera = gamemanager.getActiveCamera().GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                shakeManager.addPerlin(camera, gamemanager.getActiveCamera().GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain, gamemanager.getActiveCamera().GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain);

                startingIntensity = shakeManager.getInitialAmplitude(camera);
                startingFrequency = shakeManager.getInitialFrequency(camera);
            }

            camera.m_AmplitudeGain = Mathf.Lerp(startingIntensity, intensity, time / fadein);
            camera.m_FrequencyGain = Mathf.Lerp(startingFrequency, frequency, time / fadein);
            //Debug.Log("fadein: " + camera.m_AmplitudeGain);
            time += Time.deltaTime;
            yield return null;
        }

        time = 0;
        while (holdOnStay ? entered : time < hold)
        {
            if (camera.VirtualCamera != gamemanager.getActiveCamera())
            {
                //camera.m_AmplitudeGain = shakeManager.getInitialAmplitude(camera);
                //camera.m_FrequencyGain = shakeManager.getInitialFrequency(camera);
                camera = gamemanager.getActiveCamera().GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                shakeManager.addPerlin(camera, gamemanager.getActiveCamera().GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain, gamemanager.getActiveCamera().GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain);

                startingIntensity = shakeManager.getInitialAmplitude(camera);
                startingFrequency = shakeManager.getInitialFrequency(camera);
            }

            camera.m_AmplitudeGain = intensity;
            camera.m_FrequencyGain = frequency;
            //Debug.Log("hold: " + camera.m_AmplitudeGain);
            time += Time.deltaTime;
            yield return null;
        }

        time = 0;
        while (time < fadeout)
        {
            if (camera.VirtualCamera != gamemanager.getActiveCamera())
            {
                //camera.m_AmplitudeGain = shakeManager.getInitialAmplitude(camera);
                //camera.m_FrequencyGain = shakeManager.getInitialFrequency(camera);
                camera = gamemanager.getActiveCamera().GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                shakeManager.addPerlin(camera, gamemanager.getActiveCamera().GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain, gamemanager.getActiveCamera().GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain);

                startingIntensity = shakeManager.getInitialAmplitude(camera);
                startingFrequency = shakeManager.getInitialFrequency(camera);
            }

            camera.m_AmplitudeGain = Mathf.Lerp(intensity, startingIntensity, time / fadeout);
            camera.m_FrequencyGain = Mathf.Lerp(frequency, startingFrequency, time / fadeout);
            //Debug.Log("fadeout: " + camera.m_AmplitudeGain);
            time += Time.deltaTime;
            yield return null;
        }

        camera.m_AmplitudeGain = shakeManager.getInitialAmplitude(camera);
        camera.m_FrequencyGain = shakeManager.getInitialFrequency(camera);

        shakeManager.resetCameras();

        inuse = false;
        if (oneuse)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && holdOnStay)
        {
            if(!entered)
            {
                entered = true;
                StopAllCoroutines();
                StartCoroutine(Shake());
            }
            entered = true;
        }
    }

    public void SpawnActivate()
    {
        StopAllCoroutines();
        StartCoroutine(Shake());
    }

    public void Stop()
    {
        StopAllCoroutines();
        entered = false;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            entered = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && !holdOnStay && (oneuse ? !inuse : true) && (triggerColliderActivate ? true : !collision.isTrigger))
        {
            StopAllCoroutines();
            StartCoroutine(Shake());
        }
    }

    private void OnDrawGizmosSelected()
    {
        float scale = 0;
        switch (speed)
        {
            case Speed.x0:
                scale = 40f; break;
            case Speed.x1:
                scale = 55f; break;
            case Speed.x2:
                scale = 75f; break;
            case Speed.x3:
                scale = 90f; break;
            case Speed.x4:
                scale = 110f; break;
        }

        Gizmos.color = new Color(1f, .5f, .5f);
        Gizmos.DrawLine(transform.position, transform.position + new Vector3((scale * Time.fixedDeltaTime * 10f) * fadein, 0, 0));

        Gizmos.color = new Color(.5f, 1f, .5f);
        Gizmos.DrawLine(transform.position + new Vector3((scale * Time.fixedDeltaTime * 10f) * fadein, 0, 0), transform.position + new Vector3((scale * Time.fixedDeltaTime * 10f) * (fadein + hold), 0, 0));

        Gizmos.color = new Color(.5f, .5f, 1f);
        Gizmos.DrawLine(transform.position + new Vector3((scale * Time.fixedDeltaTime * 10f) * (fadein + hold), 0, 0), transform.position + new Vector3((scale * Time.fixedDeltaTime * 10f) * (fadein + hold + fadeout), 0, 0));
    }
}
