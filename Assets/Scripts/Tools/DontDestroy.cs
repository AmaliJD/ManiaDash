using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    static DontDestroy instance = null;

    void Awake()
    {
        if (instance != null)
        {
            //Debug.Log("bgmusic already exists");
            Destroy(gameObject);
        }
        else
        {
            //Debug.Log("bgmusic does not exist");
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
