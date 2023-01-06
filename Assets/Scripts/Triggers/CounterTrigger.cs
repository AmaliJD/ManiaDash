using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CounterTrigger : MonoBehaviour
{
    public float number;
    public NumberText numberText;

    public enum Mode
    {
        Add, Multiply, Set, AddTime, MultiplyTime, LerpTime
    }
    public Mode mode;

    [Tooltip("Lerp Time: time = 1")]
    public float start, value, end;

    [Min(0)]
    public float time;

    [Min(0)]
    public float delay;
    public bool oneuse;
    private bool inuse;

    private void Awake()
    {
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator Activate()
    {
        if(delay > 0) { yield return new WaitForSeconds(delay); }

        if(numberText != null)
        {
            number = numberText.Integer ? numberText.i : numberText.f;
        }

        switch(mode)
        {
            case Mode.Add:
                number += value;
                break;

            case Mode.Multiply:
                number *= value;
                break;

            case Mode.Set:
                number = value;
                break;

            case Mode.AddTime:
                while(Mathf.Sign(value) == 1 ? number < end : number > end)
                {
                    number += value;
                    setNumberText();
                    yield return time > .05f ? new WaitForSeconds(time) : null;
                }
                number = end;
                break;

            case Mode.MultiplyTime:
                while (value >= 1 ? number < end : number > end)
                {
                    number *= value;
                    setNumberText();
                    yield return time > .05f ? new WaitForSeconds(time) : null;
                }
                number = end;
                break;

            case Mode.LerpTime:
                if(time == 0) { time = 1; }

                number = start;
                setNumberText();
                yield return null;

                while (end > start ? number < end : number > end)
                {
                    if(end > start)
                    {
                        number += Time.deltaTime * time;
                    }
                    else
                    {
                        number -= Time.deltaTime * time;
                    }
                    setNumberText();
                    yield return null;
                }
                number = end;
                break;
        }

        setNumberText();

        inuse = oneuse;
    }

    public void setNumberText()
    {
        if(numberText != null)
        {
            if(numberText.Integer)
            {
                numberText.i = Mathf.RoundToInt(number);
            }
            else
            {
                numberText.f = number;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && !inuse)
        {
            inuse = true;
            StartCoroutine(Activate());
        }
    }

}
