using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public float speedX, speedY, speedZ;
    public bool fixedAngle;
    List<Transform> children;
    List<Quaternion> initialRotations;

    private void Awake()
    {
        visible = GetComponent<SpriteRenderer>() == null;
    }

    private void Start()
    {
        children = new List<Transform>();
        foreach (Transform t in transform)
        {
            children.Add(t);
        }

        initialRotations = new List<Quaternion>();
        foreach (Transform c in children)
        {
            initialRotations.Add(c.rotation);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Rotate(Vector3.left, speedX * Time.deltaTime);
        transform.Rotate(Vector3.up, speedY * Time.deltaTime);
        transform.Rotate(Vector3.back, speedZ * Time.deltaTime);

        if(fixedAngle)
        {
            int i = 0;
            foreach (Transform c in children)
            {
                c.rotation = initialRotations[i];
                i++;
            }
        }
    }

    bool visible;

    private void OnBecameInvisible()
    {
        //transform.GetChild(0).gameObject.SetActive(false);
        enabled = false;
        //visible = false;
    }

    private void OnBecameVisible()
    {
        //transform.GetChild(0).gameObject.SetActive(true);
        enabled = true;
        //visible = true;
    }
}
