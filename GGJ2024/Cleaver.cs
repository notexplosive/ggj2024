namespace GGJ2024;

public class Cleaver : Ability
{
    protected override bool Use(World world, Entity caster)
    {
        var foundEnemy = FindEntity(world, caster);

        if (foundEnemy.HasValue && (foundEnemy.Value.Position - caster.Position).LengthSquared() < MathUtils.Squared(Range))
        {
            world.SpawnBullet(EntityTemplate.CleaverBullet,
                new SpawnParameters
                {
                    HitDamage = Damage,
                    Position = caster.Position,
                    Velocity = MathUtils.CalculateDirectionalDisplacement(foundEnemy.Value.Position, caster.Position) * 15f,
                    CleaveCount = CleaveCount
                });
            return true;
        }

        return false;
    }

    public int Damage { get; set; } = 1;
}
