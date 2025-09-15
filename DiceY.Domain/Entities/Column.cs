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
    public int Score => _calc(_categories);

    private readonly IOrderPolicy _policy;
    private readonly CalculateScore _calc;
    private readonly ImmutableArray<Category> _categories;
    private readonly ImmutableDictionary<CategoryKey, int> _index;

    public Column(ColumnDefinition def, IEnumerable<Category> categories)
    {
        ArgumentNullException.ThrowIfNull(def, nameof(def));
        ArgumentNullException.ThrowIfNull(categories, nameof(categories));
        Key = def.Key;
        _policy = def.Policy;
        _calc = def.CalculateScore;
        _categories = [.. categories.DistinctBy(c => c.Key)];
        _index = _categories.Select((c, i) => (c.Key, i)).ToImmutableDictionary(t => t.Key, t => t.i);
    }

    private Column(ColumnKey key, IOrderPolicy policy, CalculateScore calc, ImmutableArray<Category> categories, ImmutableDictionary<CategoryKey, int> index)
    {
        Key = key;
        _policy = policy;
        _calc = calc;
        _categories = categories;
        _index = index;
    }

    public Column Fill(IReadOnlyList<Die> dice, CategoryKey categoryKey)
    {
        ArgumentNullException.ThrowIfNull(dice);
        if (!_policy.CanFill(_categories, categoryKey)) throw new ColumnFillPolicyException(Key, categoryKey);
        if (!_index.TryGetValue(categoryKey, out var i)) throw new KeyNotFoundException(categoryKey.ToString());
        var updated = _categories[i].Fill(dice);
        var newCategories = _categories.SetItem(i, updated);
        return WithCategories(newCategories);
    }

    public Column WithCategories(IEnumerable<Category> categories)
    {
        ArgumentNullException.ThrowIfNull(categories);
        var index = categories.Select((c, i) => (c.Key, i)).ToImmutableDictionary(t => t.Key, t => t.i);
        return new Column(Key, _policy, _calc, [.. categories], index);
    }
}
