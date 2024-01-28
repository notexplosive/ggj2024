using ExTween;

namespace GGJ2024;

public class Dash
{
    public float MaxSpeed { get; set; }
    public float Speed => _tweenableSpeed;
    public float CurrentCooldown { get; private set; }
    public float TotalCooldown { get; set; }
    public float Duration { get; private set; } = 0.25f;
    public bool IsDashing => !_tween.IsDone();
    private TweenableFloat _tweenableSpeed = new TweenableFloat(0);
    private SequenceTween _tween = new();

    public void Use(Entity caster)
    {
        _tween.Clear();
        _tween
            .Add(_tweenableSpeed.CallbackSetTo(caster.Speed))
            .Add(_tweenableSpeed.TweenTo(MaxSpeed, 0.15f, Ease.SineFastSlow))
            .Add(new WaitSecondsTween(Duration))
            .Add(_tweenableSpeed.TweenTo(MaxSpeed, 0.15f, Ease.SineSlowFast))
            ;
        CurrentCooldown = TotalCooldown;
    }
    
    public void Update(float dt)
    {
        if (_tween.IsDone())
        {
            CurrentCooldown -= dt;
        }
        _tween.Update(dt);
    }

    public bool CanUse()
    {
        return CurrentCooldown <= 0;
    }

    public void FreeCooldown()
    {
        CurrentCooldown = 0f;
    }
}
