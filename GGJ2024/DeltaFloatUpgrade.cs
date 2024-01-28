using System;

namespace GGJ2024;

public class DeltaFloatUpgrade : Upgrade
{
    private readonly float _delta;
    private readonly Stat<float> _stat;

    public DeltaFloatUpgrade(Stat<float> stat, float delta)
    {
        _stat = stat;
        _delta = delta;
    }

    protected override void Apply()
    {
        _stat.Value += _delta;
    }
    
    public override string Description()
    {
        var verb = "Increase";
        if (_delta < 0)
        {
            verb = "Decrease";
        }

        return $"{verb} {_stat.Name} by {MathF.Abs(_delta)}";
    }
}
