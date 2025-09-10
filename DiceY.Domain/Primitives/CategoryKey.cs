namespace DiceY.Domain.Primitives;

public readonly record struct CategoryKey(String Key)
{
    public override string ToString() => Key;
}
