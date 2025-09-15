using DiceY.Domain.Exceptions;
using DiceY.Domain.Interfaces;
using DiceY.Domain.Primitives;
using DiceY.Domain.ValueObjects;

namespace DiceY.Domain.Entities;

public sealed class Category
{
    private readonly IScoringRule _rule;
    public CategoryKey Key { get; }
    public int? Score { get; }

    public Category(CategoryDefinition def, int? score = null)
    {
        ArgumentNullException.ThrowIfNull(def);
        Key = def.Key;
        _rule = def.Rule;
        Score = score;
    }

    private Category(CategoryKey key, IScoringRule rule, int? score)
    {
        Key = key;
        _rule = rule;
        Score = score;
    }

    public bool CanFill => !Score.HasValue;

    public Category Fill(IReadOnlyList<Die> dice)
    {
        ArgumentNullException.ThrowIfNull(dice);
        if (Score.HasValue) throw new CategoryAlreadyScoredException(Key);
        return WithScore(_rule.GetScore(dice));
    }

    public Category WithScore(int? score) => new(Key, _rule, score);
}
