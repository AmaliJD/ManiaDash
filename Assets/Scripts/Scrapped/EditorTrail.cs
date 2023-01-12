using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class EditorTrail : MonoBehaviour
{
    bool playing = false;
    List<Vector2> positions;
    public Material material;

    private void Awake()
    {
        positions = new List<Vector2>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Application.isPlaying)
        {
            if(!playing)
            {
                playing = true;
                ClearPosition();
            }
            AddPosition(transform.position);
            Draw();
        }
        else
        {
            playing = false;
            Draw();
        }
    }

    void AddPosition(Vector2 vector)
    {
        positions.Add(vector);
    }

    void ClearPosition()
    {
        positions = new List<Vector2>();
    }

    private void Draw()
    {
        if(positions == null) { return; }

        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);
        material.SetPass(0);
        GL.Begin(GL.LINES);
        GL.Color(Color.green);

        foreach(Vector2 v in positions)
        {
            GL.Vertex(v);
        }

        GL.End();
    }
}
