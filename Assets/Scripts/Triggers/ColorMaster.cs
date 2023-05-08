using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.UI;
using System.Linq;

public class ColorMaster : MonoBehaviour
{
    private Dictionary<ColorReference, List<Vector4>> channelList;
    private Dictionary<ColorReference, List<float>> opacityList;
    private Dictionary<ColorReference, List<string>> triggerHashList;
    private Dictionary<ColorReference, bool> colorChanging;
    private List<ColorReference> deleteList;

    private Dictionary<GameObject, List<Vector4>> ObjchannelList;
    private Dictionary<GameObject, List<float>> ObjopacityList;
    private Dictionary<GameObject, List<string>> ObjtriggerHashList;
    private Dictionary<GameObject, bool> ObjcolorChanging;    
    private List<GameObject> deleteObjList;

    private float deleteTimer = 0;
    private const float DELETE_TIME = 30;

    private void Awake()
    {
        channelList = new Dictionary<ColorReference, List<Vector4>>();
        opacityList = new Dictionary<ColorReference, List<float>>();
        triggerHashList = new Dictionary<ColorReference, List<string>>();
        colorChanging = new Dictionary<ColorReference, bool>();
        deleteList = new List<ColorReference>();

        ObjchannelList = new Dictionary<GameObject, List<Vector4>>();
        ObjopacityList = new Dictionary<GameObject, List<float>>();
        ObjtriggerHashList = new Dictionary<GameObject, List<string>>();
        ObjcolorChanging = new Dictionary<GameObject, bool>();
        deleteObjList = new List<GameObject>();
    }

    private void LateUpdate()
    {
        foreach(ColorReference cr in channelList.Keys)
        {
            cr.Set(CalculateColor(channelList[cr], opacityList[cr]));

            if (deleteTimer >= DELETE_TIME && channelList[cr].Count - 1 == 0 && !colorChanging[cr] && !deleteList.Contains(cr))
            {
                deleteList.Add(cr);
            }
        }

        ObjchannelList = ObjchannelList.Where(x => x.Key != null).ToDictionary(x => x.Key, x => x.Value);
        ObjopacityList = ObjopacityList.Where(x => x.Key != null).ToDictionary(x => x.Key, x => x.Value);
        ObjtriggerHashList = ObjtriggerHashList.Where(x => x.Key != null).ToDictionary(x => x.Key, x => x.Value);
        ObjcolorChanging = ObjcolorChanging.Where(x => x.Key != null).ToDictionary(x => x.Key, x => x.Value);

        foreach (GameObject obj in ObjchannelList.Keys)
        {
            if (obj.GetComponent<SpriteRenderer>() != null)
            {
                SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
                renderer.color = CalculateColor(ObjchannelList[obj], ObjopacityList[obj]);
            }
            else if (obj.GetComponent<Tilemap>() != null)
            {
                Tilemap renderer = obj.GetComponent<Tilemap>();
                renderer.color = CalculateColor(ObjchannelList[obj], ObjopacityList[obj]);
            }
            else if (obj.GetComponent<Light2D>() != null)
            {
                Light2D renderer = obj.GetComponent<Light2D>();
                renderer.color = CalculateColor(ObjchannelList[obj], ObjopacityList[obj]);
            }
            else if (obj.GetComponent<Graphic>() != null)
            {
                Graphic renderer = obj.GetComponent<Graphic>();
                renderer.color = CalculateColor(ObjchannelList[obj], ObjopacityList[obj]);
            }

            if(deleteTimer >= DELETE_TIME && ObjchannelList[obj].Count - 1 == 0 && !ObjcolorChanging[obj] && !deleteObjList.Contains(obj))
            {
                deleteObjList.Add(obj);
            }
        }

        if(deleteTimer >= DELETE_TIME)
        {
            foreach (ColorReference cr in deleteList)
            {
                channelList.Remove(cr);
                opacityList.Remove(cr);
                triggerHashList.Remove(cr);
                colorChanging.Remove(cr);
            }
            foreach (GameObject obj in deleteObjList)
            {
                ObjchannelList.Remove(obj);
                ObjopacityList.Remove(obj);
                ObjtriggerHashList.Remove(obj);
                ObjcolorChanging.Remove(obj);
            }

            deleteList = new List<ColorReference>();
            deleteObjList = new List<GameObject>();
            deleteTimer = 0;
        }

        deleteTimer += Time.deltaTime;
        //Debug.Log("Colors List: " + (ObjchannelList.Keys.Count + channelList.Keys.Count) + ", timer: " + deleteTimer);

        /*string res = "";
        foreach (var item in channelList.Keys)
        {
            res += item.name + ", ";
        }
        res += channelList.Count;
        Debug.Log("List: " + res + " timer: " + deleteTimer);*/
    }

