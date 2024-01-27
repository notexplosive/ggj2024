using ExplogineMonoGame;

namespace GGJ2024;

public class HostRuntime : IRuntime
{
    private readonly IRuntime _innerRuntime;
    public HostCartridge HostCartridge { get; }

    public HostRuntime(IRuntime runtime, HostCartridge hostCartridge)
    {
        _innerRuntime = runtime;
        HostCartridge = hostCartridge;
    }

    public IWindow Window => _innerRuntime.Window;
    public ClientFileSystem FileSystem => _innerRuntime.FileSystem;
}