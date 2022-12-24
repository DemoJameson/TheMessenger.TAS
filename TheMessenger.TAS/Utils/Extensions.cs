using System;
using TheMessenger.TAS.Components;
using TheMessenger.TAS.Components.Debugs;
using UnityEngine;

namespace TheMessenger.TAS.Utils;

internal static class VectorExtensions {
    public static string ToSimpleString(this Vector2 vector2, int decimals = 2) {
        return $"{vector2.x.ToFormattedString(decimals)}, {vector2.y.ToFormattedString(decimals)}";
    }

    public static string ToSimpleString(this Vector3 vector3, int decimals = 2) {
        return $"{vector3.x.ToFormattedString(decimals)}, {vector3.y.ToFormattedString(decimals)}";
    }
}

internal static class NumberExtensions {
    public static string ToFormattedString(this float value, int decimals = 2) {
        if (decimals <= 0 || DumpInfo.Dump.Value) {
            return value.ToString();
        } else {
            return value.ToString($"F{decimals}");
        }
    }

    public static string ToFormattedString(this double value, int decimals = 2) {
        if (decimals <= 0 || DumpInfo.Dump.Value) {
            return value.ToString();
        } else {
            return value.ToString($"F{decimals}");
        }
    }

    public static int ToCeilingFrames(this float seconds) {
        return (int) Math.Ceiling(seconds * GameComponent.FixedFrameRate);
    }

    public static int ToFloorFrames(this float seconds) {
        return (int) Math.Floor(seconds * GameComponent.FixedFrameRate);
    }
}