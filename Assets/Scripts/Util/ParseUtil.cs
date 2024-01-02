using System.Globalization;
using UnityEngine;

public static class ParseUtil {
    public static bool TestInt(string s) {
        int i;
        return int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out i);
    }

    public static int GetInt(string s) {
        int i;
        if (int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out i))  return i;
        throw new CYFException("Int parse failed : \"" + s + "\"");
    }

    public static float GetFloat(string s) {
        float f;
        if (float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out f))  return f;
        throw new CYFException("Float parse failed : \"" + s + "\"");
    }

    public static float GetByte(string s) {
        byte f;
        if (byte.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out f))  return f;
        throw new CYFException("Byte parse failed : \"" + s + "\"");
    }

    public static Color GetColor(string s) {
        uint intColor;
        if (!uint.TryParse(s, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out intColor)) throw new CYFException("Float parse failed : \"" + s + "\"");
        float r = ((intColor >> 16) & 255) / 255.0f;
        float g = ((intColor >> 8)  & 255) / 255.0f;
        float b = (intColor         & 255) / 255.0f;
        return new Color(r, g, b);
    }

    public static string GetBytesFromColor(Color c, bool allowAlpha = false) {
        ulong intColor = ((ulong)Mathf.RoundToInt(c.r * 255) << 16) + ((ulong)Mathf.RoundToInt(c.g * 255) << 8) + (ulong)Mathf.RoundToInt(c.b * 255);
        if (allowAlpha)
            intColor = (intColor << 8) + (ulong)Mathf.RoundToInt(c.a * 255);
        return intColor.ToString("X" + (allowAlpha ? 8 : 6));
    }
}