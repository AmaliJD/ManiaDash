using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class LevelsCompletedRecorder : ScriptableObject
{
    [SerializeField] int[] track;

    private void Awake()
    {
        track = new int[13];
    }

    public int[] Track
    {
        get => track;
        set => track = value;
    }
}
