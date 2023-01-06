using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleTracker : MonoBehaviour
{
    private List<int> object_ids;
    private List<Vector3> object_scales;
    private List<Vector3> original_scales;

    private void Awake()
    {
        object_ids = new List<int>();
        object_scales = new List<Vector3>();
        original_scales = new List<Vector3>();
    }

    public void setNewScale(int id, Vector3 newScale)
    {
        /*if(!object_ids.Contains(id))
        {
            object_ids.Add(id);
            object_scales.Add(scale);
        }
        else
        {
            int index = object_ids.IndexOf(id);
            var objScale = object_scales[index];

            objScale.x = newScale.x;
            objScale.y = newScale.y;
        }*/

        int index = object_ids.IndexOf(id);
        /*var objScale = object_scales[index];

        objScale.x = newScale.x;
        objScale.y = newScale.y;*/

        object_scales[index] = newScale;
        //Debug.Log("Scale to: " + newScale);// object_scales[object_ids.IndexOf(id)]);
    }

    public Vector3 getBaseScale(int id, Vector3 scale)
    {
        if (!object_ids.Contains(id))
        {
            object_ids.Add(id);
            object_scales.Add(scale);
            original_scales.Add(scale);
        }
        
        int index = object_ids.IndexOf(id);// Debug.Log("Base: " + object_scales[index]);
        return object_scales[index];
    }

    public void setOriginalScale(int id)
    {
        if (object_ids.Contains(id))
        {
            int index = object_ids.IndexOf(id);
            object_scales[index] = original_scales[index];
        }
        
    }
}
