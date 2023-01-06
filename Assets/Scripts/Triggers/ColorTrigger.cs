using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.UI;
using System.Linq;

public class ColorTrigger : MonoBehaviour
{
    public bool channelmode = true, copy = false;

    [Min(0)]
    public int groupID;

    public ColorReference channel;
    public List<GameObject> objects;
    public ColorReference copy_color;

    public Color new_color;

    public bool refCurrent, invert, filter;
    public Vector3 filterRGB;

    //public bool mapToPalette;
    //public Gradient[] gradientMap;

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

    [Range(-360f, 360f)] public float hue;
    [Range(-1f, 1f)] public float sat;
    [Range(-1f, 1f)] public float val;
    [Range(-1f, 1f)] public float alpha;

    private List<Color> curr_color;

    [Min(0f)]
    public float delay;
    private bool waitdelay;

    [Min(0f)]
    public float duration;
    public bool oneuse = false;
    private bool inuse = false;

    public bool keepcolor = false;

    private ColorMaster colormaster;
    public bool cancelActivePulse;

    private GroupIDManager groupIDManager;

    public enum Speed
    {
        x0, x1, x2, x3, x4
    }
    public Speed speed = Speed.x1;
    public bool durationline;

    void Awake()
    {
        //colormaster = FindObjectOfType<ColorMaster>();
        colormaster = GameObject.FindGameObjectWithTag("Master").GetComponent<ColorMaster>();
        gameObject.transform.GetChild(0).gameObject.SetActive(false);

        if(delay != 0)
        {
            waitdelay = true;
        }

        if(new_color == null) { new_color = Color.white; }
    }

    private void Start()
    {
        if (copy && copy_color != null)
        {
            new_color = copy_color.channelcolor;
        }

        if (randomize) { new_color = Random.ColorHSV(randomizeRange.x, randomizeRange.y, randomizeRange.z, 1, randomizeRange.w, 1); }

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

        if (groupID > 0)
        {
            groupIDManager = GameObject.FindGameObjectWithTag("Master").GetComponent<GroupIDManager>();
            if (!radial)
            {
                objects.AddRange(groupIDManager.groupIDList[groupID]);
            }
            else
            {
                if (center == null) { center = transform; }
                objects.AddRange(groupIDManager.groupIDList[groupID].FindAll(obj => Vector2.Distance(center.position, obj.transform.position) <= range));

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
        }
    }

    

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && !inuse && !collision.isTrigger)
        {
            if (oneuse) { inuse = true; }
            if(!waitdelay)
            {
                Enter();
            }
            else
            {
                StartCoroutine(Delay());
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

    public void Enter()
    {
        if (alterIterMode == 1)
        {
            if (randomize) { new_color = Random.ColorHSV(randomizeRange.x, randomizeRange.y, randomizeRange.z, 1, randomizeRange.w, 1); new_color = reFilter(new_color); }
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
            StartCoroutine(colormaster.ColorChange(channel, new_color, duration));
        }
        else
        {
            if (!radial)
            {
                foreach (GameObject obj in objects)
                {
                    if (obj == null) { continue; }
                    if (alterIterMode == 2)
                    {
                        if (randomize) { new_color = Random.ColorHSV(randomizeRange.x, randomizeRange.y, randomizeRange.z, 1, randomizeRange.w, 1); new_color = reFilter(new_color); }
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
                        }
                    }
                    if (refCurrent)
                    {
                        Color currentColor = new_color;

                        if (obj.GetComponent<SpriteRenderer>() != null)
                        {
                            SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
                            currentColor = renderer.color;
                        }
                        else if (obj.GetComponent<Tilemap>() != null)
                        {
                            Tilemap renderer = obj.GetComponent<Tilemap>();
                            currentColor = renderer.color;
                        }
                        else if (obj.GetComponent<Light2D>() != null)
                        {
                            Light2D renderer = obj.GetComponent<Light2D>();
                            currentColor = renderer.color;
                        }
                        else if (obj.GetComponent<Graphic>() != null)
                        {
                            Graphic renderer = obj.GetComponent<Graphic>();
                            currentColor = renderer.color;
                        }

                        //setNewColorAsCurrentColor(currentColor);
                        new_color = currentColor;
                        new_color = reFilter(new_color);
                    }

                    colormaster.addGameObject(obj, cancelActivePulse);
                    StartCoroutine(colormaster.ColorChangeObj(obj, new_color, duration));
                }
            }
            else
            {
                StartCoroutine(RadialColorChange());
            }
        }
    }

    IEnumerator RadialColorChange()
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
                    if (randomize) { tempNewColor = Random.ColorHSV(randomizeRange.x, randomizeRange.y, randomizeRange.z, 1, randomizeRange.w, 1); tempNewColor = reFilter(tempNewColor); }
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
                    }
                }
                if (obj != null && refCurrent)
                {
                    Color currentColor = tempNewColor;

                    if (obj.GetComponent<SpriteRenderer>() != null)
                    {
                        SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
                        currentColor = renderer.color;
                    }
                    else if (obj.GetComponent<Tilemap>() != null)
                    {
                        Tilemap renderer = obj.GetComponent<Tilemap>();
                        currentColor = renderer.color;
                    }
                    else if (obj.GetComponent<Light2D>() != null)
                    {
                        Light2D renderer = obj.GetComponent<Light2D>();
                        currentColor = renderer.color;
                    }
                    else if (obj.GetComponent<Graphic>() != null)
                    {
                        Graphic renderer = obj.GetComponent<Graphic>();
                        currentColor = renderer.color;
                    }

                    //setNewColorAsCurrentColor(currentColor);
                    tempNewColor = currentColor;
                    tempNewColor = reFilter(tempNewColor);
                }

                colormaster.addGameObject(obj, cancelActivePulse);
                StartCoroutine(colormaster.ColorChangeObj(obj, tempNewColor, duration));
            }
            else
            {
                i--;
                currentRange += radialSpeed * Time.deltaTime * (increaseRange ? 1 : -1);

                yield return null;
            }

            if (i == 0 || (alterIterMode == 2 && hueshift != 0)) { new_color = tempNewColor; }
        }

        yield break;
    }

    IEnumerator Delay()
    {
        yield return new WaitForSeconds(delay);
        Enter();
    }

    public void SpawnActivate()
    {
        if (!inuse)// Enter();
        {
            if (!waitdelay)
            {
                Enter();
            }
            else
            {
                StartCoroutine(Delay());
            }
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

            Vector3 delayVector = new Vector3((scale * Time.fixedDeltaTime * 10f) * delay, 0, 0);
            Gizmos.DrawLine(transform.position + delayVector, transform.position + new Vector3((scale * Time.fixedDeltaTime * 10f) * duration, 0, 0) + delayVector);
        }

        if (radial && !channelmode)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}