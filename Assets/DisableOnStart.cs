using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableOnStart : MonoBehaviour
{
    public bool destroy;
    void Start()
    {
        if (!destroy)
            gameObject.SetActive(false);
        else
            Destroy(gameObject);
    }
}
