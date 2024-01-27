using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Rails;
using ExTween;
using ExTweenMonoGame;
using Microsoft.Xna.Framework;

namespace GGJ2024;

public class AnimatedText : IDrawHook
{
    private readonly IndirectFont _font = new("game/font", 128);

    public float Opacity { get; set; }
    public RectangleF Rectangle { get; set; }
    public string? Text { get; set; }
    public Vector2 Offset { get; set; }

    public Tweenable Animator { get; }

    public AnimatedText()
    {
        Animator = new(this);
    }

    public void Draw(Painter painter)
    {
        var text = FormattedText.FromFormatString(_font, Color.White, Text ?? string.Empty);
        painter.DrawFormattedStringWithinRectangle(text, Rectangle.Moved(Offset), Alignment.BottomCenter,
            new DrawSettings {Color = Color.White.WithMultipliedOpacity(Opacity)});
    }

    public class Tweenable
    {
        private readonly AnimatedText _animatedText;

        public Tweenable(AnimatedText animatedText)
        {
            _animatedText = animatedText;
            Opacity = new TweenableFloat(() => animatedText.Opacity, val => animatedText.Opacity = val);
            Offset = new TweenableVector2(()=> animatedText.Offset, val => animatedText.Offset = val);
        }

        private TweenableFloat Opacity { get; }
        private TweenableVector2 Offset { get; }

        public ITween AppearWithText(string text, float appearDuration)
        {
            return new SequenceTween()
                    .Add(new DynamicTween(() =>
                    {
                        if (_animatedText.Opacity > 0)
                        {
                            return FadeOut(0.25f);
                        }

                        return new EmptyTween();
                    }))
                    .Add(new CallbackTween(() =>
                    {
                        _animatedText.Text = text;
                    }))
                    .Add(Opacity.CallbackSetTo(0))
                    .Add(Offset.CallbackSetTo(Vector2.Zero))
                    .Add(new MultiplexTween()
                        .AddChannel(Opacity.TweenTo(1f, appearDuration, Ease.Linear))
                        .AddChannel(
                            new SequenceTween()
                                .Add(Offset.TweenTo(new Vector2(0, -50), appearDuration / 2f, Ease.CubicFastSlow))
                                .Add(Offset.TweenTo(new Vector2(0, 10f), appearDuration / 2f, Ease.CubicSlowFast))
                                .Add(Offset.TweenTo(Vector2.Zero, appearDuration / 4f, Ease.CubicSlowFast))
                                )
                    )
                ;
        }

        public ITween FadeOut(float fadeDuration)
        {
            return new SequenceTween()
                .Add(Opacity.TweenTo(0f, fadeDuration, Ease.Linear));
        }
    }
}
