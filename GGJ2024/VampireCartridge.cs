﻿using System;
using System.Collections.Generic;
using ExplogineCore;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Cartridges;
using ExplogineMonoGame.Data;
using ExTween;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GGJ2024;

public class VampireCartridge : BasicGameCartridge
{
    private readonly VfxCollection _vfx = new();
    private readonly Camera _camera;
    private readonly Decoration[] _decorations;
    private readonly HostRuntime _hostRuntime;
    private readonly SequenceTween _introTween = new();
    private readonly List<Ability> _playerAbilities = new();
    private readonly Stats _stats;
    private readonly World _world;
    private readonly RectangleF _worldBounds;
    private readonly TweenableFloat _barOffsetPercent = new(1f);
    private readonly Dash _dash = new();
    private float _elapsedTime;
    private readonly TweenableFloat _fadeOverlayOpacity = new(1f);
    private readonly TweenableFloat _overlayTextOpacity = new(1f);
    private bool _gameOver;
    private LevelUpOverlay? _levelUpScreen;
    private float _playerHitCooldown;
    private float _playerMoveDampen;
    private float _playerMoveTimer;
    private Vector2 _playerMoveVector;
    private bool _playerShouldFlicker;
    private Upgrades? _upgrades;
    private readonly EnemyWaves _waves;
    private float _freezeTimer;
    private readonly Bombs _dashBombs;
    private readonly Bombs _thorns;
    private readonly Bombs _explodeEnemies;

    // ReSharper disable once InconsistentNaming
    private bool StoryBeat_NoEnemies => _hostRuntime.HostCartridge.StoryProgress == 0;
    
    // ReSharper disable once InconsistentNaming
    private bool StoryBeat_Monsters => _hostRuntime.HostCartridge.StoryProgress == 1;
    
    // ReSharper disable once InconsistentNaming
    private bool StoryBeat_Weapons => _hostRuntime.HostCartridge.StoryProgress == 2;

    public VampireCartridge(HostRuntime runtime) : base(runtime)
    {
        _hostRuntime = runtime;
        _world = new World();
        _camera = new Camera(
            Runtime.Window.RenderResolution.ToRectangleF().Moved(-Runtime.Window.RenderResolution.ToVector2() / 2),
            Runtime.Window.RenderResolution);
        _waves = new EnemyWaves(_world, _camera);

        _decorations = new Decoration[64];

        var worldSize = _camera.ViewBounds.Size * 2f;
        var worldScale = 2;
        _worldBounds = RectangleF.InflateFrom(Vector2.Zero, worldSize.X / 2f * worldScale, worldSize.Y / 2f * worldScale);

        _world.SpawnEntity(EntityTemplate.Player, new SpawnParameters());

        var sword = new Sword();
        var cleaver = new Cleaver();
        _thorns = new Bombs();
        _explodeEnemies = new Bombs();
        _dashBombs = new Bombs();
        _playerAbilities.Add(sword);
        _playerAbilities.Add(cleaver);
        _stats = new(_world, _dash, sword, cleaver, _dashBombs, _thorns, _explodeEnemies);
        
        sword.IsUnlocked = true;
        
        var targetViewBounds = _camera.ViewBounds;
        _fadeOverlayOpacity.Value = 1f;
        _introTween
            .Add(_camera.TweenableViewBounds.CallbackSetTo(
                _camera.ViewBounds.GetZoomedInBounds(500, _camera.ViewBounds.Center)))
            .Add(
                new MultiplexTween()
                    .AddChannel(_camera.TweenableViewBounds.TweenTo(targetViewBounds, 1f, Ease.CubicFastSlow))
                    .AddChannel(
                        new SequenceTween()
                            .Add(new WaitSecondsTween(0.2f))
                            .Add(_fadeOverlayOpacity.TweenTo(0f, 0.6f, Ease.Linear))
                    )
            )
            .Add(_barOffsetPercent.TweenTo(-0.05f, 0.25f, Ease.CubicFastSlow))
            .Add(_barOffsetPercent.TweenTo(0f, 0.15f, Ease.CubicSlowFast))
            .Add(new WaitSecondsTween(0.5f))
            ;
    }

