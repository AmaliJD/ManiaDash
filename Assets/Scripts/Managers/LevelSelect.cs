using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using System.IO;

public class LevelSelect : MonoBehaviour
{
    public Text[] times;
    public GameObject[] coins;
    public Image[] max_diamonds;
    private int[] max_diamond_values;
    public Sprite emptyDiamonds, filledDiamonds;
    public Toggle recordToggle;
    public Toggle recordToggle100;
    public AudioSource playsound;
    public MenuState page;
    private GlobalData savedata;
    private MusicSource bgMusic;

    private void Awake()
    {
        max_diamond_values = new int[] { 10, 12, 17, 32, 20, 21, 14, 1000, 1000, 1000, 1000, 1000};
        recordToggle.isOn = PlayerPrefs.GetInt("record_playback", 0) == 1 ? true : false;
        recordToggle100.isOn = PlayerPrefs.GetInt("record_playback_100", 0) == 1 ? true : false;
        if (!Directory.Exists(Application.persistentDataPath + "/LevelRecordingData")) { recordToggle.gameObject.SetActive(false); }
        if (!(recordToggle.isActiveAndEnabled && recordToggle.isOn)) { recordToggle100.gameObject.SetActive(false); }
    }

    public int[] getMaxDiamondValues()
    {
        return max_diamond_values;
    }

    private void BeginLevel(int levelNumber)
    {
        //if(levelNumber == -1) { return; }
        if (IsLoggedIn)
        {
            PlayFabClientAPI.WritePlayerEvent(new WriteClientPlayerEventRequest()
            {
                Body = new Dictionary<string, object>() {
                { "LevelNumber", levelNumber },
                { "CurrentBest", levelNumber != -1 ? savedata.level_times[levelNumber] : savedata.extra},
                { "CurrentBest 100%", levelNumber != -1 ? savedata.level_times_allcoins[levelNumber] : savedata.extra},
                { "Icon", savedata.icon_index },
                { "Player Color 1", savedata.player_color_1 },
                { "Player Color 2", savedata.player_color_2 }
            },
                EventName = "begin_level"
            },
            result => Debug.Log("Begin Level " + levelNumber),
            error => Debug.LogError(error.GenerateErrorReport()));
        }
        else
        {
            Debug.LogError("Not Logged In");
        }
    }

    public bool IsLoggedIn
    {
        get
        {
            return PlayFabClientAPI.IsClientLoggedIn();
        }
    }

    private void Start()
    {
        savedata = GetComponent<MainMenu>().getSaveData();
        bgMusic = GetComponent<MainMenu>().getBGMusic();

        for (int i = 0; i < times.Length; i++)
        {
            // Best Time
            float time = savedata.level_times[i + 1];
            if(time == float.MaxValue)
            {
                times[i].text = "-:--:---";
            }
            else
            {
                /*int milli = (int)(time * 1000) % 1000;
                int sec = (int)(time);
                int min = (int)(sec / 60);*/

                int t = (int)time;
                int milli = (int)((time - t) * 1000);
                int min = (int)(t / 60);
                //int sec = Mathf.RoundToInt((((float)t / 60f) - (t / 60)) * 60f);
                int sec = t - (min * 60);

                times[i].text = min + ":" + (sec < 10 ? "0" : "") + sec + ":" + (milli < 100 ? "0" : "") + (milli < 10 ? "0" : "") + milli;
            }

            // Coins Collected
            coins[i].transform.GetChild(0).GetChild(0).gameObject.SetActive(savedata.levels_completed_and_coins[i + 1, 1] == 1);
            coins[i].transform.GetChild(1).GetChild(0).gameObject.SetActive(savedata.levels_completed_and_coins[i + 1, 2] == 1);
            coins[i].transform.GetChild(2).GetChild(0).gameObject.SetActive(savedata.levels_completed_and_coins[i + 1, 3] == 1);

            max_diamonds[i].gameObject.SetActive(savedata.levels_completed_and_coins[i+1, 0] == 1);
            max_diamonds[i].sprite = savedata.max_diamonds[i + 1] >= max_diamond_values[i] ? filledDiamonds : emptyDiamonds;

            TextMeshProUGUI text = max_diamonds[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            text.text = savedata.max_diamonds[i + 1] + "/" + max_diamond_values[i];
            text.color = savedata.max_diamonds[i + 1] >= max_diamond_values[i] ? Color.yellow : Color.white;
        }
    }

    public IEnumerator LoadScene(int i)
    {
        float time = 0;
        while(time < .5f)
        {
            time += Time.deltaTime;
            yield return null;
        }

        page.State = 2;
        page.SetDirty();
        GetComponent<MainMenu>().SavePrefs();
        GetComponent<MainMenu>().SaveData();
        if (i == 0)
        {
            SceneManager.LoadScene("Testroom");
            yield break;
        }
        else if (i == -1)
        {
            SceneManager.LoadScene("The Challenge");
            yield break;
        }
        SceneManager.LoadScene("Level " + i);
    }
    
    public void selectLevel(int i)
    {
        PlayerPrefs.SetInt("record_playback", recordToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("record_playback_100", recordToggle100.isOn ? 1 : 0);

        bgMusic.Stop();
        playsound.volume = GetComponent<MainMenu>().getVolumes()[1];
        playsound.Play();

        BeginLevel(i);

        GetComponent<Animator>().Play("SelectLevel");
        StartCoroutine(LoadScene(i));
    }
}
