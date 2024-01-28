namespace GGJ2024;

public class Stats
{
    public Stat<bool> HasBomb = new("Drop a Bomb every time you dash", false);
    public Stat<int> BombDamage = new("Bomb damage", 1);
    public Stat<float> BombExplosionRadius = new("Bomb explosion radius", 100f);
    
    public Stat<bool> HasThorns = new("Explode when you take damage", false);
    public Stat<int> ThornsDamage = new("Thorns damage", 1);
    public Stat<float> ThornsExplosionRadius = new("Thorn damage radius", 150f);
    
    public Stat<bool> HasCleaver = new("Cleaves through several enemies", false);
    public Stat<float> CleaverCooldown = new("Cleaver cooldown", 3f);
    public Stat<float> CleaverRange = new("Cleaver range", 500f);
    public Stat<int> CleaverDamage = new("Cleaver damage", 1);
    public Stat<int> CleaverCleaveCount = new("number of enemies Cleaved", 3);
    
    public readonly Stat<float> DashCooldown = new("Dash cooldown", 5f);
    public readonly Stat<float> DashSpeed = new("Dash speed", 20f);
    public readonly Stat<int> MaxHealth = new("Max health", 3);
    public readonly Stat<float> SwordCooldown = new("Sword cooldown", 1f);
    public readonly Stat<int> SwordDamage = new("Sword damage", 1);
    public readonly Stat<float> SwordRange = new("Sword range", 700f);
    public readonly Stat<float> ExpRadius = new("EXP Radius", 200);

    public Stats(World world, Dash dash, Sword sword, Cleaver cleaver, Bombs dashBombs, Bombs thorns, Bombs explodeEnemies)
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
        
        HasCleaver.Bind(x=> cleaver.IsUnlocked = x);
        CleaverCooldown.Bind(x => cleaver.Cooldown = x);
        CleaverDamage.Bind(x=>cleaver.Damage = x);
        CleaverRange.Bind(x=>cleaver.Range = x);
        CleaverCleaveCount.Bind(x=>cleaver.CleaveCount = x);
        
        HasBomb.Bind(x=>dashBombs.IsUnlocked = x);
        BombExplosionRadius.Bind(x=>dashBombs.ExplosionRadius = x);
        BombDamage.Bind(x=>dashBombs.Damage = x);
        
        HasThorns.Bind(x=>thorns.IsUnlocked = x);
        ThornsExplosionRadius.Bind(x=>thorns.ExplosionRadius = x);
        ThornsDamage.Bind(x=>thorns.Damage = x);
    }

}
