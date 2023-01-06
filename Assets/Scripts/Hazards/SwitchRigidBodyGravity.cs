using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchRigidBodyGravity : MonoBehaviour
{
    public float newGravity;
    public GameObject eyes, scale;

    private Rigidbody2D body;
    private bool inuse;
    
    void Start()
    {
        body = gameObject.GetComponent<Rigidbody2D>();
    }

    public IEnumerator Move()
    {
        body.gravityScale = newGravity;
        float oldRotation = body.rotation;
        float newRotation = body.rotation + 180;

        float time = 0;
        while (time < .5f)
        {
            body.rotation = Mathf.Lerp(oldRotation, newRotation, time / .5f);

            time += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        body.rotation = newRotation;

        if(gameObject.GetComponent<MoveToPositions>() != null)
        {
            gameObject.GetComponent<MoveToPositions>().enabled = true;
        }
    }

    private void Update()
    {
        eyes.transform.localPosition = new Vector3(body.velocity.x / 50, (scale.transform.localScale.y - 1) / 2, 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player" && inuse == false)
        {
            inuse = true;
            StartCoroutine(Move());
        }
    }
}
