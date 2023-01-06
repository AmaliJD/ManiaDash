using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopAssignerChange : MonoBehaviour
{
    private List<int> object_ids;
    private List<AssignerTrigger> active_trigger;

    private void Awake()
    {
        object_ids = new List<int>();
        active_trigger = new List<AssignerTrigger>();
    }

    public void Send(int id)
    {
        int index = object_ids.IndexOf(id);

        if (index != -1 && active_trigger[index] != null)
        {
            active_trigger[index].Stop();
        }
    }

    public void setActiveTrigger(AssignerTrigger g, int id)
    {
        if (object_ids.IndexOf(id) == -1)
        {
            object_ids.Add(id);
            active_trigger.Add(g);
        }

        int index = object_ids.IndexOf(id);
        object_ids[index] = id;
        active_trigger[index] = g;
    }
}
