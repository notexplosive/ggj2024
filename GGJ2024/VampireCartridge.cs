using System;
using System.Collections.Generic;
using ExplogineCore;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Cartridges;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GGJ2024;

public class VampireCartridge : BasicGameCartridge
{
    private readonly Camera _camera;
    private readonly Decoration[] _decorations;
    private readonly List<Ability> _playerAbilities = new();
    private readonly World _world;
    private readonly RectangleF _worldBounds;
    private bool _gameOver;
    private float _playerMoveDampen;
    private float _playerMoveTimer;
    private Vector2 _playerMoveVector;
    private float _spawnWaveTimer;
    private LevelUpOverlay? _levelUpScreen;

    public VampireCartridge(IRuntime runtime) : base(runtime)
    {
        _world = new World();
        _camera = new Camera(
            Runtime.Window.RenderResolution.ToRectangleF().Moved(-Runtime.Window.RenderResolution.ToVector2() / 2),
            Runtime.Window.RenderResolution);
        _decorations = new Decoration[64];

        var worldSize = _camera.ViewBounds.Size * 2f;
        _worldBounds = RectangleF.InflateFrom(Vector2.Zero, worldSize.X / 2f, worldSize.Y / 2f);

        _world.SpawnEntity(EntityTemplate.Player, new SpawnParameters());

        _world.SpawnEntity(EntityTemplate.Enemy, new SpawnParameters());
        _world.SpawnEntity(EntityTemplate.Enemy, new SpawnParameters());
        _world.SpawnEntity(EntityTemplate.Enemy, new SpawnParameters());
        _world.SpawnEntity(EntityTemplate.Enemy, new SpawnParameters());
        _world.SpawnEntity(EntityTemplate.BigEnemy, new SpawnParameters());

        _playerAbilities.Add(new Sword());
    }

    public override CartridgeConfig CartridgeConfig { get; } = new(new Point(1920, 1080));

    public override void OnCartridgeStarted()
    {
        _levelUpScreen = new LevelUpOverlay(Runtime.Window.RenderResolution.ToRectangleF());
        
        var decorationSpriteIndex = 0;
        var decorationSprites = new[]
        {
            Client.Assets.GetTexture("game/grass"),
            Client.Assets.GetTexture("game/new-shoot"),
            Client.Assets.GetTexture("game/high-grass")
        };

        var colors = new[]
        {
            Color.Green.DimmedBy(0.5f),
            Color.ForestGreen,
            Color.LimeGreen.DimmedBy(0.25f),
            Color.Lime.DimmedBy(0.25f)
        };

        for (var i = 0; i < _decorations.Length; i++)
        {
            bool foundValid;
            RectangleF rectangle;
            do
            {
                var scale = Client.Random.Dirty.NextFloat(0.85f, 1.5f);
                var point = _worldBounds.TopLeft +
                            Client.Random.Dirty.NextPositiveVector2().StraightMultiply(_worldBounds.Size);
                rectangle = RectangleF.InflateFrom(point, 40 * scale, 40 * scale);
                foundValid = true;
                for (var j = 0; j < i; j++)
                {
                    if (rectangle.Intersects(_decorations[j].Rectangle.Inflated(10, 10)))
                    {
                        foundValid = false;
                        break;
                    }
                }
            } while (!foundValid);

            decorationSpriteIndex++;
            _decorations[i] =
                SpawnRunner.AddDecoration(decorationSprites[decorationSpriteIndex % decorationSprites.Length],
                    colors[decorationSpriteIndex % colors.Length],
                    rectangle);
        }
    }

    public override void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        if (_levelUpScreen != null && _levelUpScreen.IsActive)
        {
            _levelUpScreen.UpdateInput(input, hitTestStack);
            return;
        }
        
        var inputVector = InputCalculations.CalculateInputVector(input);
        
        for (var i = 0; i < _world.Entities.Length; i++)
        {
            var entity = _world.Entities[i];
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
        
        if (_levelUpScreen != null && _levelUpScreen.IsActive)
        {
            _levelUpScreen.Update(dt);
            return;
        }

        _world.SpawnFromBuffer();
        MovePlayer(dt);
        CollectExp(dt);
        MoveEnemiesTowardsPlayer(dt);
        AdjustCamera(dt);
        ConstrainAllEntitiesToLevel();
        RunPlayerAbilities(dt);
        CalculateBulletCollision();
        MoveThingsAlongVelocity(dt);
        KillDeadThings();
        SpawnWaves(dt);
    }

