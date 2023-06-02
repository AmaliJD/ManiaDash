using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class EditSaveData : MonoBehaviour
{
    public Transform icon_button_grid, color_button_grid, shader_button_grid, coin_button_grid;
    public InputField coins, diamonds;
    public GameObject menu;
    private GameObject[] icon_button_x, color_button_x, shader_button_x, coin_button_x;

    private GlobalData savedata;

    public GameObject icon_locks, color_locks, shader_locks, transparency_lock;
    private bool locked = true;

    private void Start()
    {
        icon_button_x = new GameObject[30];
        color_button_x = new GameObject[32];
        shader_button_x = new GameObject[6];
        coin_button_x = new GameObject[15];

        int i = 0;
        foreach (Transform tr in icon_button_grid)
        {
            icon_button_x[i] = tr.GetChild(1).gameObject;
            i++;
        }

        i = 0;
        foreach (Transform tr in color_button_grid)
        {
            color_button_x[i] = tr.GetChild(1).gameObject;
            i++;
        }

        i = 0;
        foreach (Transform tr in shader_button_grid)
        {
            shader_button_x[i] = tr.GetChild(1).gameObject;
            i++;
        }

        i = 0;
        foreach (Transform tr in coin_button_grid)
        {
            coin_button_x[i] = tr.GetChild(1).gameObject;
            i++;
        }
    }

    public void getSaveData()
    {
        savedata = GetComponent<MainMenu>().getSaveData();

        int i = 0;
        foreach(int a in savedata.icon_availability)
        {
            icon_button_x[i].SetActive(a == 0);
            i++;
        }

        i = 0;
        foreach (int a in savedata.color_availability)
        {
            color_button_x[i].SetActive(a == 0);
            i++;
        }

        i = 0;
        foreach (int a in savedata.shader_availability)
        {
            if (i >= 6) continue;
            shader_button_x[i].SetActive(a == 0);
            i++;
        }

        i = 0;
        for (int a = 1; a < 5; a++)
        {
            coin_button_x[i].SetActive(savedata.levels_completed_and_coins[a, 1] == 0);
            i++;

            coin_button_x[i].SetActive(savedata.levels_completed_and_coins[a, 2] == 0);
            i++;

            coin_button_x[i].SetActive(savedata.levels_completed_and_coins[a, 3] == 0);
            i++;
        }

        coins.text = savedata.total_coins + "";
        diamonds.text = savedata.total_diamonds + "";
    }

    public void setSaveData()
    {
        savedata.total_coins = int.Parse(coins.text);
        savedata.total_diamonds = int.Parse(diamonds.text);
        SaveData();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void SaveData()
    {
        string path = Application.persistentDataPath + "/savedata.gja";
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream;

        stream = new FileStream(path, FileMode.Create);
        formatter.Serialize(stream, savedata);
        stream.Close();
    }

    public void setIcon(int i)
    {
        icon_button_x[i].SetActive(!icon_button_x[i].activeSelf);
        savedata.icon_availability[i] = 1 - savedata.icon_availability[i];
    }

    public void setColor(int i)
    {
        color_button_x[i].SetActive(!color_button_x[i].activeSelf);
        savedata.color_availability[i] = 1 - savedata.color_availability[i];
    }

    public void setShader(int i)
    {
        shader_button_x[i].SetActive(!shader_button_x[i].activeSelf);
        savedata.shader_availability[i] = 1 - savedata.shader_availability[i];
    }

    public void setLevel(int i)
    {
        
    }

    public void setCoin(int i)
    {
        coin_button_x[i].SetActive(!coin_button_x[i].activeSelf);
        savedata.levels_completed_and_coins[(i/3)+1, (i%3)+1] = coin_button_x[i].activeSelf ? 0 : 1;
    }

    public void OpenClose()
    {
        if(!menu.activeSelf)
        {
            getSaveData();
        }
        menu.SetActive(!menu.activeSelf);
    }

    public void unlockeverything()
    {
        locked = !locked;
        icon_locks.SetActive(locked);
        color_locks.SetActive(locked);
        shader_locks.SetActive(locked);
        //transparency_lock.SetActive(locked);
    }
}
