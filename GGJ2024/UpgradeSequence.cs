namespace GGJ2024;

public abstract class UpgradeSequence
{
    public abstract Upgrade? PopNextPurchasable();
    public abstract Upgrade? CheckNextPurchasable();
}
