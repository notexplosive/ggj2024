using System.Collections.Generic;
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
    private readonly Font _titleFont;
    private readonly TweenableFloat _buttonActivePercent = new(1f);
    private readonly TweenableFloat _titleActivePercent = new(1f);
    private readonly SequenceTween _tween = new();
    private List<HoverState> _hoverStates = new() { new HoverState(), new HoverState(), new HoverState() };
    private int _expToNextLevel = 10;
    private int _exp;

    public LevelUpOverlay(RectangleF screen)
    {
        _screen = screen;

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
            DrawButton(painter, buttonRect.Rectangle.Moved(new Vector2(_screen.Height * _buttonActivePercent).JustY()), _hoverStates[i].IsHovered);
        }
    }

    public void Update(float dt)
    {
        _tween.Update(dt);
    }

    public void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        if (Client.Debug.IsPassiveOrActive && input.Keyboard.GetButton(Keys.Escape, true).WasPressed)
        {
            // debug rest
            Show();
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
        // apply upgrade
        _expToNextLevel = (int) (_expToNextLevel * 1.5f);
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

    private void DrawButton(Painter painter, RectangleF button, bool isHovered)
    {
        if (isHovered)
        {
            button = button.Inflated(10, 10);
        }
        
        painter.DrawRectangle(button, new DrawSettings {Color = isHovered ? Color.Blue : Color.DarkBlue, Depth = Depth.Back});
        var buttonLayout = CreateButtonLayout(button);
        var headerArea = buttonLayout.FindElement("Header");
        var iconArea = buttonLayout.FindElement("Icon");
        var descriptionArea = buttonLayout.FindElement("Description");

        painter.DrawStringWithinRectangle(_titleFont, "Header", headerArea, Alignment.BottomCenter, new DrawSettings());
        painter.DrawStringWithinRectangle(_descriptionFont,
            "Lorem ipsum dolor sit amet sadg asdf dasdf waef awef awef awef.", descriptionArea, Alignment.TopCenter,
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
