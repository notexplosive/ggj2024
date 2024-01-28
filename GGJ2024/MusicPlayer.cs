using ExplogineMonoGame;
using ExTween;
using Microsoft.Xna.Framework.Audio;

namespace GGJ2024;

public static class MusicPlayer
{
    private static SoundEffectInstance mainSong = null!;
    private static SoundEffectInstance menuSong = null!;
    private static bool hasSetup;
    private static SequenceTween tween = new();
    private static TweenableFloat mainFader = null!;
    private static TweenableFloat menuFader = null!;

    public static void Setup()
    {
        if (MusicPlayer.hasSetup)
        {
            return;
        }
        MusicPlayer.mainSong = Client.Assets.GetSoundEffectInstance("game/game-theme");
        MusicPlayer.menuSong = Client.Assets.GetSoundEffectInstance("game/menu-theme");

        MusicPlayer.mainSong.IsLooped = true;
        MusicPlayer.menuSong.IsLooped = true;
        
        MusicPlayer.mainSong.Volume = 0;
        MusicPlayer.menuSong.Volume = 0;
        
        MusicPlayer.mainSong.Play();
        MusicPlayer.menuSong.Play();

        MusicPlayer.hasSetup = true;
        
        MusicPlayer.mainFader = new TweenableFloat(0);
        MusicPlayer.menuFader = new TweenableFloat(0);
    }

    public static void FadeToMain()
    {
        MusicPlayer.Setup();
        MusicPlayer.tween.Clear();
        MusicPlayer.tween.Add(new MultiplexTween()
            .AddChannel(MusicPlayer.mainFader.TweenTo(1, 1, Ease.Linear))
            .AddChannel(MusicPlayer.menuFader.TweenTo(0, 1, Ease.Linear))
        );
    }

    public static void FadeToMenu()
    {
        MusicPlayer.Setup();
        MusicPlayer.tween.Clear();
        MusicPlayer.tween.Add(new MultiplexTween()
            .AddChannel(MusicPlayer.mainFader.TweenTo(0, 1, Ease.Linear))
            .AddChannel(MusicPlayer.menuFader.TweenTo(1, 1, Ease.Linear))
        );
    }

    public static void FadeOut()
    {
        MusicPlayer.Setup();
        MusicPlayer.tween.Clear();
        MusicPlayer.tween.Add(new MultiplexTween()
            .AddChannel(MusicPlayer.mainFader.TweenTo(0, 1, Ease.Linear))
            .AddChannel(MusicPlayer.menuFader.TweenTo(0, 1, Ease.Linear))
        );
    }

    public static void Update(float dt)
    {
        if (MusicPlayer.hasSetup)
        {
            MusicPlayer.tween.Update(dt);

            MusicPlayer.mainSong.Volume = MusicPlayer.mainFader.Value * 0.15f;
            MusicPlayer.menuSong.Volume = MusicPlayer.menuFader.Value * 0.15f;
        }
    }
}
