namespace GGJ2024;

public class Stats
{
    public Stat<float> CleaverCooldown = new("Cleaver cooldown", 3f);
    public Stat<bool> HasCleaver = new("Equip Cleaver", false);
    public Stat<bool> HasDashBomb = new("Equip Dash Bomb", false);
    public readonly Stat<float> DashCooldown = new("Dash cooldown", 5f);
    public readonly Stat<float> DashSpeed = new("Dash speed", 20f);
    public readonly Stat<int> MaxHealth = new("Max health", 5);
    public readonly Stat<float> SwordCooldown = new("Sword cooldown", 1f);
    public readonly Stat<int> SwordDamage = new("Sword damage", 1);
    public readonly Stat<float> SwordRange = new("Sword range", 700f);

    public Stats(World world, Dash dash, Sword sword)
    {
        // Bind the stats to gameplay variables
        DashCooldown.Bind(x => dash.TotalCooldown = x);
        DashSpeed.Bind(x => dash.MaxSpeed = x);
        SwordRange.Bind(x => sword.Range = x);
        SwordCooldown.Bind(x => sword.Cooldown = x);
        SwordDamage.Bind(x => sword.Damage = x);
        MaxHealth.Bind(x =>
        {
            world.Entities[world.GetPlayerIndex()].MaxHealth = x;
            world.Entities[world.GetPlayerIndex()].Health = x;
        });
    }
}
