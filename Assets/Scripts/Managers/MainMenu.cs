using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cinemachine;
using UnityEngine.Experimental.Rendering.Universal;

public class MainMenu : MonoBehaviour
{
    public MenuState page;
    private int pagenumber;
    private GlobalData savedata;    

    public Animator MenuAnimator;
    public CinemachineVirtualCamera vin, clear;
    public MusicSource maintheme, altmaintheme, loopabletheme;
    private MusicSource bgMusic;

    public GameObject iconlocks, colorlocks;
    public GameObject sliderlock, shaderlocks;
    private float music_volume, sfx_volume;

    public GameObject new_unlocks_text;
    private bool unlock = false;

    // options menu
    public GameObject optionsMenu, deleteConfirmation;

    public Slider musicSlider, sfxSlider;
    public Text musicText, sfxText;
    public Toggle fullscreenToggle;
    private bool optionsopen;

    public Image Title;
    public Light2D TitleLight, BottomLight;
    public Sprite LogoBlue, LogoGreen, LogoYellow, LogoGray;
    private Color logolightblue, logolightgreen, logolightyellow;

    public GameObject logScreen;
    public Text logText;
    public ParticleSystem homeParticles;

    private Leaderboard leaderboard; 

    public GlobalData getSaveData()
    {
        return savedata;
    }

    public MusicSource getBGMusic()
    {
        return bgMusic;
    }

    private void Awake()
    {
        Resources.UnloadUnusedAssets();
        Cursor.visible = true;
        Debug.Log("Refresh Rate: " + Screen.currentResolution.refreshRate);
        logolightblue = new Color(0, 0.4670315f, 1);
        logolightgreen = new Color(0.1910665f, 1, 0);
        logolightyellow = new Color(1, 0.8838252f, 0);
        setTitleColor();

        leaderboard = GetComponent<Leaderboard>();

        if (page.State == 0)
        {
            bgMusic = maintheme;
            pagenumber = 0;
        }
        else
        {
            bgMusic = loopabletheme;            
            pagenumber = 2;
        }

        savedata = new GlobalData();
        LoadData();
        LoadPrefs();
    }

    void setTitleColor()
    {
        int r = Random.Range(0, 3);
        switch(r)
        {
            case 0:
                Title.sprite = LogoYellow;
                TitleLight.color = logolightyellow;
                break;
            case 1:
                Title.sprite = LogoGreen;
                TitleLight.color = logolightgreen;
                break;
            case 2:
                Title.sprite = LogoBlue;
                TitleLight.color = logolightblue;
                break;
        }
    }

    private void Start()
    {
        fullscreenToggle.isOn = Screen.fullScreen;        
        savedata.extra = 0;        
        Unlock();

        if (unlock && pagenumber == 2)
        {
            bgMusic = altmaintheme;
            new_unlocks_text.SetActive(true);
        }
        bgMusic.Play();
        OpenIcons();

        if (page.State != 0)
            MenuAnimator.Play("ReturnToLevelSelect");

        //Resources.UnloadUnusedAssets();
    }

    private void OpenIcons()
    {
        // ICON OPEN
        int index = 0;
        foreach(int i in savedata.icon_availability)
        {
            if(i == 1)
                iconlocks.transform.GetChild(index).localScale = Vector3.zero;
            index++;
        }

        // COLOR OPEN
        index = 0;
        foreach (int i in savedata.color_availability)
        {
            if(i == 1)
                colorlocks.transform.GetChild(index).localScale = Vector3.zero;
            index++;
        }

        // SHADER OPEN
        index = 0;
        foreach (int i in savedata.shader_availability)
        {
            if (i == 1)
                shaderlocks.transform.GetChild(index).localScale = Vector3.zero;
            index++;
        }
    }

