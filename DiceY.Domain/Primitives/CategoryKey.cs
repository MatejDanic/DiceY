namespace DiceY.Domain.Primitives;

public readonly record struct CategoryKey(String Value)
{
    public override string ToString() => Value;
}
