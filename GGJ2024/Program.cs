using ExplogineDesktop;
using ExplogineMonoGame;
using GGJ2024;
using Microsoft.Xna.Framework;

var config = new WindowConfigWritable
{
    WindowSize = new Point(1600, 900),
    Title = "NotExplosive.net",
        
#if !DEBUG
    Fullscreen = true
#endif
};

Bootstrap.Run(args, new WindowConfig(config), runtime =>
{
    var hostCartridge = new HostCartridge(runtime);
    var hostRuntime = new HostRuntime(runtime, hostCartridge);
    hostCartridge.Add(new GGJCartridge(hostRuntime));
    hostCartridge.Add(new VampireCartridge(hostRuntime));
    return hostCartridge;
});