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
        _rewards.Add(new LevelUpReward("Cardio", "game/person",
            new SpecificUpgradeSequence(
                new DeltaFloatUpgrade(stats.DashCooldown, -0.5f),
                new DeltaFloatUpgrade(stats.DashCooldown, -0.5f),
                new DeltaFloatUpgrade(stats.DashCooldown, -0.5f),
                new DeltaFloatUpgrade(stats.DashCooldown, -0.5f),
                new DeltaFloatUpgrade(stats.DashCooldown, -0.5f),
                new DeltaFloatUpgrade(stats.DashCooldown, -0.5f)
            )));

        _rewards.Add(new LevelUpReward("Sprint Faster", "game/person",
            new SpecificUpgradeSequence(
                new DeltaFloatUpgrade(stats.DashSpeed, 5),
                new DeltaFloatUpgrade(stats.DashSpeed, 5),
                new DeltaFloatUpgrade(stats.DashSpeed, 5),
                new DeltaFloatUpgrade(stats.DashSpeed, 5),
                new DeltaFloatUpgrade(stats.DashSpeed, 5),
                new DeltaFloatUpgrade(stats.DashSpeed, 5)
            )));

        _rewards.Add(new LevelUpReward("Swing Faster", "game/plain-dagger",
            new SpecificUpgradeSequence(
                new DeltaFloatUpgrade(stats.SwordCooldown, 0.2f),
                new DeltaFloatUpgrade(stats.SwordCooldown, -0.2f),
                new DeltaFloatUpgrade(stats.SwordCooldown, -0.2f),
                new DeltaFloatUpgrade(stats.SwordCooldown, -0.2f)
            )));

        _rewards.Add(new LevelUpReward("Hit Harder", "game/plain-dagger",
            new InfiniteUpgradeSequence(new DeltaIntUpgrade(stats.SwordDamage, 1))));
        _rewards.Add(new LevelUpReward("More Health", "game/person",
            new InfiniteUpgradeSequence(new DeltaIntUpgrade(stats.MaxHealth, 1))));
        _rewards.Add(new LevelUpReward("Reach Further", "game/plain-dagger",
            new InfiniteUpgradeSequence(new DeltaFloatUpgrade(stats.SwordRange, 25))));
        _rewards.Add(new LevelUpReward("EXP Magnet", "game/person",
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
