using System;

namespace GGJ2024;

public class Stat<T>
{
    public event Action<T> ValueChanged;
    public string Name { get; }
    public T Value
    {
        get => _value;
        set
        {
            _value = value;
            ValueChanged?.Invoke(value);
            
        }
    }

    private T _value = default!;

    public Stat(string name, T startingValue)
    {
        Name = name;
        Value = startingValue;
    }

    public void Bind(Action<T> binding)
    {
        binding(_value);
        ValueChanged += binding;
    }
}
