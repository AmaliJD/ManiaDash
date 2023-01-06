using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class HighDetail : MonoBehaviour
{
    private GameManager gamemanager;

    public Light2D light;
    public float intensity;

    void Start()
    {
        gamemanager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        bool PPX = gamemanager.getpostfx();

        if (light == null)
        {
            gamemanager.addHighDetail(gameObject);
            gameObject.SetActive(PPX);
        }
        else
        {
            gamemanager.addHighDetailLights(light, new Vector2(light.intensity, intensity));
            light.intensity = PPX ? light.intensity : intensity;
        }
        
        //gameObject.SetActive(gamemanager.getpostfx());
    }
}
