using System;
using UnityEngine;

[CreateAssetMenu]
public class ColorReference : ScriptableObject
{
    [SerializeField] private UnityEngine.Color color;
    [SerializeField] private ColorReference reference;
    private bool ref_flag = false;

    public float a
    {
        get => color.a;
        set => color.a = value;
    }

    public float r
    {
        get => color.r;
        set => color.r = value;
    }

    public float g
    {
        get => color.g;
        set => color.g = value;
    }

    public float b
    {
        get => color.b;
        set => color.b = value;
    }

    public Color channelcolor
    {
        get => color;
        set => color = value;
    }

    public ColorReference refer
    {
        get => reference;
    }

    public bool flag
    {
        get => ref_flag;
        set => ref_flag = value;
    }

    // Define every other function / property you need

    public void Set(UnityEngine.Color color)
    {
        #if UNITY_EDITOR
        //UnityEditor.Undo.RecordObject(this, "Color Channel Changed");
        #endif
        this.color = color;
        Changed?.Invoke();
    }

    public event Action Changed;

    public static implicit operator UnityEngine.Color(ColorReference color)
        => color.color;

    public static implicit operator ColorReference(UnityEngine.Color color)
        => new ColorReference() { color = color };
}