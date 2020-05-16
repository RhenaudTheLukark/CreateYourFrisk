using System;
using System.Globalization;
using UnityEngine;

public static class ParseUtil {
    public static bool TestInt(string s) {
        int i;
        if (int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out i))  return true;
        else                                                                         return false;
    }

    public static int GetInt(string s) {
        int i;
        if (int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out i))  return i;
        else                                                                         throw new CYFException("Int parse failed : \"" + s + "\"");
    }

    public static float GetFloat(string s) {
        float f;
        if (float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out f))  return f;
        else                                                                           throw new CYFException("Float parse failed : \"" + s + "\"");
    }

    public static float GetByte(string s) {
        byte f;
        if (byte.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out f))  return f;
        else                                                                                throw new CYFException("Byte parse failed : \"" + s + "\"");
    }

    public static Color GetColor(string s) {
        uint intColor;
        if (uint.TryParse(s, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out intColor)) {
            float r = ((intColor >> 16) & 255) / 255.0f;
            float g = ((intColor >> 8) & 255) / 255.0f;
            float b = (intColor & 255) / 255.0f;
            return new Color(r, g, b);
        } else
            throw new CYFException("Float parse failed : \"" + s + "\"");
    }
}