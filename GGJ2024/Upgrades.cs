using System;
using System.Collections.Generic;
using System.Linq;
using ExplogineMonoGame;

namespace GGJ2024;

public class Upgrades
{
    private readonly List<LevelUpReward> _rewards = new();

    public Upgrades(Stats stats)
    {
        _rewards.Add(new LevelUpReward("Equip Cleaver", "game/meat-cleaver",
            null,
            new SpecificUpgradeSequence(new EquipUpgrade(stats.HasCleaver, true))
            ));
        
        _rewards.Add(new LevelUpReward("Cleave Faster", "game/meat-cleaver",
            ()=>stats.HasCleaver.Value,
            new SpecificUpgradeSequence(
                new DeltaFloatUpgrade(stats.CleaverCooldown, -0.2f),
                new DeltaFloatUpgrade(stats.CleaverCooldown, -0.2f),
                new DeltaFloatUpgrade(stats.CleaverCooldown, -0.2f),
                new DeltaFloatUpgrade(stats.CleaverCooldown, -0.2f)
            )));
        
        _rewards.Add(new LevelUpReward("Cleave More", "game/meat-cleaver",
            ()=>stats.HasCleaver.Value,
            new InfiniteUpgradeSequence(new DeltaIntUpgrade(stats.CleaverCleaveCount, 1))));
        
        
        _rewards.Add(new LevelUpReward("Faster Dash Cooldown", "game/sprint",
            null,
            new SpecificUpgradeSequence(
                new DeltaFloatUpgrade(stats.DashCooldown, -0.5f),
                new DeltaFloatUpgrade(stats.DashCooldown, -0.5f),
                new DeltaFloatUpgrade(stats.DashCooldown, -0.5f),
                new DeltaFloatUpgrade(stats.DashCooldown, -0.5f),
                new DeltaFloatUpgrade(stats.DashCooldown, -0.5f),
                new DeltaFloatUpgrade(stats.DashCooldown, -0.5f)
            )));

        _rewards.Add(new LevelUpReward("Sprint Faster", "game/sprint",
            null,
            new SpecificUpgradeSequence(
                new DeltaFloatUpgrade(stats.DashSpeed, 5),
                new DeltaFloatUpgrade(stats.DashSpeed, 5),
                new DeltaFloatUpgrade(stats.DashSpeed, 5),
                new DeltaFloatUpgrade(stats.DashSpeed, 5),
                new DeltaFloatUpgrade(stats.DashSpeed, 5),
                new DeltaFloatUpgrade(stats.DashSpeed, 5)
            )));

        _rewards.Add(new LevelUpReward("Swing Faster", "game/plain-dagger",
            null,
            new SpecificUpgradeSequence(
                new DeltaFloatUpgrade(stats.SwordCooldown, 0.2f),
                new DeltaFloatUpgrade(stats.SwordCooldown, -0.2f),
                new DeltaFloatUpgrade(stats.SwordCooldown, -0.2f),
                new DeltaFloatUpgrade(stats.SwordCooldown, -0.2f)
            )));

        _rewards.Add(new LevelUpReward("Hit Harder", "game/plain-dagger",
            null,
            new InfiniteUpgradeSequence(new DeltaIntUpgrade(stats.SwordDamage, 1))));
        _rewards.Add(new LevelUpReward("More Health", "game/person",
            null,
            new InfiniteUpgradeSequence(new DeltaIntUpgrade(stats.MaxHealth, 1))));
        _rewards.Add(new LevelUpReward("Reach Further", "game/plain-dagger",
            null,
            new InfiniteUpgradeSequence(new DeltaFloatUpgrade(stats.SwordRange, 25))));
        _rewards.Add(new LevelUpReward("EXP Magnet", "game/person",
            null,
            new InfiniteUpgradeSequence(new DeltaFloatUpgrade(stats.ExpRadius, 25))));
    }

    public IEnumerable<LevelUpReward> Pull(int count)
    {
        var usableRewards = _rewards.Where(a => a.IsUsable()).ToList();
        Client.Random.Clean.Shuffle(usableRewards);

        if (usableRewards.Count < count)
        {
            throw new Exception("Not enough upgrade paths to supply shop");
        }

        for (var i = 0; i < count; i++)
        {
            yield return usableRewards[i];
        }
    }
}

public class EquipUpgrade : Upgrade
{
    private readonly Stat<bool> _stat;
    private readonly bool _targetValue;

    public EquipUpgrade(Stat<bool> stat, bool targetValue)
    {
        _stat = stat;
        _targetValue = targetValue;
    }

    protected override void Apply()
    {
        _stat.Value = _targetValue;
    }

    public override string Description()
    {
        return _stat.Name;
    }
}
