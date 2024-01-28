using System.Collections.Generic;
using System.Linq;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Input;
using ExplogineMonoGame.Layout;
using ExplogineMonoGame.Rails;
using ExTween;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GGJ2024;

public class LevelUpOverlay : IUpdateInputHook, IDrawHook, IUpdateHook
{
    private readonly Font _descriptionFont;
    private readonly Font _headerFont;
    private readonly RectangleF _screen;
    private readonly Upgrades _upgrades;
    private readonly Font _titleFont;
    private readonly TweenableFloat _buttonActivePercent = new(1f);
    private readonly TweenableFloat _titleActivePercent = new(1f);
    private readonly SequenceTween _tween = new();
    private List<HoverState> _hoverStates = new() { new HoverState(), new HoverState(), new HoverState() };
    private int _expToNextLevel = 5;
    private int _exp;
    private List<LevelUpReward> _currentRewards = new();

    public LevelUpOverlay(RectangleF screen, Upgrades upgrades)
    {
        _screen = screen;
        _upgrades = upgrades;

        _headerFont = Client.Assets.GetFont("game/font", 200);
        _titleFont = Client.Assets.GetFont("game/font", 90);
        _descriptionFont = Client.Assets.GetFont("game/font", 64);
    }

    public bool IsActive { get; private set; }

    public void Draw(Painter painter)
    {
        var layout = Layout();
        var header = layout.FindElement("Header");
        var buttons = layout.FindElements("Button");

        painter.DrawStringWithinRectangle(_headerFont.WithFontSize((int) (_headerFont.FontSize +
                                                                          _headerFont.FontSize * _buttonActivePercent /2f)),
            "Level Up!", header.Rectangle.Moved(-new Vector2(-_screen.Height * _titleActivePercent).JustY()),
            Alignment.BottomCenter, new DrawSettings {Color = Color.Yellow});

        for (var i = 0; i < buttons.Count; i++)
        {
            var buttonRect = buttons[i];
            DrawButton(painter, buttonRect.Rectangle.Moved(new Vector2(_screen.Height * _buttonActivePercent).JustY()), _hoverStates[i].IsHovered, _currentRewards[i]);
        }
    }

    public void Update(float dt)
    {
        _tween.Update(dt);
    }

    public void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        if (Client.Debug.IsPassiveOrActive)
        {
            if (input.Keyboard.GetButton(Keys.Q, true).WasPressed)
            {
                // debug rest
                Show();
            }
        }

        if (_tween.IsDone())
        {
            hitTestStack.AddInfiniteZone(Depth.Back, new HoverState());
            var layout = Layout();
            var list = layout.FindElements("Button");
            for (var i = 0; i < list.Count; i++)
            {
                var button = list[i];
                var rectangle = button.Rectangle;
                hitTestStack.AddZone(rectangle, Depth.Middle, _hoverStates[i]);
            }
            
            if (input.Mouse.GetButton(MouseButton.Left).WasPressed)
            {
                for (int i = 0; i < _hoverStates.Count; i++)
                {
                    if (_hoverStates[i].IsHovered)
                    {
                        SelectUpgrade(i);
                    }
                }
            }
        }
        else
        {
            foreach (var hoverState in _hoverStates)
            {
                hoverState.Unset();
            }
        }
    }

    private void SelectUpgrade(int i)
    {
        _currentRewards[i].Buy();
        _expToNextLevel = (int) (_expToNextLevel * 1.25f);
        _exp = 0;
        Hide();
    }

    private void Hide()
    {
        MusicPlayer.FadeToMain();
        _tween.Clear();
        _tween.Add(new MultiplexTween()
            .AddChannel(_titleActivePercent.TweenTo(-1, 0.5f, Ease.CubicFastSlow))
            .AddChannel(_buttonActivePercent.TweenTo(1, 0.5f, Ease.CubicFastSlow))
        );
        _tween.Add(new CallbackTween(() =>
        {
            IsActive = false;
        }));
    }

    public void Show()
    {
        IsActive = true;
        _titleActivePercent.Value = 1f;
        _buttonActivePercent.Value = 1f;
        _tween.Clear();
        MusicPlayer.FadeToMenu();
        _tween.Add(_titleActivePercent.TweenTo(0, 0.5f, Ease.CubicFastSlow));
        _tween.Add(_buttonActivePercent.TweenTo(0, 0.5f, Ease.CubicFastSlow));
        _currentRewards = _upgrades.Pull(3).ToList();
    }

    public LayoutArrangement Layout()
    {
        var root = new LayoutBuilder(new Style
        {
            Alignment = Alignment.Center, Margin = new Vector2(50, 150), Orientation = Orientation.Vertical,
            PaddingBetweenElements = 10
        });

        root.Add(L.FillHorizontal("Header", 128));
        var buttons = root.AddGroup(new Style {PaddingBetweenElements = 50, Orientation = Orientation.Horizontal},
            L.FillBoth());

        for (var i = 0; i < 3; i++)
        {
            buttons.Add(L.FillBoth("Button"));
        }

        return root.Bake(_screen);
    }

    private LayoutArrangement CreateButtonLayout(RectangleF rectangle)
    {
        var root = new LayoutBuilder(new Style
        {
            Alignment = Alignment.Center, Margin = Vector2.Zero, Orientation = Orientation.Vertical,
            PaddingBetweenElements = 10
        });

        var innerGroup = root.AddGroup(new Style
        {
            Margin = new Vector2(20, 20), Alignment = Alignment.Center, Orientation = Orientation.Vertical
        }, L.FillBoth());
        innerGroup.Add(L.FillHorizontal("Header", 100));

        var iconSize = 200;
        var group = innerGroup.AddGroup(new Style {Alignment = Alignment.Center}, L.FillHorizontal(iconSize));
        group.Add(L.FixedElement("Icon", iconSize, iconSize));
        innerGroup.Add(L.FillBoth("Description"));
        return root.Bake(rectangle);
    }

    private void DrawButton(Painter painter, RectangleF button, bool isHovered, LevelUpReward reward)
    {
        if (isHovered)
        {
            button = button.Inflated(10, 10);
        }
        
        painter.DrawRectangle(button, new DrawSettings {Color = isHovered ? Color.Green : Color.Green.DimmedBy(0.25f), Depth = Depth.Back});
        var buttonLayout = CreateButtonLayout(button);
        var headerArea = buttonLayout.FindElement("Header");
        var iconArea = buttonLayout.FindElement("Icon");
        var descriptionArea = buttonLayout.FindElement("Description");

        painter.DrawStringWithinRectangle(_titleFont, reward.Title, headerArea, Alignment.BottomCenter, new DrawSettings());
        painter.DrawAsRectangle(Client.Assets.GetTexture(reward.IconName), iconArea, new DrawSettings());
        painter.DrawStringWithinRectangle(_descriptionFont,
            reward.Description, descriptionArea, Alignment.TopCenter,
            new DrawSettings());
    }

    public bool IncrementExp()
    {
        _exp++;

        if (_exp >= _expToNextLevel)
        {
            Show();
            return true;
        }

        return false;
    }

    public float Percent()
    {
        return (float) _exp / _expToNextLevel;
    }
}
