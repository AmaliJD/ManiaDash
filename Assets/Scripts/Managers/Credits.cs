using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class Credits : MonoBehaviour
{
    public void Link(string url)
    {
        Application.OpenURL(url);
    }
}