    Color CalculateColor(List<Vector4> colorStack, List<float> opacityStack)
    {
        Vector4 temp = colorStack[0];
        for(int i = 1; i < colorStack.Count; i++)
        {
            temp = (opacityStack[i] * colorStack[i]) + ((1-opacityStack[i]) * temp);
        }

        return new Color(temp.x, temp.y, temp.z, temp.w);
    }

    public void addColorChannel(ColorReference channel, bool cancel)
    {
        if(!channelList.ContainsKey(channel))
        {
            channelList.Add(channel, new List<Vector4> { new Vector4(channel.r, channel.g, channel.b, channel.a)});
            opacityList.Add(channel, new List<float> { 0 });
            triggerHashList.Add(channel, new List<string> { "" });
            colorChanging.Add(channel, false);
        }
        else if(cancel)
        {
            Vector4 baseColor = channelList[channel][0];
            float baseOpacity = opacityList[channel][0];
            channelList[channel] = new List<Vector4> { baseColor };
            opacityList[channel] = new List<float> { baseOpacity };
            triggerHashList[channel] = new List<string> { "" };
        }
    }

    public void addGameObject(GameObject obj, bool cancel)
    {
        if (!ObjchannelList.ContainsKey(obj))
        {
            if (obj.GetComponent<SpriteRenderer>() != null)
            {
                SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
                ObjchannelList.Add(obj, new List<Vector4> { new Vector4(renderer.color.r, renderer.color.g, renderer.color.b, renderer.color.a) });
            }
            else if (obj.GetComponent<Tilemap>() != null)
            {
                Tilemap renderer = obj.GetComponent<Tilemap>();
                ObjchannelList.Add(obj, new List<Vector4> { new Vector4(renderer.color.r, renderer.color.g, renderer.color.b, renderer.color.a) });
            }
            else if (obj.GetComponent<Light2D>() != null)
            {
                Light2D renderer = obj.GetComponent<Light2D>();
                ObjchannelList.Add(obj, new List<Vector4> { new Vector4(renderer.color.r, renderer.color.g, renderer.color.b, renderer.color.a) });
            }
            else if (obj.GetComponent<Graphic>() != null)
            {
                Graphic renderer = obj.GetComponent<Graphic>();
                ObjchannelList.Add(obj, new List<Vector4> { new Vector4(renderer.color.r, renderer.color.g, renderer.color.b, renderer.color.a) });
            }
            
            ObjopacityList.Add(obj, new List<float> { 0 });
            ObjtriggerHashList.Add(obj, new List<string> { "" });
            ObjcolorChanging.Add(obj, false);
        }
        else if (cancel)
        {
            Vector4 baseColor = ObjchannelList[obj][0];
            float baseOpacity = ObjopacityList[obj][0];
            ObjchannelList[obj] = new List<Vector4> { baseColor };
            ObjopacityList[obj] = new List<float> { baseOpacity };
            ObjtriggerHashList[obj] = new List<string> { "" };
        }
    }

