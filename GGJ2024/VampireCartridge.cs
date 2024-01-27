using System;
using System.Collections.Generic;
using ExplogineCore;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Cartridges;
using ExplogineMonoGame.Data;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;

namespace GGJ2024;

public class VampireCartridge : BasicGameCartridge
{
    private readonly Entity[] _entities;
    private readonly Queue<BufferedSpawn> _bufferedSpawns = new();
    private readonly Camera _camera;
    private int _lastUsedIndex;
    private Vector2 _playerMoveVector;

    public VampireCartridge(IRuntime runtime) : base(runtime)
    {
        _camera = new Camera(
            Runtime.Window.RenderResolution.ToRectangleF().Moved(-Runtime.Window.RenderResolution.ToVector2() / 2),
            Runtime.Window.RenderResolution);
        _entities = new Entity[512];

        SpawnEntity(EntityTemplate.Player, new SpawnParameters());
    }

    public override CartridgeConfig CartridgeConfig { get; } = new(new Point(1920, 1080));

    private void SpawnEntity(Func<Entity> spawnAction, SpawnParameters spawnParameters)
    {
        var index = GetValidSpawnIndex();

        if (!index.HasValue)
        {
            AddBufferedSpawn(spawnAction, spawnParameters);
            return;
        }

        _entities[index.Value] = SpawnRunner.Run(spawnAction, spawnParameters);

        _lastUsedIndex = index.Value;
    }

    private void AddBufferedSpawn(Func<Entity> spawnAction, SpawnParameters spawnParameters)
    {
        _bufferedSpawns.Enqueue(new BufferedSpawn(spawnAction, spawnParameters));
    }

    [Pure]
    private int? GetValidSpawnIndex()
    {
        var index = _lastUsedIndex;

        while (_entities[index].IsActive)
        {
            index++;
            index %= _entities.Length;

            if (index == _lastUsedIndex)
            {
                return null;
            }
        }

        return index;
    }

    public override void OnCartridgeStarted()
    {
    }

    public override void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        var inputVector = InputCalculations.CalculateInputVector(input);

