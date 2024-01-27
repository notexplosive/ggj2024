using System;
using System.Reflection;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Rails;
using ExTween;
using ExTweenMonoGame;
using Microsoft.Xna.Framework;

namespace GGJ2024;

public class Face : IDrawHook, IUpdateHook
{
    public Face()
    {
        Animator = new Tweenable(this);
    }

    public Vector2 Position { get; set; }

    public float BrowTilt { get; set; }

    public float Radius { get; set; } = 64;
    public float RadiusFactor { get; set; } = 1f;
    public float OpenAmount { get; set; } = 1f;
    public float LowerOpenAmount { get; set; } = 1f;
    public float UpperOpenAmount { get; set; } = 1f;

    public float HeadTilt { get; set; }

    public float DistanceBetweenEyes { get; set; } = 128;

    public Tweenable Animator { get; }

    public void Draw(Painter painter)
    {
        DrawEye(painter, Position, LeftRight.Left);
        DrawEye(painter, Position, LeftRight.Right);
    }

    public void Update(float dt)
    {
    }

    private void DrawEye(Painter painter, Vector2 facePosition, LeftRight leftRight)
    {
        var sign = leftRight == LeftRight.Left ? -1 : 1;
        var tilt = BrowTilt * MathF.PI / 5 * sign / 2f;
        var eyePosition = facePosition + Vector2Extensions.Polar(DistanceBetweenEyes * sign, HeadTilt);
        var finalRadius = Radius * RadiusFactor;

        // eye
        painter.DrawCircle(eyePosition, finalRadius, new DrawSettings {Depth = Depth.Middle + 100});

        var rectHeight = finalRadius * 1.25f;

        var browColor = Color.Black;
        if (Client.Debug.IsActive)
        {
            browColor = Color.DarkBlue;
        }

        // upper lid
        var upperOpenAmount = Math.Min(OpenAmount, UpperOpenAmount);
        painter.DrawRectangle(
            new RectangleF(eyePosition + new Vector2(0, -finalRadius * upperOpenAmount - rectHeight / 2f),
                new Vector2(finalRadius * 3, rectHeight + 1)),
            new DrawSettings {Color = browColor, Angle = tilt, Origin = DrawOrigin.Center});

        // lower lid
        var lowerOpenAmount = Math.Min(OpenAmount, LowerOpenAmount);
        ;
        painter.DrawRectangle(
            new RectangleF(eyePosition + new Vector2(0, finalRadius * lowerOpenAmount + rectHeight / 2f),
                new Vector2(finalRadius * 3, rectHeight + 1)),
            new DrawSettings {Color = browColor, Angle = tilt, Origin = DrawOrigin.Center});
    }

