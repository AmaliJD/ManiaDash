using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json;
using TMPro;
using System.Linq;

public class Leaderboard : MonoBehaviour
{
    // On Home Page
    public Text message;
    public TextMeshProUGUI privatemessage;
    public GameObject nouser, nouserscore;
    public InputField inputusername;
    bool inputFocused;
    public GameObject popup;
    public Text popupText;

    private GlobalData savedata;
    private string playerID;
    private Stats stats;

    //private LootLockerManager llmanager;

    // On Icon Page
    public GridLayoutGroup colorgrid;

    // On Leaderboard Page
    public Material trail, wave_trail;
    public ColorReference playercolor1, playercolor2;
    public Image allcoinsimage;
    public Text timetype;
    private bool allcoins;

    public GameObject userrow;
    //public Transform Leaderboard1, Leaderboard1100, Leaderboard2, Leaderboard2100, Leaderboard3, Leaderboard3100,
    //                    Leaderboard4, Leaderboard4100, Leaderboard5, Leaderboard5100, Leaderboard6, Leaderboard6100;
    public Transform[] LeaderboardsAny, Leaderboards100;
    public Transform offBoard;
    public Animator refreshButtonAnimator;

    public Text leveltitle, usertext, besttime, best100;
    //public Button L1Button, L2Button, L3Button, L4Button, L5Button, L6Button;
    public Button[] LButtons;
    //public GameObject scroll1, scroll100, scroll2, scroll200, scroll3, scroll300,
    //                    scroll4, scroll400, scroll5, scroll500, scroll6, scroll600;
    public GameObject[] scrollsAny, scrolls100;

    private string[] LevelTitles = new string[] { "Dark Dungeon", "Topala City", "Toxic Factory", "Sky Fortress", "Dashlands", "Mystic Forest", "Volcanic Rush" }; 

    public GameObject Home, Intro, Cutoff, downloadButton;
    public Text cutoffText;

    public bool enable;
    private int setleaderboardfocus = 1;
    private int[] max_diamond_values;

    public Material[] shaderEffects;

    private void Start()
    {
        savedata = GetComponent<MainMenu>().getSaveData();
        savedata.update = 2.3f;
        max_diamond_values = GetComponent<LevelSelect>().getMaxDiamondValues();
        inputusername.text = savedata.username;
        Cutoff.SetActive(false); Login();
    }

