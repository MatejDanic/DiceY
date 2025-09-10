using DiceY.Domain.Interfaces;

namespace DiceY.Domain.Entities;

public sealed class Die
{
    private readonly int _sides;
    public int Value { get; private set; }

    public Die(int sides = 6, int value = 6)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(sides, 1, nameof(sides));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(sides, 10, nameof(sides));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, sides);
        _sides = sides;
        Value = value;
    }

    public void Roll(IRollService rng)
    {
        Value = rng.NextRoll(_sides);
    }
}
