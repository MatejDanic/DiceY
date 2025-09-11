using DiceY.Domain.Exceptions;
using DiceY.Domain.Interfaces;
using DiceY.Domain.Primitives;
using DiceY.Domain.Delegates;
using System.Collections.Immutable;

namespace DiceY.Domain.Entities;

public sealed class Column
{
    public ColumnKey Key { get; }
    private readonly IOrderPolicy _policy;
    private readonly CalculateScore _calculateScore;
    public ImmutableArray<Category> Categories { get; }
    public bool IsCompleted => Categories.All(c => c.Score.HasValue);
    public int Score => _calculateScore(Categories);

    public Column(ColumnKey key, IOrderPolicy policy, CalculateScore calculateScore, IReadOnlyList<Category> categories)
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

    public Column Fill(IReadOnlyList<Die> dice, CategoryKey categoryKey)
    {
        if (!_policy.CanFill(Categories, categoryKey))
            throw new ColumnFillPolicyException(Key, categoryKey);

        var index = Categories.IndexOf(Categories.FirstOrDefault(c => c.Key.Equals(categoryKey)));
        if (index < 0)
            throw new ArgumentException("Category not found in column.", nameof(categoryKey));

        var updated = Categories[index].Fill(dice);
        var newCategories = Categories.SetItem(index, updated);
        return new Column(Key, _policy, _calculateScore, newCategories);
    }
}