        for (var i = 0; i < _entities.Length; i++)
        {
            var entity = _entities[i];
            if (entity.HasTag(Tag.Player))
            {
                _playerMoveVector = inputVector;
            }
        }
    }

    public override void Update(float dt)
    {
        if (!Runtime.Window.IsInFocus)
        {
            return;
        }

        SpawnFromBuffer();
        MovePlayer();
        CollectExp();
        MoveEnemiesTowardsPlayer();

        // test
        SpawnEntity(EntityTemplate.Enemy, new SpawnParameters
        {
            Position =
                new Vector2(
                    Client.Random.Dirty.NextPositiveInt(Runtime.Window.RenderResolution.X),
                    Client.Random.Dirty.NextPositiveInt(Runtime.Window.RenderResolution.Y)
                ) - Runtime.Window.RenderResolution.ToVector2() / 2f
        });
    }

    private void MovePlayer()
    {
        var playerIndex = GetPlayerIndex();

        _entities[playerIndex].Position += _playerMoveVector * _entities[playerIndex].Speed;
    }

    private int GetPlayerIndex()
    {
        for (var index = 0; index < _entities.Length; index++)
        {
            var entity = _entities[index];
            if (entity.HasTag(Tag.Player))
            {
                return index;
            }
        }

        throw new Exception("Player not found");
    }

    private void MoveEnemiesTowardsPlayer()
    {
        var playerIndex = -1;
        for (var a = 0; a < _entities.Length; a++)
        {
            if (_entities[a].HasTag(Tag.Player))
            {
                playerIndex = a;
            }
        }

        if (playerIndex != -1)
        {
            var playerEntity = _entities[playerIndex];
            for (var a = 0; a < _entities.Length; a++)
            {
                if (_entities[a].HasTag(Tag.Enemy))
                {
                    var moveVector =
                        MathUtils.CalculateDirectionalDisplacement(playerEntity.Position, _entities[a].Position);

                    var newPosition = _entities[a].Position + moveVector * _entities[a].Speed;
                    if (_entities[a].HasTag(Tag.Solid))
                    {
                        for (var b = 0; b < _entities.Length; b++)
                        {
                            if (a != b && _entities[b].HasTag(Tag.Solid) && IsColliding(newPosition,
                                    _entities[a].CollideRadius, _entities[b].Position,
                                    _entities[b].CollideRadius))
                            {
                                var displacement =
                                    MathUtils.CalculateDirectionalDisplacement(_entities[a].Position,
                                        _entities[b].Position);
                                newPosition = _entities[a].Position + displacement * _entities[a].Speed;
                            }
                        }
                    }

                    _entities[a].Position = newPosition;
                }
            }
        }
    }

    private void CollectExp()
    {
        for (var a = 0; a < _entities.Length; a++)
        {
            var primary = _entities[a];
            if (primary.HasTag(Tag.Player))
            {
                for (var b = 0; b < _entities.Length; b++)
                {
                    if (_entities[b].HasTag(Tag.Exp) && IsWithinDistance(a, b, 200))
                    {
                        _entities[b].Position = Vector2.Lerp(_entities[b].Position, primary.Position, 0.25f);

                        if (IsColliding(a, b))
                        {
                            Destroy(b);
                        }
                    }
                }
            }
        }
    }

    private bool IsColliding(Vector2 positionA, float radiusA, Vector2 positionB, float radiusB)
    {
        return (positionA - positionB).LengthSquared() < MathUtils.Squared(radiusA + radiusB);
    }

    private bool IsColliding(int a, int b)
    {
        return IsWithinDistance(a, b, _entities[a].CollideRadius + _entities[b].CollideRadius);
    }

    private bool IsWithinDistance(int a, int b, float distance)
    {
        return (_entities[a].Position - _entities[b].Position).LengthSquared() < MathUtils.Squared(distance);
    }

    private void Destroy(int index)
    {
        _entities[index].IsActive = false;
    }

    private void SpawnFromBuffer()
    {
        if (_bufferedSpawns.Count > 0)
        {
            if (GetValidSpawnIndex().HasValue)
            {
                var spawn = _bufferedSpawns.Dequeue();
                SpawnEntity(spawn.SpawnAction, spawn.Parameters);
            }
        }
    }

    public override void Draw(Painter painter)
    {
        painter.BeginSpriteBatch(_camera.CanvasToScreen);
        foreach (var entity in _entities)
        {
            if (entity.IsActive)
            {
                var bodyColor = Color.White;

                if (entity.HasTag(Tag.Player))
                {
                    bodyColor = ColorExtensions.FromRgbHex(0xff8000);
                }

                painter.DrawCircle(entity.Position, entity.CollideRadius,
                    new DrawSettings {Color = Color.Purple, Depth = Depth.Middle});

                if (entity.HasTag(Tag.Player))
                {
                    painter.DrawCircle(entity.Position, entity.HurtRadius,
                        new DrawSettings {Color = bodyColor, Depth = Depth.Front});
                }
            }
        }

        painter.EndSpriteBatch();
    }

    public override void AddCommandLineParameters(CommandLineParametersWriter parameters)
    {
    }

    public override IEnumerable<ILoadEvent?> LoadEvents(Painter painter)
    {
        yield break;
    }

    public override void OnHotReload()
    {
    }
}

public class MathUtils
{
    public static float Squared(float f)
    {
        return f * f;
    }

    public static int Squared(int i)
    {
        return i * i;
    }

    public static Vector2 CalculateDirectionalDisplacement(Vector2 position, Vector2 vector2)
    {
        var displacement = position - vector2;
        if (displacement.LengthSquared() > 0)
        {
            displacement = displacement.Normalized();
        }

        return displacement;
    }
}
