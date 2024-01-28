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
            Tags = Tag.Player
        };
    }
    
    public static Entity FastEnemy()
    {
        return new Entity()
        {
            HurtRadius = 100,
            CollideRadius = 32,
            Speed = 7,
            Tags = Tag.Enemy | Tag.Solid,
            Sprite = "game/spider-alt",
            MaxHealth = 1
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
            MaxHealth = 3
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
            Sprite = "game/minotaur",
            MaxHealth = 5
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
            Sprite = "game/plain-dagger",
            RotateWithVelocity = true
        };
    }
    
    public static Entity CleaverBullet()
    {
        return new Entity()
        {
            HurtRadius = 0,
            CollideRadius = 30,
            Tags = Tag.Player,
            Angle = -MathF.PI / 4,
            // CleaveCount gets set by spawn parameters
            Spin = true,
            Sprite = "game/meat-cleaver"
        };
    }
    
    public static Entity Bomb()
    {
        return new Entity()
        {
            HurtRadius = 0,
            CollideRadius = 64,
            DetonationTimer = 1,
            Tags = Tag.Bomb | Tag.Item,
            Sprite = "game/unlit-bomb"
        };
    }
}
