namespace DiceY.Domain.Primitives;

public readonly record struct ColumnKey(string Value)
{
    public override string ToString() => Value;

    public static ColumnKey Of(string value)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(value, nameof(value));
        return new ColumnKey(value);
    }
}