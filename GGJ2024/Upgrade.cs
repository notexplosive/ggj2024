namespace GGJ2024;

public abstract class Upgrade
{
    protected abstract void Apply();

    public void Buy()
    {
        Apply();
    }

    public abstract string Description();
}
