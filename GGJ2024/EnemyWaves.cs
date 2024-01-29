using System;
using System.Collections.Generic;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExTween;

namespace GGJ2024;

public class EnemyWaves
{
    private readonly List<EnemyWave> _waves = new();

    public EnemyWaves(World world, Camera camera)
    {
        for (var level = 1; level <= 10; level++)
        {
            var shortDuration = 5f;
            var mediumDuration = 10f;
            var longerDuration = 15;

            var few = 5 * level;
            var several = 10 * level;
            var many = 50 * level;

            _waves.Add(
                new EnemyWave(
                    Trickle(world, camera, EntityTemplate.Enemy, several, shortDuration),
                    new WaitSecondsTween(mediumDuration)
                ));

            _waves.Add(
                new EnemyWave(
                    Trickle(world, camera, EntityTemplate.FastEnemy, few, longerDuration)
                ));
            
            _waves.Add(
                new EnemyWave(
                    Trickle(world, camera, EntityTemplate.FastEnemy, few, shortDuration),
                    Trickle(world, camera, EntityTemplate.FastEnemy, several, shortDuration),
                    new WaitSecondsTween(mediumDuration)
                ));

            _waves.Add(
                new EnemyWave(
                    Trickle(world, camera, EntityTemplate.Enemy, many, mediumDuration),
                    Burst(world, camera, EntityTemplate.FastEnemy, few),
                    new WaitSecondsTween(longerDuration)
                ));

            _waves.Add(
                new EnemyWave(
                    Trickle(world, camera, EntityTemplate.BigEnemy, several, mediumDuration),
                    new WaitSecondsTween(longerDuration)
                ));

            _waves.Add(
                new EnemyWave(
                    Burst(world, camera, EntityTemplate.Enemy, many),
                    Trickle(world, camera, EntityTemplate.Enemy, many, longerDuration),
                    new WaitSecondsTween(longerDuration)
                ));

            _waves.Add(
                new EnemyWave(
                    Burst(world, camera, EntityTemplate.Enemy, several),
                    Trickle(world, camera, EntityTemplate.BigEnemy, few, shortDuration),
                    Burst(world, camera, EntityTemplate.FastEnemy, many),
                    Trickle(world, camera, EntityTemplate.FastEnemy, many, shortDuration),
                    new WaitSecondsTween(mediumDuration)
                ));
        }
    }

    public int CurrentIndex { get; private set; }

    private ITween Burst(World world, Camera camera, Func<Entity> spawnFunction, int count)
    {
        var sequence = new SequenceTween();

        sequence.Add(new CallbackTween(() =>
        {
            for (var i = 0; i < count; i++)
            {
                world.SpawnEntityAlongRandomEdge(spawnFunction, camera);
            }
        }));

        return sequence;
    }

    private ITween Trickle(World world, Camera camera, Func<Entity> spawnFunction, int count, float duration)
    {
        var sequence = new SequenceTween();

        var gap = duration / count;
        for (var i = 0; i < count; i++)
        {
            sequence.Add(new CallbackTween(() => { world.SpawnEntityAlongRandomEdge(spawnFunction, camera); }));
            sequence.Add(new WaitSecondsTween(gap));
        }

        return sequence;
    }

    public void Update(float dt)
    {
        if (!_waves.IsValidIndex(CurrentIndex))
        {
            return;
        }

        _waves[CurrentIndex].Update(dt);
        if (_waves[CurrentIndex].IsComplete())
        {
            CurrentIndex++;
        }
    }
}

public class EnemyWave
{
    private readonly MultiplexTween _tween = new();

    public EnemyWave(params ITween[] subtween)
    {
        foreach (var tween in subtween)
        {
            _tween.AddChannel(tween);
        }
    }

    public void Update(float dt)
    {
        _tween.Update(dt);
    }

    public bool IsComplete()
    {
        return _tween.IsDone();
    }
}
