using System;

namespace GGJ2024;

public readonly struct BufferedSpawn
{
    public Func<Entity> SpawnAction { get; }
    public SpawnParameters Parameters { get; }

    public BufferedSpawn(Func<Entity> spawnAction, SpawnParameters spawnParameters)
    {
        SpawnAction = spawnAction;
        Parameters = spawnParameters;
    }
}
