namespace GGJ2024;

public class Sword : Ability
{
    protected override bool Use(World world, Entity caster)
    {
        var player = world.Entities[world.GetPlayerIndex()];
        Entity? foundEnemy = null;
        foreach (var pendingEntity in world.Entities)
        {
            if (pendingEntity.HasTag(Tag.Enemy))
            {
                if (foundEnemy.HasValue)
                {
                    var currentDistance = (foundEnemy.Value.Position - player.Position).LengthSquared();
                    var newDistance = (pendingEntity.Position - player.Position).LengthSquared();

                    if (newDistance < currentDistance)
                    {
                        foundEnemy = pendingEntity;
                    }
                }
                else
                {
                    foundEnemy = pendingEntity;
                }
            }
        }

        if (foundEnemy.HasValue)
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