    public override CartridgeConfig CartridgeConfig { get; } = new(new Point(1920, 1080));

    public override void OnCartridgeStarted()
    {
        _upgrades = new Upgrades(_stats);
        _levelUpScreen = new LevelUpOverlay(Runtime.Window.RenderResolution.ToRectangleF(), _upgrades);

        var decorationSpriteIndex = 0;
        var decorationSprites = new[]
        {
            Client.Assets.GetTexture("game/grass"),
            Client.Assets.GetTexture("game/new-shoot"),
            Client.Assets.GetTexture("game/high-grass")
        };

        var colors = new[]
        {
            Color.Green.DesaturatedBy(0.25f),
            Color.ForestGreen,
            Color.LimeGreen.DesaturatedBy(0.25f),
            Color.Lime.DesaturatedBy(0.25f)
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

        if (!StoryBeat_NoEnemies)
        {
            MusicPlayer.FadeToMain();
        }
    }

    public override void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        if (Client.Debug.IsPassiveOrActive)
        {
            if (input.Keyboard.GetButton(Keys.E, true).WasPressed)
            {
                for (int i = 0; i < 10; i++)
                {
                    _levelUpScreen?.IncrementExp();
                }
            }
        }
        
        if (!_introTween.IsDone())
        {
            return;
        }

        if (input.Keyboard.GetButton(Keys.Escape).WasPressed)
        {
            _introTween.Add(_fadeOverlayOpacity.TweenTo(1f, 0.5f, Ease.Linear));
            _introTween.Add(_overlayTextOpacity.TweenTo(0f, 0.5f, Ease.Linear));
            _introTween.Add(new WaitSecondsTween(0.5f));
            _introTween.Add(new CallbackTween(() =>
            {
                _hostRuntime.HostCartridge.StoryProgress++;

                // Win the game at 10 waves
                if (_waves.CurrentIndex >= 10)
                {
                    _hostRuntime.HostCartridge.HasWon = true;
                }
                
                _hostRuntime.HostCartridge.RegenerateCartridge<GGJCartridge>();
                _hostRuntime.HostCartridge.SwapTo<GGJCartridge>();
            }));
        }

        if (_levelUpScreen != null && _levelUpScreen.IsActive)
        {
            _levelUpScreen.UpdateInput(input, hitTestStack);
            return;
        }

        var inputVector = InputCalculations.CalculateInputVector(input);

        if (_dash.CanUse() && !MathUtils.IsVerySmall(inputVector) && !StoryBeat_Monsters && !StoryBeat_NoEnemies)
        {
            if (Client.Input.Keyboard.GetButton(Keys.Space).WasPressed ||
                Client.Input.Keyboard.GetButton(Keys.LeftShift).WasPressed ||
                Client.Input.Keyboard.GetButton(Keys.RightShift).WasPressed)
            {
                GGJSoundPlayer.Play("game/bong", 1f, -1f);
                var player = _world.Entities[_world.GetPlayerIndex()];
                _dash.Use(player);

                if (_dashBombs.IsUnlocked)
                {
                    _world.SpawnEntity(EntityTemplate.Bomb, new SpawnParameters{Position = player.Position});
                }
            }
        }

        if (!_dash.IsDashing)
        {
            for (var i = 0; i < _world.Entities.Length; i++)
            {
                var entity = _world.Entities[i];
                if (entity.HasTag(Tag.Player))
                {
                    _playerMoveVector = inputVector;
                }
            }
        }
    }

