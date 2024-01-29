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
    private AnimatedText _animatedText = null!;
    private int _currentPage;
    private Cutscene _cutscene = new();
    private Face _face = null!;

    public GGJCartridge(HostRuntime hostRuntime) : base(hostRuntime)
    {
        _hostRuntime = hostRuntime;
    }

    public override CartridgeConfig CartridgeConfig { get; } = new(new Point(1920, 1080));

    public override void OnCartridgeStarted()
    {
        var dialogues = new List<string>
        {
            "Dynamic/intro.json",
            "Dynamic/intro2.json",
            "Dynamic/intro3.json",
            "Dynamic/intro4.json",
            "Dynamic/intro5.json",
        };

        var dialoguePath = "Dynamic/interlude.json";
        if (_hostRuntime.HostCartridge.IsPostGame)
        {
            dialoguePath = "Dynamic/postgame_interlude.json";
        }
        else if (_hostRuntime.HostCartridge.HasWon)
        {
            dialoguePath = "Dynamic/ending.json";
            _hostRuntime.HostCartridge.IsPostGame = true;
        }
        
        if (dialogues.IsValidIndex(_hostRuntime.HostCartridge.StoryProgress))
        {
            dialoguePath = dialogues[_hostRuntime.HostCartridge.StoryProgress];
        }
        
        _cutscene = JsonConvert.DeserializeObject<Cutscene>(
            Client.Debug.RepoFileSystem.ReadFile(dialoguePath))!;

        _animatedText = new AnimatedText
        {
            Rectangle = Runtime.Window.RenderResolution.ToRectangleF().Inflated(-100, -100)
        };

        _face = new Face
        {
            Position = Runtime.Window.RenderResolution.ToRectangleF().Center,
            Radius = 80
        };

        RunPage(_cutscene.Dialogue[_currentPage]);

        // initialize first frame
        _tween.Update(0);
        
        MusicPlayer.FadeOut();
    }

    private void RunPage(DialogueLine line)
    {
        var hasText = !string.IsNullOrEmpty(line.Text);
        _tween.Clear();
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

        if (input.Keyboard.GetButton(Keys.Space).WasPressed)
        {
            if (_cutscene.Dialogue.IsValidIndex(_currentPage))
            {
                NextPage();
            }
            else
            {
                _tween.Add(new CallbackTween(() => { LoadGame(); }));
            }
        }
    }

    private void NextPage()
    {
        _currentPage++;

        if (_cutscene.Dialogue.IsValidIndex(_currentPage))
        {
            RunPage(_cutscene.Dialogue[_currentPage]);
        }
    }

    public override void Update(float dt)
    {
        _tween.Update(dt);

        if (!_cutscene.Dialogue.IsValidIndex(_currentPage))
        {
            LoadGame();
            return;
        }

        if (string.IsNullOrEmpty(_cutscene.Dialogue[_currentPage].Text) && _tween.IsDone())
        {
            if (_cutscene.Dialogue.IsValidIndex(_currentPage))
            {
                NextPage();
            }
        }
    }

    public override void Draw(Painter painter)
    {
        painter.BeginSpriteBatch();
        _face?.Draw(painter);
        painter.EndSpriteBatch();
        
        painter.BeginSpriteBatch();
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
            return new WaitUntilTween(() => true);
        }

        return new WaitUntilTween(() => _input.Keyboard.GetButton(key).WasPressed);
    }
}
