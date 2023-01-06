using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    private float length, startpos, ypos, cam_y, cam_y0;
    public GameObject cam;
    public float parallexEffect, speedX, altLength, parallexY;
    private float distSpeed;
    public bool center, multiplyByScale;
    private Quaternion startRotation;

    void Start()
    {
        startpos = center ? 0 : transform.position.x;
        ypos = transform.position.y;
        startRotation = transform.rotation;
        cam_y = cam.transform.position.y;
        cam_y0 = cam.transform.position.y;
        length = GetComponent<SpriteRenderer>().bounds.size.x;//(multiplyByScale ? transform.localScale.x : 1)
        if(altLength != 0) { length = altLength; }
        //Debug.Log(length);
    }
    void Update()
    {
        cam_y = cam.transform.position.y;

        float temp = (cam.transform.position.x * (1 - parallexEffect));
        float dist_x = (cam.transform.position.x * parallexEffect);

        transform.rotation = startRotation;
        transform.position = new Vector3(startpos + dist_x + distSpeed, ypos + ((cam_y - cam_y0) * parallexY)/*transform.position.y - (cam_y - cam_y0)*/, transform.position.z);
        //transform.position = Vector3.MoveTowards(transform.position, new Vector3(startpos + dist_x, ypos + ((cam_y - cam_y0) * parallexY), transform.position.z), 10000 * Time.deltaTime);

        /*
        float cam_r = cam.transform.rotation.eulerAngles.z;
        if(cam_r > 180) { cam_r = cam_r - 360; }

        transform.rotation = Quaternion.Euler(new Vector3(0, 0, -cam_r/2));
        Debug.Log("cam rotation: " + cam.transform.rotation.eulerAngles.z);*/

        //if (center) { return; }
        if (temp >= startpos + length)
        {
            startpos += /*length*/temp - startpos;
        }
        else if (temp <= startpos - length)
        {
            startpos -= /*length*/startpos - temp;
        }

        distSpeed += (speedX * Time.deltaTime);
        if (distSpeed >= length) distSpeed = 0;
        else if (distSpeed <= -length) distSpeed = 0;
        //Debug.Log(distSpeed + "   " + length);
    }//*/
}

