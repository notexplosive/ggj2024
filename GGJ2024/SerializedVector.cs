using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace GGJ2024;

[Serializable]
public struct SerializedVector
{
    public SerializedVector(float x, float y)
    {
        X = x;
        Y = y;
    }

    [JsonProperty("x")]
    public float X { get; set; }
    
    [JsonProperty("y")]
    public float Y { get; set; }

    public Vector2 ToVector2()
    {
        return new(X, Y);
    }
}
