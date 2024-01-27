using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GGJ2024;

public static class InputCalculations
{
    public static Vector2 CalculateInputVector(ConsumableInput consumableInput)
    {
        var result = new Vector2();
        var keyboard = consumableInput.Keyboard;
        
        if (keyboard.GetButton(Keys.Left).IsDown || keyboard.GetButton(Keys.A).IsDown)
        {
            result.X = -1;
        }
        
        if (keyboard.GetButton(Keys.Right).IsDown || keyboard.GetButton(Keys.D).IsDown)
        {
            result.X = 1;
        }
        
        if (keyboard.GetButton(Keys.Down).IsDown || keyboard.GetButton(Keys.S).IsDown)
        {
            result.Y = 1;
        }
        
        if (keyboard.GetButton(Keys.Up).IsDown || keyboard.GetButton(Keys.W).IsDown)
        {
            result.Y = -1;
        }

        if (result.Length() > 0)
        {
            return result.Normalized();
        }

        return result;
    }
}
