using DiceY.Domain.Delegates;
using DiceY.Domain.Interfaces;
using DiceY.Domain.Primitives;

namespace DiceY.Domain.ValueObjects;

public sealed record ColumnDefinition
{
    public ColumnKey Key;
    public IOrderPolicy Policy;
    public CalculateScore CalculateScore;

    public ColumnDefinition(ColumnKey key, IOrderPolicy policy, CalculateScore calculateScore)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        ArgumentNullException.ThrowIfNull(policy, nameof(policy));
        ArgumentNullException.ThrowIfNull(calculateScore, nameof(calculateScore));
        Key = key;
        Policy = policy;
        CalculateScore = calculateScore;
    }
}
