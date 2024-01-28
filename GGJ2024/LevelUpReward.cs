using System;

namespace GGJ2024;

public class LevelUpReward
{
    private readonly Func<bool> _prerequisite;
    public UpgradeSequence Upgrades { get; }
    public string Title { get; }
    public string IconName { get; }
    public string Description => Upgrades.CheckNextPurchasable()?.Description() ?? "Nothing";

    public LevelUpReward(string title, string iconName, Func<bool>? prerequisite, UpgradeSequence upgradeUpgrades)
    {
        _prerequisite = prerequisite ?? AlwaysAllow;
        Upgrades = upgradeUpgrades;
        Title = title;
        IconName = iconName;
    }

    private bool AlwaysAllow()
    {
        return true;
    }

    public bool IsUsable()
    {
        return _prerequisite() && Upgrades.CheckNextPurchasable() != null;
    }

    public void Buy()
    {
        Upgrades.PopNextPurchasable()!.Buy();
    }
}
