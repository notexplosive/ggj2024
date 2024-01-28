using System;
using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GGJ2024;

public static class SpawnRunner
{
    public static Entity Run(Func<Entity> spawnAction, SpawnParameters spawnParameters)
    {
        var spawned = spawnAction();
        var entity = spawned with
        {
            Position = spawnParameters.Position,
            Velocity = spawnParameters.Velocity,
            HitDamage = spawnParameters.HitDamage ?? spawned.HitDamage,
            
            // Basics
            IsActive = true,
            Health = spawned.MaxHealth
        };

        return entity;
    }
    
    public static Decoration AddDecoration(Texture2D texture, Color color, RectangleF rectangle)
    {
        return new Decoration {Texture = texture, Rectangle = rectangle, Color = color};
    }
}
