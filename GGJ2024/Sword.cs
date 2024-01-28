namespace GGJ2024;

public class Sword : Ability
{
    protected override bool Use(World world, Entity caster)
    {
        var foundEnemy = FindEntity(world, caster);

        if (foundEnemy.HasValue && (foundEnemy.Value.Position - caster.Position).LengthSquared() < MathUtils.Squared(Range))
        {
            world.SpawnBullet(EntityTemplate.SwordBullet,
                new SpawnParameters
                {
                    Position = caster.Position,
                    Velocity = MathUtils.CalculateDirectionalDisplacement(foundEnemy.Value.Position, caster.Position) * 30f
                });
            return true;
        }

        return false;
    }
}
