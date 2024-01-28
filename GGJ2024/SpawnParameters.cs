using Microsoft.Xna.Framework;

namespace GGJ2024;

/// <summary>
/// Passed to a particular spawn instance
/// </summary>
public readonly struct SpawnParameters
{
    public Vector2 Position { get; init; }
    public Vector2 Velocity { get; init; }
    public int? HitDamage { get; init; }
}