    public ITween DoAnimation(string animationName, float lineDuration)
    {
        var method = Animator.GetType().GetMethod(animationName,
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        if (method == null)
        {
            Client.Debug.LogError($"{animationName} does not exist");
            return new EmptyTween();
        }

        var parameters = new object[] {lineDuration};

        if (method.GetParameters().Length == 0)
        {
            parameters = null;
        }

        var result = method.Invoke(Animator, parameters) as ITween;

        if (result == null)
        {
            throw new Exception($"Failed to call {animationName}");
        }

        return result;
    }

    public class Tweenable
    {
        private readonly TweenableFloat _browTilt;
        private readonly TweenableFloat _distance;
        private readonly Face _face;
        private readonly TweenableFloat _lowerOpen;
        private readonly TweenableFloat _openAmount;
        private readonly TweenableFloat _upperOpen;

        public Tweenable(Face face)
        {
            _face = face;
            Position = new TweenableVector2(() => _face.Position, val => _face.Position = val);
            RadiusFactor = new TweenableFloat(() => _face.RadiusFactor, val => _face.RadiusFactor = val);
            _distance = new TweenableFloat(() => _face.DistanceBetweenEyes, val => _face.DistanceBetweenEyes = val);
            _openAmount = new TweenableFloat(() => _face.OpenAmount, val => _face.OpenAmount = val);
            _lowerOpen = new TweenableFloat(() => _face.LowerOpenAmount, val => _face.LowerOpenAmount = val);
            _upperOpen = new TweenableFloat(() => _face.UpperOpenAmount, val => _face.UpperOpenAmount = val);
            _browTilt = new TweenableFloat(() => _face.BrowTilt, val => _face.BrowTilt = val);
            HeadTilt = new TweenableFloat(() => _face.HeadTilt, val => _face.HeadTilt = val);
        }

        public TweenableVector2 Position { get; }
        public TweenableFloat HeadTilt { get; }
        public TweenableFloat RadiusFactor { get; }

        public ITween Wake()
        {
            return new SequenceTween()
                    .Add(_openAmount.CallbackSetTo(0f))
                    .Add(_openAmount.TweenTo(0.1f, 1f, Ease.CubicFastSlow))
                    .Add(_openAmount.TweenTo(0f, 0.15f, Ease.CubicFastSlow))
                    .Add(_openAmount.TweenTo(0.25f, 0.25f, Ease.CubicFastSlow))
                    .Add(_openAmount.TweenTo(0f, 0.15f, Ease.CubicFastSlow))
                    .Add(_openAmount.TweenTo(0.55f, 0.25f, Ease.CubicFastSlow))
                ;
        }

        public ITween Blink(float fullDuration)
        {
            var duration = fullDuration / 2f;
            return new SequenceTween()
                    .Add(_openAmount.TweenTo(0, duration, Ease.CubicFastSlow))
                    .Add(_openAmount.TweenTo(1f, duration / 2f, Ease.CubicFastSlow))
                ;
        }

        public ITween Sleepy(float duration)
        {
            return new MultiplexTween()
                    .AddChannel(_openAmount.TweenTo(0.5f, duration, Ease.CubicFastSlow))
                    .AddChannel(_browTilt.TweenTo(0f, duration, Ease.CubicFastSlow))
                    .AddChannel(_lowerOpen.TweenTo(1f, duration, Ease.CubicFastSlow))
                    .AddChannel(RadiusFactor.TweenTo(1f, duration, Ease.CubicFastSlow))
                ;
        }

        public ITween Sad(float duration)
        {
            return new MultiplexTween()
                    .AddChannel(_browTilt.TweenTo(1f, duration, Ease.CubicFastSlow))
                    .AddChannel(_openAmount.TweenTo(1f, duration, Ease.CubicFastSlow))
                    .AddChannel(_lowerOpen.TweenTo(1f, duration, Ease.CubicFastSlow))
                    .AddChannel(RadiusFactor.TweenTo(1f, duration, Ease.CubicFastSlow))
                ;
        }

        public ITween Morose(float duration)
        {
            return new MultiplexTween()
                    .AddChannel(_browTilt.TweenTo(1f, duration, Ease.CubicFastSlow))
                    .AddChannel(_openAmount.TweenTo(1f, duration, Ease.CubicFastSlow))
                    .AddChannel(_lowerOpen.TweenTo(0.7f, duration, Ease.CubicFastSlow))
                    .AddChannel(RadiusFactor.TweenTo(1.2f, duration, Ease.CubicFastSlow))
                ;
        }

        public ITween Mad(float duration)
        {
            return new MultiplexTween()
                    .AddChannel(_browTilt.TweenTo(-1f, duration, Ease.CubicFastSlow))
                    .AddChannel(_openAmount.TweenTo(0.75f, duration, Ease.CubicFastSlow))
                    .AddChannel(_lowerOpen.TweenTo(1f, duration, Ease.CubicFastSlow))
                    .AddChannel(RadiusFactor.TweenTo(1f, duration, Ease.CubicFastSlow))
                ;
        }

        public ITween Happy(float duration)
        {
            return new MultiplexTween()
                    .AddChannel(_browTilt.TweenTo(0.15f, duration, Ease.CubicFastSlow))
                    .AddChannel(_lowerOpen.TweenTo(0.65f, duration, Ease.CubicFastSlow))
                    .AddChannel(_openAmount.TweenTo(1f, duration, Ease.CubicFastSlow))
                    .AddChannel(RadiusFactor.TweenTo(1f, duration, Ease.CubicFastSlow))
                ;
        }

        public ITween Focused(float fullDuration)
        {
            return new SequenceTween()
                    .Add(
                        _openAmount.TweenTo(1f, fullDuration / 4, Ease.CubicFastSlow)
                        )
                    .Add(
                        _openAmount.TweenTo(0.5f, fullDuration * 3 / 4, Ease.CubicFastSlow)
                    )
                ;
        }

        public ITween Idle(float duration)
        {
            return new MultiplexTween()
                    .AddChannel(_browTilt.TweenTo(0f, duration, Ease.CubicFastSlow))
                    .AddChannel(_lowerOpen.TweenTo(1f, duration, Ease.CubicFastSlow))
                    .AddChannel(_openAmount.TweenTo(1f, duration, Ease.CubicFastSlow))
                    .AddChannel(RadiusFactor.TweenTo(1f, duration, Ease.CubicFastSlow))
                ;
        }

        public ITween Shocked(float duration)
        {
            return new MultiplexTween()
                    .AddChannel(_browTilt.TweenTo(0.5f, duration, Ease.CubicFastSlow))
                    .AddChannel(
                        new SequenceTween()
                            .Add(RadiusFactor.TweenTo(0.8f, duration/2f, Ease.CubicFastSlow))
                            .Add(RadiusFactor.TweenTo(1f, duration/2f, Ease.CubicSlowFast))
                        )
                    .AddChannel(_openAmount.TweenTo(1.5f, duration, Ease.CubicFastSlow))
                    .AddChannel(_lowerOpen.TweenTo(1f, duration, Ease.CubicFastSlow))
                ;
        }

        public ITween None(float duration)
        {
            return new WaitSecondsTween(duration);
        }
    }
}
