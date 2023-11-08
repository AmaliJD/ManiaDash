using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GlobalData
{
    public float[] player_color_1;
    public float[] player_color_2;
    public int[] color_availability;
    public int[] shader_availability;
    public int[] max_diamonds;
    public float p1_opacity;
    public int shader_effect;

    public int icon_index, p1_index, p2_index;
    public int[] icon_availability;

    public int[,] levels_completed_and_coins;
    public float[] level_times;
    public float[] level_times_allcoins;

    public int total_diamonds;
    public int total_coins;

    public float update;
    public int extra;

    public string username;

    public GlobalData()
    {
        player_color_1 = new float[4];
        player_color_2 = new float[4];
        p1_opacity = 1;
        shader_effect = 0;

        icon_index = 0;
        p1_index = 4;
        p2_index = 7;

        color_availability = new int[32];
        icon_availability = new int[30];
        shader_availability = new int[10];
        max_diamonds = new int[13];

        levels_completed_and_coins = new int[13, 4];
        /*level_times = new float[13, 2];

        for(int i = 0; i < level_times.GetLength(0); i++)
        {
            for(int j = 0; j < level_times.GetLength(1); j++)
            {
                level_times[i, j] = float.MaxValue;
            }
        }*/

        level_times = new float[13];
        level_times_allcoins = new float[13];
        for (int i = 0; i < level_times.Length; i++)
        {
            level_times[i] = float.MaxValue;
            level_times_allcoins[i] = float.MaxValue;
        }

        total_diamonds = 0;
        total_coins = 0;
        update = 2.24f;
        extra = 0;

        // initial setup
        for(int z = 0; z < 6; z++)
        {
            icon_availability[z] = 1;
        }
        color_availability[1] = 1;
        color_availability[4] = 1;
        color_availability[7] = 1;
        color_availability[10] = 1;
        color_availability[13] = 1;
        shader_availability[0] = 1;
    }

    public void SaveLevelData(int level_index, int[] coins, float time, float all_coins_time, int add_diamonds, int add_coins)
    {
        levels_completed_and_coins[level_index, 0] = 1;
        levels_completed_and_coins[level_index, 1] = coins[0];
        levels_completed_and_coins[level_index, 2] = coins[1];
        levels_completed_and_coins[level_index, 3] = coins[2];

        //level_times[level_index, 0] = Mathf.Min(time, level_times[level_index, 0]);
        //level_times[level_index, 1] = Mathf.Min(all_coins_time, level_times[level_index, 1]);
        level_times[level_index] = Mathf.Min(time, level_times[level_index]);
        level_times_allcoins[level_index] = Mathf.Min(all_coins_time, level_times_allcoins[level_index]);

        total_diamonds += add_diamonds;
        total_coins += add_coins;

        max_diamonds[level_index] = Mathf.Max(add_diamonds, max_diamonds[level_index]);
    }

    public void SaveColorPurchaces(int index)
    {
        color_availability[index] = 1;
    }

    public void SaveIconPurchaces(int index)
    {
        icon_availability[index] = 1;
    }

    public void SaveShaderSelection(int value)
    {
        shader_effect = value;
    }

    public void SavePlayerSelection(Color p1, Color p2, int i)
    {
        player_color_1[0] = p1.r;
        player_color_1[1] = p1.g;
        player_color_1[2] = p1.b;
        player_color_1[3] = p1.a;

        player_color_2[0] = p2.r;
        player_color_2[1] = p2.g;
        player_color_2[2] = p2.b;
        player_color_2[3] = p2.a;

        icon_index = i;
    }
}