    private void Unlock()
    {
        // LEVEL UNLOCK

        // ICON UNLOCK
        if (savedata.icon_availability[6] == 0 && savedata.levels_completed_and_coins[1, 0] == 1)
        {
            savedata.icon_availability[6] = 1;
            unlock = true;
        }

        if (savedata.icon_availability[7] == 0 && savedata.total_coins >= 3)
        {
            savedata.icon_availability[7] = 1;
            unlock = true;
        }

        if (savedata.icon_availability[8] == 0 && savedata.total_coins >= 6)
        {
            savedata.icon_availability[8] = 1;
            unlock = true;
        }

        if (savedata.icon_availability[9] == 0 && savedata.level_times[1] <= 90)
        {
            savedata.icon_availability[9] = 1;
            unlock = true;
        }

        if (savedata.icon_availability[10] == 0 && savedata.levels_completed_and_coins[2, 0] == 1)
        {
            savedata.icon_availability[10] = 1;
            unlock = true;
        }

        if (savedata.icon_availability[11] == 0 && savedata.levels_completed_and_coins[3, 0] == 1)
        {
            savedata.icon_availability[11] = 1;
            unlock = true;
        }

        if (savedata.icon_availability[12] == 0 && savedata.levels_completed_and_coins[4, 0] == 1)
        {
            savedata.icon_availability[12] = 1;
            unlock = true;
        }

        if (savedata.icon_availability[19] == 0 && savedata.levels_completed_and_coins[5, 0] == 1)
        {
            savedata.icon_availability[19] = 1;
            unlock = true;
        }

        if (savedata.icon_availability[20] == 0 && savedata.levels_completed_and_coins[6, 0] == 1)
        {
            savedata.icon_availability[20] = 1;
            unlock = true;
        }

        if (savedata.icon_availability[21] == 0 && savedata.total_coins >= 12)
        {
            savedata.icon_availability[21] = 1;
            unlock = true;
        }

        if (savedata.icon_availability[22] == 0 && savedata.total_coins >= 15)
        {
            savedata.icon_availability[22] = 1;
            unlock = true;
        }

        if (savedata.icon_availability[23] == 0 && savedata.total_coins >= 18)
        {
            savedata.icon_availability[23] = 1;
            unlock = true;
        }

        // COLOR UNLOCK
        if (savedata.color_availability[0] == 0 && savedata.level_times[2] <= 80)
        {
            savedata.color_availability[0] = 1;
            unlock = true;
        }

        if (savedata.color_availability[2] == 0 && savedata.levels_completed_and_coins[1, 0] == 1)
        {
            savedata.color_availability[2] = 1;
            unlock = true;
        }

        if (savedata.color_availability[3] == 0 && savedata.level_times[3] <= 140)
        {
            savedata.color_availability[3] = 1;
            unlock = true;
        }

        if (savedata.color_availability[5] == 0 && savedata.levels_completed_and_coins[2, 0] == 1)
        {
            savedata.color_availability[5] = 1;
            unlock = true;
        }

        if (savedata.color_availability[6] == 0 && savedata.level_times[4] <= 120)
        {
            savedata.color_availability[6] = 1;
            unlock = true;
        }

        if (savedata.color_availability[8] == 0 && savedata.levels_completed_and_coins[3, 0] == 1)
        {
            savedata.color_availability[8] = 1;
            unlock = true;
        }

        if (savedata.color_availability[9] == 0 && savedata.level_times[5] <= 100)
        {
            savedata.color_availability[9] = 1;
            unlock = true;
        }

        if (savedata.color_availability[11] == 0 && savedata.level_times[6] <= 75)
        {
            savedata.color_availability[11] = 1;
            unlock = true;
        }

        // SLIDER UNLOCK
        if (savedata.total_coins >= 9)
        {
            sliderlock.SetActive(false);
        }
    }

    private void LoadData()
    {
        string path = Application.persistentDataPath + "/savedata.gja"/*"C:/Users/hp/Documents/Unity/GDL/Assets/savedata.dat"*/;
        if (File.Exists(path))
        {
            Debug.Log("Sava Data Successfully Found");
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            savedata = formatter.Deserialize(stream) as GlobalData;
            if (savedata.shader_availability == null)
            {
                savedata.shader_availability = new int[10];
                savedata.shader_availability[0] = 1;
            }
            if (savedata.max_diamonds == null)
            {
                savedata.max_diamonds = new int[13];
            }

            stream.Close();
        }
        else if (File.Exists(Application.persistentDataPath.Substring(0, Application.persistentDataPath.IndexOf("Mania")) + "Geometry Jump/savedata.gja"))
        {
            Debug.LogError("Found File in /Geometry Jump");
            path = Application.persistentDataPath + "/savedata.gja";
            File.Copy(Application.persistentDataPath.Substring(0, Application.persistentDataPath.IndexOf("Mania")) + "Geometry Jump/savedata.gja", path);

            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            savedata = formatter.Deserialize(stream) as GlobalData;
            if (savedata.shader_availability == null)
            {
                savedata.shader_availability = new int[10];
                savedata.shader_availability[0] = 1;
            }
            if (savedata.max_diamonds == null)
            {
                savedata.max_diamonds = new int[13];
            }

            stream.Close();
        }
        else
        {
            Debug.LogError("No Save File Found");
        }

        savedata.update = 2.16f;
    }

    public void SaveData()
    {
        string path = Application.persistentDataPath + "/savedata.gja";
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream;

        stream = new FileStream(path, FileMode.Create);

        if (savedata.shader_availability == null)
        {
            savedata.shader_availability = new int[10];
            savedata.shader_availability[0] = 1;
        }
        if (savedata.max_diamonds == null)
        {
            savedata.max_diamonds = new int[13];
        }
        formatter.Serialize(stream, savedata);
        stream.Close();
    }

