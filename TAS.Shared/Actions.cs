using System;
using System.Collections.Generic;

namespace TAS.Shared;

[Flags]
public enum Actions {
    None = 0,
    Left = 1 << 0,
    Right = 1 << 1,
    Up = 1 << 2,
    Down = 1 << 3,
    JumpAndConfirm = 1 << 4,
    RopeDartAndBack = 1 << 5,
    Attack = 1 << 6,
    Shuriken = 1 << 7,
    Tabi = 1 << 8,
    Map = 1 << 9,
    Inventory = 1 << 10,
    Pause = 1 << 11,
}

public static class ActionsUtils {
    public static readonly Dictionary<char, Actions> Chars = new() {
        {'L', Actions.Left},
        {'R', Actions.Right},
        {'U', Actions.Up},
        {'D', Actions.Down},
        {'J', Actions.JumpAndConfirm},
        {'G', Actions.RopeDartAndBack},
        {'A', Actions.Attack},
        {'S', Actions.Shuriken},
        {'T', Actions.Tabi},
        {'M', Actions.Map},
        {'I', Actions.Inventory},
        {'P', Actions.Pause},
    };
}