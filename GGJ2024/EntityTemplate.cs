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
    
    public static Entity Enemy()
    {
        return new Entity()
        {
            HurtRadius = 32,
            CollideRadius = 16,
            Speed = 2,
            Tags = Tag.Enemy | Tag.Solid
        };
    }

    public static Entity Exp()
    {
        return new Entity()
        {
            HurtRadius = 0,
            CollideRadius = 8,
            Tags = Tag.Item | Tag.Exp
        };
    }
}
