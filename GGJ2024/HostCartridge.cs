using ExplogineMonoGame;
using ExplogineMonoGame.Cartridges;

namespace GGJ2024;

public class HostCartridge : HotReloadCartridge
{
    public int StoryProgress { get; set; } = 0;

    public HostCartridge(IRuntime runtime, params Cartridge[] startingCartridges) : base(runtime, startingCartridges)
    {
    }

    protected override void BeforeUpdate(float dt)
    {
        MusicPlayer.Update(dt);
    }

    public override void OnHotReload()
    {
        StoryProgress = 0;
    }
}