using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Microsoft.Xna.Framework;

namespace GGJ2024;

public struct Entity
{
    public Entity()
    {
    }

    public Vector2 Position { get; set; } = default;
    public float CollideRadius { get; set; } = 16;
    public float HurtRadius { get; set; } = 32;
    public float Angle { get; set; } = 0;
    public bool IsActive { get; set; } = false;
    public Tag Tags { get; set; } = Tag.Empty;
    public float Speed { get; set; } = 1f;
    public string? Sprite { get; set; } = null;
    public Vector2 Velocity { get; set; } = default;
    public int Health { get; set; } = 1;
    public int MaxHealth { get; set; } = 1;
    public int HitDamage { get; set; } = 1;
    public float HurtTimer { get; set; } = 0f;

    [Pure]
    public bool HasTag(Tag tag)
    {
        if (!IsActive)
        {
            return false;
        }

        return (Tags & tag) > 0;
    }
}