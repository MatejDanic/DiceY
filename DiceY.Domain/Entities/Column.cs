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
    public IReadOnlyList<Category> Categories => _categories;
    public bool IsCompleted => _categories.All(c => c.Score.HasValue);
    public int Score => _calculateScore(_categories);

    private readonly IOrderPolicy _policy;
    private readonly CalculateScore _calculateScore;
    private readonly ImmutableArray<Category> _categories;

    public Column(ColumnDefinition def, IEnumerable<Category> categories)
    {
        ArgumentNullException.ThrowIfNull(def);
        ArgumentNullException.ThrowIfNull(categories);
        Key = def.Key;
        _policy = def.Policy;
        _calculateScore = def.CalculateScore;
        _categories = [.. categories.DistinctBy(c => c.Key)];
        if (_categories.IsDefault) throw new ArgumentException(nameof(categories));
    }

    private Column(ColumnKey key, IOrderPolicy policy, CalculateScore calc, ImmutableArray<Category> categories)
    {
        Key = key;
        _policy = policy;
        _calculateScore = calc;
        _categories = categories;
    }

    public Column Fill(IReadOnlyList<Die> dice, CategoryKey categoryKey)
    {
        ArgumentNullException.ThrowIfNull(dice);
        if (!_policy.CanFill(_categories, categoryKey))
            throw new ColumnFillPolicyException(Key, categoryKey);

        var category = _categories.Where(c => c.Key == categoryKey).First();
        var updated = category.Fill(dice);
        var newCategories = _categories.SetItem(_categories.IndexOf(category), updated);
        return new Column(Key, _policy, _calculateScore, newCategories);
    }
}