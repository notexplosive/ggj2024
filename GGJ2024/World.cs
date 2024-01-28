using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;

namespace GGJ2024;

public class World
{
    private readonly Queue<BufferedSpawn> _bufferedSpawns = new();
    private int _lastUsedEntityIndex;
    private int _lastUsedBulletIndex;
    public Entity[] Bullets { get; } = new Entity[128];
    public Entity[] Entities { get; } = new Entity[256];

    public int GetPlayerIndex()
    {
        for (var index = 0; index < Entities.Length; index++)
        {
            var entity = Entities[index];
            if (entity.HasTag(Tag.Player))
            {
                return index;
            }
        }

        throw new Exception("Player not found");
    }

    public void DestroyEntity(int index)
    {
        Entities[index].IsActive = false;
    }
    
    public void DestroyBullet(int index)
    {
        Bullets[index].IsActive = false;
    }

    public bool IsColliding(Vector2 positionA, float radiusA, Vector2 positionB, float radiusB)
    {
        return (positionA - positionB).LengthSquared() < MathUtils.Squared(radiusA + radiusB);
    }
    
    public bool IsColliding(Entity a, Entity b)
    {
        return IsColliding(a.Position, a.CollideRadius, b.Position, b.CollideRadius);
    }

    public bool IsColliding(int a, int b)
    {
        return IsWithinDistance(a, b, Entities[a].CollideRadius + Entities[b].CollideRadius);
    }

    public bool IsWithinDistance(int a, int b, float distance)
    {
        return (Entities[a].Position - Entities[b].Position).LengthSquared() < MathUtils.Squared(distance);
    }

    private void AddBufferedSpawn(Func<Entity> spawnAction, SpawnParameters spawnParameters)
    {
        _bufferedSpawns.Enqueue(new BufferedSpawn(spawnAction, spawnParameters));
    }

    [Pure]
    private int? GetValidSpawnIndex(Entity[] list, int lastUsedIndex)
    {
        var index = lastUsedIndex;

        while (list[index].IsActive)
        {
            index++;
            index %= list.Length;

            if (index == lastUsedIndex)
            {
                return null;
            }
        }

        return index;
    }

    public void SpawnFromBuffer()
    {
        if (_bufferedSpawns.Count > 0)
        {
            if (GetValidSpawnIndex(Entities, _lastUsedEntityIndex).HasValue)
            {
                var spawn = _bufferedSpawns.Dequeue();
                SpawnEntity(spawn.SpawnAction, spawn.Parameters);
            }
        }
    }

    public void SpawnBullet(Func<Entity> spawnAction, SpawnParameters spawnParameters)
    {
        var index = GetValidSpawnIndex(Bullets, _lastUsedBulletIndex);

        if (index.HasValue)
        {
            Bullets[index.Value] = SpawnRunner.Run(spawnAction, spawnParameters);
            _lastUsedBulletIndex = index.Value;
        }
    }
    
    public void SpawnEntity(Func<Entity> spawnAction, SpawnParameters spawnParameters)
    {
        var index = GetValidSpawnIndex(Entities, _lastUsedEntityIndex);

        if (!index.HasValue)
        {
            AddBufferedSpawn(spawnAction, spawnParameters);
            return;
        }

        Entities[index.Value] = SpawnRunner.Run(spawnAction, spawnParameters);

        _lastUsedEntityIndex = index.Value;
    }

    public bool AreEnemies(Entity a, Entity b)
    {
        return (a.HasTag(Tag.Player) && b.HasTag(Tag.Enemy)) || (a.HasTag(Tag.Enemy) && b.HasTag(Tag.Player));
    }
}
