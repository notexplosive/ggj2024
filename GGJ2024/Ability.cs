using ExplogineMonoGame.Rails;

namespace GGJ2024;

public abstract class Ability
{
    private float _currentCooldownTimer;

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

    public float Cooldown { get; set; } = 1f;

    protected abstract bool Use(World world, Entity caster);
}