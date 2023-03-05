using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorExtension
{
    public static Vector2 Rotate(this Vector2 v, float degrees)
    {
        Vector2 newVector = new Vector2(v.x, v.y);
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = newVector.x;
        float ty = newVector.y;
        newVector.x = (cos * tx) - (sin * ty);
        newVector.y = (sin * tx) + (cos * ty);
        return newVector;
    }

    public static Vector2 RotateRange(this Vector2 v, float degrees)
    {
        float randDegrees = Random.Range(-degrees, degrees);
        Vector2 newVector = v.Rotate(randDegrees);

        return newVector;
    }

    public static Vector2 RotateRangeExclude(this Vector2 v, float degrees, float exclude)
    {
        int rand = Random.Range(0, 2);
        float randDegrees = rand == 0 ? Random.Range(exclude, degrees) : Random.Range(-exclude, -degrees);
        Vector2 newVector = v.Rotate(randDegrees);

        return newVector;
    }

    public static float InverseLerp(Vector4 a, Vector4 b, Vector4 value)
    {
        Vector4 AB = b - a;
        Vector4 AV = value - a;
        return Vector4.Dot(AV, AB) / Vector4.Dot(AB, AB);
    }

    public static Vector4 remap(Vector4 origFrom, Vector4 origTo, Vector4 targetFrom, Vector4 targetTo, Vector4 value)
    {
        float rel = VectorExtension.InverseLerp(origFrom, origTo, value);
        return Vector4.Lerp(targetFrom, targetTo, rel);
    }

    public static Vector4 ColorAsVector4(this Color color)
    {
        return new Vector4(color.r, color.g, color.b, color.a);
    }

    public static Color Vector4AsColor(this Vector4 vector)
    {
        return new Color(vector.x, vector.y, vector.z, vector.w);
    }

    public static Vector3 SetXY(this Vector3 vector, Vector2 XY)
    {
        vector.x = XY.x;
        vector.y = XY.y;

        return vector;
    }
    public static Vector3 AddX(this Vector3 vector, float X)
    {
        vector.x += X;
        return vector;
    }
    public static Vector3 AddY(this Vector3 vector, float Y)
    {
        vector.y += Y;
        return vector;
    }
    public static Vector3 AddZ(this Vector3 vector, float Z)
    {
        vector.z += Z;
        return vector;
    }
    public static Vector2 AddX(this Vector2 vector, float X)
    {
        vector.x += X;
        return vector;
    }
    public static Vector2 AddY(this Vector2 vector, float Y)
    {
        vector.y += Y;
        return vector;
    }
    public static Vector3 AddZ(this Vector2 vector, float Z)
    {
        Vector3 vec = (Vector3)vector;
        vec.z += Z;
        return vec;
    }

    public static Vector3 SetX(this Vector3 vector, float X)
    {
        vector.x = X;
        return vector;
    }
    public static Vector3 SetY(this Vector3 vector, float Y)
    {
        vector.y = Y;
        return vector;
    }
    public static Vector3 SetZ(this Vector3 vector, float Z)
    {
        vector.z = Z;
        return vector;
    }
    public static Vector2 SetX(this Vector2 vector, float X)
    {
        vector.x = X;
        return vector;
    }
    public static Vector2 SetY(this Vector2 vector, float Y)
    {
        vector.y = Y;
        return vector;
    }
    public static Vector3 SetZ(this Vector2 vector, float Z)
    {
        Vector3 vec = (Vector3)vector;
        vec.z = Z;
        return vec;
    }

    public static Vector2 Invert(this Vector2 vector)
    {
        return new Vector2(1 / vector.x, 1 / vector.y);
    }

    public static Vector3 Invert(this Vector3 vector)
    {
        return new Vector3(1 / vector.x, 1 / vector.y, 1 / vector.z);
    }

    public static Vector2 Divide(Vector2 vector1, Vector2 vector2)
    {
        return Vector2.Scale(vector1, vector2.Invert());
    }

    public static Vector3 Divide(Vector3 vector1, Vector3 vector2)
    {
        return Vector3.Scale(vector1, vector2.Invert());
    }

    public static Vector2 SetNaNToOne(this Vector2 vector)
    {
        return new Vector2(float.IsNaN(vector.x) ? 1 : vector.x, float.IsNaN(vector.y) ? 1 : vector.y);
    }

    // COLOR
    public static Color GetHueColor(this Color color)
    {
        float Saturation = color.Saturation();
        if (Saturation == 0)
            return Color.red;
        float Hue = color.Hue();
        return Color.red.SetHue(Hue);
    }
    public static Color Sepia(this Color color)
    {
        color.r = (color.r * .393f) + (color.g * .769f) + (color.b * .189f);
        color.g = (color.r * .349f) + (color.g * .686f) + (color.b * .168f);
        color.b = (color.r * .272f) + (color.g * .534f) + (color.b * .131f);
        return color;
    }
    public static Color Invert(this Color color)
    {
        color = new Color(1 - color.r, 1 - color.g, 1 - color.b, color.a);
        return color;
    }
    public static Color GrayScale(this Color color)
    {
        color = new Color(color.grayscale, color.grayscale, color.grayscale, color.a);
        return color;
    }
    public static Color SetAlpha(this Color color, float Alpha)
    {
        color.a = Alpha;
        return color;
    }
    public static Color SetRed(this Color color, float R)
    {
        color.r = R;
        return color;
    }
    public static Color SetBlue(this Color color, float G)
    {
        color.b = G;
        return color;
    }
    public static Color SetGreen(this Color color, float B)
    {
        color.g = B;
        return color;
    }
    public static Color MultiplyAlpha(this Color A, Color B)
    {
        return A.SetAlpha(A.a * B.a);
    }
    public static bool RGBMatches(this Color A, Color B)
    {
        return A.r == B.r && A.g == B.g && A.b == B.b;
    }
    public static Color SetHue(this Color color, float Hue)
    {
        Hue = Mathf.Repeat(Hue, 1);
        return Color.HSVToRGB(Hue, color.Saturation(), color.Brightness()).SetAlpha(color.a);
    }
    public static Color SetSaturation(this Color color, float saturation)
    {
        saturation = Mathf.Clamp01(saturation);
        return Color.HSVToRGB(color.Hue(), saturation, color.Brightness()).SetAlpha(color.a);
    }
    public static Color SetBrightness(this Color color, float brightness)
    {
        brightness = Mathf.Clamp01(brightness);
        return Color.HSVToRGB(color.Hue(), color.Saturation(), brightness).SetAlpha(color.a);
    }
    public static float Hue(this Color color)
    {
        Color.RGBToHSV(color, out float H, out _, out _);
        return H;
    }
    public static float Saturation(this Color color)
    {
        Color.RGBToHSV(color, out _, out float H, out _);
        return H;
    }
    public static float Brightness(this Color color)
    {
        Color.RGBToHSV(color, out _, out _, out float H);
        return H;
    }
}