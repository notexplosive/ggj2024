using System.Collections.Generic;
using ExplogineCore;
using ExplogineMonoGame;
using ExplogineMonoGame.Cartridges;
using ExplogineMonoGame.Data;
using ExTween;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;

namespace GGJ2024;

public class GGJCartridge : BasicGameCartridge
{
    private readonly HostRuntime _hostRuntime;
    private readonly SequenceTween _tween = new();
    private AnimatedText? _animatedText;
    private AsyncInput? _asyncInput;
    private Cutscene _cutscene = new();
    private Face? _face;

    public GGJCartridge(HostRuntime hostRuntime) : base(hostRuntime)
    {
        _hostRuntime = hostRuntime;
    }

    public override CartridgeConfig CartridgeConfig { get; } = new(new Point(1920, 1080));

    public override void OnCartridgeStarted()
    {
        _cutscene = JsonConvert.DeserializeObject<Cutscene>(
            Client.Debug.RepoFileSystem.ReadFile("Dynamic/intro.json"))!;

        _animatedText = new AnimatedText
        {
            Rectangle = Runtime.Window.RenderResolution.ToRectangleF().Inflated(0, -100)
        };
        _asyncInput = new AsyncInput();

        _face = new Face
        {
            Position = Runtime.Window.RenderResolution.ToRectangleF().Center,
            Radius = 80
        };

        foreach (var line in _cutscene.Dialogue)
        {
            var hasText = !string.IsNullOrEmpty(line.Text);
            _tween.Add(
                new MultiplexTween()
                    .AddChannel(new DynamicTween(() =>
                    {
                        if (hasText)
                        {
                            return _animatedText.Animator.AppearWithText(line.Text!, 0.25f);
                        }

                        return _animatedText.Animator.FadeOut(0.25f);
                    }))
                    .AddChannel(_face.DoAnimation(line.Animation, line.Duration))
                    .AddChannel(_face.Animator.HeadTilt.TweenTo(line.Tilt, line.Duration, Ease.CubicFastSlow))
                    .AddChannel(_face.Animator.Position.TweenTo(
                        line.Position.ToVector2().StraightMultiply(Runtime.Window.RenderResolution), line.Duration,
                        Ease.CubicFastSlow))
            );

            if (hasText)
            {
                _tween.Add(_asyncInput.WaitForKeypress(Keys.Space));
            }
        }

        _tween.Add(new CallbackTween(() => LoadGame()));
    }

    private void LoadGame()
    {
        _hostRuntime.HostCartridge.RegenerateCartridge<VampireCartridge>();
        _hostRuntime.HostCartridge.SwapTo<VampireCartridge>();
    }

    public override void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        if (input.Keyboard.GetButton(Keys.Escape).WasPressed && Client.Debug.IsPassiveOrActive)
        {
            LoadGame();
        }
        
        _asyncInput?.UpdateInput(input, hitTestStack);
    }

    public override void Update(float dt)
    {
        _tween.Update(dt);
    }

    public override void Draw(Painter painter)
    {
        painter.BeginSpriteBatch();
        _face?.Draw(painter);
        _animatedText?.Draw(painter);
        painter.EndSpriteBatch();
    }

    public override void AddCommandLineParameters(CommandLineParametersWriter parameters)
    {
    }

    public override IEnumerable<ILoadEvent> LoadEvents(Painter painter)
    {
        yield break;
    }

    public override void Unload()
    {
    }

    public override void OnHotReload()
    {
    }
}

public class AsyncInput
{
    private ConsumableInput? _input;

    public void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        _input = input;
    }

    public WaitUntilTween WaitForKeypress(Keys key)
    {
        if (_input == null)
        {
            return new WaitUntilTween(()=>true);
        }
        return new WaitUntilTween(() => _input.Keyboard.GetButton(key).WasPressed);
    }
}
