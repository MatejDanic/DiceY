using DiceY.Domain.Exceptions;
using DiceY.Domain.Interfaces;
using DiceY.Domain.Primitives;
using DiceY.Domain.Delegates;

namespace DiceY.Domain.Entities;

public sealed class Column
{
    public ColumnKey Key { get; }
    private readonly IOrderPolicy _policy;
    private readonly CalculateScore _calculateScore;
    public IReadOnlyList<Category> Categories { get; }
    public bool IsCompleted => Categories.All(c => c.Score.HasValue);
    public int Score => _calculateScore(Categories);

    public Column(ColumnKey key, IOrderPolicy policy, CalculateScore calculateScore, SortedSet<Category> categories)
    {
        ArgumentNullException.ThrowIfNull(key, nameof(key));
        ArgumentNullException.ThrowIfNull(policy, nameof(policy));
        ArgumentNullException.ThrowIfNull(calculateScore, nameof(calculateScore));
        ArgumentNullException.ThrowIfNull(categories, nameof(categories));
        Key = key;
        _policy = policy;
        _calculateScore = calculateScore;
        Categories = [.. categories.Distinct()];
    }


    public void Fill(IReadOnlyList<Die> dice, CategoryKey categoryKey)
    {
        if (_policy.CanFill(Categories, categoryKey)) throw new ColumnFillPolicyException(Key, categoryKey);
        throw new NotImplementedException();
    }
}

