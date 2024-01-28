namespace GGJ2024;

public class InfiniteUpgradeSequence : UpgradeSequence
{
    private readonly Upgrade _upgrade;

    public InfiniteUpgradeSequence(Upgrade upgrade)
    {
        _upgrade = upgrade;
    }
    
    public override Upgrade? CheckNextPurchasable()
    {
        return _upgrade;
    }

    public override Upgrade? PopNextPurchasable()
    {
        return _upgrade;
    }
}
