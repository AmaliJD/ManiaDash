using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.Experimental.Rendering.Universal;

public class IconController : MonoBehaviour
{
    public ColorReference playercolor1;
    public Color p1;
    public ColorReference playercolor2;
    public Color p2;
    public GameObject[] icon_array;
    public int index;
    public Material trail, wave_trail;
    public ParticleSystem death_effect;
    public Material[] shaderEffects;
    public Material dashFlame;
    public int shader_effect;

    public Transform[] icon_body_parts;

    public Light2D playerLight;
    public float shaderIntensity;

    public bool random;

    void Awake()
    {
        if (random)
        {
            index = Random.Range(0, 30);
            shader_effect = Random.Range(0, 6);

            float hr = 0, h2 = 0, sr = 0, vr = 0, ar = p1.a;
            Color.RGBToHSV(p1, out hr, out sr, out vr);

            float R1 = Random.Range(0f, 360f);
            float R2 = Random.Range(.1f * 360f, .9f * 360f);
            int V = Random.Range(0, 21);

            hr += (R1 / 360);
            h2 = hr;
            h2 += (R2 / 360);

            if (hr > 1) { hr -= 1; }
            else if (hr < 0) { hr += 1; }

            if (h2 > 1) { h2 -= 1; }
            else if (h2 < 0) { h2 += 1; }

            p1 = Color.HSVToRGB(hr, V == 20 ? 0 : sr, V == 19 ? 0 : vr);
            p1.a = ar;

            p2 = Color.HSVToRGB(h2, sr, vr);
            p2.a = ar;
        }
        else
        {
            LoadData();
        }

        playercolor1.Set(p1);
        playercolor2.Set(p2);

        death_effect.startColor = p1;

        //*
        float h = 0, s = 0, v = 0, a = p1.a;
        Color.RGBToHSV(p1, out h, out s, out v);
        p1 = Color.HSVToRGB(h, s, 1);
        //*/

        trail.SetColor("_BaseColor", p2 != Color.black ? p2 : p1 != Color.black ? p1 : Color.white);
        wave_trail.SetColor("_BaseColor", p1);

        dashFlame.SetColor("_Color1", p1);
        dashFlame.SetColor("_Color2", p2*2);

        Transform ICON = null;

        int i = 0;
        foreach(GameObject icon in icon_array)
        {
            if(i != index)
            {
                icon.SetActive(false);
            }
            else
            {
                ICON = icon.transform;
            }
            i++;
        }

        if(shader_effect != 0 && shader_effect != 1) { playerLight.intensity = shaderIntensity; }

        if(shader_effect != 0)
        {
            //shader_effect = 5; // Negative Shader!
            foreach (Transform t in ICON)
            {
                if (t.gameObject.name.Contains("Shader"))
                {
                    t.GetComponent<SpriteRenderer>().material = shaderEffects[shader_effect-1];
                    //t.gameObject.SetActive(true);
                    t.gameObject.SetActive
                    (
                        (shader_effect != 4 && shader_effect != 5 && shader_effect != 2) ? !t.gameObject.name.Contains("Part") : t.gameObject.name.Contains("Part")
                    );

                    if (t.GetComponent<RendererColorAssigner>() != null) t.GetComponent<RendererColorAssigner>().alpha = 1;

                    if (shader_effect == 4)
                    {
                        if (t.GetComponent<RendererColorAssigner>() != null) t.GetComponent<RendererColorAssigner>().enabled = false;
                        if (t.GetComponent<RendererColorAssigner>() != null) t.GetComponent<RendererColorAssigner>().sat = -1;
                        t.GetComponent<SpriteRenderer>().color = Color.white;
                    }
                    else if (shader_effect == 2 && (t.GetComponent<RendererColorAssigner>() != null && t.GetComponent<RendererColorAssigner>().ColorReference == playercolor1))
                    {
                        t.GetComponent<RendererColorAssigner>().ColorReference = playercolor2;
                    }
                }
            }

            foreach (Transform bodypart in icon_body_parts)
            {
                foreach (Transform t in bodypart)
                {
                    if (t.gameObject.name.Contains("Shader"))
                    {
                        t.GetComponent<SpriteRenderer>().material = shaderEffects[shader_effect - 1];
                        t.gameObject.SetActive(true);
                        /*t.gameObject.SetActive
                        (
                            (shader_effect != 4 && shader_effect != 5) ? !t.gameObject.name.Contains("Part") : t.gameObject.name.Contains("Part")
                        );*/

                        if (t.GetComponent<RendererColorAssigner>() != null) t.GetComponent<RendererColorAssigner>().alpha = 1;

                        if (shader_effect == 4)
                        {
                            if (t.GetComponent<RendererColorAssigner>() != null) t.GetComponent<RendererColorAssigner>().enabled = false;
                            if (t.GetComponent<RendererColorAssigner>() != null) t.GetComponent<RendererColorAssigner>().sat = -1;
                            t.GetComponent<SpriteRenderer>().color = Color.white;
                        }
                        else if (shader_effect == 2 && t.GetComponent<RendererColorAssigner>().ColorReference == playercolor1)
                        {
                            t.GetComponent<RendererColorAssigner>().ColorReference = playercolor2;
                        }
                    }
                }
            }
        }
    }

    private void LoadData()
    {
        string path = Application.persistentDataPath + "/savedata.gja";
        GlobalData savedata = new GlobalData();
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            savedata = formatter.Deserialize(stream) as GlobalData;
            stream.Close();
        }
        else
        {
            Debug.LogError("No Save File Found");
        }

        index = savedata.icon_index;
        p1 = new Color(savedata.player_color_1[0], savedata.player_color_1[1], savedata.player_color_1[2], 1);
        p2 = new Color(savedata.player_color_2[0], savedata.player_color_2[1], savedata.player_color_2[2], 1);
        shader_effect = savedata.shader_effect;
    }

    public GameObject getIcon()
    {
        return icon_array[index];
    }

    public int getIconIndex()
    {
        return index;
    }
}
