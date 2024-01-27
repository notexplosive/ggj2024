using System;

namespace GGJ2024;

public static class SpawnRunner
{
    public static Entity Run(Func<Entity> spawnAction, SpawnParameters spawnParameters)
    {
        var entity = spawnAction() with
        {
            Position = spawnParameters.Position,
            
            // Basics
            IsActive = true,
            Angle = 0
        };

        return entity;
    }
}
