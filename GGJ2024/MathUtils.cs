using System;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;

namespace GGJ2024;

public class MathUtils
{
    public static float Squared(float f)
    {
        return f * f;
    }

    public static int Squared(int i)
    {
        return i * i;
    }

    public static Vector2 CalculateDirectionalDisplacement(Vector2 position, Vector2 vector2)
    {
        var displacement = position - vector2;
        if (displacement.LengthSquared() > 0)
        {
            displacement = displacement.Normalized();
        }

        return displacement;
    }

    public static bool IsVerySmall(Vector2 vector)
    {
        return vector.LengthSquared() < 0.1f;
    }

    public static Vector2 GetRandomPointAlongEdge(RectangleF rect, RectEdge edge)
    {
        switch (edge)
        {
            case RectEdge.Left:
                return new Vector2(rect.Left, Client.Random.Clean.NextFloat(0, rect.Height));
            case RectEdge.Right:
                return new Vector2(rect.Right, Client.Random.Clean.NextFloat(0, rect.Height));
            case RectEdge.Top:
                return new Vector2(Client.Random.Clean.NextFloat(0, rect.Width), rect.Top);
            case RectEdge.Bottom:
                return new Vector2(Client.Random.Clean.NextFloat(0, rect.Width), rect.Bottom);
        }

        throw new Exception("invalid edge");
    }
}
