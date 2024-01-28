using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExTween;
using ExTweenMonoGame;
using Microsoft.Xna.Framework;

namespace GGJ2024;

public class VfxInstance
{
    public VfxInstance(Vector2 position, float radius)
    {
        Radius.Value = radius;
        Position.Value = position;
    }

    public string? SpriteName { get; set; }
    public TweenableFloat Radius { get; } = new(100);
    public TweenableVector2 Position { get; } = new();
    public TweenableFloat Angle { get; } = new();
    public TweenableFloat Opacity { get; } = new(1f);
    public Color BaseColor { get; set; } = Color.White;
    public SequenceTween Tween { get; } = new();
    
    public void Draw(Painter painter)
    {
        if (SpriteName != null)
        {
            var texture = Client.Assets.GetTexture(SpriteName);
            var scale =  Radius/texture.Width;
            painter.DrawAtPosition(texture, Position, new Scale2D(scale), new DrawSettings{Origin = DrawOrigin.Center, Angle = Angle, Color = BaseColor.WithMultipliedOpacity(Opacity)});
        }
        else
        {
            painter.DrawCircle(Position, Radius, new DrawSettings{Color = BaseColor.WithMultipliedOpacity(Opacity)});
        }
    }

    public void Update(float dt)
    {
        Tween.Update(dt);
    }

    public bool ShouldDie()
    {
        return Tween.IsDone();
    }
}