    public IEnumerator Pulse(string triggerHash, ColorReference channel, Color color, float fadein, float hold, float fadeout, Gradient gradient = null)
    {
        if (triggerHashList[channel].Contains(triggerHash))
        {
            yield break;
        }

        float opacity = 0;
        int layer = 1;
        Vector4 colorVector = new Vector4(color.r, color.g, color.b, color.a);
        int colorIndex = 0;

        channelList[channel].Add(colorVector);
        opacityList[channel].Add(opacity);
        triggerHashList[channel].Add(triggerHash);

        colorIndex = channelList[channel].Count - 1;
        if (gradient != null)
        {
            channelList[channel][colorIndex] = gradient.Evaluate(0);
        }

        layer = triggerHashList[channel].IndexOf(triggerHash);
        int startingLayer = layer;

        float time = 0;
        if (fadein == 0)
        {
            layer = triggerHashList[channel].IndexOf(triggerHash);
            if (layer == -1)
            {
                yield break;
            }
            opacity = 1;
            opacityList[channel][layer] = opacity;
            yield return null;
        }
        else
        {
            while (time <= fadein)
            {
                layer = triggerHashList[channel].IndexOf(triggerHash);
                if (layer == -1)
                {
                    yield break;
                }
                opacity = Mathf.Lerp(0, 1, time / fadein);
                opacityList[channel][layer] = opacity;
                time += Time.deltaTime;
                yield return null;
            }

            layer = triggerHashList[channel].IndexOf(triggerHash);
            if (layer == -1)
            {
                yield break;
            }
            opacity = 1;
            opacityList[channel][layer] = opacity;
            yield return null;
        }

        time = 0;
        while (time <= hold)
        {
            layer = triggerHashList[channel].IndexOf(triggerHash);
            if (layer == -1)
            {
                yield break;
            }
            else if(layer != startingLayer)
            {
                colorIndex -= startingLayer - layer;
                startingLayer = layer;
            }
            if (gradient != null && hold != 0)
            {
                channelList[channel][colorIndex] = gradient.Evaluate(Mathf.Clamp01(time/hold));
            }
            time += Time.deltaTime;
            yield return null;
        }
        if (gradient != null)
        {
            channelList[channel][colorIndex] = gradient.Evaluate(1);
        }

        time = 0;
        if (fadeout == 0)
        {
            layer = triggerHashList[channel].IndexOf(triggerHash);
            if (layer == -1)
            {
                yield break;
            }
            opacity = 0;
            opacityList[channel][layer] = opacity;
            yield return null;
        }
        else
        {
            while (time <= fadeout)
            {
                layer = triggerHashList[channel].IndexOf(triggerHash);
                if (layer == -1)
                {
                    yield break;
                }
                opacity = Mathf.Lerp(1, 0, time / fadeout);
                opacityList[channel][layer] = opacity;
                time += Time.deltaTime;
                yield return null;
            }

            layer = triggerHashList[channel].IndexOf(triggerHash);
            if (layer == -1)
            {
                yield break;
            }
            opacity = 0;
            opacityList[channel][layer] = opacity;
            yield return null;
        }

        yield return new WaitForEndOfFrame();
        layer = triggerHashList[channel].IndexOf(triggerHash);
        if (layer != -1)
        {
            opacityList[channel].RemoveAt(layer);
            channelList[channel].RemoveAt(layer);
            triggerHashList[channel].RemoveAt(layer);
        }
    }

    public IEnumerator PulseObj(string triggerHash, GameObject obj, Color color, float fadein, float hold, float fadeout, Gradient gradient = null)
    {
        if (ObjtriggerHashList[obj].Contains(triggerHash))
        {
            yield break;
        }

        float opacity = 0;
        int layer = 1;
        Vector4 colorVector = new Vector4(color.r, color.g, color.b, color.a);
        int colorIndex = 0;

        ObjchannelList[obj].Add(colorVector);
        ObjopacityList[obj].Add(opacity);
        ObjtriggerHashList[obj].Add(triggerHash);

        colorIndex = ObjchannelList[obj].Count - 1;
        if (gradient != null)
        {
            ObjchannelList[obj][colorIndex] = gradient.Evaluate(0);
        }

        layer = ObjtriggerHashList[obj].IndexOf(triggerHash);
        int startingLayer = layer;

        float time = 0;
        if (fadein == 0)
        {
            layer = ObjtriggerHashList[obj].IndexOf(triggerHash);
            if (layer == -1)
            {
                yield break;
            }
            opacity = 1;
            ObjopacityList[obj][layer] = opacity;
            yield return null;
        }
        else
        {
            while (time <= fadein)
            {
                layer = ObjtriggerHashList[obj].IndexOf(triggerHash);
                if (layer == -1)
                {
                    yield break;
                }
                opacity = Mathf.Lerp(0, 1, time / fadein);
                ObjopacityList[obj][layer] = opacity;
                time += Time.deltaTime;
                yield return null;
            }

            layer = ObjtriggerHashList[obj].IndexOf(triggerHash);
            if (layer == -1)
            {
                yield break;
            }
            opacity = 1;
            ObjopacityList[obj][layer] = opacity;
            yield return null;
        }

        time = 0;
        while (time <= hold)
        {
            layer = ObjtriggerHashList[obj].IndexOf(triggerHash);
            if (layer == -1)
            {
                yield break;
            }
            else if (layer != startingLayer)
            {
                colorIndex -= startingLayer - layer;
                startingLayer = layer;
            }
            if (gradient != null && hold != 0)
            {
                ObjchannelList[obj][colorIndex] = gradient.Evaluate(Mathf.Clamp01(time / hold));
            }
            time += Time.deltaTime;
            yield return null;
        }
        if (gradient != null)
        {
            ObjchannelList[obj][colorIndex] = gradient.Evaluate(1);
        }

        time = 0;
        if (fadeout == 0)
        {
            layer = ObjtriggerHashList[obj].IndexOf(triggerHash);
            if (layer == -1)
            {
                yield break;
            }
            opacity = 0;
            ObjopacityList[obj][layer] = opacity;
            yield return null;
        }
        else
        {
            while (time <= fadeout)
            {
                layer = ObjtriggerHashList[obj].IndexOf(triggerHash);
                if (layer == -1)
                {
                    yield break;
                }
                opacity = Mathf.Lerp(1, 0, time / fadeout);
                ObjopacityList[obj][layer] = opacity;
                time += Time.deltaTime;
                yield return null;
            }

            layer = ObjtriggerHashList[obj].IndexOf(triggerHash);
            if (layer == -1)
            {
                yield break;
            }
            opacity = 0;
            ObjopacityList[obj][layer] = opacity;
            yield return null;
        }

        yield return new WaitForEndOfFrame();
        layer = ObjtriggerHashList[obj].IndexOf(triggerHash);
        if (layer != -1)
        {
            ObjopacityList[obj].RemoveAt(layer);
            ObjchannelList[obj].RemoveAt(layer);
            ObjtriggerHashList[obj].RemoveAt(layer);
        }
    }

