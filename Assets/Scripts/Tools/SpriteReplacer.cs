using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SpriteReplacer : MonoBehaviour
{
    public Sprite target, replacement;
    private SpriteRenderer[] spriteRenderers;

    public void replaceSprite()
    {
        spriteRenderers = FindObjectsOfType<SpriteRenderer>();

        foreach(SpriteRenderer sr in spriteRenderers)
        {
            if(sr.sprite == target)
            {
                sr.sprite = replacement;
            }
        }
    }
}
