namespace DiceY.Domain.Primitives;

public readonly record struct ColumnKey(String Value)
{
    public override string ToString() => Value;
}
