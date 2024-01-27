using System;
using System.Collections.Generic;

namespace GGJ2024;

[Serializable]
public class DialogueLine
{
    public float Duration { get; set; } = 1f;
    public string? Text { get; set; }
    public string Animation { get; set; } = "Idle";
    public float Tilt { get; set; }
    public SerializedVector Position { get; set; } = new SerializedVector(0.5f, 0.5f);
}

[Serializable]
public class Cutscene
{
    public List<DialogueLine> Dialogue { get; set; } = new();
}