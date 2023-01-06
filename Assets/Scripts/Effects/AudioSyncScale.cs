using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSyncScale : AudioSyncer
{
    public Vector2 beatScale, restScale;

    private IEnumerator MoveToScale(Vector2 target)
    {
        Vector2 curr = transform.localScale;
        Vector2 initial = curr;
        float timer = 0;

        while(curr != target)
        {
            curr = Vector2.Lerp(initial, target, timer / timeToBeat);
            timer += Time.deltaTime;

            transform.localScale = curr;

            yield return null;
        }

        isBeat = false;
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (isBeat) return;

        transform.localScale = Vector2.Lerp(transform.localScale, restScale, restSmoothTime * Time.deltaTime);
    }

    public override void OnBeat()
    {
        base.OnBeat();

        StopCoroutine("MoveToScale");
        StartCoroutine("MoveToScale", beatScale);
    }
}
