using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public static class BuildHelper
{
    const string UNDO_ALIGN_X = "Align X";
    const string UNDO_ALIGN_Y = "Align Y";
    const string UNDO_ALIGN_R = "Align R";
    const string UNDO_ALIGN_RR = "Align R Override";
    const string UNDO_ACTIVATE = "Activate";
    const string UNDO_ENABLE = "Enable";
    const string UNDO_DISABLE = "Disable";
    const string UNDO_LAYER_ORDER_UP = "Layer Order Up";
    const string UNDO_LAYER_ORDER_DOWN = "Layer Order Down";

    [MenuItem("BuildHelper/Align X #X", isValidateFunction: true)]
    public static bool AlignXValidate()
    {
        return Selection.gameObjects.Length > 2;
    }

    [MenuItem("BuildHelper/Align Y #Y", isValidateFunction: true)]
    public static bool AlignYValidate()
    {
        return Selection.gameObjects.Length > 2;
    }

    [MenuItem("BuildHelper/Align R #R", isValidateFunction: true)]
    public static bool AlignRValidate()
    {
        return Selection.gameObjects.Length > 2;
    }

    [MenuItem("BuildHelper/Align R (Override) #&R", isValidateFunction: true)]
    public static bool AlignROverrideValidate()
    {
        return Selection.gameObjects.Length > 2;
    }

    [MenuItem("BuildHelper/Activate #A", isValidateFunction: true)]
    public static bool ActivateValidate()
    {
        return Selection.gameObjects.Length > 0;
    }

    [MenuItem("BuildHelper/Enable All Children #1", isValidateFunction: true)]
    public static bool EnableAllChildrenValidate()
    {
        return Selection.gameObjects.Length > 0 && Selection.transforms.Length > 0;
    }

    [MenuItem("BuildHelper/Disable All Children #2", isValidateFunction: true)]
    public static bool DisableAllChildrenValidate()
    {
        return Selection.gameObjects.Length > 0 && Selection.transforms.Length > 0;
    }

    [MenuItem("BuildHelper/Increase Order In Layer #=", isValidateFunction: true)]
    public static bool IncreaseOrderInLayerValidate()
    {
        return Selection.gameObjects.Length > 0 && Selection.transforms.Length > 0;
    }

    [MenuItem("BuildHelper/Decrease Order In Layer #-", isValidateFunction: true)]
    public static bool DecreaseOrderInLayerValidate()
    {
        return Selection.gameObjects.Length > 0 && Selection.transforms.Length > 0;
    }

    [MenuItem("BuildHelper/Align X #X")]
    public static void AlignX()
    {
        //GameObject[] xList = Selection.gameObjects.OrderBy(obj => obj.transform.position.x).ToArray();
        Transform[] xList = Selection.GetTransforms(SelectionMode.TopLevel).OrderBy(obj => obj.position.x).ToArray();
        float step = (xList[xList.Length-1].position.x - xList[0].position.x) / (xList.Length-1);
        float startPos = xList[0].position.x;

        int i = 0;
        foreach(Transform obj in xList)
        {
            Undo.RecordObject(obj.transform, UNDO_ALIGN_X);
            obj.position = obj.position.SetX(startPos + (i * step));
            i++;
        }
    }

    [MenuItem("BuildHelper/Align Y #Y")]
    public static void AlignY()
    {
        //GameObject[] yList = Selection.gameObjects.OrderBy(obj => obj.transform.position.y).ToArray();
        Transform[] yList = Selection.GetTransforms(SelectionMode.TopLevel).OrderBy(obj => obj.position.y).ToArray();
        float step = (yList[yList.Length - 1].position.y - yList[0].position.y) / (yList.Length-1);
        float startPos = yList[0].position.y;

        int i = 0;
        foreach (Transform obj in yList)
        {
            Undo.RecordObject(obj.transform, UNDO_ALIGN_Y);
            obj.position = obj.position.SetY(startPos + (i * step));
            i++;
        }
    }

    [MenuItem("BuildHelper/Align R #R")]
    public static void AlignR()
    {
        Transform[] xList = Selection.GetTransforms(SelectionMode.TopLevel).OrderBy(obj => obj.position.x).ToArray();// Selection.gameObjects.OrderBy(obj => obj.transform.position.x).ToArray();
        Vector3 pivot = GetPivot(Selection.GetTransforms(SelectionMode.TopLevel), false);
        float radius = Vector3.Distance(xList[0].position, pivot);

        float startAngle = Vector3.SignedAngle(xList[0].position - pivot, Vector3.left, Vector3.back);
        if(startAngle < 0)
        {
            startAngle = 360 + startAngle;
        }

        Debug.Log("Start Angle: " + startAngle);

        float step = 360f / (float)xList.Length;

        float i = 0;
        foreach (Transform obj in xList)
        {
            var angle = startAngle + i*step;
            Debug.Log("Next Angle: " + angle);
            var direction = new Vector3(-Mathf.Cos(Mathf.Deg2Rad * angle), -Mathf.Sin(Mathf.Deg2Rad * angle), 0).normalized;
            Undo.RecordObject(obj.transform, UNDO_ALIGN_R);
            obj.position = pivot + radius * direction;
            i++;
        }
    }

    [MenuItem("BuildHelper/Align R (Override) #&R")]
    public static void AlignROverride()
    {
        //GameObject[] xList = Selection.gameObjects.OrderBy(obj => obj.transform.position.x).ToArray();
        Transform[] xList = Selection.GetTransforms(SelectionMode.TopLevel).OrderBy(obj => obj.position.x).ToArray();
        Vector3 pivot = GetPivot(Selection.GetTransforms(SelectionMode.TopLevel), true);
        float radius = Vector3.Distance(xList[0].position, pivot);

        float startAngle = Vector3.SignedAngle(xList[0].position - pivot, Vector3.left, Vector3.back);
        if (startAngle < 0)
        {
            startAngle = 360 + startAngle;
        }

        Debug.Log("Start Angle: " + startAngle);

        float step = 360f / (float)xList.Length;

        float i = 0;
        foreach (Transform obj in xList)
        {
            var angle = startAngle + i * step;
            Debug.Log("Next Angle: " + angle);
            var direction = new Vector3(-Mathf.Cos(Mathf.Deg2Rad * angle), -Mathf.Sin(Mathf.Deg2Rad * angle), 0).normalized;
            Undo.RecordObject(obj.transform, UNDO_ALIGN_RR);
            obj.position = pivot + radius * direction;
            i++;
        }
    }

    public static Vector3 GetPivot(Transform[] transforms, bool overrideParent)
    {
        Transform firstParent = transforms[0].parent;
        bool sameParent = firstParent != null;
        foreach(Transform tr in transforms)
        {
            if(tr.parent != firstParent && firstParent != null)
            {
                sameParent = false;
                continue;
            }
        }
        if(sameParent && !overrideParent)
        {
            return firstParent.position;
        }

        float minX = Mathf.Infinity;
        float minY = Mathf.Infinity;
        float minZ = Mathf.Infinity;

        float maxX = -Mathf.Infinity;
        float maxY = -Mathf.Infinity;
        float maxZ = -Mathf.Infinity;

        foreach (Transform tr in transforms)
        {
            if (tr.position.x < minX)
                minX = tr.position.x;
            if (tr.position.y < minY)
                minY = tr.position.y;
            if (tr.position.z < minZ)
                minZ = tr.position.z;

            if (tr.position.x > maxX)
                maxX = tr.position.x;
            if (tr.position.y > maxY)
                maxY = tr.position.y;
            if (tr.position.z > maxZ)
                maxZ = tr.position.z;
        }

        return new Vector3((minX + maxX) / 2.0f, (minY + maxY) / 2.0f, (minZ + maxZ) / 2.0f);
    }

    /*public static Vector3 SetX(this Vector3 vector, float value)
    {
        vector = new Vector3(value, vector.y, vector.z);
        return vector;
    }

    public static Vector3 SetY(this Vector3 vector, float value)
    {
        vector = new Vector3(vector.x, value, vector.z);
        return vector;
    }

    public static Vector3 SetZ(this Vector3 vector, float value)
    {
        vector = new Vector3(vector.x, vector.y, value);
        return vector;
    }*/

    [MenuItem("BuildHelper/Activate #A")]
    public static void Activate()
    {
        GameObject[] colorTriggerList = Selection.gameObjects.Where(obj => obj.GetComponent<ColorTrigger>() && obj.GetComponent<ColorTrigger>().channelmode).ToArray();
        GameObject[] pulseTriggerList = Selection.gameObjects.Where(obj => obj.GetComponent<PulseTrigger>() && obj.GetComponent<PulseTrigger>().channelmode).ToArray();

        foreach (GameObject obj in colorTriggerList)
        {
            ColorTrigger ct = obj.GetComponent<ColorTrigger>();
            Undo.RecordObject(ct.channel, UNDO_ACTIVATE);
            ct.channel.Set(ct.new_color);
        }

        foreach (GameObject obj in pulseTriggerList)
        {
            PulseTrigger ct = obj.GetComponent<PulseTrigger>();
            Undo.RecordObject(ct.channel, UNDO_ACTIVATE);
            ct.channel.Set(ct.new_color);
        }
    }

    [MenuItem("BuildHelper/Enable All Children #1")]
    public static void EnableAllChildren()
    {
        List<GameObject> objectsList = new List<GameObject>();

        foreach(Transform t in Selection.transforms)
        {
            foreach (Transform child in t)
            {
                objectsList.Add(child.gameObject);
            }
        }

        foreach (GameObject obj in objectsList)
        {
            //ColorTrigger ct = obj.GetComponent<ColorTrigger>();
            //Undo.RecordObject(ct.channel, UNDO_Enable);
            Undo.RecordObject(obj, UNDO_ENABLE);
            //ct.channel.Set(ct.new_color);
            obj.SetActive(true);
        }
    }

    [MenuItem("BuildHelper/Disable All Children #2")]
    public static void DisableAllChildren()
    {
        List<GameObject> objectsList = new List<GameObject>();

        foreach (Transform t in Selection.transforms)
        {
            foreach (Transform child in t)
            {
                objectsList.Add(child.gameObject);
            }
        }

        foreach (GameObject obj in objectsList)
        {
            //ColorTrigger ct = obj.GetComponent<ColorTrigger>();
            Undo.RecordObject(obj, UNDO_DISABLE);
            //ct.channel.Set(ct.new_color);
            obj.SetActive(false);
        }
    }

    [MenuItem("BuildHelper/Increase Order In Layer #=")]
    public static void IncreaseOrderInLayer()
    {
        List<SpriteRenderer> spriteList = Selection.gameObjects
                                            .Where(x => x.GetComponent<SpriteRenderer>() != null)
                                            .ToList()
                                            .ConvertAll(x => x.GetComponent<SpriteRenderer>());

        foreach (SpriteRenderer sprt in spriteList)
        {
            Undo.RecordObject(sprt, UNDO_LAYER_ORDER_UP);
            sprt.sortingOrder++;
        }
    }

    [MenuItem("BuildHelper/Decrease Order In Layer #-")]
    public static void DecreaseOrderInLayer()
    {
        List<SpriteRenderer> spriteList = Selection.gameObjects
                                            .Where(x => x.GetComponent<SpriteRenderer>() != null)
                                            .ToList()
                                            .ConvertAll(x => x.GetComponent<SpriteRenderer>());

        foreach (SpriteRenderer sprt in spriteList)
        {
            Undo.RecordObject(sprt, UNDO_LAYER_ORDER_DOWN);
            sprt.sortingOrder--;
        }
    }
}
