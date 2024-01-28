using ExplogineMonoGame.Rails;

namespace GGJ2024;

public abstract class Ability
{
    private float _currentCooldownTimer;
    
    public bool IsUnlocked { get; set; }

    public void Update(float dt, World world, Entity caster)
    {
        _currentCooldownTimer -= dt;

        if (_currentCooldownTimer < 0)
        {
            if (Use(world, caster))
            {
                _currentCooldownTimer = Cooldown;
            }
        }
    }

    public float Cooldown { get; set; }
    public int CleaveCount { get; set; }
    public float Range { get; set; }

    protected abstract bool Use(World world, Entity caster);
    
    protected Entity? FindEntity(World world, Entity caster)
    {
        Entity? foundEnemy = null;
        foreach (var pendingEntity in world.Entities)
        {
            if (pendingEntity.HasTag(Tag.Enemy))
            {
                if (foundEnemy.HasValue)
                {
                    var currentDistance = (foundEnemy.Value.Position - caster.Position).LengthSquared();
                    var newDistance = (pendingEntity.Position - caster.Position).LengthSquared();

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

        return foundEnemy;
    }
}