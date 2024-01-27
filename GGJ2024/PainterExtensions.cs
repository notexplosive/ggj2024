using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;

namespace GGJ2024;

public static class PainterExtensions
{
    public static void DrawCircle(this Painter painter, Vector2 position, float radius, DrawSettings drawSettings)
    {
        painter.DrawAtPosition(Client.Assets.GetTexture("game/circle"), position - new Vector2(radius), new Scale2D(radius / 64f), drawSettings);
    }
}
