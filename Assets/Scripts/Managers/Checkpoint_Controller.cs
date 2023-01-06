using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Checkpoint_Controller : MonoBehaviour
{
    private GameManager gamemanager;

    private List<Checkpoint> c_scripts;
    private List<Vector3> c_positions;
    private List<int> c_states;
    private int currentActive = -1;
    void Awake()
    {
        gamemanager = FindObjectOfType<GameManager>();

        c_scripts = new List<Checkpoint>();
        c_positions = new List<Vector3>();
        c_states = new List<int>();

        int i = 0;
        foreach (Transform child in transform)
        {
            c_scripts.Add(child.gameObject.GetComponent<Checkpoint>());
            c_positions.Add(child.position);
            c_states.Add(c_scripts[i].getState());
            c_scripts[i].setIndex(i);
            i++;
        }
    }

    public void updateStates(int index)
    {
        int prevCurr = currentActive;
        if (currentActive != -1 && currentActive != index)
        {
            c_scripts[currentActive].Green();
        }
        currentActive = index;

        gamemanager.resolveCoins(prevCurr != index);

        if(prevCurr != index) { gamemanager.getController().IncrementCheckpointCount(1); }
    }

    public Vector3 getTransform()
    {
        return c_positions[currentActive];
    }

    public bool getReversed()
    {
        return c_scripts[currentActive].reversed;
    }

    public bool getMini()
    {
        return c_scripts[currentActive].mini;
    }

    public int getIndex()
    {
        return currentActive;
    }

    public int getSpeed()
    {
        return Convert.ToInt32(c_scripts[currentActive].speed.ToString().Substring(1));
    }
}