    public void changePage(int i)
    {
        switch(i)
        {
            case 0:
                switch(pagenumber)
                {
                    case 1:
                        setTitleColor();
                        MenuAnimator.Play("IconSelect-Home");
                        break;
                    case 2:
                        setTitleColor();
                        MenuAnimator.Play("LevelSelect-Home");
                        break;
                    case 3:
                        setTitleColor();
                        MenuAnimator.Play("Leaderboard-Home");
                        leaderboard.DeleteAllLeaderboards();
                        break;
                    case 4:
                        setTitleColor();
                        MenuAnimator.Play("Credits-Home");
                        break;
                }

                vin.Priority = 10;
                clear.Priority = 5;
                bgMusic.GetComponent<AudioHighPassFilter>().enabled = false;
                bgMusic.GetComponent<AudioLowPassFilter>().enabled = false;
                pagenumber = 0;
                break;

            case 1:
                MenuAnimator.Play("Home-IconSelect");
                clear.Priority = 10;
                vin.Priority = 5;
                bgMusic.GetComponent<AudioHighPassFilter>().enabled = true;
                bgMusic.GetComponent<AudioLowPassFilter>().enabled = false;
                pagenumber = 1;
                break;

            case 2:
                new_unlocks_text.SetActive(false);
                MenuAnimator.Play("Home-LevelSelect");
                pagenumber = 2;
                break;

            case 3:
                MenuAnimator.Play("Home-Leaderboard");
                clear.Priority = 10;
                vin.Priority = 5;
                bgMusic.GetComponent<AudioHighPassFilter>().enabled = false;
                bgMusic.GetComponent<AudioLowPassFilter>().enabled = true;
                pagenumber = 3;
                leaderboard.ReGetLeaderboards();
                break;

            case 4:
                MenuAnimator.Play("Home-Credits");
                bgMusic.GetComponent<AudioHighPassFilter>().enabled = true;
                bgMusic.GetComponent<AudioLowPassFilter>().enabled = true;
                pagenumber = 4;
                break;
        }
    }

    private void Update()
    {
        music_volume = musicSlider.value;
        sfx_volume = sfxSlider.value;
        musicText.text = "Music: " + (int)(music_volume * 100);
        sfxText.text = "Sfx: " + (int)(sfx_volume * 100);

        bgMusic.audio.volume = music_volume;

        if (Input.GetKeyDown("escape"))
        {
            //SaveData();
            //Application.Quit();
            if (pagenumber == 0 && !optionsopen)
            {
                Application.Quit();
            }
            else if(pagenumber == 0 && optionsopen)
            {
                optionsopen = false;
                TitleLight.gameObject.SetActive(true);
                deleteConfirmation.SetActive(false);
                optionsMenu.SetActive(false);
                //closeLogs();
            }
            else
            {
                changePage(0);
            }
        } 

        if (((Input.GetKeyDown("f") || Input.GetKeyDown(KeyCode.F11)) && !optionsopen) || fullscreenToggle.isOn != Screen.fullScreen)
        {
            toggleFullscreen();
        }
    }

    void OnApplicationQuit()
    {
        SaveData();
        Resources.UnloadUnusedAssets();
    }

    public void SavePrefs()
    {
        PlayerPrefs.SetFloat("music_volume", music_volume);
        PlayerPrefs.SetFloat("sfx_volume", sfx_volume);
        PlayerPrefs.Save();
    }

    public void LoadPrefs()
    {
        music_volume = PlayerPrefs.GetFloat("music_volume", .8f);
        sfx_volume = PlayerPrefs.GetFloat("sfx_volume", 1);
        musicSlider.value = music_volume;
        sfxSlider.value = sfx_volume;
    }

    public float[] getVolumes()
    {
        return new float[] { music_volume, sfx_volume };
    }

    public void OpenOptions()
    {
        optionsMenu.SetActive(true);
        TitleLight.gameObject.SetActive(false);
        optionsopen = true;
    }

    public void DeleteSave()
    {
        File.Copy(Application.persistentDataPath + "/savedata.gja", Application.persistentDataPath + "/savedata - Backup.gja", true);
        File.Delete(Application.persistentDataPath + "/savedata.gja");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void toggleFullscreen()
    {
        if (!Screen.fullScreen)
        {
            Screen.SetResolution(Screen.resolutions[Screen.resolutions.Length - 1].width, Screen.resolutions[Screen.resolutions.Length - 1].height, true);
            Screen.fullScreen = true;
            fullscreenToggle.isOn = true;
        }
        else
        {
            Screen.fullScreen = false;
            fullscreenToggle.isOn = false;
        }
    }

    public void Setquality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void openLogs()
    {
        try
        {
            TitleLight.gameObject.SetActive(false);
            BottomLight.gameObject.SetActive(false);
            homeParticles.Stop();
            //homeParticles.enableEmission = false;
            logScreen.SetActive(true);
            string path = Application.persistentDataPath + "/Player.log";
            StreamReader reader = new StreamReader(path);
            logText.text = reader.ReadToEnd();
            reader.Close();
        }
        catch
        {
            closeLogs();
        }
    }

    public void closeLogs()
    {
        TitleLight.gameObject.SetActive(true);
        BottomLight.gameObject.SetActive(true);
        homeParticles.Play();
        //homeParticles.enableEmission = true;
        logScreen.SetActive(false);
    }
}
