using DiceY.Domain.Interfaces;

namespace DiceY.Domain.Entities;

public sealed class Die
{
    private readonly int _sides;
    public int Value { get; }

    public Die(int sides = 6, int value = 6)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(sides, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(sides, 11);
        ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, sides);
        _sides = sides;
        Value = value;
    }

    public Die Roll(IRollService rng)
    {
        ArgumentNullException.ThrowIfNull(rng);
        return WithValue(rng.NextRoll(_sides));
    }

    public Die WithValue(int value) => new(_sides, value);
}
