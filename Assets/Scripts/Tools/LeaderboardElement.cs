using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardElement : MonoBehaviour
{
    private void OnBecameInvisible()
    {
        if (!transform.parent.gameObject.activeSelf) return;
        foreach(Transform t in transform)
        {
            t.gameObject.SetActive(false);
        }
    }

    private void OnBecameVisible()
    {
        foreach (Transform t in transform)
        {
            t.gameObject.SetActive(true);
        }
    }
}
