using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IconSelect : MonoBehaviour
{
    // Color Reference
    public ColorReference p1, p2;

    // Icon Select
    public GameObject iconSelected, leaderboardIconSelected;
    private int curr_icon_selected = 0;

    public GridLayoutGroup icongrid, iconlocks;
    public GameObject selectorIcon;
    private Vector2 icon_pos_base = new Vector2(-605f, 96.6f);

    // Color Select
    public GridLayoutGroup colorgrid, colorlocks;
    public GameObject selectorP1, selectorP2;
    public Slider P1P2Slider, P1OpacitySlider;
    public Text P1Text, P2Text, P1OpacityText;
    public Text P1SliderText, P2SliderText;
    private bool player1 = true;
    private Vector2 color_pos_base = new Vector2(-692.8002f, -356.4f);

    // Shader Select
    public GridLayoutGroup shadergrid, shaderlocks;
    public GameObject selectorShader;
    private Vector2 shader_pos_base = new Vector2(-868.3003f, 90.60015f);
    public Material[] shaderEffects;

    // Collectables
    public Text total_diamonds, total_coins;
    public AudioSource buy;

    private GlobalData savedata;

    private void Start()
    {
        savedata = GetComponent<MainMenu>().getSaveData();

        player1 = false;
        setSelector(savedata.p2_index);
        player1 = true;
        setSelector(savedata.p1_index);
        setIconSelector(savedata.icon_index);
        setShaderSelector(savedata.shader_effect);
        P1OpacitySlider.value = savedata.p1_opacity * 4;
    }

    void Update()
    {
        if (Input.GetKeyDown("1") && !GetComponent<MainMenu>().optionsMenu.activeSelf)
        {
            setP1P2Slider(1);
        }
        else if (Input.GetKeyDown("2") && !GetComponent<MainMenu>().optionsMenu.activeSelf)
        {
            setP1P2Slider(0);
        }

        player1 = P1P2Slider.value == 1;
        P1SliderText.color = player1 ? Color.white : new Color(1, 1, 1, .17646f);
        P2SliderText.color = !player1 ? Color.white : new Color(1, 1, 1, .17646f);
        P1OpacityText.text = (P1OpacitySlider.value * 25) + "";

        total_diamonds.text = "x" + savedata.total_diamonds;
        total_coins.text = "x" + savedata.total_coins;

        savedata.p1_opacity = P1OpacitySlider.value / 4f;
        p1.Set(new Color(p1.r, p1.g, p1.b, P1OpacitySlider.value / 4f));
    }

    public void switchP1P2Slider()
    {
        P1P2Slider.value = 1 - P1P2Slider.value;
    }

    public void setP1P2Slider(int i)
    {
        P1P2Slider.value = i;
    }

    public void setSelectorSame(bool pl2)
    {
        if(pl2 && player1)
        {
            setSelector(selectorP2.GetComponent<SelectorPosition>().selectorPosition);
        }
        else if(!pl2 && !player1)
        {
            setSelector(selectorP1.GetComponent<SelectorPosition>().selectorPosition);
        }
        else
        {
            return;
        }

        P1Text.fontSize = 60;
        P1Text.rectTransform.anchoredPosition = new Vector3(-.18f, .18f, 0);
        P2Text.fontSize = 60;
        P2Text.rectTransform.anchoredPosition = new Vector3(.18f, -.18f, 0);
    }

    public void setSelector(int i)
    {
        int x = i % 16;
        int y = -(i / 16);

        Vector2 pos = new Vector2(color_pos_base.x + x * (colorgrid.cellSize.x + colorgrid.spacing.x) * 77, color_pos_base.y + y * (colorgrid.cellSize.y + colorgrid.spacing.y) * 77);

        if (player1)
        {
            selectorP1.GetComponent<SelectorPosition>().selectorPosition = i;
            selectorP1.GetComponent<RectTransform>().anchoredPosition = pos;
            if (i == 30)
            {
                P1Text.color = Color.black;
            }
            else
            {
                P1Text.color = Color.white;
            }

            Color c = colorgrid.transform.GetChild(i).GetComponent<Image>().color;
            p1.Set(c);
            savedata.player_color_1[0] = c.r;
            savedata.player_color_1[1] = c.g;
            savedata.player_color_1[2] = c.b;
            savedata.p1_index = i;
        }
        else
        {
            selectorP2.GetComponent<SelectorPosition>().selectorPosition = i;
            selectorP2.GetComponent<RectTransform>().anchoredPosition = pos;
            if (i == 30)
            {
                P2Text.color = Color.black;
            }
            else
            {
                P2Text.color = Color.white;
            }

            Color c = colorgrid.transform.GetChild(i).GetComponent<Image>().color;
            p2.Set(c);
            savedata.player_color_2[0] = c.r;
            savedata.player_color_2[1] = c.g;
            savedata.player_color_2[2] = c.b;
            savedata.p2_index = i;
        }

        P1Text.fontSize = 100;
        P1Text.rectTransform.anchoredPosition = new Vector3(0, 0, 0);
        P2Text.fontSize = 100;
        P2Text.rectTransform.anchoredPosition = new Vector3(0, 0, 0);
    }

    public void setIconSelector(int i)
    {
        int x = i % 10;
        int y = -(i / 10);

        Vector2 pos = new Vector2(icon_pos_base.x + x * (icongrid.cellSize.x + icongrid.spacing.x) * 112, icon_pos_base.y + y * (icongrid.cellSize.y + icongrid.spacing.y) * 112);
        selectorIcon.GetComponent<RectTransform>().anchoredPosition = pos;

        iconSelected.transform.GetChild(curr_icon_selected).gameObject.SetActive(false);
        leaderboardIconSelected.transform.GetChild(curr_icon_selected).gameObject.SetActive(false);
        curr_icon_selected = i;

        setShaderEffectOnIcon();
        iconSelected.transform.GetChild(curr_icon_selected).gameObject.SetActive(true);
        leaderboardIconSelected.transform.GetChild(curr_icon_selected).gameObject.SetActive(true);

        savedata.icon_index = i;
    }

    public void setShaderEffectOnIcon()
    {
        if (savedata.shader_effect != 0)
        {
            foreach (Transform t in iconSelected.transform.GetChild(curr_icon_selected))
            {
                if (t.gameObject.name.Contains("Shader"))
                {
                    t.GetComponent<SpriteRenderer>().material = shaderEffects[savedata.shader_effect];
                    //t.gameObject.SetActive(true);
                    t.gameObject.SetActive
                    (
                        (savedata.shader_effect != 4 && savedata.shader_effect != 5 && savedata.shader_effect != 2) ? !t.gameObject.name.Contains("Part") : t.gameObject.name.Contains("Part")
                    );

                    if (t.GetComponent<RendererColorAssigner>() != null)
                    {
                        t.GetComponent<RendererColorAssigner>().alpha = 1;

                        if (savedata.shader_effect == 4)
                        {
                            if (t.gameObject.name.Contains("Body")) { t.GetComponent<RendererColorAssigner>().ColorReference = p1; }
                            
                            t.GetComponent<RendererColorAssigner>().sat = -1;
                            t.GetComponent<SpriteRenderer>().color = Color.white;
                        }
                        else if (savedata.shader_effect == 2)
                        {
                            if (t.gameObject.name.Contains("Body")) { t.GetComponent<RendererColorAssigner>().ColorReference = p2; }
                            t.GetComponent<RendererColorAssigner>().sat = 0;
                            t.GetComponent<SpriteRenderer>().color = t.GetComponent<RendererColorAssigner>().ColorReference.channelcolor;
                        }
                        else
                        {
                            if (t.gameObject.name.Contains("Body")) { t.GetComponent<RendererColorAssigner>().ColorReference = p1; }
                            t.GetComponent<RendererColorAssigner>().sat = 0;
                            t.GetComponent<SpriteRenderer>().color = t.GetComponent<RendererColorAssigner>().ColorReference.channelcolor;
                        }

                        t.GetComponent<RendererColorAssigner>().AssignSelf();
                    }
                }
            }

            foreach (Transform t in leaderboardIconSelected.transform.GetChild(curr_icon_selected))
            {
                if (t.gameObject.name.Contains("Shader"))
                {
                    t.GetComponent<SpriteRenderer>().material = shaderEffects[savedata.shader_effect];
                    //t.gameObject.SetActive(true);
                    t.gameObject.SetActive
                    (
                        (savedata.shader_effect != 4 && savedata.shader_effect != 5 && savedata.shader_effect != 2) ? !t.gameObject.name.Contains("Part") : t.gameObject.name.Contains("Part")
                    );

                    if (t.GetComponent<RendererColorAssigner>() != null)
                    {
                        t.GetComponent<RendererColorAssigner>().alpha = 1;

                        if (savedata.shader_effect == 4)
                        {
                            if (t.gameObject.name.Contains("Body")) { t.GetComponent<RendererColorAssigner>().ColorReference = p1; }
                            t.GetComponent<RendererColorAssigner>().sat = -1;
                            t.GetComponent<SpriteRenderer>().color = Color.white;
                        }
                        else if (savedata.shader_effect == 2)
                        {
                            if (t.gameObject.name.Contains("Body")) { t.GetComponent<RendererColorAssigner>().ColorReference = p2; }
                            t.GetComponent<RendererColorAssigner>().sat = 0;
                            t.GetComponent<SpriteRenderer>().color = t.GetComponent<RendererColorAssigner>().ColorReference.channelcolor;
                        }
                        else
                        {
                            if (t.gameObject.name.Contains("Body")) { t.GetComponent<RendererColorAssigner>().ColorReference = p1; }
                            t.GetComponent<RendererColorAssigner>().sat = 0;
                            t.GetComponent<SpriteRenderer>().color = t.GetComponent<RendererColorAssigner>().ColorReference.channelcolor;
                        }

                        t.GetComponent<RendererColorAssigner>().AssignSelf();
                    }
                }
            }
        }
        else
        {
            foreach (Transform t in iconSelected.transform.GetChild(curr_icon_selected))
            {
                if (t.gameObject.name.Contains("Shader"))
                {
                    t.gameObject.SetActive(false);
                }
            }

            foreach (Transform t in leaderboardIconSelected.transform.GetChild(curr_icon_selected))
            {
                if (t.gameObject.name.Contains("Shader"))
                {
                    t.gameObject.SetActive(false);
                }
            }
        }
    }

    public void setShaderSelector(int i)
    {
        int x = i % 2;
        int y = -(i / 2);

        Vector2 pos = new Vector2(shader_pos_base.x + x * (shadergrid.cellSize.x + shadergrid.spacing.x) * 130, shader_pos_base.y + y * (shadergrid.cellSize.y + shadergrid.spacing.y) * 130);
        selectorShader.GetComponent<RectTransform>().anchoredPosition = pos;

        savedata.shader_effect = (int)i;
        setShaderEffectOnIcon();
    }

    public void buyIcon(float i)
    {
        buy.volume = GetComponent<MainMenu>().getVolumes()[1];
        int price = Mathf.RoundToInt((i - (int)i) * 100);

        if (price <= savedata.total_diamonds)
        {
            buy.Stop();
            iconlocks.transform.GetChild((int)i).localScale = Vector3.zero;
            savedata.icon_availability[(int)i] = 1;
            savedata.total_diamonds -= price;
            buy.Play();
        }
        GetComponent<MainMenu>().SaveData();
    }

    public void buyColor(float i)
    {
        buy.volume = GetComponent<MainMenu>().getVolumes()[1];
        int price = Mathf.RoundToInt((i - (int)i) * 100);

        if (price <= savedata.total_diamonds)
        {
            buy.Stop();
            colorlocks.transform.GetChild((int)i).localScale = Vector3.zero;
            savedata.color_availability[(int)i] = 1;
            savedata.total_diamonds -= price;
            buy.Play();
        }
    }

    public void buyShader(float i)
    {
        buy.volume = GetComponent<MainMenu>().getVolumes()[1];
        int price = Mathf.RoundToInt((i - (int)i) * 100);
        if (price == 0) price = 100;

        if (price <= savedata.total_diamonds)
        {
            buy.Stop();
            shaderlocks.transform.GetChild((int)i).localScale = Vector3.zero;
            savedata.shader_availability[(int)i] = 1;
            savedata.total_diamonds -= price;
            buy.Play();
        }
    }
}
