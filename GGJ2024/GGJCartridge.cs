using System.Collections.Generic;
using ExplogineCore;
using ExplogineMonoGame;
using ExplogineMonoGame.Cartridges;
using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;

namespace GGJ2024;

public class GGJCartridge : BasicGameCartridge
{
    public GGJCartridge(IRuntime runtime) : base(runtime)
    {
    }

    public override CartridgeConfig CartridgeConfig { get; } = new(new Point(1920, 1080));
    public override void OnCartridgeStarted()
    {
        Client.Debug.Log("Awakened");
    }

    public override void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        
    }

    public override void Update(float dt)
    {
        
    }

    public override void Draw(Painter painter)
    {
        
    }

    public override void AddCommandLineParameters(CommandLineParametersWriter parameters)
    {
        
    }

    public override IEnumerable<ILoadEvent> LoadEvents(Painter painter)
    {
        Client.Debug.Log("Load");
        yield break;
    }

    public override void Unload()
    {
    }

    public override void OnHotReload()
    {
    }
}