using ExplogineMonoGame;
using ExplogineMonoGame.Cartridges;

namespace GGJ2024;

public class HostCartridge : HotReloadCartridge
{
    public HostCartridge(IRuntime runtime, params Cartridge[] startingCartridges) : base(runtime, startingCartridges)
    {
    }
}