    public void Login()
    {
        PlayFabSettings.DisableFocusTimeCollection = true;
        if (IsLoggedIn)
        {
            Debug.Log("Already Logged In");

            var req = new GetAccountInfoRequest{};
            PlayFabClientAPI.GetAccountInfo(req, OnGetAccountInfoSuccess, OnError);
            return;
        }
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true/*,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true,
            }*/
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnError);
        //Debug.Log("Device ID: " + SystemInfo.deviceUniqueIdentifier);
    }

    public bool IsLoggedIn
    {
        get
        {
            return PlayFabClientAPI.IsClientLoggedIn();
        }
    }

    /*IEnumerator BufferUploadAssets()
    {
        while(llmanager.loggedin == 0)
        {
            yield return new WaitForSecondsRealtime(.5f);
        }

        if(llmanager.loggedin == 1)
        {
            llmanager.CreateRecordingAsset(1, false);
            //llmanager.UploadPlayerFile(1, false);
        }
    }*/

    void OnGetAccountInfoSuccess(GetAccountInfoResult result)
    {
        playerID = result.AccountInfo.PlayFabId;
        GetTitleData();
        SendIconData();
        SaveStats();
        if (savedata.username != "" && savedata.username != null) { SendLeaderboard();}
        //StartCoroutine(GetAllLeaderboards());
    }

    void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Login Successful!");
        playerID = result.PlayFabId;
        GetTitleData();
        SendIconData();
        SaveStats();
        if (savedata.username != "" && savedata.username != null) { SendLeaderboard();}
        //StartCoroutine(GetAllLeaderboards());
    }

    void GetTitleData()
    {
        PlayFabClientAPI.GetTitleData(new GetTitleDataRequest(), OnTitleDataGet, OnError);
    }

    void OnTitleDataGet(GetTitleDataResult result)
    {
        if (result.Data == null || !result.Data.ContainsKey("Current Update")
            || !result.Data.ContainsKey("Cutoff Update")
            || !result.Data.ContainsKey("Current Message")
            || !result.Data.ContainsKey("Outdated Message")
            || !result.Data.ContainsKey("Cutoff Message"))
        {
            message.text = "";
            Debug.Log("No Title Data Found");
        }
        else
        {
            float current_update = float.Parse(result.Data["Current Update"].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture);
            float cutoff_update = float.Parse(result.Data["Cutoff Update"].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture);
            if (savedata.update >= current_update)
            {
                message.text = result.Data["Current Message"];
                Debug.Log("Running Lastest Version (" + savedata.update + ") of Mania Dash. Current Version is " + current_update);
            }
            else if (savedata.update > cutoff_update)
            {
                message.text = result.Data["Outdated Message"];
                downloadButton.SetActive(true);
                Debug.Log("Running Outdated Version (" + savedata.update + ") of Mania Dash. Current Version is " + current_update);
            }
            else if (savedata.update <= cutoff_update)
            {
                cutoffText.text = result.Data["Cutoff Message"];
                Cutoff.SetActive(true);
                Destroy(Home);
                //Destroy(Intro);
                //downloadButton.SetActive(true);
                Debug.Log("Running Unnacceptable Version (" + savedata.update + ") of Mania Dash.   Version Must Higher Than " + cutoff_update + "   The Current Version is " + current_update);
            }
        }
        /*if (result.Data == null || !result.Data.ContainsKey("Message 0.49"))
        {
            message.text = "";
        }
        else
        {
            message.text = result.Data["Message 0.49"];
        }*/
        
        if (result.Data != null && (savedata.username != null && savedata.username != "") && result.Data.ContainsKey(savedata.username))
        {
            privatemessage.text = result.Data[savedata.username];
        }

        if (!Cutoff.activeSelf) { Destroy(Cutoff); }
    }

    void SaveStats()
    {
        stats = new Stats(savedata.username, savedata.p1_index, savedata.p2_index, savedata.icon_index, (int)(savedata.p1_opacity * 100), Mathf.RoundToInt(savedata.update * 1000));
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { "Stats", JsonConvert.SerializeObject(stats) }
                /*{"Username", "OmegaFalcon"},
                {"PlayerColor1", "1"},
                {"PlayerColor2", "0"},
                {"IconIndex", "2"}*/
            },
            Permission = UserDataPermission.Public
        };
        PlayFabClientAPI.UpdateUserData(request, OnDataSend, OnError);
    }

    void SendUsername()
    {
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = inputusername.text
        };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnDisplayNameUpdate, OnUsernameError);
    }

    void OnDisplayNameUpdate(UpdateUserTitleDisplayNameResult result)
    {
        string prevuser = savedata.username;
        popupText.text = "Username Granted";
        popup.SetActive(true);
        //Debug.Log("Username Updated");
        savedata.username = inputusername.text + "";
        inputusername.DeactivateInputField();
        inputFocused = false;

        SendIconData();
        SaveStats();
        //llmanager.setLootLockerUsername(savedata.username);
        if (prevuser == "")
            SendLeaderboard();
    }

    void SendIconData()
    {
        string allDiamondsGet = "";
        int i = 0;
        foreach(int d in max_diamond_values)
        {
            allDiamondsGet += savedata.max_diamonds[i+1] >= max_diamond_values[i] ? "1" : "0";
            i++;
        }

        var request = new UpdateAvatarUrlRequest
        {
            ImageUrl = (string)(savedata.p1_index + "," + savedata.p2_index + "," + savedata.icon_index + "," + savedata.p1_opacity + "," + savedata.shader_effect + "," + allDiamondsGet + ",")
        };
        PlayFabClientAPI.UpdateAvatarUrl(request, OnSendIconData, OnError);
    }

    void OnSendIconData(EmptyResponse result)
    {
        Debug.Log("Update Avatar Data Success!");
    }

    void OnUsernameError(PlayFabError error)
    {
        if (inputusername.text == "")
        {
            popupText.text = "Invalid Username";
        }
        else
        {
            popupText.text = "Username Already Taken";
        }
        popup.SetActive(true);
        Debug.LogError(error.GenerateErrorReport());
        inputusername.text = savedata.username;
        inputusername.ActivateInputField();
        inputFocused = true;
    }

    void OnError(PlayFabError error)
    {
        Debug.LogError("PLAYFAB ERROR " + error.GenerateErrorReport());
    }

    public void SendLeaderboard()
    {
        for (int i = 1; i <= 7; i++)
        {
            if (savedata.level_times[i] != float.MaxValue)
            {
                int score = -(int)(savedata.level_times[i] * 1000);
                var request = new UpdatePlayerStatisticsRequest
                {
                    Statistics = new List<StatisticUpdate>
                    {
                        new StatisticUpdate
                        {
                            StatisticName = "Level " + i,
                            Value = score
                        }
                    }
                };
                PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderboardSend, OnError);
            }
            /*else
            {
                Debug.Log("No Time For Level " + i);
            }*/

            if (savedata.level_times_allcoins[i] != float.MaxValue)
            {
                int score = -(int)(savedata.level_times_allcoins[i] * 1000);
                var request = new UpdatePlayerStatisticsRequest
                {
                    Statistics = new List<StatisticUpdate>
                    {
                        new StatisticUpdate
                        {
                            StatisticName = "Level " + i + " Coins",
                            Value = score
                        }
                    }
                };
                PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderboardSend, OnError);
            }
            /*else
            {
                Debug.Log("No 100% Time For Level " + i);
            }*/
        }
    }

    void OnLeaderboardSend(UpdatePlayerStatisticsResult result)
    {
        //Debug.Log("Leaderboard Update Successful!");
    }

    int[] offboardPositions = new int[] { -1, -1, -1, -1, -1, -1, -1 };
    int[] offboardPositions100 = new int[] { -1, -1, -1, -1, -1, -1, -1 };
    public IEnumerator GetOffboardLeaderboard()
    {
        Transform icons = offBoard.GetChild(2);
        int p1 = savedata.p1_index;
        int p2 = savedata.p2_index;

        Color pc1 = colorgrid.transform.GetChild(p1).GetComponent<Image>().color;
        Color pc2 = colorgrid.transform.GetChild(p2).GetComponent<Image>().color;

        icons.GetChild(savedata.icon_index).gameObject.SetActive(true);
        foreach (Transform part in icons.GetChild(savedata.icon_index))
        {
            UserColor uc = part.GetComponent<UserColor>();
            if (uc != null)
            {
                uc.alpha = ((float)(savedata.p1_opacity));
                if (uc != null)
                {
                    Color c = uc.setColor(pc1, pc2);

                    part.GetComponent<Image>().color = c;
                }
            }
        }

        //lbget = true;
        for (int i = 1; i <= 7; i++)
        {
            if (savedata.level_times[i] != float.MaxValue)
            {
                getleaderboardcontinue = false;
                var request = new GetLeaderboardAroundPlayerRequest
                {
                    StatisticName = "Level " + i,
                    MaxResultsCount = 1
                };
                boardnumber = i;
                PlayFabClientAPI.GetLeaderboardAroundPlayer(request, OnLeaderboardAroundPlayerGet, OnError);
            }

            while (!getleaderboardcontinue)
                yield return null;

            if (savedata.level_times_allcoins[i] != float.MaxValue)
            {
                getleaderboardcontinue = false;
                var request = new GetLeaderboardAroundPlayerRequest
                {
                    StatisticName = "Level " + i + " Coins",
                    MaxResultsCount = 1
                };
                boardnumber = i + 100;
                PlayFabClientAPI.GetLeaderboardAroundPlayer(request, OnLeaderboardAroundPlayerGet, OnError);
            }

            while (!getleaderboardcontinue)
                yield return null;
        }
        //lbget = false;
        displayOffboardScore(1);
    }

    void OnLeaderboardAroundPlayerGet(GetLeaderboardAroundPlayerResult result)
    {
        if (result.Leaderboard[0].Position >= 100)
        {
            if (boardnumber < 100)
            {
                offboardPositions[boardnumber - 1] = result.Leaderboard[0].Position;
            }
            else
            {
                offboardPositions100[(boardnumber - 100) - 1] = result.Leaderboard[0].Position;
            }
        }
        else
        {
            //Debug.Log("Board Number " + boardnumber);
            if (boardnumber < 100)
            {
                offboardPositions[boardnumber - 1] = -1;
            }
            else
            {
                offboardPositions100[(boardnumber - 100) - 1] = -1;
            }
        }

        getleaderboardcontinue = true;
    }


    bool lbget;
    public IEnumerator GetAllLeaderboards()
    {
        lbget = true;
        getleaderboardcontinue = true;
        for (int i = 1; i <= 7; i++)
        {
            if (true)//savedata.level_times[i] != float.MaxValue)
            {
                getleaderboardcontinue = false;
                var request = new GetLeaderboardRequest
                {
                    StatisticName = "Level " + i,
                    StartPosition = 0,
                    MaxResultsCount = 100,
                    ProfileConstraints = new PlayerProfileViewConstraints
                    {
                        ShowDisplayName = true,
                        ShowAvatarUrl = true,
                    }
                };
                boardnumber = i;
                PlayFabClientAPI.GetLeaderboard(request, OnLeaderboardGet, OnError);
            }

            while (!getleaderboardcontinue)
                yield return null;

            if (true)//savedata.level_times_allcoins[i] != float.MaxValue)
            {
                getleaderboardcontinue = false;
                var request = new GetLeaderboardRequest
                {
                    StatisticName = "Level " + i + " Coins",
                    StartPosition = 0,
                    MaxResultsCount = 100,
                    ProfileConstraints = new PlayerProfileViewConstraints
                    {
                        ShowDisplayName = true,
                        ShowAvatarUrl = true,
                    }
                };
                boardnumber = i + 100;
                PlayFabClientAPI.GetLeaderboard(request, OnLeaderboardGet, OnError);
            }

            while (!getleaderboardcontinue)
                yield return null;
        }

        StartCoroutine(GetOffboardLeaderboard());
    }

    int boardnumber = 1;
    bool getleaderboardcontinue = true;
    void OnLeaderboardGet(GetLeaderboardResult result)
    {
        //Debug.Log("Leaderboard Retrieval Successful!");
        StartCoroutine(GetLeaderboard(result, boardnumber));
        //getleaderboardcontinue = true;
        /*foreach (var item in result.Leaderboard)
        {
            GetStats(item.PlayFabId);
            Debug.Log(item.Position+1 + " " + item.DisplayName + " " + item.StatValue + " " + otherplayersstats.icon);
        }*/
    }

    IEnumerator GetLeaderboard(GetLeaderboardResult result, int board)
    {
        Transform Leaderboard = LeaderboardsAny[0];//Leaderboard1;
        float playerpos = 0, totalpos = 0;
        int level = 0;

        Leaderboard = board < 100 ? LeaderboardsAny[board - 1] : Leaderboards100[board - 101];
        level = board < 100 ? board : board - 100;
        /*switch (board)
        {
            case 1:
                Leaderboard = Leaderboard1; level = 1;
                break;
            case 101:
                Leaderboard = Leaderboard1100; level = 1;
                break;
            case 2:
                Leaderboard = Leaderboard2; level = 2;
                break;
            case 102:
                Leaderboard = Leaderboard2100; level = 2;
                break;
            case 3:
                Leaderboard = Leaderboard3; level = 3;
                break;
            case 103:
                Leaderboard = Leaderboard3100; level = 3;
                break;
            case 4:
                Leaderboard = Leaderboard4; level = 4;
                break;
            case 104:
                Leaderboard = Leaderboard4100; level = 4;
                break;
            case 5:
                Leaderboard = Leaderboard5; level = 5;
                break;
            case 105:
                Leaderboard = Leaderboard5100; level = 5;
                break;
            case 6:
                Leaderboard = Leaderboard6; level = 6;
                break;
            case 106:
                Leaderboard = Leaderboard6100; level = 6;
                break;
        }*/

        getleaderboardcontinue = true;

        foreach (var item in result.Leaderboard)
        {
            yield return null;
            totalpos++;
            /*statsgotten = false;
            GetStats(item.PlayFabId);

            while (!statsgotten)
            {
                yield return null;
            }*/

            //Debug.Log("Avatar " + item.Profile.DisplayName + ": " + item.Profile.AvatarUrl);

            //Stats thisstats = new Stats(otherplayersstats.username, otherplayersstats.p1, otherplayersstats.p2, otherplayersstats.icon, otherplayersstats.opacity, otherplayersstats.version);
            //Debug.Log(board + " --- " + thisstats.username + " has icon: " + thisstats.icon);
            //getleaderboardcontinue = true;
            if (item.PlayFabId == playerID)
            {
                playerpos = (float)item.Position;
            }

            if (item.Position >= Leaderboard.GetChild(0).childCount)
            {
                Instantiate(userrow, Leaderboard.GetChild(0));
            }

            Transform row = Leaderboard.GetChild(0).GetChild(item.Position);
            Image diamond = row.GetChild(1).GetComponent<Image>();
            Transform icons = row.GetChild(2);
            Text posTxt = row.GetChild(3).GetComponent<Text>();
            Text nameTxt = row.GetChild(4).GetComponent<Text>();
            Text timeTxt = row.GetChild(5).GetComponent<Text>();

            // POSITION
            posTxt.text = item.Position + 1 + "";

            // NAME
            nameTxt.text = item.DisplayName + "";

            // TIME
            float time = (float)(-item.StatValue) / 1000;
            int T = (int)time;
            int milli = -item.StatValue - (T * 1000);
            int min = (int)(T / 60);
            int sec = T - (min * 60);

            timeTxt.text = min + ":" + (sec < 10 ? "0" : "") + sec + ":" + (milli < 100 ? "0" : "") + (milli < 10 ? "0" : "") + milli;

            switch (item.Position)
            {
                case 0:
                    nameTxt.color = Color.cyan;
                    posTxt.color = Color.cyan;
                    timeTxt.color = Color.cyan;
                    break;
                case 1:
                    nameTxt.color = new Color(.33f, 1, 1);
                    posTxt.color = new Color(.33f, 1, 1);
                    timeTxt.color = new Color(.33f, 1, 1);
                    break;
                case 2:
                    nameTxt.color = new Color(.67f, 1, 1);
                    posTxt.color = new Color(.67f, 1, 1);
                    timeTxt.color = new Color(.67f, 1, 1);
                    break;
            }

            if (item.PlayFabId == playerID)
            {
                nameTxt.color = Color.yellow;
                posTxt.color = Color.yellow;
                timeTxt.color = Color.yellow;
            }

            // ICON
            //int p1 = thisstats.p1;
            //int p2 = thisstats.p2;
            string avatarurl = item.Profile.AvatarUrl;
            int p1 = 4, p2 = 7, icon = 0;
            float opacity = 1;
            int diamondGet = 0, shaderEffect = 0;

            if (avatarurl != null && avatarurl != "")
            {
                p1 = int.Parse(avatarurl.Substring(0, avatarurl.IndexOf(",")));
                avatarurl = avatarurl.Substring(avatarurl.IndexOf(",") + 1);
                p2 = int.Parse(avatarurl.Substring(0, avatarurl.IndexOf(",")));
                avatarurl = avatarurl.Substring(avatarurl.IndexOf(",") + 1);
                icon = int.Parse(avatarurl.Substring(0, avatarurl.IndexOf(",")));
                avatarurl = avatarurl.Substring(avatarurl.IndexOf(",") + 1);

                try
                {
                    if (avatarurl.Contains(","))
                    {
                        opacity = float.Parse(avatarurl.Substring(0, avatarurl.IndexOf(",")));
                        avatarurl = avatarurl.Substring(avatarurl.IndexOf(",") + 1);
                        shaderEffect = int.Parse(avatarurl.Substring(0, avatarurl.IndexOf(",")));
                        avatarurl = avatarurl.Substring(avatarurl.IndexOf(",") + 1);
                        diamondGet = int.Parse(avatarurl.Substring(0, avatarurl.IndexOf(","))[level - 1].ToString());
                    }
                    else // if player data from pre 2.0
                    {
                        opacity = float.Parse(avatarurl);
                        shaderEffect = Random.Range(0, 6);
                        diamondGet = 0;
                    }
                }
                catch
                {
                    opacity = 1;
                    shaderEffect = 0;
                    diamondGet = 0;
                }
            }

            Color pc1 = colorgrid.transform.GetChild(p1).GetComponent<Image>().color;
            Color pc2 = colorgrid.transform.GetChild(p2).GetComponent<Image>().color;

            int i = 0;
            foreach (Transform t in icons)
            {
                if (i != icon)
                {
                    t.gameObject.SetActive(false);
                }
                else
                {
                    t.gameObject.SetActive(true);
                    foreach (Transform part in t)
                    {
                        UserColor uc = part.GetComponent<UserColor>();
                        if(uc != null)
                        {
                            uc.alpha = opacity/*((float)(thisstats.opacity)) / 100f*/;
                            if (uc != null)
                            {
                                Color c = uc.setColor(pc1, pc2);

                                part.GetComponent<Image>().color = c;
                            }
                        }

                        // eeh shaders don't look good on the leaderboard :/
                        /*if(part.name.Contains("Shader") && shaderEffect != 0)
                        {
                            part.gameObject.SetActive(true);
                            part.GetComponent<Image>().material = shaderEffects[shaderEffect];

                            if (shaderEffect == 4) { part.GetComponent<Image>().color = Color.white; };
                        }*/
                    }
                }
                i++;

                //yield return null;
            }

            // DIAMOND
            diamond.color = diamondGet == 1 ? Color.white : Color.clear;
        }

        if (playerpos > 9)
            Leaderboard.GetComponent<ScrollRect>().verticalScrollbar.value = 1 - Mathf.Clamp((playerpos / totalpos), 0, totalpos);

        if (board == 107)
        {
            lbget = false;
        }

        //getleaderboardcontinue = true;
    }

    public void ReGetLeaderboards()
    {
        StopAllCoroutines();
        lbget = false;
        StartCoroutine(GetAllLeaderboards());
    }

    public void DeleteAllLeaderboards()
    {
        StopAllCoroutines();
        lbget = false;
        StartCoroutine(DeleteLeaderboards());
    }

    public IEnumerator DeleteLeaderboards()
    {
        foreach(Transform tr in LeaderboardsAny.Concat(Leaderboards100).ToArray())
        {
            Transform fitter = tr.GetChild(0);

            foreach (Transform fit in fitter)
            {
                Destroy(fit.gameObject);
            }

            yield return null; 
        }
    }



    void OnDataSend(UpdateUserDataResult result)
    {
        Debug.Log("Update User Data Successful!");
    }

    void GetStats(string ID)
    {
        var request = new GetUserDataRequest
        {
            PlayFabId = ID
        };
        PlayFabClientAPI.GetUserData(request, OnDataGet, OnError);
    }

    void OnDataGet(GetUserDataResult result)
    {
        Debug.Log("Result contains key: " + result.Data.ContainsKey("Stats"));
        /*if (result.Data != null && result.Data.ContainsKey("Stats"))
        {
            otherplayersstats = JsonConvert.DeserializeObject<Stats>(result.Data["Stats"].Value);
        }
        else
        {
            Debug.LogError("Player Data Not Found");
            otherplayersstats = new Stats("", 4, 7, 0, 100, 0);
        }

        statsgotten = true;*/
    }

    public void setInputFocus()
    {
        inputFocused = true;
    }

    // UPDATE // --------------------------------------------------------------------------------------
    private void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown("enter")) && inputFocused)
        {
            SendUsername();
        }

        refreshButtonAnimator.speed = lbget ? 1 : 0;

        /*GameObject L1 = allcoins ? Leaderboard1100.gameObject : Leaderboard1.gameObject;
        GameObject L2 = allcoins ? Leaderboard2100.gameObject : Leaderboard2.gameObject;
        GameObject L3 = allcoins ? Leaderboard3100.gameObject : Leaderboard3.gameObject;
        GameObject L4 = allcoins ? Leaderboard4100.gameObject : Leaderboard4.gameObject;
        GameObject L5 = allcoins ? Leaderboard5100.gameObject : Leaderboard5.gameObject;
        GameObject L6 = allcoins ? Leaderboard6100.gameObject : Leaderboard6.gameObject;*/

        GameObject[] activePercentLeaderboard = new GameObject[LeaderboardsAny.Length];
        for(int l = 0; l < activePercentLeaderboard.Length; l++)
        {
            activePercentLeaderboard[l] = allcoins ? Leaderboards100[l].gameObject : LeaderboardsAny[l].gameObject;
        }

        if (!enable)
        {
            /*L1.SetActive(false); setButtonOn(L1Button, setleaderboardfocus == 1 ? true : false);
            L2.SetActive(false); setButtonOn(L2Button, setleaderboardfocus == 2 ? true : false);
            L3.SetActive(false); setButtonOn(L3Button, setleaderboardfocus == 3 ? true : false);
            L4.SetActive(false); setButtonOn(L4Button, setleaderboardfocus == 4 ? true : false);
            L5.SetActive(false); setButtonOn(L5Button, setleaderboardfocus == 5 ? true : false);
            L6.SetActive(false); setButtonOn(L6Button, setleaderboardfocus == 6 ? true : false);*/

            for (int l = 0; l < activePercentLeaderboard.Length; l++)
            {
                activePercentLeaderboard[l].SetActive(false);
                setButtonOn(LButtons[l], setleaderboardfocus == l+1 ? true : false);
            }
        }

        float time = 0, time100 = 0;

        int activeBoard = -1;

        try
        { activeBoard = System.Array.IndexOf(activePercentLeaderboard, activePercentLeaderboard.First(element => element.activeSelf)); }
        catch { }

        if (activeBoard != -1)
        {
            time = savedata.level_times[activeBoard + 1];
            time100 = savedata.level_times_allcoins[activeBoard + 1];

            for (int i = 0; i < LButtons.Length; i++)
            {
                setButtonOn(LButtons[i], i == activeBoard);
            }
            leveltitle.text = LevelTitles[activeBoard];
        }
        else if (enable)
        {
            setLeaderboardFocus(setleaderboardfocus);
        }

        /*if (L1.activeSelf)
        {
            time = savedata.level_times[1];
            time100 = savedata.level_times_allcoins[1];
            setButtonOn(L1Button, true);
            setButtonOn(L2Button, false);
            setButtonOn(L3Button, false);
            setButtonOn(L4Button, false);
            setButtonOn(L5Button, false);
            setButtonOn(L6Button, false);
            leveltitle.text = "Dark Dungeon";
        }
        else if (L2.activeSelf)
        {
            time = savedata.level_times[2];
            time100 = savedata.level_times_allcoins[2];
            setButtonOn(L1Button, false);
            setButtonOn(L2Button, true);
            setButtonOn(L3Button, false);
            setButtonOn(L4Button, false);
            setButtonOn(L5Button, false);
            setButtonOn(L6Button, false);
            leveltitle.text = "Topala City";
        }
        else if (L3.activeSelf)
        {
            time = savedata.level_times[3];
            time100 = savedata.level_times_allcoins[3];
            setButtonOn(L1Button, false);
            setButtonOn(L2Button, false);
            setButtonOn(L3Button, true);
            setButtonOn(L4Button, false);
            setButtonOn(L5Button, false);
            setButtonOn(L6Button, false);
            leveltitle.text = "Toxic Factory";
        }
        else if (L4.activeSelf)
        {
            time = savedata.level_times[4];
            time100 = savedata.level_times_allcoins[4];
            setButtonOn(L1Button, false);
            setButtonOn(L2Button, false);
            setButtonOn(L3Button, false);
            setButtonOn(L4Button, true);
            setButtonOn(L5Button, false);
            setButtonOn(L6Button, false);
            leveltitle.text = "Sky Fortress";
        }
        else if (L5.activeSelf)
        {
            time = savedata.level_times[5];
            time100 = savedata.level_times_allcoins[5];
            setButtonOn(L1Button, false);
            setButtonOn(L2Button, false);
            setButtonOn(L3Button, false);
            setButtonOn(L4Button, false);
            setButtonOn(L5Button, true);
            setButtonOn(L6Button, false);
            leveltitle.text = "Dashlands";
        }
        else if (L6.activeSelf)
        {
            time = savedata.level_times[6];
            time100 = savedata.level_times_allcoins[6];
            setButtonOn(L1Button, false);
            setButtonOn(L2Button, false);
            setButtonOn(L3Button, false);
            setButtonOn(L4Button, false);
            setButtonOn(L5Button, false);
            setButtonOn(L6Button, true);
            leveltitle.text = "Mystic Forest";
        }
        else if(enable)
        {
            setLeaderboardFocus(setleaderboardfocus);
        }*/

        if (time != float.MaxValue)
        {
            int t = (int)time;
            int milli = (int)((time - t) * 1000);
            int min = (int)(t / 60);
            int sec = t - (min * 60);
            besttime.text = "Best Time: " + min + ":" + (sec < 10 ? "0" : "") + sec + ":" + (milli < 100 ? "0" : "") + (milli < 10 ? "0" : "") + milli;
        }
        else
        {
            besttime.text = "Best Time: -:--:---";
        }

        if (time100 != float.MaxValue)
        {
            int t = (int)time100;
            int milli = (int)((time100 - t) * 1000);
            int min = (int)(t / 60);
            int sec = t - (min * 60);
            best100.text = "100% Time: " + min + ":" + (sec < 10 ? "0" : "") + sec + ":" + (milli < 100 ? "0" : "") + (milli < 10 ? "0" : "") + milli;
        }
        else
        {
            best100.text = "100% Time: -:--:---";
        }

        usertext.text = savedata.username;
        nouser.SetActive(savedata.username == "" || savedata.username == null);
        nouserscore.SetActive(savedata.username == "" || savedata.username == null);

        wave_trail.SetColor("_BaseColor", playercolor1.channelcolor);
        trail.SetColor("_BaseColor", playercolor2.channelcolor);
    }

    public void setButtonOn(Button button, bool on)
    {
        if (on)
        {
            ColorBlock colorblock = button.colors;
            colorblock.normalColor = new Color32(255, 255, 255, 255);
            colorblock.highlightedColor = new Color32(245, 245, 245, 112);
            colorblock.pressedColor = new Color32(200, 200, 200, 255);
            colorblock.selectedColor = new Color32(245, 245, 245, 255);
            colorblock.disabledColor = new Color32(128, 128, 128, 255);

            button.colors = colorblock;
            button.GetComponentInChildren<Text>().color = Color.black;
        }
        else
        {
            ColorBlock colorblock = button.colors;
            colorblock.normalColor = new Color32(255, 255, 242, 13);
            colorblock.highlightedColor = new Color32(245, 245, 245, 112);
            colorblock.pressedColor = new Color32(200, 200, 200, 255);
            colorblock.selectedColor = new Color32(245, 245, 245, 255);
            colorblock.disabledColor = new Color32(128, 128, 128, 255);

            button.colors = colorblock;
            button.GetComponentInChildren<Text>().color = Color.white;
        }
    }

    public void FetchLeaderboards()
    {
        if (!lbget)
        {
            StartCoroutine(GetAllLeaderboards());
        }
    }

    
    public void setLeaderboardFocus(int i)
    {
        setleaderboardfocus = i;
        GameObject[] activePercentLeaderboard = new GameObject[LeaderboardsAny.Length];
        for (int l = 0; l < activePercentLeaderboard.Length; l++)
        {
            activePercentLeaderboard[l] = allcoins ? Leaderboards100[l].gameObject : LeaderboardsAny[l].gameObject;
            activePercentLeaderboard[l].SetActive(l+1 == i);
        }
        /*GameObject L1 = allcoins ? Leaderboard1100.gameObject : Leaderboard1.gameObject;
        GameObject L2 = allcoins ? Leaderboard2100.gameObject : Leaderboard2.gameObject;
        GameObject L3 = allcoins ? Leaderboard3100.gameObject : Leaderboard3.gameObject;
        GameObject L4 = allcoins ? Leaderboard4100.gameObject : Leaderboard4.gameObject;
        GameObject L5 = allcoins ? Leaderboard5100.gameObject : Leaderboard5.gameObject;
        GameObject L6 = allcoins ? Leaderboard6100.gameObject : Leaderboard6.gameObject;

        L1.SetActive(i == 1);
        L2.SetActive(i == 2);
        L3.SetActive(i == 3);
        L4.SetActive(i == 4);
        L5.SetActive(i == 5);
        L6.SetActive(i == 6);*/

        for (int j = 0; j < scrollsAny.Length; j++)
        {
            scrollsAny[j].SetActive(LeaderboardsAny[j].gameObject.activeSelf);
            scrolls100[j].SetActive(Leaderboards100[j].gameObject.activeSelf);
        }

        /*scroll1.SetActive(Leaderboard1.gameObject.activeSelf);
        scroll100.SetActive(Leaderboard1100.gameObject.activeSelf);
        scroll2.SetActive(Leaderboard2.gameObject.activeSelf);
        scroll200.SetActive(Leaderboard2100.gameObject.activeSelf);
        scroll3.SetActive(Leaderboard3.gameObject.activeSelf);
        scroll300.SetActive(Leaderboard3100.gameObject.activeSelf);
        scroll4.SetActive(Leaderboard4.gameObject.activeSelf);
        scroll400.SetActive(Leaderboard4100.gameObject.activeSelf);
        scroll5.SetActive(Leaderboard5.gameObject.activeSelf);
        scroll500.SetActive(Leaderboard5100.gameObject.activeSelf);
        scroll6.SetActive(Leaderboard6.gameObject.activeSelf);
        scroll600.SetActive(Leaderboard6100.gameObject.activeSelf);*/

        //if (Leaderboard1100.gameObject.activeSelf || Leaderboard2100.gameObject.activeSelf || Leaderboard3100.gameObject.activeSelf
        //     || Leaderboard4100.gameObject.activeSelf || Leaderboard5100.gameObject.activeSelf || Leaderboard6100.gameObject.activeSelf)
        // i = 100 + i;

        foreach (Transform tr in Leaderboards100)
        {
            if (tr.gameObject.activeSelf)
            {
                i = 100 + i;
                break;
            }
        }

        displayOffboardScore(i);
    }

    public void toggleAllCoins()
    {
        allcoins = !allcoins;

        /*GameObject[] activePercentLeaderboard = new GameObject[LeaderboardsAny.Length];
        for (int l = 0; l < activePercentLeaderboard.Length; l++)
        {
            activePercentLeaderboard[l] = !allcoins ? Leaderboards100[l].gameObject : LeaderboardsAny[l].gameObject;
        }*/

        Transform[] AllLeaderboards = LeaderboardsAny.Concat(Leaderboards100).ToArray();
        /*GameObject activeBoard = AllLeaderboards.First(element => element.gameObject.activeSelf).gameObject;
        int activeBoardIndex = System.Array.IndexOf(AllLeaderboards, activeBoard);
        int activeBoardIndexMod = activeBoardIndex - activeBoardIndex >= LeaderboardsAny.Length ? LeaderboardsAny.Length : 0;// activeBoardIndex % LeaderboardsAny.Length;

        if(activeBoardIndex < LeaderboardsAny.Length)
        {
            LeaderboardsAny[activeBoardIndexMod].gameObject.SetActive(false);
            Leaderboards100[activeBoardIndexMod].gameObject.SetActive(true);
            allcoinsimage.color = Color.white;
            timetype.text = "Best Time 100%";
            displayOffboardScore(activeBoardIndexMod+101);
        }
        else
        {
            LeaderboardsAny[activeBoardIndexMod].gameObject.SetActive(true);
            Leaderboards100[activeBoardIndexMod].gameObject.SetActive(false);
            allcoinsimage.color = new Color(.41f, .41f, .41f);
            timetype.text = "Best Time";
            displayOffboardScore(activeBoardIndexMod+1);
        }*/
        for(int i = 0; i < AllLeaderboards.Length; i++)
        {
            if(AllLeaderboards[i].gameObject.activeSelf)
            {
                if(i >= LeaderboardsAny.Length)
                {
                    LeaderboardsAny[i - LeaderboardsAny.Length].gameObject.SetActive(true);
                    Leaderboards100[i - LeaderboardsAny.Length].gameObject.SetActive(false);
                    allcoinsimage.color = new Color(.41f, .41f, .41f);
                    timetype.text = "Best Time";
                    displayOffboardScore(i - LeaderboardsAny.Length + 1);
                }
                else
                {
                    LeaderboardsAny[i].gameObject.SetActive(false);
                    Leaderboards100[i].gameObject.SetActive(true);
                    allcoinsimage.color = Color.white;
                    timetype.text = "Best Time 100%";
                    displayOffboardScore(i + 101);
                }
                break;
            }
        }

        /*if (Leaderboard1.gameObject.activeSelf)
        {
            Leaderboard1.gameObject.SetActive(false);
            Leaderboard1100.gameObject.SetActive(true);
            allcoinsimage.color = Color.white;
            timetype.text = "Best Time 100%";
            displayOffboardScore(1);
        }
        else if (Leaderboard1100.gameObject.activeSelf)
        {
            Leaderboard1100.gameObject.SetActive(false);
            Leaderboard1.gameObject.SetActive(true);
            allcoinsimage.color = new Color(.41f, .41f, .41f);
            timetype.text = "Best Time";
            displayOffboardScore(101);
        }
        else if (Leaderboard2.gameObject.activeSelf)
        {
            Leaderboard2.gameObject.SetActive(false);
            Leaderboard2100.gameObject.SetActive(true);
            allcoinsimage.color = Color.white;
            timetype.text = "Best Time 100%";
            displayOffboardScore(2);
        }
        else if (Leaderboard2100.gameObject.activeSelf)
        {
            Leaderboard2100.gameObject.SetActive(false);
            Leaderboard2.gameObject.SetActive(true);
            allcoinsimage.color = new Color(.41f, .41f, .41f);
            timetype.text = "Best Time";
            displayOffboardScore(102);
        }
        else if (Leaderboard3.gameObject.activeSelf)
        {
            Leaderboard3.gameObject.SetActive(false);
            Leaderboard3100.gameObject.SetActive(true);
            allcoinsimage.color = Color.white;
            timetype.text = "Best Time 100%";
            displayOffboardScore(3);
        }
        else if (Leaderboard3100.gameObject.activeSelf)
        {
            Leaderboard3100.gameObject.SetActive(false);
            Leaderboard3.gameObject.SetActive(true);
            allcoinsimage.color = new Color(.41f, .41f, .41f);
            timetype.text = "Best Time";
            displayOffboardScore(103);
        }
        else if (Leaderboard4.gameObject.activeSelf)
        {
            Leaderboard4.gameObject.SetActive(false);
            Leaderboard4100.gameObject.SetActive(true);
            allcoinsimage.color = Color.white;
            timetype.text = "Best Time 100%";
            displayOffboardScore(4);
        }
        else if (Leaderboard4100.gameObject.activeSelf)
        {
            Leaderboard4100.gameObject.SetActive(false);
            Leaderboard4.gameObject.SetActive(true);
            allcoinsimage.color = new Color(.41f, .41f, .41f);
            timetype.text = "Best Time";
            displayOffboardScore(104);
        }
        else if (Leaderboard5.gameObject.activeSelf)
        {
            Leaderboard5.gameObject.SetActive(false);
            Leaderboard5100.gameObject.SetActive(true);
            allcoinsimage.color = Color.white;
            timetype.text = "Best Time 100%";
            displayOffboardScore(5);
        }
        else if (Leaderboard5100.gameObject.activeSelf)
        {
            Leaderboard5100.gameObject.SetActive(false);
            Leaderboard5.gameObject.SetActive(true);
            allcoinsimage.color = new Color(.41f, .41f, .41f);
            timetype.text = "Best Time";
            displayOffboardScore(105);
        }
        else if (Leaderboard6.gameObject.activeSelf)
        {
            Leaderboard6.gameObject.SetActive(false);
            Leaderboard6100.gameObject.SetActive(true);
            allcoinsimage.color = Color.white;
            timetype.text = "Best Time 100%";
            displayOffboardScore(6);
        }
        else if (Leaderboard6100.gameObject.activeSelf)
        {
            Leaderboard6100.gameObject.SetActive(false);
            Leaderboard6.gameObject.SetActive(true);
            allcoinsimage.color = new Color(.41f, .41f, .41f);
            timetype.text = "Best Time";
            displayOffboardScore(106);
        }*/

        /*scroll1.SetActive(Leaderboard1.gameObject.activeSelf);
        scroll100.SetActive(Leaderboard1100.gameObject.activeSelf);
        scroll2.SetActive(Leaderboard2.gameObject.activeSelf);
        scroll200.SetActive(Leaderboard2100.gameObject.activeSelf);
        scroll3.SetActive(Leaderboard3.gameObject.activeSelf);
        scroll300.SetActive(Leaderboard3100.gameObject.activeSelf);
        scroll4.SetActive(Leaderboard4.gameObject.activeSelf);
        scroll400.SetActive(Leaderboard4100.gameObject.activeSelf);
        scroll5.SetActive(Leaderboard5.gameObject.activeSelf);
        scroll500.SetActive(Leaderboard5100.gameObject.activeSelf);
        scroll6.SetActive(Leaderboard6.gameObject.activeSelf);
        scroll600.SetActive(Leaderboard6100.gameObject.activeSelf);*/

        for (int j = 0; j < scrollsAny.Length; j++)
        {
            scrollsAny[j].SetActive(LeaderboardsAny[j].gameObject.activeSelf);
            scrolls100[j].SetActive(Leaderboards100[j].gameObject.activeSelf);
        }
    }

    public void displayOffboardScore(int table)
    {
        Text nameTxt = offBoard.GetChild(4).GetComponent<Text>();
        nameTxt.text = savedata.username;

        Image diamond = offBoard.GetChild(1).GetComponent<Image>();

        /*Transform icons = offBoard.GetChild(2);
        int p1 = savedata.p1_index;
        int p2 = savedata.p2_index;

        Color pc1 = colorgrid.transform.GetChild(p1).GetComponent<Image>().color;
        Color pc2 = colorgrid.transform.GetChild(p2).GetComponent<Image>().color;*/

        /*int i = 0;
        foreach (Transform t in icons)
        {
            if (i != savedata.icon_index)
            {
                t.gameObject.SetActive(false);
            }
            else
            {
                t.gameObject.SetActive(true);
                foreach (Transform part in t)
                {
                    UserColor uc = part.GetComponent<UserColor>();
                    if (uc != null)
                    {
                        uc.alpha = ((float)(savedata.p1_opacity));
                        if (uc != null)
                        {
                            Color c = uc.setColor(pc1, pc2);

                            part.GetComponent<Image>().color = c;
                        }
                    }
                }
            }
            i++;
        }*/
        /*icons.GetChild(savedata.icon_index).gameObject.SetActive(true);
        foreach (Transform part in icons.GetChild(savedata.icon_index))
        {
            UserColor uc = part.GetComponent<UserColor>();
            if (uc != null)
            {
                uc.alpha = ((float)(savedata.p1_opacity));
                if (uc != null)
                {
                    Color c = uc.setColor(pc1, pc2);

                    part.GetComponent<Image>().color = c;
                }
            }
        }*/

        Text posTxt = offBoard.GetChild(3).GetComponent<Text>();
        Text timeTxt = offBoard.GetChild(5).GetComponent<Text>();

        if (table < 100)
        {
            if (offboardPositions[table - 1] != -1)
            {
                posTxt.text = offboardPositions[table - 1] + "";

                float time = savedata.level_times[table];
                int t = (int)time;
                int milli = (int)((time - t) * 1000);
                int min = (int)(t / 60);
                int sec = t - (min * 60);

                timeTxt.text = min + ":" + (sec < 10 ? "0" : "") + sec + ":" + (milli < 100 ? "0" : "") + (milli < 10 ? "0" : "") + milli;

                offBoard.gameObject.SetActive(true);
            }
            else
            {
                offBoard.gameObject.SetActive(false);
            }
        }
        else
        {
            table = table - 100;

            if (offboardPositions100[table - 1] != -1)
            {
                posTxt.text = offboardPositions100[table - 1] + "";

                float time = savedata.level_times_allcoins[table];
                int t = (int)time;
                int milli = (int)((time - t) * 1000);
                int min = (int)(t / 60);
                int sec = t - (min * 60);

                timeTxt.text = min + ":" + (sec < 10 ? "0" : "") + sec + ":" + (milli < 100 ? "0" : "") + (milli < 10 ? "0" : "") + milli;

                offBoard.gameObject.SetActive(true);
            }
            else
            {
                offBoard.gameObject.SetActive(false);
            }
        }
        //offBoard.gameObject.SetActive(true);
        //lbget = false;
        diamond.color = savedata.max_diamonds[table] >= max_diamond_values[table-1] ? Color.white : Color.clear;
    }

    public string getPlayerID()
    {
        return playerID;
    }
}

public class Stats
{
    public string username;
    public int p1, p2, icon;
    public int opacity;
    public int version;

    public Stats(string user, int pc1, int pc2, int ico, int opa, int ver)
    {
        this.username = user;
        this.p1 = pc1;
        this.p2 = pc2;
        this.icon = ico;
        this.opacity = opa;
        this.version = ver;
    }
}

public class LeaderboardStats
{
    public string username;
    public int p1, p2, icon;
    public Color playercolor1, playercolor2;
    public float opacity, time;

    public LeaderboardStats(string user, int pc1, int pc2, int ico, Color pl1, Color pl2, float o, float t)
    {
        this.username = user;
        this.p1 = pc1;
        this.p2 = pc2;
        this.p2 = pc2;
        this.icon = ico;
        this.playercolor1 = pl1;
        this.playercolor1 = pl2;
        this.opacity = o;
        this.time = t;
    }
}