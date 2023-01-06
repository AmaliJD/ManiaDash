using System;
using UnityEngine;

[CreateAssetMenu]
public class MenuState : ScriptableObject
{
    [SerializeField] int state;
    //[SerializeField] bool[] level100complete;

    public int State
    {
        get => state;
        set => state = value;
    }

    /*public bool[] Level100
    {
        get => level100complete;
        set => level100complete = value;
    }*/
}