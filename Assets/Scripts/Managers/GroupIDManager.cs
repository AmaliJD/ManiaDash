using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupIDManager : MonoBehaviour
{
    [HideInInspector]
    public Dictionary<int, List<GameObject>> groupIDList;

    private void Awake()
    {
        groupIDList = new Dictionary<int, List<GameObject>>();
        
        foreach(GroupID obj in FindObjectsOfType<GroupID>())
        {
            foreach(int i in obj.ID)
            {
                if (i > 0)
                {
                    if (groupIDList.ContainsKey(i))
                    {
                        groupIDList[i].Add(obj.GetGameObject());
                    }
                    else
                    {
                        groupIDList.Add(i, new List<GameObject>());
                        groupIDList[i].Add(obj.GetGameObject());
                    }
                }
            }
        }
    }
}
