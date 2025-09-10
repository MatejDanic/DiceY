using DiceY.Domain.Exceptions;
using DiceY.Domain.Interfaces;
using DiceY.Domain.Primitives;

namespace DiceY.Domain.Entities;

public sealed class Category
{
    public CategoryKey Key { get; }
    private readonly IScoringRule _rule;
    public int? Score { get; private set; }

    public Category(CategoryKey key, IScoringRule rule)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        ArgumentNullException.ThrowIfNull(rule, nameof(rule));
        Key = key;
        _rule = rule;
    }

    public void Fill(IReadOnlyList<Die> dice)
    {
        if (Score.HasValue) throw new CategoryAlreadyScoredException(Key);
        Score = _rule.GetScore(dice);
        throw new NotImplementedException();
    }
}
