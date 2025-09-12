using DiceY.Domain.Interfaces;
using DiceY.Domain.Primitives;

namespace DiceY.Domain.ValueObjects;

public sealed record CategoryDefinition
{
    public CategoryKey Key;
    public IScoringRule Rule;

    public CategoryDefinition(CategoryKey key, IScoringRule rule)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        ArgumentNullException.ThrowIfNull(rule, nameof(rule));
        Key = key;
        Rule = rule;
    }
}