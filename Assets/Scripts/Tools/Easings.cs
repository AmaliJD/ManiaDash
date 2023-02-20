using UnityEngine;

public static class Easings
{
    public enum Ease
    {
        Linear,
        QuadInOut, QuadIn, QuadOut,
        CubeInOut, CubeIn, CubeOut,
        QuartInOut, QuartIn, QuartOut,
        QuintInOut, QuintIn, QuintOut,
        ElasInOut, ElasIn, ElasOut,
        ExpoInOut, ExpoIn, ExpoOut,
        SinInOut, SinIn, SinOut,
        BackInOut, BackIn, BackOut,
        BounceInOut, BounceIn, BounceOut,
        Custom,
    }

    private const float NATURAL_LOG_OF_2 = 0.693147181f;

    //
    // Easing functions
    //

    public static float Linear(float t)
    {
        return t;
    }

    public static float QuadInOut(float t)
    {
        t = Mathf.Clamp01(t);
        return t < 0.5 ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;
    }

    public static float QuadIn(float t)
    {
        t = Mathf.Clamp01(t);
        return t * t;
    }

    public delegate float ValueFunction(float t);
    public static ValueFunction GetEasingValue(Ease easingFunction)
    {
        switch(easingFunction)
        {
            case Ease.Linear:
                return Linear;

            case Ease.QuadInOut:
                return QuadInOut;

            case Ease.QuadIn:
                return QuadIn;
        }

        return null;
    }
}