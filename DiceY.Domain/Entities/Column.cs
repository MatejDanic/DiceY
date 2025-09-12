using DiceY.Domain.Delegates;
using DiceY.Domain.Exceptions;
using DiceY.Domain.Interfaces;
using DiceY.Domain.Primitives;
using DiceY.Domain.ValueObjects;
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

    public Column(ColumnDefinition def, IEnumerable<Category> categories)
    {
        ArgumentNullException.ThrowIfNull(def);
        ArgumentNullException.ThrowIfNull(categories);

        Key = def.Key;
        _policy = def.Policy;
        _calculateScore = def.CalculateScore;

        Categories = [.. categories.DistinctBy(c => c.Key)];
    }

    private Column(ColumnKey key, IOrderPolicy policy, CalculateScore calc, ImmutableArray<Category> categories)
    {
        Key = key;
        _policy = policy;
        _calculateScore = calc;
        Categories = categories;
    }

    public Column Fill(IReadOnlyList<Die> dice, CategoryKey categoryKey)
    {
        ArgumentNullException.ThrowIfNull(dice);
        if (!_policy.CanFill(Categories, categoryKey))
            throw new ColumnFillPolicyException(Key, categoryKey);

        var index = Categories
            .Select((c, i) => new { c, i })
            .FirstOrDefault(x => x.c.Key.Equals(categoryKey))?.i ?? -1;
        if (index < 0)
            throw new ArgumentException($"Category {categoryKey} not found in column {Key}", nameof(categoryKey));

        var updated = Categories[index].Fill(dice);
        var newCategories = Categories.SetItem(index, updated);

        return new Column(Key, _policy, _calculateScore, newCategories);
    }
}
