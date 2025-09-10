namespace DiceY.Domain.Primitives;

public readonly record struct ColumnKey(String Key)
{
    public override string ToString() => Key;
}
