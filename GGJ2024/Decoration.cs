using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GGJ2024;

public readonly struct Decoration
{
    public Decoration()
    {
    }

    public RectangleF Rectangle { get; init; } = default;
    public Texture2D? Texture { get; init; } = null;
    public Color Color { get; init; } = Color.White;
}