    private void SpawnWaves(float dt)
    {
        _spawnWaveTimer -= dt;

        if (_spawnWaveTimer < 0)
        {
            _spawnWaveTimer = 5;

            var edges = new[] {RectEdge.Left, RectEdge.Right, RectEdge.Bottom, RectEdge.Top};

            // todo: waves
            for (var i = 0; i < 5; i++)
            {
                var edge = Client.Random.Clean.GetRandomElement(edges);
                var point = VampireCartridge.GetRandomPointAlongEdge(_camera.ViewBounds.Inflated(64, 64), edge);

                _world.SpawnEntity(EntityTemplate.Enemy, new SpawnParameters {Position = point});
            }
        }
    }

    private static Vector2 GetRandomPointAlongEdge(RectangleF rect, RectEdge edge)
    {
        switch (edge)
        {
            case RectEdge.Left:
                return new Vector2(rect.Left, Client.Random.Clean.NextFloat(0, rect.Height));
            case RectEdge.Right:
                return new Vector2(rect.Right, Client.Random.Clean.NextFloat(0, rect.Height));
            case RectEdge.Top:
                return new Vector2(Client.Random.Clean.NextFloat(0, rect.Width), rect.Top);
            case RectEdge.Bottom:
                return new Vector2(Client.Random.Clean.NextFloat(0, rect.Width), rect.Bottom);
        }

        throw new Exception("invalid edge");
    }

    private void KillDeadThings()
    {
        var playerIndex = _world.GetPlayerIndex();
        for (var i = 0; i < _world.Entities.Length; i++)
        {
            if (_world.Entities[i].IsActive && !_world.Entities[i].HasTag(Tag.Item) && _world.Entities[i].Health <= 0)
            {
                if (i == playerIndex)
                {
                    _gameOver = true;
                }
                else
                {
                    _world.DestroyEntity(i);
                    _world.SpawnEntity(EntityTemplate.Exp,
                        new SpawnParameters {Position = _world.Entities[i].Position});
                }
            }
        }
    }

    private void CalculateBulletCollision()
    {
        for (var bulletIndex = 0; bulletIndex < _world.Bullets.Length; bulletIndex++)
        {
            var bullet = _world.Bullets[bulletIndex];
            if (bullet.IsActive)
            {
                var foundEntity = -1;
                for (var entityIndex = 0; entityIndex < _world.Entities.Length; entityIndex++)
                {
                    var entity = _world.Entities[entityIndex];
                    if (_world.AreEnemies(bullet, entity) && _world.IsColliding(bullet, entity))
                    {
                        foundEntity = entityIndex;
                        break;
                    }
                }

                if (foundEntity != -1)
                {
                    _world.DestroyBullet(bulletIndex);
                    _world.Entities[foundEntity].Health -= bullet.HitDamage;
                }
            }
        }
    }

    private void MoveThingsAlongVelocity(float dt)
    {
        for (var i = 0; i < _world.Entities.Length; i++)
        {
            if (_world.Entities[i].IsActive)
            {
                _world.Entities[i].Position += _world.Entities[i].Velocity * dt * 60f;
            }
        }

        for (var i = 0; i < _world.Bullets.Length; i++)
        {
            if (_world.Bullets[i].IsActive)
            {
                _world.Bullets[i].Position += _world.Bullets[i].Velocity * dt * 60f;
            }
        }
    }

    private void RunPlayerAbilities(float dt)
    {
        foreach (var ability in _playerAbilities)
        {
            ability.Update(dt, _world, _world.Entities[_world.GetPlayerIndex()]);
        }
    }

    private void ConstrainAllEntitiesToLevel()
    {
        for (var i = 0; i < _world.Entities.Length; i++)
        {
            if (!_worldBounds.Contains(_world.Entities[i].Position))
            {
                _world.Entities[i].Position = _world.Entities[i].Position.ConstrainedTo(_worldBounds);
            }
        }
    }