    public IEnumerator PulseCurve(string triggerHash, ColorReference channel, Color color, float duration, AnimationCurve curve, Gradient gradient = null)
    {
        if (triggerHashList[channel].Contains(triggerHash))
        {
            yield break;
        }

        float opacity = 0;
        int layer = 1;
        Vector4 colorVector = new Vector4(color.r, color.g, color.b, color.a);
        int colorIndex = 0;

        channelList[channel].Add(colorVector);
        opacityList[channel].Add(opacity);
        triggerHashList[channel].Add(triggerHash);

        colorIndex = channelList[channel].Count - 1;
        if (gradient != null)
        {
            channelList[channel][colorIndex] = gradient.Evaluate(0);
        }

        layer = triggerHashList[channel].IndexOf(triggerHash);
        int startingLayer = layer;

        float time = 0;
        if (duration == 0)
        {
            layer = triggerHashList[channel].IndexOf(triggerHash);
            if (layer == -1)
            {
                yield break;
            }
            opacity = 0;
            opacityList[channel][layer] = opacity;
            yield return null;
        }
        else
        {
            while (time <= duration)
            {
                layer = triggerHashList[channel].IndexOf(triggerHash);
                if (layer == -1)
                {
                    yield break;
                }
                else if (layer != startingLayer)
                {
                    colorIndex -= startingLayer - layer;
                    startingLayer = layer;
                }
                if (gradient != null && duration != 0)
                {
                    channelList[channel][colorIndex] = gradient.Evaluate(Mathf.Clamp01(time / duration));
                }

                opacity = Mathf.Clamp(curve.Evaluate(time / duration), 0, 1);
                opacityList[channel][layer] = opacity;
                time += Time.deltaTime;
                yield return null;
            }

            layer = triggerHashList[channel].IndexOf(triggerHash);
            if (layer == -1)
            {
                yield break;
            }
            
            opacity = 0;
            opacityList[channel][layer] = opacity;
            yield return null;
        }

        yield return new WaitForEndOfFrame();
        layer = triggerHashList[channel].IndexOf(triggerHash);
        if (layer == -1)
        {
            opacityList[channel].RemoveAt(layer);
            channelList[channel].RemoveAt(layer);
            triggerHashList[channel].RemoveAt(layer);
        }
    }

