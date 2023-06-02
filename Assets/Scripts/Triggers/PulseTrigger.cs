using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.UI;
using System.Linq;

public class PulseTrigger : MonoBehaviour
{
    public float delay; private bool delaying = false;

    public bool channelmode = true, copy = false;

    public enum AssignerType { Default, All, Sprite, Light, Tilemap, UI};
    public AssignerType assignerType;

    [Min(0)]
    public int groupID;

    public ColorReference channel;
    public List<GameObject> objects;
    public ColorReference copy_color;
    public Color new_color;

    public Gradient newColorGradient;
    public bool useGradient;

    [Range(-360f, 360f)] public float hue;
    [Range(-1f, 1f)] public float sat;
    [Range(-1f, 1f)] public float val;
    [Range(-1f, 1f)] public float alpha;

    public bool refCurrent, invert, filter;
    public Vector3 filterRGB;

    public bool randomize;
    public Vector4 randomizeRange;

    [Range(-1, 1)]
    public float hueshift;

    [Range(0, 2)]
    public int alterIterMode;

    public bool radial;
    public enum RadialMode { rOut, rIn, xPos, xNeg, yPos, yNeg }
    public RadialMode radialmode;

    [Min(0.1f)]
    public float range, radialSpeed;
    public bool recalcCenter;
    public Transform center;

    [Min(0f)]
    public float fadein, hold, duration;
    public float refresh;
    public bool usecurve;
    public AnimationCurve curve;
    public bool oneuse = false;
    private bool finished = true, inuse = false;
    private float refreshTime;

    private ColorMaster colormaster;
    private int timesActivated = 0;
    public bool cancelActivePulse;

    private GroupIDManager groupIDManager;

    public enum Speed
    {
        x0, x1, x2, x3, x4
    }
    public Speed speed = Speed.x1;
    public bool durationline;

    public bool previewMode;

    // Start is called before the first frame update
    void Awake()
    {
        //colormaster = FindObjectOfType<ColorMaster>();
        colormaster = GameObject.FindGameObjectWithTag("Master").GetComponent<ColorMaster>();
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        refreshTime = Time.time;
    }

    private void Start()
    {
        if (copy && copy_color != null)
        {
            new_color = copy_color.channelcolor;
        }

        if (randomize)
        {
            new_color = Random.ColorHSV(randomizeRange.x, randomizeRange.y, randomizeRange.z, 1, randomizeRange.w, 1);
            if (useGradient) { newColorGradient = randomizeGradient(newColorGradient); }
        }

        if (useGradient) { newColorGradient = shiftGradient(newColorGradient); }

        float h = 0, s = 0, v = 0, a = new_color.a;
        Color.RGBToHSV(new_color, out h, out s, out v);

        h += (hue / 360);
        s += sat;
        v += val;
        a += alpha;

        if (h > 1) { h -= 1; }
        else if (h < 0) { h += 1; }

        if (s > 1) { s = 1; }
        else if (s < 0) { s = 0; }

        if (v > 1) { v = 1; }
        else if (v < 0) { v = 0; }

        if (a > 1) { a = 1; }
        else if (a < 0) { a = 0; }

        new_color = Color.HSVToRGB(h, s, v);
        new_color.a = a;

        if (invert)
        {
            new_color = new Color(1 - new_color.r, 1 - new_color.g, 1 - new_color.b, a);
        }
        if (filter)
        {
            new_color = new Color(filterRGB.x * new_color.r, filterRGB.y * new_color.g, filterRGB.z * new_color.b, a);
        }

        if (groupID > 0)
        {
            groupIDManager = GameObject.FindGameObjectWithTag("Master").GetComponent<GroupIDManager>();
            if (!radial)
            {
                objects.AddRange(groupIDManager.groupIDList[groupID]);
            }
            else
            {
                if(center == null) { center = transform; }
                objects.AddRange(groupIDManager.groupIDList[groupID].FindAll(obj => Vector2.Distance(center.position, obj.transform.position) <= range));

                switch(radialmode)
                {
                    case RadialMode.rOut:
                        objects.Sort((obj1, obj2) => Vector2.Distance(center.position, obj1.transform.position).CompareTo(Vector2.Distance(center.position, obj2.transform.position))); break;

                    case RadialMode.rIn:
                        objects.Sort((obj1, obj2) => -Vector2.Distance(center.position, obj1.transform.position).CompareTo(Vector2.Distance(center.position, obj2.transform.position))); break;

                    case RadialMode.xPos:
                        objects = objects.OrderByDescending(obj => -obj.transform.position.x).ToList(); break;

                    case RadialMode.xNeg:
                        objects = objects.OrderByDescending(obj => obj.transform.position.x).ToList(); break;

                    case RadialMode.yPos:
                        objects = objects.OrderByDescending(obj => -obj.transform.position.y).ToList(); break;

                    case RadialMode.yNeg:
                        objects = objects.OrderByDescending(obj => obj.transform.position.y).ToList(); break;
                }
                
            }
        }
    }
    
