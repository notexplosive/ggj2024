using System;
using System.Collections.Generic;
using ExplogineMonoGame;

namespace GGJ2024;

public class VfxCollection
{
    private List<VfxInstance> _content = new();
    public void Update(float dt)
    {
        foreach (var vfx in _content)
        {
            vfx.Update(dt);
        }
        
        _content.RemoveAll(a=>a.ShouldDie());
    }

    public void Draw(Painter painter)
    {
        foreach (var vfx in _content)
        {
            vfx.Draw(painter);
        }
    }

    public void Add(VfxInstance instance)
    {
        _content.Add(instance);
    }
}