    public IEnumerator PulseObjCurve(string triggerHash, GameObject obj, Color color, float duration, AnimationCurve curve, Gradient gradient = null)
    {
        if (ObjtriggerHashList[obj].Contains(triggerHash))
        {
            yield break;
        }

        float opacity = 0;
        int layer = 1;
        Vector4 colorVector = new Vector4(color.r, color.g, color.b, color.a);
        int colorIndex = 0;

        ObjchannelList[obj].Add(colorVector);
        ObjopacityList[obj].Add(opacity);
        ObjtriggerHashList[obj].Add(triggerHash);

        colorIndex = ObjchannelList[obj].Count - 1;
        if (gradient != null)
        {
            ObjchannelList[obj][colorIndex] = gradient.Evaluate(0);
        }

        layer = ObjtriggerHashList[obj].IndexOf(triggerHash);
        int startingLayer = layer;

        float time = 0;
        if (duration == 0)
        {
            layer = ObjtriggerHashList[obj].IndexOf(triggerHash);
            if (layer == -1)
            {
                yield break;
            }
            opacity = 0;
            ObjopacityList[obj][layer] = opacity;
            yield return null;
        }
        else
        {
            while (time <= duration)
            {
                layer = ObjtriggerHashList[obj].IndexOf(triggerHash);
                if (layer == -1)
                {
                    yield break;
                }
                else if (layer != startingLayer)
                {
                    colorIndex -= startingLayer - layer;
                    startingLayer = layer;
                }
                if (gradient != null && duration != 0)
                {
                    ObjchannelList[obj][colorIndex] = gradient.Evaluate(Mathf.Clamp01(time / duration));
                }

                opacity = Mathf.Clamp(curve.Evaluate(time / duration), 0, 1);
                ObjopacityList[obj][layer] = opacity;
                time += Time.deltaTime;
                yield return null;
            }

            layer = ObjtriggerHashList[obj].IndexOf(triggerHash);
            if (layer == -1)
            {
                yield break;
            }
            opacity = 0;
            ObjopacityList[obj][layer] = opacity;
            yield return null;
        }

        yield return new WaitForEndOfFrame();
        layer = ObjtriggerHashList[obj].IndexOf(triggerHash);
        if (layer != -1)
        {
            ObjopacityList[obj].RemoveAt(layer);
            ObjchannelList[obj].RemoveAt(layer);
            ObjtriggerHashList[obj].RemoveAt(layer);
        }
    }

    public IEnumerator ColorChange(ColorReference channel, Color color, float duration)
    {
        colorChanging[channel] = true;
        opacityList[channel][0]++;
        int iter = (int)opacityList[channel][0];

        Color start = new Color(channelList[channel][0].x, channelList[channel][0].y, channelList[channel][0].z, channelList[channel][0].w);
        Color curr = start;

        float time = 0;
        if (duration == 0)
        {
            channelList[channel][0] = new Vector4(color.r, color.g, color.b, color.a);
            yield return null;
        }
        else
        {
            while (time <= duration)
            {
                if(iter != opacityList[channel][0])
                {
                    colorChanging[channel] = false;
                    yield break;
                }

                curr = Color.Lerp(start, color, time / duration);
                channelList[channel][0] = new Vector4(curr.r, curr.g, curr.b, curr.a);
                time += Time.deltaTime;
                yield return null;
            }

            if (iter != opacityList[channel][0])
            {
                colorChanging[channel] = false;
                yield break;
            }
            curr = color;
            channelList[channel][0] = new Vector4(curr.r, curr.g, curr.b, curr.a);
            yield return null;
        }

        colorChanging[channel] = false;
    }

    public IEnumerator ColorChangeObj(GameObject obj, Color color, float duration)
    {
        ObjcolorChanging[obj] = true;
        ObjopacityList[obj][0]++;
        int iter = (int)ObjopacityList[obj][0];

        Color start = new Color(ObjchannelList[obj][0].x, ObjchannelList[obj][0].y, ObjchannelList[obj][0].z, ObjchannelList[obj][0].w);
        Color curr = start;

        float time = 0;
        if (duration == 0)
        {
            ObjchannelList[obj][0] = new Vector4(color.r, color.g, color.b, color.a);
            yield return null;
        }
        else
        {
            while (time <= duration)
            {
                if (iter != ObjopacityList[obj][0])
                {
                    ObjcolorChanging[obj] = false;
                    yield break;
                }

                curr = Color.Lerp(start, color, time / duration);
                ObjchannelList[obj][0] = new Vector4(curr.r, curr.g, curr.b, curr.a);
                time += Time.deltaTime;
                yield return null;
            }

            if (iter != ObjopacityList[obj][0])
            {
                ObjcolorChanging[obj] = false;
                yield break;
            }
            curr = color;
            ObjchannelList[obj][0] = new Vector4(curr.r, curr.g, curr.b, curr.a);
            yield return null;
        }

        ObjcolorChanging[obj] = false;
    }
}