using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class CreateSaveFile : MonoBehaviour
{
    private GlobalData savedata;

    public string username;

    public float[] player_color_1;
    public float[] player_color_2;
    public float p1_opacity;
    public int[] color_availability;
    public int[] shader_availability;      
    public int shader_effect;
    public int[] max_diamonds;

    public int icon_index, p1_index, p2_index;
    public int[] icon_availability;

    public int[,] levels_completed_and_coins;
    public int[] levels_completed_and_coins_binary;
    public float[] level_times;
    public float[] level_times_allcoins;

    public int total_diamonds;
    public int total_coins;

    public float update;
    public int extra;    

    public void GenerateSaveFile()
    {
        savedata = new GlobalData();

        savedata.username = username;
        savedata.player_color_1 = player_color_1;
        savedata.player_color_2 = player_color_2;
        savedata.p1_opacity = p1_opacity;
        savedata.color_availability = color_availability;
        savedata.shader_availability = shader_availability;
        savedata.shader_effect = shader_effect;
        savedata.max_diamonds = max_diamonds;

        savedata.icon_index = icon_index;
        savedata.p1_index = p1_index;
        savedata.p2_index = p2_index;
        savedata.icon_availability = icon_availability;

        //levels_completed_and_coins = new int[13, 4];
        for(int i = 0; i < 13; i++)
        {
            savedata.levels_completed_and_coins[i, 3] = levels_completed_and_coins_binary[i] % 10;
            savedata.levels_completed_and_coins[i, 2] = levels_completed_and_coins_binary[i] % 10;
            savedata.levels_completed_and_coins[i, 1] = levels_completed_and_coins_binary[i] % 10;
            savedata.levels_completed_and_coins[i, 0] = levels_completed_and_coins_binary[i] % 10;
        }        

        savedata.level_times = level_times;
        savedata.level_times_allcoins = level_times_allcoins;

        for(int i = 0; i < savedata.level_times.Length; i++)
        {
            if(savedata.level_times[i] == 0)
            {
                savedata.level_times[i] = float.MaxValue;
            }
            if (savedata.level_times_allcoins[i] == 0)
            {
                savedata.level_times_allcoins[i] = float.MaxValue;
            }
        }

        savedata.total_diamonds = total_diamonds;
        savedata.total_coins = total_coins;

        savedata.update = update;
        savedata.extra = extra;

        // initial setup
        for (int z = 0; z < 6; z++)
        {
            savedata.icon_availability[z] = 1;
        }
        savedata.color_availability[1] = 1;
        savedata.color_availability[4] = 1;
        savedata.color_availability[7] = 1;
        savedata.color_availability[10] = 1;
        savedata.color_availability[13] = 1;
        savedata.shader_availability[0] = 1;

        SaveData();
    }

    private void SaveData()
    {
        string path = Application.persistentDataPath + "/savedata - " + username + ".gja";
        BinaryFormatter formatter = new BinaryFormatter();
        GlobalData data = new GlobalData();
        FileStream stream;

        if (File.Exists(path))
        {

            stream = new FileStream(path, FileMode.Open);
            savedata = formatter.Deserialize(stream) as GlobalData;
            stream.Close();
        }

        stream = new FileStream(path, FileMode.Create);
        formatter.Serialize(stream, savedata);
        stream.Close();
    }
}
