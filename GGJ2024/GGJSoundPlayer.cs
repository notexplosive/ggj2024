using ExplogineMonoGame;
using Microsoft.Xna.Framework.Audio;

namespace GGJ2024;

public static class GGJSoundPlayer
{
    public static void Play(string soundName, float volume = 0.5f, float pitch = 0f)
    {
        var sound = Client.Assets.GetSoundEffectInstance(soundName);
        sound.Stop();
        sound.Volume = volume;
        sound.Pitch = pitch;
        sound.Play();
    }
    
    public static void PlayIfAble(string soundName, float volume = 0.5f, float pitch = 0f)
    {
        var sound = Client.Assets.GetSoundEffectInstance(soundName);
        if (sound.State == SoundState.Stopped)
        {
            sound.Volume = volume;
            sound.Pitch = pitch;
            sound.Play();
        }
    }
}