    private void AdjustCamera(float dt)
    {
        var player = _world.Entities[_world.GetPlayerIndex()];
        var cameraInset = RectangleF.InflateFrom(_camera.CenterPosition, 100, 100);
        if (!cameraInset.Contains(player.Position))
        {
            if (player.Position.X > cameraInset.Right)
            {
                _camera.CenterPosition += new Vector2(player.Position.X - cameraInset.Right, 0);
            }

            if (player.Position.X < cameraInset.Left)
            {
                _camera.CenterPosition += new Vector2(player.Position.X - cameraInset.Left, 0);
            }

            if (player.Position.Y < cameraInset.Top)
            {
                _camera.CenterPosition += new Vector2(0, player.Position.Y - cameraInset.Top);
            }

            if (player.Position.Y > cameraInset.Bottom)
            {
                _camera.CenterPosition += new Vector2(0, player.Position.Y - cameraInset.Bottom);
            }
        }

        _camera.ViewBounds = _camera.ViewBounds.ConstrainedTo(_worldBounds);
    }

    private void MovePlayer(float dt)
    {
        _playerMoveTimer += dt;
        if (MathUtils.IsVerySmall(_playerMoveVector))
        {
            _playerMoveDampen = Math.Max(0, _playerMoveDampen - dt * 4);
        }
        else
        {
            _playerMoveDampen = 1f;
        }

        var playerIndex = _world.GetPlayerIndex();

        _world.Entities[playerIndex].Position += _playerMoveVector * _world.Entities[playerIndex].Speed * dt * 60;
    }

    private void MoveEnemiesTowardsPlayer(float dt)
    {
        var playerIndex = _world.GetPlayerIndex();
        var playerEntity = _world.Entities[playerIndex];
        for (var a = 0; a < _world.Entities.Length; a++)
        {
            if (_world.Entities[a].HasTag(Tag.Enemy))
            {
                var moveVector =
                    MathUtils.CalculateDirectionalDisplacement(playerEntity.Position, _world.Entities[a].Position);

                _world.Entities[a].Position += moveVector * _world.Entities[a].Speed * dt * 60f;
            }
        }

        for (var a = 0; a < _world.Entities.Length; a++)
        {
            if (_world.Entities[a].HasTag(Tag.Solid))
            {
                for (var b = 0; b < _world.Entities.Length; b++)
                {
                    if (a != b && _world.Entities[b].HasTag(Tag.Solid) && _world.IsColliding(a, b))
                    {
                        var displacement =
                            MathUtils.CalculateDirectionalDisplacement(_world.Entities[a].Position,
                                _world.Entities[b].Position);
                        if (displacement.LengthSquared() < MathUtils.Squared(1))
                        {
                            displacement = Client.Random.Clean.NextNormalVector2();
                        }

                        _world.Entities[a].Position += displacement / 2f;
                        _world.Entities[b].Position -= displacement / 2f;
                    }
                }
            }
        }
    }

    private void CollectExp(float dt)
    {
        for (var a = 0; a < _world.Entities.Length; a++)
        {
            var primary = _world.Entities[a];
            if (primary.HasTag(Tag.Player))
            {
                for (var b = 0; b < _world.Entities.Length; b++)
                {
                    if (_world.Entities[b].HasTag(Tag.Exp) && _world.IsWithinDistance(a, b, 200))
                    {
                        _world.Entities[b].Position = Vector2.Lerp(_world.Entities[b].Position, primary.Position,
                            0.25f * dt * 60f);

                        if (_world.IsColliding(a, b))
                        {
                            _world.DestroyEntity(b);
                            _levelUpScreen?.IncrementExp();
                        }
                    }
                }
            }
        }
    }

