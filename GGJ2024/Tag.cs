using System;

namespace GGJ2024;

[Flags]
public enum Tag
{
    Empty = 0,
    Player = 1,
    Enemy = 2,
    Item = 4,
    Exp = 8,
    BonusItem = 16,
    Solid = 32,
    Bomb = 64
}
