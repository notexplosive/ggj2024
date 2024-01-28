namespace GGJ2024;

public class LevelUpReward
{
    public UpgradeSequence Upgrades { get; }
    public string Title { get; }
    public string IconName { get; }
    public string Description => Upgrades.CheckNextPurchasable()?.Description() ?? "Nothing";

    public LevelUpReward(string title, string iconName, UpgradeSequence upgradeUpgrades)
    {
        Upgrades = upgradeUpgrades;
        Title = title;
        IconName = iconName;
    }

    public bool IsUsable()
    {
        return Upgrades.CheckNextPurchasable() != null;
    }

    public void Buy()
    {
        Upgrades.PopNextPurchasable()!.Buy();
    }
}