    public override void Draw(Painter painter)
    {
        var scale = 0.25f;

        painter.BeginSpriteBatch(_camera.CanvasToScreen);
        painter.DrawLineRectangle(_worldBounds, new LineDrawSettings());
        painter.EndSpriteBatch();

        painter.BeginSpriteBatch(_camera.CanvasToScreen);
        foreach (var decoration in _decorations)
        {
            if (decoration.Texture != null)
            {
                painter.DrawAsRectangle(decoration.Texture, decoration.Rectangle,
                    new DrawSettings {Color = decoration.Color});
            }
        }

        painter.EndSpriteBatch();

        painter.BeginSpriteBatch(_camera.CanvasToScreen);
        foreach (var entity in _world.Entities)
        {
            if (entity.IsActive)
            {
                var bodyColor = Color.White;

                if (entity.HasTag(Tag.Player))
                {
                    bodyColor = ColorExtensions.FromRgbHex(0xff8000);
                }

                if (entity.HasTag(Tag.Enemy) && !string.IsNullOrEmpty(entity.Sprite))
                {
                    var texture = Client.Assets.GetTexture(entity.Sprite);
                    var localScale = entity.CollideRadius / texture.Width * 2;
                    painter.DrawAtPosition(texture, entity.Position,
                        new Scale2D(localScale),
                        new DrawSettings
                        {
                            Origin = DrawOrigin.Center,
                            Color = bodyColor
                        });
                }
                else if (entity.HasTag(Tag.Player))
                {
                    Texture2D texture;

                    if (MathUtils.IsVerySmall(_playerMoveVector) || _playerMoveVector.X == 0)
                    {
                        texture = Client.Assets.GetTexture("game/person");
                    }
                    else
                    {
                        texture = Client.Assets.GetTexture("game/walk");
                    }

                    var flipX = _playerMoveVector.X < 0;

                    painter.DrawAtPosition(texture, entity.Position + new Vector2(0, texture.Height / 2f * scale),
                        new Scale2D(scale),
                        new DrawSettings
                        {
                            Flip = new XyBool(flipX, false),
                            Origin = new DrawOrigin(new Vector2(texture.Width / 2f, texture.Height)),
                            Angle = MathF.Sin(_playerMoveTimer * 30f) * _playerMoveDampen / 5,
                            Color = bodyColor
                        });
                }
                else if (entity.HasTag(Tag.Exp))
                {
                    painter.DrawCircle(entity.Position, entity.CollideRadius,
                        new DrawSettings {Color = Color.Yellow, Depth = Depth.Back});
                }
                else
                {
                    painter.DrawCircle(entity.Position, entity.CollideRadius,
                        new DrawSettings {Color = Color.Purple, Depth = Depth.Middle});
                }
            }
        }

        painter.EndSpriteBatch();

        painter.BeginSpriteBatch(_camera.CanvasToScreen);
        var color = Color.OrangeRed;
        foreach (var bullet in _world.Bullets)
        {
            if (bullet.IsActive)
            {
                if (bullet.Sprite != null)
                {
                    var texture2D = Client.Assets.GetTexture(bullet.Sprite);
                    painter.DrawAtPosition(texture2D, bullet.Position, new Scale2D(scale),
                        new DrawSettings
                        {
                            Origin = DrawOrigin.Center, Color = color,
                            Angle = bullet.Angle + bullet.Velocity.GetAngleFromUnitX()
                        });
                }
                else
                {
                    painter.DrawCircle(bullet.Position, bullet.CollideRadius, new DrawSettings {Color = color});
                }
            }
        }

        painter.EndSpriteBatch();

        painter.BeginSpriteBatch();
        var insetScreen = Runtime.Window.RenderResolution.ToRectangleF().Inflated(0, -100);

        var font = Client.Assets.GetFont("game/font", 64);
        var y = insetScreen.Top - font.Height;

        var expBar = RectangleF.FromSizeAlignedWithin(insetScreen, new Vector2(insetScreen.Width - 100, 50),
            Alignment.TopCenter);

        painter.DrawStringWithinRectangle(font, "Wave 1",
            new RectangleF(new Vector2(expBar.X, y), new Vector2(expBar.Width, font.Height)), Alignment.BottomCenter,
            new DrawSettings());
        painter.DrawRectangle(expBar, new DrawSettings {Color = Color.Yellow.DimmedBy(0.5f), Depth = Depth.Back});

        var percent = _levelUpScreen.Percent();
        var expFill = expBar.ResizedOnEdge(RectEdge.Right, new Vector2(-expBar.Width * (1 - percent), 0));
        painter.DrawRectangle(expFill, new DrawSettings {Color = Color.Yellow, Depth = Depth.Middle});
        painter.EndSpriteBatch();
        
        if (_levelUpScreen != null && _levelUpScreen.IsActive)
        {
            painter.BeginSpriteBatch();
            painter.DrawRectangle(Runtime.Window.RenderResolution.ToRectangleF(), new DrawSettings{Color = Color.Black.WithMultipliedOpacity(0.75f)});
            painter.EndSpriteBatch();
            
            painter.BeginSpriteBatch();
            _levelUpScreen.Draw(painter);
            painter.EndSpriteBatch();
        }
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