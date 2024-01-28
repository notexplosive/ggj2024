using System;

namespace GGJ2024;

public class EntityTemplate
{
    public static Entity Player()
    {
        return new Entity()
        {
            HurtRadius = 16,
            CollideRadius = 32,
            Speed = 8,
            MaxHealth = 5,
            Tags = Tag.Player
        };
    }
    
    public static Entity Enemy()
    {
        return new Entity()
        {
            HurtRadius = 80,
            CollideRadius = 64,
            Speed = 4,
            Tags = Tag.Enemy | Tag.Solid,
            Sprite = "game/skeleton",
            MaxHealth = 2
        };
    }
    
    public static Entity BigEnemy()
    {
        return new Entity()
        {
            HurtRadius = 120,
            CollideRadius = 90,
            Speed = 3f,
            Tags = Tag.Enemy | Tag.Solid,
            Sprite = "game/minotaur"
        };
    }

    public static Entity Exp()
    {
        return new Entity()
        {
            HurtRadius = 0,
            CollideRadius = 16,
            Tags = Tag.Item | Tag.Exp
        };
    }

    public static Entity SwordBullet()
    {
        return new Entity()
        {
            HurtRadius = 0,
            CollideRadius = 30,
            Tags = Tag.Player,
            Angle = -MathF.PI / 4,
            Sprite = "game/plain-dagger"
        };
    }
}
