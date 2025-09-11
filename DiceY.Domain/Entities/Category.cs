using DiceY.Domain.Exceptions;
using DiceY.Domain.Interfaces;
using DiceY.Domain.Primitives;

namespace DiceY.Domain.Entities;

public sealed class Category
{
    public CategoryKey Key { get; }
    private readonly IScoringRule _rule;
    public int? Score { get; }

    public Category(CategoryKey key, IScoringRule rule, int? score = null)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        ArgumentNullException.ThrowIfNull(rule, nameof(rule));
        Key = key;
        _rule = rule;
        Score = score;
    }

    public Category Fill(IReadOnlyList<Die> dice)
    {
        if (Score.HasValue)
            throw new CategoryAlreadyScoredException(Key);

        var newScore = _rule.GetScore(dice);
        return new Category(Key, _rule, newScore);
    }
}
