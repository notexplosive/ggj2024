using System;

namespace GGJ2024;

public class DeltaIntUpgrade : Upgrade
{
    private readonly int _delta;
    private readonly Stat<int> _stat;

    public DeltaIntUpgrade(Stat<int> stat, int delta)
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

        return $"{verb} {_stat.Name} by {Math.Abs(_delta)}";
    }
}
