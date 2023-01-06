using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.Networking;

public class LootLockerManager : MonoBehaviour
{
    private Leaderboard leaderboard;
    private string playerID, username;

    [HideInInspector]
    public int loggedin = 0;

    private void Awake()
    {
        leaderboard = GetComponent<Leaderboard>();
        //Login();
    }

    public void Login()
    {
        LootLockerSDKManager.StartGuestSession(response =>
        {
            if(!response.success)
            {
                Debug.LogError("LootLocker Login Failed");
                loggedin = -1;
                return;
            }
            Debug.Log("LootLocker Login Successful");
            loggedin = 1;
        });
    }

    public void CreateRecordingAsset(int levelNumber, bool allcoins)
    {
        string assetName = playerID + "_level_" + levelNumber + "_" + (!allcoins ? "best" : "100");
        LootLockerSDKManager.CreatingAnAssetCandidate(assetName, response =>
        {
            if (!response.success)
            {
                Debug.LogError("LootLocker Create Asset Candidate Failed");
                return;
            }

            UploadRecordingAsset(response.asset_candidate_id, levelNumber, allcoins, assetName);
        });
    }

    public void UploadRecordingAsset(int asset_candidate_id, int levelNumber, bool allcoins, string assetName)
    {
        string path = Application.persistentDataPath + "/LevelRecordingData/level_" + levelNumber + "_" + (!allcoins ? "best" : "100") + ".dat";
        if (File.Exists(path))
        {
            LootLocker.LootLockerEnums.FilePurpose recordingFileType = LootLocker.LootLockerEnums.FilePurpose.file;
            string fileName = assetName + ".dat";

            LootLockerSDKManager.AddingFilesToAssetCandidates(asset_candidate_id, path, fileName, recordingFileType, response =>
            {
                if (!response.success)
                {
                    Debug.LogError("Error Uploading Asset Candidate");
                    return;
                }

                LootLockerSDKManager.UpdatingAnAssetCandidate(asset_candidate_id, true, uploadResponse =>
                {
                    if(!uploadResponse.success)
                    {
                        Debug.LogError("Error Updating Asset Candidate");
                        return;
                    }
                    Debug.Log("Asset Candidate Succesfully Uploaded");
                });
            });
        }
    }

    /*public void UploadPlayerFile(int levelNumber, bool allcoins)
    {
        string path = Application.persistentDataPath + "/LevelRecordingData/level_" + levelNumber + "_" + (!allcoins ? "best" : "100") + ".dat";

        if (File.Exists(path))
        {
            LootLocker.LootLockerEnums.FilePurpose recordingFileType = LootLocker.LootLockerEnums.FilePurpose.file;
            string fileName = playerID + "_level_" + levelNumber + "_" + (!allcoins ? "best" : "100") + ".dat";

            //LootLockerSDKManager.GetPlayerInfo(resp => { });

            LootLockerSDKManager.UploadPlayerFile(path, "Level " + levelNumber + (!allcoins ? " Best" : " 100%"), true, response =>
            {
                if (!response.success)
                {
                    Debug.LogError("Error Uploading Level " + levelNumber + (!allcoins ? " Best" : " 100%") + " Player File");
                    return;
                }
            });
        }

        DownloadPlayerFiles(levelNumber, allcoins);
    }

    public void DownloadPlayerFiles(int levelNumber, bool allcoins)
    {
        LootLockerSDKManager.GetAllPlayerFiles(response =>
        {
            if (!response.success)
            {
                Debug.LogError("Error Downloading Level " + levelNumber + (!allcoins ? " Best" : " 100%") + " Player File");
                return;
            }

            string path = Application.persistentDataPath + "/LevelRecordingData/Downloaded/";
            if (!Directory.Exists(Application.persistentDataPath + "/LevelRecordingData/Downloaded"))
            {
                Directory.CreateDirectory(Application.persistentDataPath + "/LevelRecordingData/Downloaded");
            }
            string fileName = playerID + "_level_" + levelNumber + "_" + (!allcoins ? "best" : "100") + ".dat";
            File.WriteAllText(path+fileName, response.items[0].url);
        });
    }*/

    public void setPlayerID(string pID)
    {
        playerID = pID;
    }

    public void setLootLockerUsername(string user)
    {
        username = user;
        //LootLockerSDKManager.SetPlayerName(username, response => { });
    }
}
