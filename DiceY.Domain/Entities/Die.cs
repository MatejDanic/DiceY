using DiceY.Domain.Interfaces;

namespace DiceY.Domain.Entities;

public sealed class Die
{
    private readonly int _sides;
    public int Value { get; }

    public Die(int sides = 6, int value = 6)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(sides, 1, nameof(sides));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(sides, 10, nameof(sides));
        ArgumentOutOfRangeException.ThrowIfLessThan(value, 1, nameof(sides));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, sides, nameof(value));
        _sides = sides;
        Value = value;
    }

    public Die Roll(IRollService rng)
    {
        ArgumentNullException.ThrowIfNull(rng);
        var newValue = rng.NextRoll(_sides);
        return new Die(_sides, newValue);
    }
}