    private void ExplodeAt(Vector2 position, float explosionRadius, int damage, Color? color = null)
    {
        var vfx = new VfxInstance(position, 64);
        vfx.Angle.Value = Client.Random.Dirty.NextFloat(0, MathF.PI * 2);
        vfx.BaseColor = color ?? Color.White;
        vfx.Tween
            .Add(vfx.Radius.TweenTo(explosionRadius * 1.2f, 0.15f, Ease.Linear))
            .Add(vfx.Opacity.TweenTo(0, 0.1f, Ease.Linear))
            ;
        _vfx.Add(vfx);
        _world.SpawnBullet(() => new Entity()
        {
            Velocity = new Vector2(0,0),
            CollideRadius = explosionRadius,
            Tags = Tag.Player,
            HitsMultipleTargetsOnSameFrame = true,
            Hide = true,
        }, new SpawnParameters() {Position = position, HitDamage = damage });
    }

    public override void Update(float dt)
    {
        if (!Runtime.Window.IsInFocus)
        {
            return;
        }

        if (_freezeTimer > 0)
        {
            _freezeTimer -= dt;
            return;
        }

        _elapsedTime += dt;


        if (_gameOver)
        {
            EnemyJeers();
        }

        _introTween.Update(dt);

        if (!_introTween.IsDone())
        {
            return;
        }

        if (_levelUpScreen != null && _levelUpScreen.IsActive)
        {
            _levelUpScreen.Update(dt);
            return;
        }

        SpinSpinningBullets(dt);
        
        _dash.Update(dt);
        if (_dash.IsDashing)
        {
            var player = _world.Entities[_world.GetPlayerIndex()];
            var vfx = new VfxInstance(player.Position - _playerMoveVector * 5, player.CollideRadius * 2);
            vfx.BaseColor = Color.LightBlue;
            vfx.SpriteName = "game/falling-leaf";
            vfx.Angle.Value = Client.Random.Dirty.NextFloat() * MathF.PI * 2;
            vfx.Tween.Add(
                new MultiplexTween()
                    .AddChannel(vfx.Angle.TweenTo(MathF.PI * 2, 3f, Ease.CubicFastSlow))
                    .AddChannel(
                        new SequenceTween()
                            .Add(new WaitSecondsTween(Client.Random.Dirty.NextFloat(0.25f, 0.5f)))
                            .Add(vfx.Radius.TweenTo(0f, 0.5f, Ease.CubicFastSlow))
                        
                        )
                    .AddChannel(vfx.Position.TweenTo(vfx.Position + Client.Random.Dirty.NextNormalVector2() * 100, 0.5f, Ease.CubicFastSlow))
            );
            
            _vfx.Add(vfx);
        }
        
        _vfx.Update(dt);
        UpdateAllHurtTimers(dt);
        PlayMovementSounds();

        _world.SpawnFromBuffer();
        if (!_gameOver)
        {
            if (!StoryBeat_NoEnemies)
            {
                var multiplier = 1f;

                if (StoryBeat_Monsters || StoryBeat_Weapons)
                {
                    multiplier = 3;
                }
                _waves.Update(dt * multiplier);
            }

            MovePlayer(dt);
            MoveEnemiesTowardsPlayer(dt);

            if (_introTween.IsDone())
            {
                AdjustCamera(dt);
            }

            if (!StoryBeat_Monsters)
            {
                RunPlayerAbilities(dt);
            }

            CalculateBulletCollision();
            CollectExp(dt);
            CalculatePlayerHurt(dt);
            TickBombsAndExplode(dt);
        }

        ConstrainAllEntitiesToLevel();
        MoveThingsAlongVelocity(dt);
        KillDeadThings();
    }

    private void TickBombsAndExplode(float dt)
    {
        for (int i = 0; i < _world.Entities.Length; i++)
        {
            if (_world.Entities[i].HasTag(Tag.Bomb))
            {
                _world.Entities[i].DetonationTimer -= dt;

                if (_world.Entities[i].DetonationTimer < 0)
                {
                    _world.DestroyEntity(i);
                    ExplodeAt(_world.Entities[i].Position, _dashBombs.ExplosionRadius, _dashBombs.Damage, Color.White);
                }
            }
        }
    }

    private void SpinSpinningBullets(float dt)
    {
        for (int i = 0; i < _world.Bullets.Length; i++)
        {
            if (_world.Bullets[i].Spin)
            {
                _world.Bullets[i].Angle += dt * 30;
            }
        }
    }

