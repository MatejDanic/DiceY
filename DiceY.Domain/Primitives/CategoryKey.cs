namespace DiceY.Domain.Primitives;

public readonly record struct CategoryKey(string Value)
{
    public override string ToString() => Value;

    public static CategoryKey Of(string value)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(value, nameof(value));
        return new CategoryKey(value);
    }
}