using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupID : MonoBehaviour
{
    [Min(0)]
    public List<int> ID;

    public GameObject GetGameObject()
    {
        return gameObject;
    }
}