    private void PlayMovementSounds()
    {
        if (!MathUtils.IsVerySmall(_playerMoveVector) && !_gameOver)
        {
            GGJSoundPlayer.PlayIfAble("game/step", 0.15f);
        }
    }

    private void UpdateAllHurtTimers(float dt)
    {
        for (var i = 0; i < _world.Entities.Length; i++)
        {
            if (_world.Entities[i].IsActive && _world.Entities[i].HurtTimer > 0)
            {
                _world.Entities[i].HurtTimer -= dt;
            }
        }
    }

    private void EnemyJeers()
    {
        for (var i = 0; i < _world.Entities.Length; i++)
        {
            if (_world.Entities[i].HasTag(Tag.Enemy))
            {
                _world.Entities[i].Angle = MathF.Sin(_elapsedTime * 20f + i) * 0.3f;
            }
        }
    }

    private void CalculatePlayerHurt(float dt)
    {
        if (_playerHitCooldown > 0 || _dash.IsDashing)
        {
            _playerHitCooldown -= dt;
            return;
        }

        var playerIndex = _world.GetPlayerIndex();
        var player = _world.Entities[playerIndex];
        foreach (var entity in _world.Entities)
        {
            if (entity.HasTag(Tag.Enemy))
            {
                // THIS USES PLAYER HURT RADIUS
                if (_world.IsColliding(player.Position, player.HurtRadius, entity.Position, entity.CollideRadius))
                {
                    if (_thorns.IsUnlocked)
                    {
                        ExplodeAt(player.Position, _thorns.ExplosionRadius, _thorns.Damage, Color.LightGreen);
                    }
                    
                    GGJSoundPlayer.Play("engine/ouch");
                    _playerHitCooldown = 1.25f;
                    _freezeTimer = 0.1f;
                    _world.Entities[playerIndex].Health--;
                    break;
                }
            }
        }
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
                    if (!_gameOver)
                    {
                        _freezeTimer = 0.5f;
                        _introTween.Add(new CallbackTween(() =>
                        {
                            _gameOver = true;
                        }));
                        _introTween.Add(_fadeOverlayOpacity.TweenTo(0.25f, 1f, Ease.Linear));
                        MusicPlayer.FadeOut();
                    }
                }
                else
                {
                    GGJSoundPlayer.Play("game/burp", 0.5f, -1f);
                    _world.DestroyEntity(i);
                    
                    for(float angle = 0f; angle < MathF.PI*2; angle += MathF.PI / 4)
                    {
                        var vfx = new VfxInstance(_world.Entities[i].Position, _world.Entities[i].CollideRadius);
                        vfx.Tween
                            .Add(
                                new MultiplexTween()
                                    .AddChannel(vfx.Position.TweenTo(vfx.Position + Vector2Extensions.Polar(100, angle), 0.25f, Ease.CubicFastSlow))
                                    .AddChannel(vfx.Opacity.TweenTo(0f, 0.15f, Ease.Linear))
                                    .AddChannel(vfx.Radius.TweenTo(10f, 0.15f, Ease.CubicFastSlow))
                            )
                            ;
                        _vfx.Add(vfx);
                    }

                    if (!StoryBeat_Weapons)
                    {
                        _world.SpawnEntity(EntityTemplate.Exp,
                            new SpawnParameters {Position = _world.Entities[i].Position});
                    }
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
                if (bullet.HitsMultipleTargetsOnSameFrame)
                {
                    for (var entityIndex = 0; entityIndex < _world.Entities.Length; entityIndex++)
                    {
                        var entity = _world.Entities[entityIndex];
                        if (IsBulletHittingEntity(entityIndex, bullet, entity))
                        {
                            ResolveBulletHitsEntity(bulletIndex, entityIndex, bullet);
                        }
                    }
                    
                    _world.DestroyBullet(bulletIndex);
                }
                else
                {
                    var foundEntityIndex = -1;
                    for (var entityIndex = 0; entityIndex < _world.Entities.Length; entityIndex++)
                    {
                        var entity = _world.Entities[entityIndex];
                        if (IsBulletHittingEntity(entityIndex, bullet, entity))
                        {
                            foundEntityIndex = entityIndex;
                            break;
                        }
                    }

                    if (foundEntityIndex != -1)
                    {
                        ResolveBulletHitsEntity(bulletIndex, foundEntityIndex, bullet);
                    }
                }
                
            }
        }
    }

    private void ResolveBulletHitsEntity(int bulletIndex, int foundEntity, Entity bullet)
    {
        GGJSoundPlayer.Play("game/hit", 0.5f, (Client.Random.Dirty.NextFloat() - 0.5f) / 2f);

        _world.Bullets[bulletIndex].CleaveCount--;
        if (_world.Bullets[bulletIndex].CleaveCount <= 0 && !_world.Bullets[bulletIndex].HitsMultipleTargetsOnSameFrame)
        {
            _world.DestroyBullet(bulletIndex);
        }

        _world.Entities[foundEntity].Health -= bullet.HitDamage;
        _world.Entities[foundEntity].HurtTimer = 0.25f;
    }

    private bool IsBulletHittingEntity(int entityIndex, Entity bullet, Entity entity)
    {
        // THIS USES HURT RADIUS
        return _world.Entities[entityIndex].HurtTimer <= 0 && _world.AreEnemies(bullet, entity) &&
               _world.IsColliding(bullet.Position, bullet.CollideRadius,
                   entity.Position, entity.HurtRadius);
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
            if (ability.IsUnlocked)
            {
                ability.Update(dt, _world, _world.Entities[_world.GetPlayerIndex()]);
            }
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
        else if (_dash.IsDashing)
        {
            _playerMoveDampen = 0f;
        }
        else
        {
            _playerMoveDampen = 1f;
        }

        var playerIndex = _world.GetPlayerIndex();
        
        if (_playerMoveVector.X < 0)
        {
            _world.Entities[playerIndex].FlipX = true;
        }
        
        if (_playerMoveVector.X > 0)
        {
            _world.Entities[playerIndex].FlipX = false;
        }


        var speed = _world.Entities[playerIndex].Speed;

        if (_dash.IsDashing)
        {
            speed = _dash.Speed;
        }

        _world.Entities[playerIndex].Position += _playerMoveVector * speed * dt * 60;
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

        // dislodge from each other
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
                    if (_world.Entities[b].HasTag(Tag.Exp) && _world.IsWithinDistance(a, b, _stats.ExpRadius.Value))
                    {
                        _world.Entities[b].Position = Vector2.Lerp(_world.Entities[b].Position, primary.Position,
                            0.25f * dt * 60f);

                        if (_world.IsColliding(a, b))
                        {
                            _world.DestroyEntity(b);
                            GGJSoundPlayer.Play("game/blem", 0.75f);
                            
                            if ((bool) _levelUpScreen?.IncrementExp())
                            {
                                _world.Entities[a].Health = _world.Entities[a].MaxHealth;
                                _dash.FreeCooldown();
                            }
                        }
                    }
                }
            }
        }
    }

    public override void Draw(Painter painter)
    {
        var scale = 0.25f;

        painter.Clear(Color.Green.DesaturatedBy(0.8f).DimmedBy(0.2f));
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

                if (entity.HurtTimer > 0)
                {
                    bodyColor = Color.Red.BrightenedBy(0.5f);
                }

                if (entity.HasTag(Tag.Player))
                {
                    bodyColor = ColorExtensions.FromRgbHex(0xff8000);

                    if (_dash.IsDashing)
                    {
                        bodyColor = Color.LightBlue;
                    }
                }

                if ((entity.HasTag(Tag.Enemy) || entity.HasTag(Tag.Bomb)) && !string.IsNullOrEmpty(entity.Sprite))
                {
                    var texture = Client.Assets.GetTexture(entity.Sprite);
                    var localScale = entity.CollideRadius / texture.Width * 2;
                    painter.DrawAtPosition(texture, entity.Position,
                        new Scale2D(localScale),
                        new DrawSettings
                        {
                            Origin = DrawOrigin.Center,
                            Color = bodyColor,
                            Angle = entity.Angle
                        });
                }
                else if (entity.HasTag(Tag.Player))
                {
                    if (!_gameOver)
                    {
                        Texture2D texture;

                        if (MathUtils.IsVerySmall(_playerMoveVector))
                        {
                            texture = Client.Assets.GetTexture("game/person");
                        }
                        else if (_dash.IsDashing)
                        {
                            texture = Client.Assets.GetTexture("game/sprint");
                        }
                        else
                        {
                            texture = Client.Assets.GetTexture("game/walk");
                        }

                        if (_playerHitCooldown > 0)
                        {
                            _playerShouldFlicker = !_playerShouldFlicker;
                        }
                        else
                        {
                            _playerShouldFlicker = false;
                        }

                        var flickerOpacity = 1f;
                        if (_playerShouldFlicker)
                        {
                            bodyColor = Color.Red;
                            flickerOpacity = 0.25f;
                        }

                        if (_introTween.IsDone() && !StoryBeat_NoEnemies)
                        {
                            // health bar
                            painter.DrawRectangle(
                                new RectangleF(
                                    entity.Position - new Vector2(texture.Width / 2f * scale,
                                        texture.Height / 2f * scale + 40),
                                    new Vector2(texture.Width * scale, 20)),
                                new DrawSettings {Color = Color.DarkRed.DimmedBy(0.25f), Depth = Depth.Back});

                            painter.DrawRectangle(
                                new RectangleF(
                                    entity.Position - new Vector2(texture.Width / 2f * scale,
                                        texture.Height / 2f * scale + 40),
                                    new Vector2(texture.Width * scale * ((float) entity.Health / entity.MaxHealth),
                                        20)),
                                new DrawSettings {Color = Color.DarkRed, Depth = Depth.Back - 10});

                            if (!StoryBeat_Monsters)
                            {
                                // dash
                                painter.DrawRectangle(
                                    new RectangleF(
                                            entity.Position - new Vector2(texture.Width / 2f * scale,
                                                texture.Height / 2f * scale + 40),
                                            new Vector2(
                                                texture.Width * scale * Math.Min(1,
                                                    1 - _dash.CurrentCooldown / _dash.TotalCooldown), 10))
                                        .Moved(new Vector2(0, 20)),
                                    new DrawSettings {Color = Color.LightBlue, Depth = Depth.Back - 10});
                            }
                        }

                        var angle = MathF.Sin(_playerMoveTimer * 30f) * _playerMoveDampen / 5;
                        if(_dash.IsDashing)
                        {
                            angle = _playerMoveVector.X * _dash.Speed / 60f;
                        }

                        painter.DrawAtPosition(texture, entity.Position + new Vector2(0, texture.Height / 2f * scale),
                            new Scale2D(scale),
                            new DrawSettings
                            {
                                Flip = new XyBool(entity.FlipX, false),
                                Origin = new DrawOrigin(new Vector2(texture.Width / 2f, texture.Height)),
                                Angle = angle,
                                Color = bodyColor.WithMultipliedOpacity(flickerOpacity)
                            });
                    }
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
            if (bullet.IsActive && !bullet.Hide)
            {
                if (bullet.Sprite != null)
                {
                    var texture2D = Client.Assets.GetTexture(bullet.Sprite);
                    var angle = bullet.Angle;
                    if (bullet.RotateWithVelocity)
                    {
                        angle = bullet.Angle + bullet.Velocity.GetAngleFromUnitX();
                    }
                    
                    painter.DrawAtPosition(texture2D, bullet.Position, new Scale2D(scale),
                        new DrawSettings
                        {
                            Origin = DrawOrigin.Center, Color = color,
                            Angle = angle
                        });
                }
                else
                {
                    painter.DrawCircle(bullet.Position, bullet.CollideRadius, new DrawSettings {Color = color});
                }
            }
        }

        painter.EndSpriteBatch();
        
        
        painter.BeginSpriteBatch(_camera.CanvasToScreen);
        _vfx.Draw(painter);
        painter.EndSpriteBatch();

        var smallFont = Client.Assets.GetFont("game/font", 80);
        var bigFont = Client.Assets.GetFont("game/font", 256);

        if (!_gameOver && !StoryBeat_NoEnemies && !StoryBeat_Monsters && !StoryBeat_Weapons)
        {
            painter.BeginSpriteBatch();
            var insetScreen = Runtime.Window.RenderResolution.ToRectangleF().Inflated(0, -100);
            insetScreen = insetScreen.Moved(-insetScreen.Size.JustY() * _barOffsetPercent);

            var y = insetScreen.Top - smallFont.Height;

            var expBar = RectangleF.FromSizeAlignedWithin(insetScreen, new Vector2(insetScreen.Width - 100, 50),
                Alignment.TopCenter);

            painter.DrawStringWithinRectangle(smallFont, $"Wave {_waves.CurrentIndex + 1}",
                new RectangleF(new Vector2(expBar.X, y), new Vector2(expBar.Width, smallFont.Height)),
                Alignment.BottomCenter,
                new DrawSettings());
            painter.DrawRectangle(expBar, new DrawSettings {Color = Color.Yellow.DimmedBy(0.5f), Depth = Depth.Back});

            var percent = _levelUpScreen?.Percent() ?? 0;
            var expFill = expBar.ResizedOnEdge(RectEdge.Right, new Vector2(-expBar.Width * (1 - percent), 0));
            painter.DrawRectangle(expFill, new DrawSettings {Color = Color.Yellow, Depth = Depth.Middle});
            painter.EndSpriteBatch();

            if (_levelUpScreen != null && _levelUpScreen.IsActive)
            {
                painter.BeginSpriteBatch();
                painter.DrawRectangle(Runtime.Window.RenderResolution.ToRectangleF(),
                    new DrawSettings {Color = Color.Black.WithMultipliedOpacity(0.75f)});
                painter.EndSpriteBatch();

                painter.BeginSpriteBatch();
                _levelUpScreen.Draw(painter);
                painter.EndSpriteBatch();
            }
        }

        painter.BeginSpriteBatch();
        painter.DrawRectangle(Runtime.Window.RenderResolution.ToRectangleF(),
            new DrawSettings {Color = Color.Black.WithMultipliedOpacity(_fadeOverlayOpacity)});
        painter.EndSpriteBatch();

        var textOpacity = _overlayTextOpacity;
        if (_gameOver)
        {
            painter.BeginSpriteBatch();
            painter.DrawStringWithinRectangle(bigFont, "GAME OVER", Runtime.Window.RenderResolution.ToRectangleF(),
                Alignment.Center,
                new DrawSettings {Angle = MathF.Sin(_elapsedTime) * 0.1f, Origin = DrawOrigin.Center, Color = Color.White.WithMultipliedOpacity(textOpacity)});
            painter.DrawStringWithinRectangle(smallFont, "\n\n\n\nPress Esc to Restart",
                Runtime.Window.RenderResolution.ToRectangleF(), Alignment.Center, new DrawSettings
                {
                    Angle = MathF.Sin(_elapsedTime) * 0.1f,
                    Origin = DrawOrigin.Center,
                    Color = Color.White.WithMultipliedOpacity(textOpacity)
                });
            painter.EndSpriteBatch();
        }

        if (StoryBeat_NoEnemies && _elapsedTime > 5)
        {
            painter.BeginSpriteBatch();
            painter.DrawStringWithinRectangle(smallFont, "Press Esc to Exit",
                Runtime.Window.RenderResolution.ToRectangleF(), Alignment.Center, new DrawSettings
                {
                    Angle = MathF.Sin(_elapsedTime) * 0.1f,
                    Origin = DrawOrigin.Center,
                    Color = Color.White.WithMultipliedOpacity(textOpacity)
                });
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