    Color reFilter(Color color)
    {
        float h = 0, s = 0, v = 0, a = color.a;
        Color.RGBToHSV(color, out h, out s, out v);

        h += (hue / 360);
        s += sat;
        v += val;
        a += alpha;

        if (h > 1) { h -= 1; }
        else if (h < 0) { h += 1; }

        if (s > 1) { s = 1; }
        else if (s < 0) { s = 0; }

        if (v > 1) { v = 1; }
        else if (v < 0) { v = 0; }

        if (a > 1) { a = 1; }
        else if (a < 0) { a = 0; }

        color = Color.HSVToRGB(h, s, v);
        color.a = a;

        if (invert)
        {
            color = new Color(1 - color.r, 1 - color.g, 1 - color.b, a);
        }
        if (filter)
        {
            color = new Color(filterRGB.x * color.r, filterRGB.y * color.g, filterRGB.z * color.b, a);
        }

        return color;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && !inuse && !collision.isTrigger && (Time.time - refreshTime >= refresh) && !delaying)
        {
            if (oneuse) { inuse = true; }
            refreshTime = Time.time;

            if (delay > 0)
            {
                delaying = true;
                //StopAllCoroutines();
                //StartCoroutine(DelayEnter());
                Invoke("Enter", delay);
            }
            else
            {
                Enter();
            }
        }
    }

    public IEnumerator DelayEnter()
    {
        yield return new WaitForSeconds(delay);
        Enter();
        delaying = false;
    }

    Gradient randomizeGradient(Gradient gr)
    {
        GradientColorKey[] gck = gr.colorKeys;
        for (int i = 0; i < gck.Length; i++)
        {
            gck[i].color = Random.ColorHSV(randomizeRange.x, randomizeRange.y, randomizeRange.z, 1, randomizeRange.w, 1);
            gck[i].color = reFilter(gck[i].color);
        }
        gr.SetKeys(gck, gr.alphaKeys);

        return gr;
    }

    Gradient shiftGradient(Gradient gr)
    {
        GradientColorKey[] gck = gr.colorKeys;
        for (int i = 0; i < gck.Length; i++)
        {
            float h = 0, s = 0, v = 0;
            Color.RGBToHSV(gck[i].color, out h, out s, out v);

            h += hueshift;
            if (h > 1) { h -= 1; }
            else if (h < 0) { h += 1; }

            gck[i].color = Color.HSVToRGB(h, s, v);

            gck[i].color = reFilter(gck[i].color);
        }

        GradientAlphaKey[] gak = gr.alphaKeys;
        for (int i = 0; i < gak.Length; i++)
        {
            float a = gak[i].alpha;
            a += alpha;

            if (a > 1) { a = 1; }
            else if (a < 0) { a = 0; }

            gak[i].alpha = a;
        }
        gr.SetKeys(gck, gak);

        return gr;
    }

    public void EditorPulse()
    {
        /*if(colormaster == null)
            colormaster = GameObject.FindGameObjectWithTag("Master").GetComponent<ColorMaster>();

        Enter();*/
        Debug.Log("This button doesn't do anything right now. It's supposed to play the pulse in edit mode");
    }

    public void Enter()
    {
        delaying = false;

        if (alterIterMode == 1)
        {
            if (randomize)
            {
                new_color = Random.ColorHSV(randomizeRange.x, randomizeRange.y, randomizeRange.z, 1, randomizeRange.w, 1);
                new_color = reFilter(new_color);

                if (useGradient) { newColorGradient = randomizeGradient(newColorGradient); }
            }
            if (hueshift != 0)
            {
                float h = 0, s = 0, v = 0, a = new_color.a;
                Color.RGBToHSV(new_color, out h, out s, out v);

                h += hueshift;
                if(h > 1) { h -= 1; }
                else if (h < 0) { h += 1; }

                new_color = Color.HSVToRGB(h, s, v);
                new_color.a = a;

                new_color = reFilter(new_color);

                if(useGradient){ newColorGradient = shiftGradient(newColorGradient); }
            }
        }

        if (channelmode)
        {
            if (refCurrent)
            {
                //setNewColorAsCurrentColor(channel.channelcolor);
                new_color = channel.channelcolor;
                new_color = reFilter(new_color);
            }

            colormaster.addColorChannel(channel, cancelActivePulse);

            Gradient passGradient = new Gradient();
            passGradient.SetKeys(newColorGradient.colorKeys, newColorGradient.alphaKeys);
            if (!usecurve)
            {
                StartCoroutine(colormaster.Pulse(GetHashCode() + (timesActivated + ""), channel, new_color, fadein, hold, duration, useGradient ? passGradient : null));
            }
            else
            {
                StartCoroutine(colormaster.PulseCurve(GetHashCode() + (timesActivated + ""), channel, new_color, duration, curve, useGradient ? passGradient : null));
            }
            
        }
        else
        {
            if (!radial)
            {
                foreach (GameObject obj in objects)
                {
                    if (obj == null) { continue; }
                    if(alterIterMode == 2)
                    {
                        if (randomize)
                        {
                            new_color = Random.ColorHSV(randomizeRange.x, randomizeRange.y, randomizeRange.z, 1, randomizeRange.w, 1); new_color = reFilter(new_color);
                            if (useGradient) { newColorGradient = shiftGradient(newColorGradient); }
                        }
                        if (hueshift != 0)
                        {
                            float h = 0, s = 0, v = 0, a = new_color.a;
                            Color.RGBToHSV(new_color, out h, out s, out v);

                            h += hueshift;
                            if (h > 1) { h -= 1; }
                            else if (h < 0) { h += 1; }

                            new_color = Color.HSVToRGB(h, s, v);
                            new_color.a = a;

                            new_color = reFilter(new_color);

                            if (useGradient) { newColorGradient = shiftGradient(newColorGradient); }
                        }
                    }
                    if (refCurrent)
                    {
                        Color currentColor = new_color;

                        if (obj.GetComponent<SpriteRenderer>() != null/* && (assignerType == AssignerType.Default || assignerType == AssignerType.Sprite)*/)
                        {
                            SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
                            currentColor = renderer.color;
                        }
                        else if (obj.GetComponent<Tilemap>() != null/*&& (assignerType == AssignerType.Default || assignerType == AssignerType.Tilemap)*/)
                        {
                            Tilemap renderer = obj.GetComponent<Tilemap>();
                            currentColor = renderer.color;
                        }
                        else if (obj.GetComponent<Light2D>() != null/* && (assignerType == AssignerType.Default || assignerType == AssignerType.Light)*/)
                        {
                            Light2D renderer = obj.GetComponent<Light2D>();
                            currentColor = renderer.color;
                        }
                        else if (obj.GetComponent<Graphic>() != null/* && (assignerType == AssignerType.Default || assignerType == AssignerType.UI)*/)
                        {
                            Graphic renderer = obj.GetComponent<Graphic>();
                            currentColor = renderer.color;
                        }

                        //setNewColorAsCurrentColor(currentColor);
                        new_color = currentColor;
                        new_color = reFilter(new_color);
                    }

                    colormaster.addGameObject(obj, cancelActivePulse);

                    Gradient passGradient = new Gradient();
                    passGradient.SetKeys(newColorGradient.colorKeys, newColorGradient.alphaKeys);
                    if (!usecurve)
                    {
                        StartCoroutine(colormaster.PulseObj(obj.GetHashCode() + (GetHashCode() + "") + (timesActivated + ""), obj, new_color, fadein, hold, duration, useGradient ? passGradient : null));
                    }
                    else
                    {
                        StartCoroutine(colormaster.PulseObjCurve(obj.GetHashCode() + (GetHashCode() + "") + (timesActivated + ""), obj, new_color, duration, curve, useGradient ? passGradient : null));
                    }
                }
            }
            else
            {
                StartCoroutine(RadialPulse());
            }
        }

        timesActivated++;        
    }

    IEnumerator RadialPulse()
    {
        if (recalcCenter)
        {
            //objects.Sort((obj1, obj2) => Vector2.Distance(center.position, obj1.transform.position).CompareTo(Vector2.Distance(center.position, obj2.transform.position)));
            switch (radialmode)
            {
                case RadialMode.rOut:
                    objects.Sort((obj1, obj2) => Vector2.Distance(center.position, obj1.transform.position).CompareTo(Vector2.Distance(center.position, obj2.transform.position))); break;

                case RadialMode.rIn:
                    objects.Sort((obj1, obj2) => -Vector2.Distance(center.position, obj1.transform.position).CompareTo(Vector2.Distance(center.position, obj2.transform.position))); break;

                case RadialMode.xPos:
                    objects = objects.OrderByDescending(obj => -obj.transform.position.x).ToList(); break;

                case RadialMode.xNeg:
                    objects = objects.OrderByDescending(obj => obj.transform.position.x).ToList(); break;

                case RadialMode.yPos:
                    objects = objects.OrderByDescending(obj => -obj.transform.position.y).ToList(); break;

                case RadialMode.yNeg:
                    objects = objects.OrderByDescending(obj => obj.transform.position.y).ToList(); break;
            }
        }

        List<GameObject> tempObjects = objects.ToList();

        Vector2 centerPosition = center.position;
        float currentRange = Vector2.Distance(tempObjects[0].transform.position, centerPosition);
        Color tempNewColor = new_color;
        Gradient tempGradient = new Gradient();
        tempGradient.SetKeys(newColorGradient.colorKeys, newColorGradient.alphaKeys);
        float startPos = 0;

        bool increaseRange = true;
        switch (radialmode)
        {
            case RadialMode.rOut:
                increaseRange = true; break;

            case RadialMode.rIn:
                increaseRange = false; break;

            case RadialMode.xPos:
                increaseRange = true; currentRange = tempObjects[0].transform.position.x; break;

            case RadialMode.xNeg:
                increaseRange = false; currentRange = tempObjects[0].transform.position.x; break;

            case RadialMode.yPos:
                increaseRange = true; currentRange = tempObjects[0].transform.position.y; break;

            case RadialMode.yNeg:
                increaseRange = false; currentRange = tempObjects[0].transform.position.y; break;
        }

        for (int i = 0; i < tempObjects.Count; i++)
        {
            GradientColorKey[] gck = tempGradient.colorKeys;
            GradientAlphaKey[] gak = tempGradient.alphaKeys;
            tempGradient = new Gradient();
            tempGradient.SetKeys(gck, gak);
            GameObject obj = tempObjects[i];

            bool boolToCheck = false;

            switch (radialmode)
            {
                case RadialMode.rOut:
                    boolToCheck = Vector2.Distance(obj.transform.position, centerPosition) <= currentRange; break;

                case RadialMode.rIn:
                    boolToCheck = Vector2.Distance(obj.transform.position, centerPosition) >= currentRange; break;

                case RadialMode.xPos:
                    boolToCheck = obj.transform.position.x <= currentRange; break;

                case RadialMode.xNeg:
                    boolToCheck = obj.transform.position.x >= currentRange; break;

                case RadialMode.yPos:
                    boolToCheck = obj.transform.position.y <= currentRange; break;

                case RadialMode.yNeg:
                    boolToCheck = obj.transform.position.y >= currentRange; break;
            }

            if (boolToCheck)//Vector2.Distance(obj.transform.position, center.position) <= currentRange)
            {
                if (alterIterMode == 2)
                {
                    if (randomize)
                    {
                        tempNewColor = Random.ColorHSV(randomizeRange.x, randomizeRange.y, randomizeRange.z, 1, randomizeRange.w, 1); tempNewColor = reFilter(tempNewColor);
                        if (useGradient) { tempGradient = randomizeGradient(tempGradient); }
                    }
                    if (hueshift != 0)
                    {
                        float h = 0, s = 0, v = 0, a = tempNewColor.a;
                        Color.RGBToHSV(tempNewColor, out h, out s, out v);

                        h += hueshift;
                        if (h > 1) { h -= 1; }
                        else if (h < 0) { h += 1; }

                        tempNewColor = Color.HSVToRGB(h, s, v);
                        tempNewColor.a = a;

                        tempNewColor = reFilter(tempNewColor);
                        if (useGradient) { tempGradient = shiftGradient(tempGradient); }
                    }
                }
                if (obj != null && refCurrent)
                {
                    Color currentColor = tempNewColor;

                    if (obj.GetComponent<SpriteRenderer>() != null/* && (assignerType == AssignerType.Default || assignerType == AssignerType.Sprite)*/)
                    {
                        SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
                        currentColor = renderer.color;
                    }
                    else if (obj.GetComponent<Tilemap>() != null/*&& (assignerType == AssignerType.Default || assignerType == AssignerType.Tilemap)*/)
                    {
                        Tilemap renderer = obj.GetComponent<Tilemap>();
                        currentColor = renderer.color;
                    }
                    else if (obj.GetComponent<Light2D>() != null/* && (assignerType == AssignerType.Default || assignerType == AssignerType.Light)*/)
                    {
                        Light2D renderer = obj.GetComponent<Light2D>();
                        currentColor = renderer.color;
                    }
                    else if (obj.GetComponent<Graphic>() != null/* && (assignerType == AssignerType.Default || assignerType == AssignerType.UI)*/)
                    {
                        Graphic renderer = obj.GetComponent<Graphic>();
                        currentColor = renderer.color;
                    }

                    //setNewColorAsCurrentColor(currentColor);
                    tempNewColor = currentColor;
                    tempNewColor = reFilter(tempNewColor);
                }

                colormaster.addGameObject(obj, cancelActivePulse);

                Gradient passGradient = new Gradient();
                passGradient.SetKeys(tempGradient.colorKeys, tempGradient.alphaKeys);
                if (!usecurve)
                {
                    StartCoroutine(colormaster.PulseObj(obj.GetHashCode() + (GetHashCode() + "") + (timesActivated + ""), obj, tempNewColor, fadein, hold, duration, useGradient ? passGradient : null));
                }
                else
                {
                    StartCoroutine(colormaster.PulseObjCurve(obj.GetHashCode() + (GetHashCode() + "") + (timesActivated + ""), obj, tempNewColor, duration, curve, useGradient ? passGradient : null));
                }
            }
            else
            {
                timesActivated++;
                i--;
                currentRange += radialSpeed * Time.deltaTime * (increaseRange ? 1 : -1);

                yield return null;
            }

            if(i == 0 || (alterIterMode == 2 && hueshift != 0)) { new_color = tempNewColor; if (useGradient) { newColorGradient = tempGradient; } }
        }

        yield break;
    }

    public void SpawnActivate()
    {
        if (!inuse)
        {
            if (delay > 0)
            {
                delaying = true;
                //StopAllCoroutines();
                //StartCoroutine(DelayEnter());
                Invoke("Enter", delay);
            }
            else
            {
                Enter();
            }
            //Enter();
        }
        if (oneuse) { inuse = true; }
    }

    private void OnDrawGizmos()
    {
        if (durationline)
        {
            float scale = 0;
            switch (speed)
            {
                case Speed.x0:
                    scale = 40f; break;
                case Speed.x1:
                    scale = 55f; break;
                case Speed.x2:
                    scale = 75f; break;
                case Speed.x3:
                    scale = 90f; break;
                case Speed.x4:
                    scale = 110f; break;
            }

            Gizmos.color = new Color(1f, .5f, .5f);
            Gizmos.DrawLine(transform.position, transform.position + new Vector3((scale * Time.fixedDeltaTime * 10f) * fadein, 0, 0));

            Gizmos.color = new Color(.5f, 1f, .5f);
            Gizmos.DrawLine(transform.position + new Vector3((scale * Time.fixedDeltaTime * 10f) * fadein, 0, 0), transform.position + new Vector3((scale * Time.fixedDeltaTime * 10f) * (fadein + hold), 0, 0));

            Gizmos.color = new Color(.5f, .5f, 1f);
            Gizmos.DrawLine(transform.position + new Vector3((scale * Time.fixedDeltaTime * 10f) * (fadein + hold), 0, 0), transform.position + new Vector3((scale * Time.fixedDeltaTime * 10f) * (fadein + hold + duration), 0, 0));
        }

        if (radial && !channelmode)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}